// ========================================
// 4. Models/DTOs/PostDtos.cs
// ========================================
namespace MyBlogApi.Models.DTOs
{
    public class CreatePostDto
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    public class PostResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string AuthorName { get; set; } = string.Empty;
    }
}