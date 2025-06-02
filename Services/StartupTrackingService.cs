using Microsoft.Extensions.Hosting;
using MyBlogApi.Services;

namespace MyBlogApi.Services
{
    /// <summary>
    /// A hosted service that runs when the application starts and tracks startup-related information.
    /// </summary>
    public class StartupTrackingService : IHostedService
    {
        private readonly ILogger<StartupTrackingService> _logger;
        private readonly IApplicationStateService _appState;
        
        public StartupTrackingService(
            ILogger<StartupTrackingService> logger,
            IApplicationStateService appState)
        {
            _logger = logger;
            _appState = appState;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Runs on application start
            _logger.LogInformation("🚀 Application starting up at {StartTime:yyyy-MM-dd HH:mm:ss} UTC", 
                _appState.StartTime);
            
            // I will setup other tasks here:
            // - Check database connectivity
            // - Warm up caches
            // - Send startup notifications
            // - Log system information
            
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            _logger.LogInformation("Environment: {Environment}", environment);
            
            // Log some useful system information
            _logger.LogInformation("Operating System: {OS}", Environment.OSVersion);
            _logger.LogInformation("Machine Name: {MachineName}", Environment.MachineName);
            _logger.LogInformation("Processor Count: {ProcessorCount}", Environment.ProcessorCount);
            
            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            // Runs when shutting down
            var uptime = _appState.Uptime;
            _logger.LogInformation("👋 Application shutting down after running for {Uptime}", 
                _appState.UptimeString);
            
            // I'm going to set up cleanup tasks here:
            // - Save state
            // - Close connections gracefully
            // - Send shutdown notifications
            
            return Task.CompletedTask;
        }
    }
}