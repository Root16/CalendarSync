using Microsoft.Graph;
using System.Net.Http.Headers;

namespace Calendula
{
    public class GraphService
    {
        private GraphServiceClient Client { get; set; }

        public GraphService(string accessToken)
        {
            var authProvider = new DelegateAuthenticationProvider(request =>
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                return Task.CompletedTask;
            });
            Client = new GraphServiceClient(authProvider);
        }

        public async Task<IEnumerable<Event>> GetEventsInRangeAsync(string start, string end)
        {
            var queryOptions = new[]
            {
                new QueryOption("startDateTime", start),
                new QueryOption("endDateTime", end)
            };
            var allEvents = new List<Event>();
            var calendarEvents = await Client.Me.CalendarView.Request(queryOptions).GetAsync();
            allEvents.AddRange(calendarEvents);
            while (calendarEvents.NextPageRequest != null)
            {
                calendarEvents = await calendarEvents.NextPageRequest.GetAsync();
                allEvents.AddRange(calendarEvents);
            }
            return allEvents;
        }

        public async Task<Event> GetEventByIdAsync(string eventId)
        {
            try
            {
                var calendarEvent = await Client.Me.Events[eventId].Request().GetAsync();
                return calendarEvent;
            }
            catch
            {
                return null;
            }
        }

        public async Task<string> CreateEventAsync(Event calendarEvent)
        {
            var response = await Client.Me.Events.Request().AddAsync(calendarEvent);
            return response.Id;
        }

        public async Task UpdateEventAsync(Event calendarEvent)
        {
            await Client.Me.Events[calendarEvent.Id].Request().UpdateAsync(calendarEvent);
        }

        public async Task DeleteEventAsync(string eventId)
        {
            await Client.Me.Events[eventId].Request().DeleteAsync();
        }
    }
}
