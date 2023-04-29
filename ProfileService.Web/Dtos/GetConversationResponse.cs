namespace ProfileService.Web.Dtos;

public record GetConversationResponse(
        List<ListConversationsResponseItem> Conversations,
        string? NextUri
    );