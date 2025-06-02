using Microsoft.AspNetCore.Mvc;
using MyBlogApi.Services;

namespace MyBlogApi.Controllers
{
    [ApiController]
    [Route("")] // Root endpoint
    public class HomeController : ControllerBase
    {
        private readonly IApplicationStateService _appState;
        
        public HomeController(IApplicationStateService appState)
        {
            _appState = appState;
        }
        
        // Home Route
        [HttpGet]
        public IActionResult GetHome()
        {
            var response = new
            {
                message = "Welcome to My Blog API",
                status = "Running",
                timestamp = DateTime.UtcNow,
                documentation = "/swagger",
                health = "Healthy",
                endpoints = new
                {
                    authentication = new[]
                    {
                        "POST /api/auth/register - Create a new account",
                        "POST /api/auth/login - Login to get JWT token"
                    },
                    posts = new[]
                    {
                        "GET /api/posts - Get all posts (requires authentication)",
                        "GET /api/posts/{id} - Get specific post",
                        "POST /api/posts - Create new post (requires authentication)",
                        "PUT /api/posts/{id} - Update post (requires authentication)",
                        "DELETE /api/posts/{id} - Delete post (requires authentication)"
                    }
                }
            };

            return Ok(response);
        }

        // /health Route
        [HttpGet("health")]
        public IActionResult GetHealth()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                uptime = _appState.UptimeString,
                uptimeSeconds = _appState.Uptime.TotalSeconds,
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                startTime = _appState.StartTime,
                version = _appState.Version
            });
        }

        // /api-info Route
        [HttpGet("api-info")]
        public IActionResult GetApiInfo()
        {
            return Ok(new
            {
                name = "My Blog API",
                version = "1.0.0",
                description = "A RESTful API for managing blog posts with JWT authentication",
                technologies = new[]
                {
                    "ASP.NET Core 8.0",
                    "Entity Framework Core",
                    "PostgreSQL",
                    "JWT Authentication",
                    "Swagger/OpenAPI"
                },
                developer = "Brenden Leib",
                repository = "https://github.com/pixelprecision/myblogapi" // Update this
            });
        }
    }
}