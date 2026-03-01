namespace WebsiteSellLaptop.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "uploads");
        bool DeleteImage(string imageUrl);
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "uploads")
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            try
            {
                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(extension))
                    throw new InvalidOperationException("Chỉ chấp nhận file ảnh (.jpg, .jpeg, .png, .gif, .webp)");

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                    throw new InvalidOperationException("Kích thước file không được vượt quá 5MB");

                // Create folder if not exists
                var uploadsFolder = Path.Combine(_environment.WebRootPath, folder);
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Generate unique filename
                var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return web path
                return $"/{folder}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                throw;
            }
        }

        public bool DeleteImage(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            try
            {
                var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return false;
            }
        }
    }
}
