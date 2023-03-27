using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Controllers;


[ApiController]
[Route("[controller]")]
public class ImageController : ControllerBase
{
    //Trying to create a Blob Client
    private string _connectionString =
        "DefaultEndpointsProtocol=https;AccountName=imageuploadstorages;AccountKey=ugqKduEzlHm802VFttk+wVq2UjEdl1QqeEoZCbPC5pTBvEFmPpKbujPPmi4RxG99c8FBxR40Tuih+ASts7/Kqg==;EndpointSuffix=core.windows.net";

    private readonly IImageStore _imageStore;

    public ImageController(IImageStore imageStore)
    {
        _imageStore = imageStore;
    }

    [HttpPost]
    public async Task<ActionResult<UploadImageRequest>>UploadImage([FromForm] UploadImageRequest request)
    {
        {
            //guid associated with file
            var guid = Guid.NewGuid();
            BlobContainerClient blobContainerClient = new BlobContainerClient(_connectionString, "image");
            string fileName = null;
            var requestFiles = Request.Form.Files;
            foreach (IFormFile file in requestFiles)
            {
                fileName = file.FileName;
                string extension = Path.GetExtension(fileName);
                if (extension != ".png" && !extension.Equals(".jpg"))
                {
                    throw new FormatException("The submitted file must be of type png or jpg.");
                }
                using (var stream = new MemoryStream())
                {
                    await request.File.CopyToAsync(stream);
                    stream.Position = 0;
                    string name = string.Concat(guid.ToString(), ".png");
                    await blobContainerClient.UploadBlobAsync(name, stream);
                }
            }
            var image = new Image(guid.ToString());
            await _imageStore.UpsertImage(image);
            return Ok("Your picture was succesfully uploaded, please use this code: \n" + guid + "\nwhen creating your profile.");
        }
    }


    [HttpGet("[action]")]
    public async Task<ActionResult> DownloadImage(Guid guid)
    {
        
        var existingImage = await _imageStore.GetImage(guid.ToString());
        if (existingImage == null)
        {   
            return NotFound("The image you are trying to download cannot be found. Please try another guid.");
        }
        
        BlobClient blobClient = new BlobClient(_connectionString, "image", 
            string.Concat(guid.ToString(),".png"));;

        using (var stream = new MemoryStream())
        {
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;
            var contentType = (await blobClient.GetPropertiesAsync()).Value.ContentType;
            return File(stream.ToArray(), contentType, blobClient.Name);
        }
    }
}