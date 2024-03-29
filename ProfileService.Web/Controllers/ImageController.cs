﻿using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProfileService.Web.Configuration;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Controllers;

[ApiController]
[Route("api/images")]
public class ImageController : ControllerBase
{
    private readonly ConnectionStrings _connectionString;
    private readonly IImageStore _imageStore;
    private readonly ILogger<ImageController> _logger;


    public ImageController(IImageStore imageStore, ILogger<ImageController> logger, IOptions<ConnectionStrings> connectionStrings)
    {
        _imageStore = imageStore;
        _connectionString = connectionStrings.Value;
        _logger = logger;

    }

    [HttpPost]
    public async Task<ActionResult<UploadImageResponse>> UploadImage([FromForm] UploadImageRequest request)
    {
        using (_logger.BeginScope("{uploadImage}", request))

        {
            var guid = Guid.NewGuid();
            BlobContainerClient blobContainerClient = new BlobContainerClient(_connectionString.ImageUploadStorage, "image");
            var requestFiles = Request.Form.Files;
            foreach (IFormFile file in requestFiles)
            {
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
            return Ok(new UploadImageResponse(guid.ToString()));
        }
    }

    [HttpGet("{guid}")]
    public async Task<ActionResult> DownloadImage(Guid guid)
    {

        var existingImage = await _imageStore.GetImage(guid.ToString());
        if (existingImage == null)
        {
            return NotFound("The image you are trying to download cannot be found. Please try another guid.");
        }

        BlobClient blobClient = new BlobClient(_connectionString.ImageUploadStorage, "image",
            string.Concat(guid.ToString(), ".png"));

        using (var stream = new MemoryStream())
        {
            await blobClient.DownloadToAsync(stream);
            stream.Position = 0;
            return new FileContentResult(stream.ToArray(), "image/png");
        }
    }
}