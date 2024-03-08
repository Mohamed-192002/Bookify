namespace Bookify.Web.Services
{
    public interface IImageServices
    {
        Task<(bool IsUploaded, string? errorMessage)> UploadAsync(IFormFile image, string fileName, string folderPath, bool hasThumnail);
        void Delete(string imagePath,string?thumbnailPath=null);
    }
}
