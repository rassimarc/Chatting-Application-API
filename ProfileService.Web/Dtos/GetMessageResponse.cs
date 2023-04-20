namespace ProfileService.Web.Dtos;

public record GetMessageResponse(
    string Text,
    string SenderUsername,
    long UnixTime
    );