using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ProfileService.Web.Configuration;
using ProfileService.Web.Dtos;

namespace ProfileService.Web.Services;

public class CreateConversationHostedService : IHostedService
{
    private readonly IConversationService _conversationService;
    private readonly IConversationSerializer _conversationSerializer;
    private readonly ServiceBusProcessor _processor;

    public CreateConversationHostedService(
        ServiceBusClient serviceBusClient, 
        IConversationService conversationService,
        IConversationSerializer conversationSerializer,
        IOptions<ServiceBusSettings> options)
    {
        _conversationService = conversationService;
        _conversationSerializer = conversationSerializer;
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

        ConversationRequest conversation = _conversationSerializer.DeserializeConversation(data);
        await _conversationService.AddConversation(conversation);

        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }
}