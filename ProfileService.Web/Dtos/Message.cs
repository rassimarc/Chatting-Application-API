using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record Message(
    string messageId,
    Guid conversationId,
    string senderUsername,
    string text,
    long unixTime
);