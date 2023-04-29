namespace ProfileService.Web.Dtos;

public record ListConversationsResponseItem(string id, Profile recipient, long lastModifiedUnixTime);