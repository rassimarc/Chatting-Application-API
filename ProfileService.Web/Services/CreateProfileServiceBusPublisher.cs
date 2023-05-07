using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ProfileService.Web.Configuration;
using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public class CreateProfileServiceBusPublisher : ICreateProfilePublisher
{
    private readonly IProfileSerializer _profileSerializer;
    private readonly ServiceBusSender _sender;

    public CreateProfileServiceBusPublisher(
        ServiceBusClient serviceBusClient,
        IProfileSerializer profileSerializer,
        IOptions<ServiceBusSettings> options)
    {
        _profileSerializer = profileSerializer;
        _sender = serviceBusClient.CreateSender(options.Value.CreateProfileQueueName);
    }

    public Task Send(Profile profile)
    {
        var serialized = _profileSerializer.SerializeProfile(profile);
        return _sender.SendMessageAsync(new ServiceBusMessage(serialized));
    }
}