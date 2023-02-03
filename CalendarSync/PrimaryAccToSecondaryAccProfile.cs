using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalendarSync
{
    public class PrimaryAccToSecondaryAccProfile : SyncProfile
    {
        public PrimaryAccToSecondaryAccProfile(string refreshToken)
        {
            RefreshToken = refreshToken;
            SubjectPrefix = "[Root16]";
        }

        public override Event MapEvent(Event e)
        {
            var mapped = new Event
            {
                Subject = SubjectPrefix,
                Start = e.Start,
                End = e.End,
                IsAllDay = e.IsAllDay,
                ShowAs = e.ShowAs,
                Importance = e.Importance,
                Sensitivity = e.Sensitivity,
            };

            return mapped;
        }
    }
}
