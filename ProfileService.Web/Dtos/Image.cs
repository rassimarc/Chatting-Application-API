using System.ComponentModel.DataAnnotations;

namespace ProfileService.Web.Dtos;

public record Image(
    [Required] string id
    );