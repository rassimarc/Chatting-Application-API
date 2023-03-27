using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record Message(
    [Required] Guid conversationId,
    [Required] Guid messageId,
    [Required] string text,
    [Required] UnixDateTime time
    );