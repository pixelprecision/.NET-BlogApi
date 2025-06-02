// ========================================
// Controllers/PostsController.cs
// ========================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MyBlogApi.Data;
using MyBlogApi.Models;
using MyBlogApi.Models.DTOs;

namespace MyBlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires authentication for all endpoints in this controller
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/posts/my-posts
        [HttpGet("my-posts")]
        public async Task<IActionResult> GetMyPosts()
        {
            // Get the current user's ID from the JWT token claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (userId == null)
            {
                return Unauthorized();
            }

            // Query posts for the current user, including user information
            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.User) // Include user data for author name
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    AuthorName = $"{p.User.FirstName} {p.User.LastName}"
                })
                .ToListAsync();

            return Ok(posts);
        }

        // POST: api/posts
        [HttpPost]
        public async Task<IActionResult> CreatePost(CreatePostDto createPostDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (userId == null)
            {
                return Unauthorized();
            }

            var post = new Post
            {
                Title = createPostDto.Title,
                Content = createPostDto.Content,
                UserId = userId
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Reload with user data to return complete response
            await _context.Entry(post)
                .Reference(p => p.User)
                .LoadAsync();

            var response = new PostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                AuthorName = $"{post.User.FirstName} {post.User.LastName}"
            };

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, response);
        }

        // GET: api/posts/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPost(int id)
        {
            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            var response = new PostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                AuthorName = $"{post.User.FirstName} {post.User.LastName}"
            };

            return Ok(response);
        }
    }
}