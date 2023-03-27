using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record ConversationRequest(
    [Required] List<string> participants,
    [Required] SendMessageRequest firstMessage
);