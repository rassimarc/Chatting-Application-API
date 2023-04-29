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

    public ConversationService(IProfileStore profileStore)
    {
        _profileStore = profileStore;
    }
    public MessageResponse GetMessages(List<Message> messages, string? continuationToken, string conversationId, int? limit, long lastseenmessagetime)
    {
        var getMessageResponse = new List<GetMessageResponse>();
        foreach (var message in messages)
        {
            getMessageResponse.Add(
                new GetMessageResponse(message.text, message.senderUsername, message.unixTime)
            );
        }

        var nextUri = "";
        if (continuationToken != null)
        {
            nextUri = $"/api/conversations/{conversationId}/messages";
            if (limit != null) nextUri += $"?limit={limit}";
            if (continuationToken != null)
            {
                continuationToken = WebUtility.UrlEncode(continuationToken);
                nextUri += $"&continuationToken={continuationToken}";
            }

            if (lastseenmessagetime > 0) nextUri += $"&lastSeenMessageTime={lastseenmessagetime}";
        }

        var messageResponse = new MessageResponse(getMessageResponse, nextUri);
        return messageResponse;
    }

    public async Task<GetConversationResponse> GetConversations(List<Conversation> conversations, string username,
        int? limit, string? continuationToken,
        long lastSeenConversationTime)
    {
        var conversationResponseList = new List<ListConversationsResponseItem>();
        string participant;
        foreach (var conversation in conversations)
        {
            if (conversation.participants[0] == username) participant = conversation.participants[1];
            else participant = conversation.participants[0];
            conversationResponseList.Add(new ListConversationsResponseItem
                (conversation.conversationId,
                    conversation.lastModified,
                    await _profileStore.GetProfile(participant)
                )
            );
        }

        var nextUri = "";
        if (continuationToken != null){
            nextUri = $"/api/conversations?username={username}";
            if (limit != null) nextUri += $"&limit={limit}";
            if (continuationToken != null)
            {
                continuationToken = WebUtility.UrlEncode(continuationToken);
                nextUri += $"&continuationToken={continuationToken}";
            }
            if (lastSeenConversationTime > 0) nextUri += $"&lastSeenConversationTime={lastSeenConversationTime}";
        }
        var conversationResponse = new GetConversationResponse(conversationResponseList, nextUri);
        return conversationResponse;
    }
}