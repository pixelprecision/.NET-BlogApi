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
        
        // New properties for image support
        // This stores the URL or path to the image
        public string? ImageUrl { get; set; }
        
        // This stores the original filename uploaded by the user
        public string? ImageFileName { get; set; }
        
        // This stores the MIME type (e.g., "image/jpeg", "image/png")
        public string? ImageContentType { get; set; }
        
        // This stores the file size in bytes
        public long? ImageFileSize { get; set; }
        public string? ImageAltText { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Foreign key relationship to User
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!; // Navigation property
    }
}