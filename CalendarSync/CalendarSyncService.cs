﻿using Microsoft.Extensions.Logging;
using Microsoft.Graph;

namespace CalendarSync
{
	public class CalendarSyncService
	{
		private GraphService SourceGraph { get; set; }
		private GraphService DestinationGraph { get; set; }
		private DataverseService Dataverse { get; }
		private ILogger Log { get; set; }
		private SyncProfile SourceProfile { get; }
		private SyncProfile DestinationProfile { get; }
		private readonly string ClientId = "";
		private readonly string OrgConnectionString = "";

		public CalendarSyncService(SyncProfile sourceProfile, SyncProfile destProfile, ILogger logger, string clientId, string orgConnectionString)
		{
			SourceProfile = sourceProfile;
			DestinationProfile = destProfile;
			OrgConnectionString = orgConnectionString;
			Dataverse = new DataverseService(logger, OrgConnectionString);
			Log = logger;
			ClientId = clientId;
		}

		private async Task InitGraphClientsAsync()
		{
			var auth = new AuthService(ClientId);
			if (SourceGraph == null)
			{
				var response = await auth.GetToken(SourceProfile.RefreshToken);
				SourceGraph = new GraphService(response.AccessToken);
			}
			if (DestinationGraph == null)
			{
				var response = await auth.GetToken(DestinationProfile.RefreshToken);
				DestinationGraph = new GraphService(response.AccessToken);
			}
		}

		public async Task SyncRangeBidirectionalAsync(string start, string end)
		{
			await SyncRangeAsync(start, end);
			var reverseClient = new CalendarSyncService(DestinationProfile, SourceProfile, Log, ClientId, OrgConnectionString);
			await reverseClient.SyncRangeAsync(start, end);
		}

		public async Task SyncRangeAsync(string start, string end)
		{
			Log.LogInformation($"Syncing from {start} to {end}.");
			await InitGraphClientsAsync();

			// sync changes from source to dest
			var events = await SourceGraph.GetEventsInRangeAsync(start, end);
			var destinationIds = new HashSet<string>();

			foreach (var e in events)
			{
				var destKey = await SyncEventAsync(e);
				if (destKey != null)
				{
					destinationIds.Add(destKey);
				}
			}

			// delete mapped events in dest that were removed in source
			events = await DestinationGraph.GetEventsInRangeAsync(start, end);
			var eventsToDelete = events.Where(e => e.Subject.StartsWith(SourceProfile.SubjectPrefix) && !destinationIds.Contains(e.Id));
			foreach (var e in eventsToDelete)
			{
				await Dataverse.DeleteEventAsync(e.Id);
				await DestinationGraph.DeleteEventAsync(e.Id);
			}
		}

		public async Task<string> SyncEventAsync(Event e)
		{
			if (e.Subject.StartsWith(DestinationProfile.SubjectPrefix))
			{
				Log.LogInformation("Skipping event that originated in destination calendar.");
				return null;
			}

			// get or create event in d365
			var record = await Dataverse.GetOrCreateEventAsync(e, SourceProfile.SubjectPrefix);

			// map src event to dest event
			var mappedEvent = SourceProfile.MapEvent(e);
			var destEvent = record.DestinationKey != null ? await DestinationGraph.GetEventByIdAsync(record.DestinationKey) : null;

			if (destEvent == null)
			{
				// create in dest
				var destKey = await DestinationGraph.CreateEventAsync(mappedEvent);

				// update dest key in d365
				await Dataverse.UpdateDestKeyAsync(record.RecordId, destKey);

				// update these stats in case event was deleted
				await Dataverse.UpdateEventTimeAsync(record.RecordId, e, SourceProfile.SubjectPrefix);
				return destKey;
			}
			else
			{
				// get event from dest
				var graphEvent = await DestinationGraph.GetEventByIdAsync(record.DestinationKey);
				mappedEvent.Id = graphEvent.Id;
				await DestinationGraph.UpdateEventAsync(mappedEvent);
				await Dataverse.UpdateEventTimeAsync(record.RecordId, e, SourceProfile.SubjectPrefix);
				return graphEvent.Id;
			}
		}
	}
}