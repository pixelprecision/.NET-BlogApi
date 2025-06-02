// ========================================
// 2. Models/Post.cs
// ========================================
namespace MyBlogApi.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key relationship to User
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!; // Navigation property
    }
}