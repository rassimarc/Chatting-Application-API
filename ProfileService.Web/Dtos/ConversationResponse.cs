namespace ProfileService.Web.Dtos;

public record ConversationResponse(List<GetConversationResponse> Conversation, string? NextUri);