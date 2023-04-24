using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Web.Dtos;
using ProfileService.Web.Services;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProfileService.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost]
        public async Task<ActionResult<UploadImageRequest>> UploadImage(IFormFile file)
        {
            try
            {
                var guid = await _imageService.UploadImageAsync(file);
                return Ok($"Your picture was successfully uploaded, please use this code: {guid} when creating your profile.");
            }
            catch (FormatException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{guid}")]
        public async Task<IActionResult> DownloadImage(Guid guid)
        {
            try
            {
                var imageData = await _imageService.DownloadImageAsync(guid);
                return File(imageData, "image/png");
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
