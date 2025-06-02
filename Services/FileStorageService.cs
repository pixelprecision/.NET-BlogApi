using System.IO;

namespace MyBlogApi.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveImageAsync(IFormFile image, string subfolder = "posts");
        void DeleteImage(string filePath);
        bool IsValidImage(IFormFile file);
        string GetImageUrl(string fileName);
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FileStorageService> _logger;
        
        // Define allowed image types for security
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string[] _allowedMimeTypes = { 
            "image/jpeg", 
            "image/png", 
            "image/gif", 
            "image/webp" 
        };
        
        // Maximum file size (5MB by default)
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB in bytes

        public FileStorageService(
            IWebHostEnvironment environment, 
            IConfiguration configuration,
            ILogger<FileStorageService> logger)
        {
            _environment = environment;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> SaveImageAsync(IFormFile image, string subfolder = "posts")
        {
            // Validate the image first
            if (!IsValidImage(image))
            {
                throw new ArgumentException("Invalid image file");
            }

            // Create a unique filename to prevent conflicts
            var fileExtension = Path.GetExtension(image.FileName).ToLowerInvariant();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            
            // Determine where to save the file
            // Using wwwroot/uploads/posts/ structure
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", subfolder);
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
                _logger.LogInformation("Created directory: {Directory}", uploadsFolder);
            }

            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // Save the file
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                
                _logger.LogInformation("Saved image: {FileName} to {FilePath}", 
                    image.FileName, filePath);

                // Return the relative path that will be stored in the database
                return $"/uploads/{subfolder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image file");
                
                // Clean up if something went wrong
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                
                throw;
            }
        }

        public void DeleteImage(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;

            try
            {
                // Convert the relative path to absolute path
                var absolutePath = Path.Combine(_environment.WebRootPath, 
                    filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                    _logger.LogInformation("Deleted image: {FilePath}", absolutePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image file: {FilePath}", filePath);
                // Don't throw - we don't want to break the flow if image deletion fails
            }
        }

        public bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check file size
            if (file.Length > _maxFileSize)
            {
                _logger.LogWarning("File {FileName} exceeds maximum size. Size: {Size} bytes", 
                    file.FileName, file.Length);
                return false;
            }

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                _logger.LogWarning("File {FileName} has invalid extension: {Extension}", 
                    file.FileName, extension);
                return false;
            }

            // Check MIME type (more secure than just checking extension)
            if (!_allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning("File {FileName} has invalid MIME type: {MimeType}", 
                    file.FileName, file.ContentType);
                return false;
            }

            // Additional security: Check the file's actual content
            // This prevents someone from renaming a .exe to .jpg
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    // Read the first few bytes to check file signature
                    var buffer = new byte[8];
                    stream.Read(buffer, 0, buffer.Length);
                    stream.Seek(0, SeekOrigin.Begin);

                    // Check for common image file signatures
                    if (IsJpeg(buffer) || IsPng(buffer) || IsGif(buffer) || IsWebP(buffer))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image file content");
                return false;
            }

            return false;
        }

        public string GetImageUrl(string fileName)
        {
            // This method helps construct the full URL for an image
            // You might want to use a CDN or different base URL in production
            var baseUrl = _configuration["BaseUrl"] ?? "https://localhost:5001";
            return $"{baseUrl}{fileName}";
        }

        // Helper methods to check file signatures (magic numbers)
        private bool IsJpeg(byte[] bytes)
        {
            return bytes.Length >= 3 && 
                   bytes[0] == 0xFF && 
                   bytes[1] == 0xD8 && 
                   bytes[2] == 0xFF;
        }

        private bool IsPng(byte[] bytes)
        {
            return bytes.Length >= 8 &&
                   bytes[0] == 0x89 &&
                   bytes[1] == 0x50 &&
                   bytes[2] == 0x4E &&
                   bytes[3] == 0x47 &&
                   bytes[4] == 0x0D &&
                   bytes[5] == 0x0A &&
                   bytes[6] == 0x1A &&
                   bytes[7] == 0x0A;
        }

        private bool IsGif(byte[] bytes)
        {
            return bytes.Length >= 6 &&
                   bytes[0] == 0x47 && // G
                   bytes[1] == 0x49 && // I
                   bytes[2] == 0x46 && // F
                   bytes[3] == 0x38 && // 8
                   (bytes[4] == 0x37 || bytes[4] == 0x39) && // 7 or 9
                   bytes[5] == 0x61; // a
        }

        private bool IsWebP(byte[] bytes)
        {
            return bytes.Length >= 4 &&
                   bytes[0] == 0x52 && // R
                   bytes[1] == 0x49 && // I
                   bytes[2] == 0x46 && // F
                   bytes[3] == 0x46;   // F
        }
    }
}