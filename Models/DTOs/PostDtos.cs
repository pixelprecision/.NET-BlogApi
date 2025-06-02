// ========================================
// 4. Models/DTOs/PostDtos.cs
// ========================================
namespace MyBlogApi.Models.DTOs
{
    public class CreatePostDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ImageAltText { get; set; }
    }
    
    // For creating posts with file upload
    public class CreatePostWithImageDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }  // This represents an uploaded file
        public string? ImageAltText { get; set; }
    }
    
    // For updating posts
    public class UpdatePostDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageAltText { get; set; }
    }

    // Enhanced response DTO with image information
    public class PostResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? ImageFileName { get; set; }
        public string? ImageAltText { get; set; }
        public long? ImageFileSize { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorId { get; set; } = string.Empty;
    }
}