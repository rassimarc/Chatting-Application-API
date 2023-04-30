using System.Net;
using ProfileService.Web.Dtos;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Services;

public class ConversationService : IConversationService
{
    private readonly IProfileStore _profileStore;
    private readonly IConversationStore _conversationStore;
    private readonly IMessageStore _messageStore;

    public ConversationService(IProfileStore profileStore, IConversationStore conversationStore, IMessageStore messageStore)
    {
        _profileStore = profileStore;
        _conversationStore = conversationStore;
        _messageStore = messageStore;
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
            continuationToken = WebUtility.UrlEncode(continuationToken);
            nextUri += $"&continuationToken={continuationToken}";
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
            continuationToken = WebUtility.UrlEncode(continuationToken);
            nextUri += $"&continuationToken={continuationToken}";
            if (lastSeenConversationTime > 0) nextUri += $"&lastSeenConversationTime={lastSeenConversationTime}";
        }
        var conversationResponse = new GetConversationResponse(conversationResponseList, nextUri);
        return conversationResponse;
    }

    public async Task<ConversationResponse> AddConversation(ConversationRequest conversation)
    {
        long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                var conversationId = Guid.NewGuid();
                var message = new Message(
                    conversation.FirstMessage.Id,
                    conversationId.ToString(),
                    conversation.FirstMessage.SenderUsername,
                    conversation.FirstMessage.Text,
                    time
                );
                
                var conversationdb = new Conversation(
                    conversationId.ToString(),
                    time,
                    conversation.Participants
                );
                
                var conversationresponse = new ConversationResponse(
                    conversationId.ToString(),
                    time
                );
                
                await _conversationStore.AddConversation(conversationdb);
                await _messageStore.AddMessage(message);
                return conversationresponse;
    }

    public async Task<SendMessageResponse> AddMessage(SendMessageRequest message, string conversationId, Conversation existingConversation)
    {
        long time = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var messageDb = new Message(
            message.Id,
            conversationId,
            message.SenderUsername,
            message.Text,
            time
        );

        var messageresponse = new SendMessageResponse(
            time
        );
        
        var upsertConversation = new Conversation(
            conversationId,
            time,
            existingConversation.participants
        );
        await _messageStore.AddMessage(messageDb);
        await _conversationStore.AddConversation(upsertConversation);
        return messageresponse;
    }
}