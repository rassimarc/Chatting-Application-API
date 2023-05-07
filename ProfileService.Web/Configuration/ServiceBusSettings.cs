namespace ProfileService.Web.Configuration;

public class ServiceBusSettings
{
    public string ConnectionString { get; set; }
    public string CreateProfileQueueName { get; set; }
    
    public string CreateConversationQueueName { get; set; }
    
    public string CreateMessageQueueName { get; set; }
}