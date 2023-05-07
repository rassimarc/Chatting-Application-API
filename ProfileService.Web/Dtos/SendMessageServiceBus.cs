namespace ProfileService.Web.Dtos;

public record SendMessageServiceBus(
    SendMessageRequest message,
    string conversationId,
    Conversation existingConversation
    );