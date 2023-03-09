using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendula
{
    public class PrimaryAccToSecondaryAccProfile : SyncProfile
    {
        public PrimaryAccToSecondaryAccProfile(string username, string subjectPrefix)
        {
            Username = username;
            SubjectPrefix = subjectPrefix;
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
