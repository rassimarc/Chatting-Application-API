using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ProfileService.Web.Configuration;
using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public class CreateMessageServiceBusPublisher : ICreateMessagePublisher
{
    private readonly IMessageSerializer _messageSerializer;
    private readonly ServiceBusSender _sender;

    public CreateMessageServiceBusPublisher(
        ServiceBusClient serviceBusClient,
        IMessageSerializer messageSerializer,
        IOptions<ServiceBusSettings> options)
    {
        _messageSerializer = messageSerializer;
        _sender = serviceBusClient.CreateSender(options.Value.CreateProfileQueueName);
    }

    public Task Send(SendMessageServiceBus message)
    {
        var serialized = _messageSerializer.SerializeMessage(message);
        return _sender.SendMessageAsync(new ServiceBusMessage(serialized));
    }


}