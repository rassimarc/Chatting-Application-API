using System.ComponentModel.DataAnnotations;

namespace ProfileService.Web.Dtos;

public record Message(
    [Required] Guid messageId,
    [Required] Guid conversationId,
    [Required] string text
    
    
);