using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record ConversationResponse(
    Guid conversationId,
    long createdUnixTime
    );