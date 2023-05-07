using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ProfileService.Web.Configuration;
using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public class CreateConversationServiceBusPublisher : ICreateConversationPublisher
{
    private readonly IConversationSerializer _conversationSerializer;
    private readonly ServiceBusSender _sender;

    public CreateConversationServiceBusPublisher(
        ServiceBusClient serviceBusClient,
        IConversationSerializer conversationSerializer,
        IOptions<ServiceBusSettings> options)
    {
        _conversationSerializer = _conversationSerializer;
        _sender = serviceBusClient.CreateSender(options.Value.CreateConversationQueueName);
    }

    public Task Send(ConversationRequest conversation)
    {
        var serialized = _conversationSerializer.SerializeConversation(conversation);
        return _sender.SendMessageAsync(new ServiceBusMessage(serialized));
    }

    public Task Send(Conversation conversation)
    {
        throw new NotImplementedException();
    }
}