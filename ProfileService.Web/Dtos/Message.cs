using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record Message(
    Guid messageId,
    Guid conversationId,
    string senderUsername,
    string text,
    string time
);