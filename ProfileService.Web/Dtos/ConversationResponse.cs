namespace ProfileService.Web.Dtos;

public record ConversationResponse(List<GetConversationResponse> Messages, string? NextUri);