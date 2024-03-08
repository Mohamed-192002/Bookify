using Bookify.Web.Core.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Security.Claims;

namespace Bookify.Web.Services
{
    public class ImageServices : IImageServices
    {
        private readonly IWebHostEnvironment webHostEnvironment;
        private List<string> allowedExtensionsImage = new() { ".jpg", ".png", ".jpeg" };
        private int maxSizeForImage = 2097152;

        public ImageServices(IWebHostEnvironment _webHostEnvironment)
        {
            webHostEnvironment = _webHostEnvironment;
        }

        public async Task<(bool IsUploaded, string? errorMessage)> UploadAsync(IFormFile image, string fileName, string folderPath, bool hasThumnail)
        {
            if (!allowedExtensionsImage.Contains(Path.GetExtension(image.FileName)))
                return (false, Errors.AllowImagesExtensions);

            if (image.Length > maxSizeForImage)
                return (false, Errors.AllowImagesSize);

            var path = Path.Combine($"{webHostEnvironment.WebRootPath}{folderPath}", fileName);

            using var fileStream = new FileStream(path, FileMode.Create);
            await image.CopyToAsync(fileStream);
            fileStream.Dispose();

            if (hasThumnail)
            {
                var Thumbpath = Path.Combine($"{webHostEnvironment.WebRootPath}{folderPath}/thumb", fileName);
                using var Loadedimage = Image.Load(image.OpenReadStream());
                var ratio = (float)Loadedimage.Width / 200;
                var hight = Loadedimage.Height / ratio;
                Loadedimage.Mutate(i => i.Resize(width: 200, height: (int)hight));
                Loadedimage.Save(Thumbpath);
            }

            return (true, null);
        }

        public void Delete(string imagePath, string? thumbnailPath = null)
        {
            var oldPath = $"{webHostEnvironment.WebRootPath}{imagePath}";

            if (File.Exists(oldPath))
                File.Delete(oldPath);
            if (thumbnailPath is not null)
            {
                var oldThumbPath = $"{webHostEnvironment.WebRootPath}{thumbnailPath}";
                if (File.Exists(oldThumbPath))
                    File.Delete(oldThumbPath);
            }
        }
    }
}
