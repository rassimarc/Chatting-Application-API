namespace ProfileService.Web.Dtos;

public record GetConversationResponse(
        Guid Id,
        long LastModifiedUnixTime,
        Profile Recipient 
    );