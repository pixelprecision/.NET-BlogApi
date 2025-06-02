namespace MyBlogApi
{
    /// <summary>
    /// Holds application-wide information that needs to be accessed from various parts of the system.
    /// </summary>
    public static class ApplicationInfo
    {
        public static DateTime StartTime { get; set; }

        /// <summary>
        /// Calculates how long the application has been running.
        /// </summary>
        public static TimeSpan Uptime => DateTime.UtcNow - StartTime;

        /// <summary>
        /// A human-friendly version of the uptime.
        /// </summary>
        public static string UptimeString
        {
            get
            {
                var uptime = Uptime;
                if (uptime.TotalDays >= 1)
                {
                    return $"{(int)uptime.TotalDays} days, {uptime.Hours} hours, {uptime.Minutes} minutes";
                }
                else if (uptime.TotalHours >= 1)
                {
                    return $"{(int)uptime.TotalHours} hours, {uptime.Minutes} minutes";
                }
                else
                {
                    return $"{(int)uptime.TotalMinutes} minutes, {uptime.Seconds} seconds";
                }
            }
        }

        /// <summary>
        /// Additional metadata about the application
        /// </summary>
        public static string Version { get; set; } = "1.0.0";
        public static string Environment { get; set; } = "";
    }
}