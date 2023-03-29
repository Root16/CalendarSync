using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Extensions;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using Entity = Microsoft.Xrm.Sdk.Entity;

namespace Calendula
{
    public class DataverseService
    {
        private ServiceClient Client { get; set; }
        private ILogger Log { get; set; }

        public DataverseService(ILogger logger, string orgConnectionString)
        {
            Client = new ServiceClient(orgConnectionString);
            Log = logger;
        }

        public async Task<DataverseEventResponse> GetOrCreateEventAsync(Event e, string sourceName)
        {
            Log.LogDebug($"Querying Dataverse for calendar event {e.Id}");

            var query = new QueryByAttribute("pl_calendarevent");
            query.AddAttributeValue("pl_sourcekey", e.Id);
            query.ColumnSet = new ColumnSet("pl_calendareventid", "pl_destinationkey");
            query.TopCount = 1;
            var response = await Client.RetrieveMultipleAsync(query);
            if (response.Entities.Any())
            {
                Log.LogDebug("Found a matching calendar event in Dataverse.");
                return new DataverseEventResponse(response.Entities.First());
            }
            else
            {
                var create = new Entity("pl_calendarevent")
                {
                    ["pl_sourcekey"] = e.Id,
                    ["pl_name"] = $"{sourceName} {e.Subject}",
                    ["pl_start"] = e.Start.ToDateTime(),
                    ["pl_end"] = e.End.ToDateTime(),
                };
                create.Id = await Client.CreateAsync(create);
                Log.LogDebug("Created a new calendar event in Dataverse.");
                return new DataverseEventResponse(create);
            }
        }

        public async Task UpdateDestKeyAsync(Guid id, string destinationKey)
        {
            await Client.UpdateAsync(new Entity("pl_calendarevent", id)
            {
                ["pl_destinationkey"] = destinationKey,
            });
            Log.LogDebug("Updated destination key in Dataverse.");
        }

        public async Task UpdateEventTimeAsync(Guid id, Event e, string sourceName)
        {
            await Client.UpdateAsync(new Entity("pl_calendarevent", id)
            {
                ["pl_name"] = $"{sourceName} {e.Subject}",
                ["pl_start"] = e.Start.ToDateTime(),
                ["pl_end"] = e.End.ToDateTime()
            });
            Log.LogDebug("Updated event time in Dataverse.");
        }

        public async Task DeleteEventAsync(string destinationId)
        {
            var query = new QueryExpression("pl_calendarevent");
            query.ColumnSet = new ColumnSet("pl_calendareventid");
            query.TopCount = 1;
            query.Criteria.AddCondition("pl_destinationkey", ConditionOperator.Equal, destinationId);
            var response = await Client.RetrieveMultipleAsync(query);
            if (!response.Entities.Any())
            {
                return;
            }

            var e = response.Entities.First();
            await Client.DeleteAsync(e.LogicalName, e.Id);

            Log.LogDebug("Deleted calendar event in Dataverse.");
        }
    }

    public class DataverseEventResponse
    {
        internal Guid RecordId { get; set; }
        internal string? DestinationKey { get; set; }
        internal DataverseEventResponse(Entity calendarEvent)
        {
            RecordId = calendarEvent.Id;
            DestinationKey = calendarEvent.GetAttributeValue<string?>("pl_destinationkey");
        }
    }
}
