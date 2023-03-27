using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record SendMessageRequest(
    [Required] Guid messageId,
    [Required] string senderUsername, 
    [Required] string text
);