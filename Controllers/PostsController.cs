using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MyBlogApi.Data;
using MyBlogApi.Models;
using MyBlogApi.Models.DTOs;
using MyBlogApi.Services;

namespace MyBlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly ILogger<PostsController> _logger;

        public PostsController(
            ApplicationDbContext context, 
            IFileStorageService fileStorage,
            ILogger<PostsController> logger)
        {
            _context = context;
            _fileStorage = fileStorage;
            _logger = logger;
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

            // Validate image URL if provided
            if (!string.IsNullOrEmpty(createPostDto.ImageUrl))
            {
                // Basic URL validation
                if (!Uri.TryCreate(createPostDto.ImageUrl, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                {
                    return BadRequest(new { error = "Invalid image URL format" });
                }
            }

            var post = new Post
            {
                Title = createPostDto.Title,
                Content = createPostDto.Content,
                ImageUrl = createPostDto.ImageUrl,
                ImageAltText = createPostDto.ImageAltText,
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
                ImageUrl = post.ImageUrl,
                ImageAltText = post.ImageAltText,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                AuthorName = $"{post.User.FirstName} {post.User.LastName}",
                AuthorId = post.User.Id
            };

            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, response);
        }
        
        [HttpPost("with-image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreatePostWithImage([FromForm] CreatePostWithImageDto createPostDto)
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
                ImageAltText = createPostDto.ImageAltText,
                UserId = userId
            };

            // Handle image upload if provided
            if (createPostDto.Image != null)
            {
                try
                {
                    // Validate the image
                    if (!_fileStorage.IsValidImage(createPostDto.Image))
                    {
                        return BadRequest(new { error = "Invalid image file. Please upload a valid image (JPEG, PNG, GIF, or WebP) under 5MB." });
                    }

                    // Save the image
                    var imagePath = await _fileStorage.SaveImageAsync(createPostDto.Image);
                    
                    // Store image information in the post
                    post.ImageUrl = imagePath;
                    post.ImageFileName = createPostDto.Image.FileName;
                    post.ImageContentType = createPostDto.Image.ContentType;
                    post.ImageFileSize = createPostDto.Image.Length;
                    
                    _logger.LogInformation("Image uploaded for post: {FileName}, Size: {Size} bytes", 
                        createPostDto.Image.FileName, createPostDto.Image.Length);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading image for post");
                    return StatusCode(500, new { error = "Error uploading image. Please try again." });
                }
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // Reload with user data
            await _context.Entry(post)
                .Reference(p => p.User)
                .LoadAsync();

            var response = MapToResponseDto(post);
            return CreatedAtAction(nameof(GetPost), new { id = post.Id }, response);
        }

        // GET: api/posts/my-posts
        [HttpGet("my-posts")]
        public async Task<IActionResult> GetMyPosts()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (userId == null)
            {
                return Unauthorized();
            }

            var posts = await _context.Posts
                .Where(p => p.UserId == userId)
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    ImageFileName = p.ImageFileName,
                    ImageAltText = p.ImageAltText,
                    ImageFileSize = p.ImageFileSize,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    AuthorName = $"{p.User.FirstName} {p.User.LastName}",
                    AuthorId = p.User.Id
                })
                .ToListAsync();

            return Ok(posts);
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
                ImageUrl = post.ImageUrl,
                ImageFileName = post.ImageFileName,
                ImageAltText = post.ImageAltText,
                ImageFileSize = post.ImageFileSize,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                AuthorName = $"{post.User.FirstName} {post.User.LastName}",
                AuthorId = post.User.Id
            };

            return Ok(response);
        }

        // PUT: api/posts/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, UpdatePostDto updateDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (userId == null)
            {
                return Unauthorized();
            }

            var post = await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (post == null)
            {
                return NotFound(new { error = "Post not found or you don't have permission to edit it" });
            }

            // Update only provided fields
            if (!string.IsNullOrEmpty(updateDto.Title))
                post.Title = updateDto.Title;
                
            if (!string.IsNullOrEmpty(updateDto.Content))
                post.Content = updateDto.Content;
                
            if (updateDto.ImageUrl != null) // Can be empty string to remove image
            {
                if (!string.IsNullOrEmpty(updateDto.ImageUrl))
                {
                    // Validate URL
                    if (!Uri.TryCreate(updateDto.ImageUrl, UriKind.Absolute, out var uri) ||
                        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                    {
                        return BadRequest(new { error = "Invalid image URL format" });
                    }
                }
                post.ImageUrl = updateDto.ImageUrl;
            }
                
            if (updateDto.ImageAltText != null)
                post.ImageAltText = updateDto.ImageAltText;

            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            var response = new PostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                ImageAltText = post.ImageAltText,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                AuthorName = $"{post.User.FirstName} {post.User.LastName}",
                AuthorId = post.User.Id
            };

            return Ok(response);
        }
        
        [HttpPut("{id}/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdatePostImage(int id, IFormFile image)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (userId == null)
            {
                return Unauthorized();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (post == null)
            {
                return NotFound(new { error = "Post not found or you don't have permission to edit it" });
            }

            if (image == null)
            {
                return BadRequest(new { error = "No image file provided" });
            }

            try
            {
                // Validate the new image
                if (!_fileStorage.IsValidImage(image))
                {
                    return BadRequest(new { error = "Invalid image file" });
                }

                // Delete the old image if it exists and is a local file
                if (!string.IsNullOrEmpty(post.ImageUrl) && post.ImageUrl.StartsWith("/uploads/"))
                {
                    _fileStorage.DeleteImage(post.ImageUrl);
                }

                // Save the new image
                var imagePath = await _fileStorage.SaveImageAsync(image);
                
                // Update post with new image information
                post.ImageUrl = imagePath;
                post.ImageFileName = image.FileName;
                post.ImageContentType = image.ContentType;
                post.ImageFileSize = image.Length;
                post.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated image for post {PostId}", id);

                var response = MapToResponseDto(post);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating image for post {PostId}", id);
                return StatusCode(500, new { error = "Error updating image" });
            }
        }

        // DELETE: api/posts/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (userId == null)
            {
                return Unauthorized();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (post == null)
            {
                return NotFound(new { error = "Post not found or you don't have permission to delete it" });
            }

            // Clean up the image file if it exists
            if (!string.IsNullOrEmpty(post.ImageUrl) && post.ImageUrl.StartsWith("/uploads/"))
            {
                _fileStorage.DeleteImage(post.ImageUrl);
                _logger.LogInformation("Deleted image file for post {PostId}", id);
            }

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        [HttpDelete("{id}/image")]
        public async Task<IActionResult> DeletePostImage(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            if (userId == null)
            {
                return Unauthorized();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (post == null)
            {
                return NotFound(new { error = "Post not found or you don't have permission to edit it" });
            }

            // Delete the image file if it's a local file
            if (!string.IsNullOrEmpty(post.ImageUrl) && post.ImageUrl.StartsWith("/uploads/"))
            {
                _fileStorage.DeleteImage(post.ImageUrl);
            }

            // Clear image fields
            post.ImageUrl = null;
            post.ImageFileName = null;
            post.ImageContentType = null;
            post.ImageFileSize = null;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted image from post {PostId}", id);

            return NoContent();
        }
        
        private PostResponseDto MapToResponseDto(Post post)
        {
            return new PostResponseDto
            {
                Id = post.Id,
                Title = post.Title,
                Content = post.Content,
                ImageUrl = post.ImageUrl,
                ImageFileName = post.ImageFileName,
                ImageAltText = post.ImageAltText,
                ImageFileSize = post.ImageFileSize,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                AuthorName = $"{post.User.FirstName} {post.User.LastName}",
                AuthorId = post.User.Id
            };
        }
    }
}