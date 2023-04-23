namespace ProfileService.Web.Dtos;

public record MessageResponse(List<GetMessageResponse> Messages, string NextUri);