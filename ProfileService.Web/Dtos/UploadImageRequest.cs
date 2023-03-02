using System.ComponentModel.DataAnnotations;
namespace ProfileService.Web.Dtos;

public record UploadImageRequest(
    [Required] IFormFile File
);
