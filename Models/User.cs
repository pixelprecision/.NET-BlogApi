// ========================================
// 1. Models/User.cs
// ========================================
// This extends ASP.NET Core Identity's IdentityUser, which provides
// built-in properties like Email, UserName, PasswordHash, etc.
using Microsoft.AspNetCore.Identity;

namespace MyBlogApi.Models
{
    public class User : IdentityUser
    {
        // Adding custom properties beyond what Identity provides
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation property - Entity Framework will use this to create
        // the relationship between users and posts
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}