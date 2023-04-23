using System.Net;
using System.Text;
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
        var encodedContinuationToken = WebUtility.UrlEncode(continuationToken);
        var adjustedContinuationToken = Uri.EscapeDataString(encodedContinuationToken);
        var nextUri = $"/api/conversations//messages?continuationToken={adjustedContinuationToken}";
        var messageResponse = new MessageResponse(getMessageResponse, nextUri);
        return messageResponse;
    }
}