using System.ComponentModel.DataAnnotations;

namespace ProfileService.Web.Dtos;

public record UserConversation(
    [Required] string username,
    [Required] Guid conversationId
    );