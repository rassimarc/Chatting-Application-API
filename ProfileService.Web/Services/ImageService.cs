using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;
namespace ProfileService.Web.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageStore _imageStore;
        private readonly BlobContainerClient _blobContainerClient;

        public ImageService(IImageStore imageStore, string connectionString, string containerName)
        {
            _imageStore = imageStore;
            _blobContainerClient = new BlobContainerClient(connectionString, containerName);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            // Generate GUID and file name
            var guid = Guid.NewGuid();
            var fileName = $"{guid}.png";

            // Validate file extension
            var extension = Path.GetExtension(file.FileName);
            if (extension != ".png" && !extension.Equals(".jpg"))
            {
                throw new FormatException("The submitted file must be of type png or jpg.");
            }

            // Upload file to Azure Blob Storage
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Position = 0;
                await _blobContainerClient.UploadBlobAsync(fileName, stream);
            }

            // Store image metadata in the database
            var image = new Image(guid.ToString());
            await _imageStore.UpsertImage(image);

            // Return GUID as the image identifier
            return guid.ToString();
        }

        public async Task<byte[]> DownloadImageAsync(Guid guid)
        {
            // Get image metadata from the database
            var existingImage = await _imageStore.GetImage(guid.ToString());
            if (existingImage == null)
            {
                throw new ArgumentException("The image you are trying to download cannot be found. Please try another GUID.");
            }

            // Download image from Azure Blob Storage
            var fileName = $"{guid}.png";
            var blobClient = _blobContainerClient.GetBlobClient(fileName);
            using (var stream = new MemoryStream())
            {
                await blobClient.DownloadToAsync(stream);
                stream.Position = 0;
                return stream.ToArray();
            }
        }

        public Task UploadImage(UploadImageRequest request)
        {
            throw new NotImplementedException();
        }

        public Task DownloadImage(Guid guid)
        {
            throw new NotImplementedException();
        }
    }
    
}
