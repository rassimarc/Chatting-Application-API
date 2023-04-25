using System.Net;
using System.Text;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Services;


public class ConversationService : IConversationService
{
    private readonly IProfileStore _profileStore;

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
    
    public async Task<ConversationResponse> GetConversations(List<Conversation> conversations,
        string? continuationToken, string username, int limit)
    {
        var getConversationResponse = new List<ConversationResponse>();
        foreach (var conversation in conversations)
        {
            var existingProfile = await _profileStore.GetProfile(username);

            getConversationResponse.Add(
                new GetConversationResponse(conversation.conversationId, conversation.lastModified, existingProfile)
            );
        }
        var nextUri = $"/api/conversations/{username}/messages?limit={limit}";
        if (continuationToken != null)
        {
            var encodedContinuationToken = WebUtility.UrlEncode(continuationToken);
            var adjustedContinuationToken = Uri.EscapeDataString(encodedContinuationToken);
            nextUri += $"&continuationToken={adjustedContinuationToken}";
        }
        var conversationResponse = new ConversationResponse(getConversationResponse, nextUri);
        return conversationResponse;
    }
    
    
}