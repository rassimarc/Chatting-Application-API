﻿using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public interface IConversationService
{
    MessageResponse GetMessages(List<Message> messages, string? continuationToken, string conversationId, int? limit, long lastseemmessagetime);

    Task<GetConversationResponse> GetConversations(List<Conversation> conversations, string username, int? limit,
        string? continuationtoken, long lastSeenConversationTime);
}