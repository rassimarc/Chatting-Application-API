using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record ConversationRequest(
    [Required] string[] Participants,
    [Required] SendMessageRequest FirstMessage
);