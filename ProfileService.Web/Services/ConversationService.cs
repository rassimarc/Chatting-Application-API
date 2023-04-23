using System.Web;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public class ConversationService : IConversationService
{
    public MessageResponse GetMessages(List<Message> messages, string? continuationToken)
    {
        var getMessageResponse = new List<GetMessageResponse>();
        foreach (var message in messages)
        {
            getMessageResponse.Add(
                new GetMessageResponse(message.text, message.senderUsername, message.unixTime)
            );
        }
        var encodedContinuationToken = HttpUtility.UrlEncode(continuationToken);
        var messageResponse = new MessageResponse(getMessageResponse, encodedContinuationToken);
        return messageResponse;
    }
}