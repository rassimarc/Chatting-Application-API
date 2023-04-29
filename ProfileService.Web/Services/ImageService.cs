namespace ProfileService.Web.Services;

public class ImageService : IImageService
{
    public Task<string> UploadImageAsync(IFormFile file)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> DownloadImageAsync(Guid guid)
    {
        throw new NotImplementedException();
    }
}