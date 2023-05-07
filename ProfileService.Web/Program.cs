using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using ProfileService.Web.Configuration;
using ProfileService.Web.Services;
using ProfileService.Web.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Configuration
builder.Services.Configure<CosmosSettings>(builder.Configuration.GetSection("Cosmos"));
builder.Services.Configure<ConnectionStrings>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.Configure<ServiceBusSettings>(builder.Configuration.GetSection("ServiceBus"));


// Add Services

builder.Services.AddSingleton<IProfileStore, CosmosProfileStore>();
builder.Services.AddSingleton<IConversationStore, CosmosConversationStore>();
builder.Services.AddSingleton<IMessageStore, CosmosMessageStore>();
builder.Services.AddSingleton<IImageStore, CosmosImageStore>();
builder.Services.AddSingleton<IConversationService, ConversationService>();
//builder.Services.AddSingleton<IImageService, ImageService>();

builder.Services.AddSingleton(sp =>
{
    var cosmosOptions = sp.GetRequiredService<IOptions<CosmosSettings>>();
    return new CosmosClient(cosmosOptions.Value.ConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var serviceBusOptions = sp.GetRequiredService<IOptions<ServiceBusSettings>>();
    return new ServiceBusClient(serviceBusOptions.Value.ConnectionString);
});

builder.Services.AddSingleton<IProfileService, ProfileService.Web.Services.ProfileService>();
builder.Services.AddSingleton<ICreateProfilePublisher, CreateProfileServiceBusPublisher>();
builder.Services.AddSingleton<IProfileSerializer, JsonProfileSerializer>();    
builder.Services.AddHostedService<CreateProfileHostedService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }