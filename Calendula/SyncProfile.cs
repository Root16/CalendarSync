using Microsoft.Graph;

namespace Calendula
{
    public abstract class SyncProfile
    {
        public string SubjectPrefix { get; set; }
        public string RefreshToken { get; set; }
        public abstract Event MapEvent(Event calendarEvent);
    }
}
