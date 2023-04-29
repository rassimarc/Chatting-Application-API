using System.Net;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public class ConversationService : IConversationService
{
    public MessageResponse GetMessages(List<Message> messages, string? continuationToken, string conversationId, int? limit, long lastseenmessagetime)
    {
        var getMessageResponse = new List<GetMessageResponse>();
        foreach (var message in messages)
        {
            getMessageResponse.Add(
                new GetMessageResponse(message.text, message.senderUsername, message.unixTime)
            );
        }

        var nextUri = $"/api/conversations/{conversationId}/messages";
        if (limit != null) nextUri += $"?limit={limit}";
        if (continuationToken != null)
        {
            continuationToken = WebUtility.UrlEncode(continuationToken);
            nextUri += $"&continuationToken={continuationToken}";
        }
        if (lastseenmessagetime > 0) nextUri += $"&lastSeenMessageTime={lastseenmessagetime}";
        var messageResponse = new MessageResponse(getMessageResponse, nextUri);
        return messageResponse;
    }
}