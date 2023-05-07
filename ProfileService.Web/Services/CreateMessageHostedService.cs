using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ProfileService.Web.Configuration;

namespace ProfileService.Web.Services;

public class CreateMessageHostedService : IHostedService
{
    private readonly IConversationService _conversationService;
    private readonly IMessageSerializer _messageSerializer;
    private readonly ServiceBusProcessor _processor;

    public CreateMessageHostedService(
        ServiceBusClient serviceBusClient, 
        IConversationService conversationService,
        IMessageSerializer messageSerializer,
        IOptions<ServiceBusSettings> options)
    {
        _conversationService = conversationService;
        _messageSerializer = messageSerializer;
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

        var message = _messageSerializer.DeserializeMessage(data);
        await _conversationService.AddMessageServiceBus(message);

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}