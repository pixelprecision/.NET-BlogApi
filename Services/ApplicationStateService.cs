namespace MyBlogApi.Services
{
    /// <summary>
    /// A service that maintains application state information.
    /// </summary>
    public interface IApplicationStateService
    {
        DateTime StartTime { get; }
        TimeSpan Uptime { get; }
        string UptimeString { get; }
        string Version { get; }
    }

    /// <summary>
    /// Implementation of the application state service.
    /// </summary>
    public class ApplicationStateService : IApplicationStateService
    {
        private readonly DateTime _startTime;
        
        public ApplicationStateService()
        {
            _startTime = DateTime.UtcNow;
        }

        public DateTime StartTime => _startTime;
        
        public TimeSpan Uptime => DateTime.UtcNow - _startTime;
        
        public string UptimeString
        {
            get
            {
                var uptime = Uptime;
                
                // Format the uptime in a human-readable way
                var parts = new List<string>();
                
                if (uptime.Days > 0)
                    parts.Add($"{uptime.Days} day{(uptime.Days == 1 ? "" : "s")}");
                
                if (uptime.Hours > 0)
                    parts.Add($"{uptime.Hours} hour{(uptime.Hours == 1 ? "" : "s")}");
                
                if (uptime.Minutes > 0)
                    parts.Add($"{uptime.Minutes} minute{(uptime.Minutes == 1 ? "" : "s")}");
                
                // Always show seconds if uptime is less than a minute
                if (parts.Count == 0 || uptime.TotalMinutes < 1)
                    parts.Add($"{uptime.Seconds} second{(uptime.Seconds == 1 ? "" : "s")}");
                
                return string.Join(", ", parts);
            }
        }
        
        public string Version => "1.0.0";  // You could read this from assembly attributes
    }
}