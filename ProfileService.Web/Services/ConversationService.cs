using System.Net;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public class ConversationService : IConversationService
{
    public MessageResponse GetMessages(List<Message> messages, string? continuationToken, string conversationId, int limit)
    {
        var getMessageResponse = new List<GetMessageResponse>();
        foreach (var message in messages)
        {
            getMessageResponse.Add(
                new GetMessageResponse(message.text, message.senderUsername, message.unixTime)
            );
        }

        var list = getMessageResponse.OrderByDescending(message => message.UnixTime).ToList();
        var nextUri = $"/api/conversations/{conversationId}/messages?limit={limit}";
        if (continuationToken != null)
        {
            var encodedContinuationToken = WebUtility.UrlEncode(continuationToken);
            var adjustedContinuationToken = Uri.EscapeDataString(encodedContinuationToken);
            nextUri += $"&continuationToken={adjustedContinuationToken}";
        }
        var messageResponse = new MessageResponse(list, nextUri);
        return messageResponse;
    }
}