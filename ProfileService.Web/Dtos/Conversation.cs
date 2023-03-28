using Microsoft.Azure.Cosmos.Serialization.HybridRow;

namespace ProfileService.Web.Dtos;

public record Conversation(
    Guid conversationId,
    UnixDateTime lastModified,
    
    
    );