using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ProfileService.Web.Configuration;

namespace ProfileService.Web.Services;

public class CreateProfileHostedService : IHostedService
{
    private readonly IProfileService _profileService;
    private readonly IProfileSerializer _profileSerializer;
    private readonly ServiceBusProcessor _processor;

    public CreateProfileHostedService(
        ServiceBusClient serviceBusClient, 
        IProfileService profileService,
        IProfileSerializer profileSerializer,
        IOptions<ServiceBusSettings> options)
    {
        _profileService = profileService;
        _profileSerializer = profileSerializer;
        _processor = serviceBusClient.CreateProcessor(options.Value.CreateProfileQueueName);
        
        // add handler to process messages
        _processor.ProcessMessageAsync += MessageHandler;

        // add handler to process any errors
        _processor.ProcessErrorAsync += ErrorHandler;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _processor.StartProcessingAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _processor.StopProcessingAsync(cancellationToken);
    }
    
    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string data = args.Message.Body.ToString();
        Console.WriteLine($"Received: {data}");

        var profile = _profileSerializer.DeserializeProfile(data);
        await _profileService.CreateProfile(profile);

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}