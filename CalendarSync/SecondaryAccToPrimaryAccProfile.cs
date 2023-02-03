﻿using Microsoft.Graph;

namespace CalendarSync
{
    public class SecondaryAccToPrimaryAccProfile : SyncProfile
    {
        public SecondaryAccToPrimaryAccProfile(string refreshToken)
        {
            RefreshToken = refreshToken;
            SubjectPrefix = "[RSM]";
        }

        public override Event MapEvent(Event e)
        {
            var mapped = new Event
            {
                Subject = $"{SubjectPrefix} {e.Subject}",
                Start = e.Start,
                End = e.End,
                IsAllDay = e.IsAllDay,
                ShowAs = e.ShowAs,
                BodyPreview = e.BodyPreview,
                Importance = e.Importance,
                Sensitivity = e.Sensitivity,
            };

            return mapped;
        }
    }
}
