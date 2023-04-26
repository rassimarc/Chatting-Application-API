using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record GetConversationResponse(
    Guid conversationId,
    long LastModifiedGuidTime,
    Profile recipients
    );