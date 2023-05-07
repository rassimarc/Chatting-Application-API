using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using ProfileService.Web.Dtos;
using ProfileService.Web.Services;
using ProfileService.Web.Storage;

namespace ProfileService.Web.Tests.Controllers;

    public class ConversationControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly Mock<IProfileStore> _profileStoreMock = new();
        private readonly Mock<IConversationStore> _conversationStoreMock = new();
        private readonly Mock<IMessageStore> _messageStoreMock = new();
        private readonly Mock<IConversationService> _conversationServiceMock = new();
        private readonly HttpClient _httpClientConversation;

        private readonly Profile _profile1 = new(
            username: "FooBar1",
            firstName: "Foo1",
            lastName: "Bar1",
            ProfilePictureId: Guid.NewGuid().ToString()
        );
        
        private readonly Profile _profile2 = new(
            username: "FooBar2",
            firstName: "Foo2",
            lastName: "Bar2",
            ProfilePictureId: Guid.NewGuid().ToString()
        );

        public ConversationControllerTests(WebApplicationFactory<Program> factory)
        {
            _httpClientConversation = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services => { 
                    services.AddSingleton(_conversationStoreMock.Object);
                    services.AddSingleton(_profileStoreMock.Object);
                    services.AddSingleton(_conversationServiceMock.Object);
                    services.AddSingleton(_messageStoreMock.Object);
                });
            }).CreateClient();
        }

        [Fact]
        public async Task AddConversation()
        {
            var conversation = new ConversationRequest
            (
                Participants: new[] { _profile1.username, _profile2.username },
                FirstMessage: new SendMessageRequest
                (
                    SenderUsername: _profile1.username,
                    Text: "Hello, user2!",
                    Id: "msg1"
                )
            );

            _profileStoreMock
                .Setup(x => x.GetProfile(_profile1.username))
                .ReturnsAsync(_profile1);
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile2.username))
                .ReturnsAsync(_profile2);
            _conversationServiceMock
                .Setup(m => m.AddConversation(conversation))
                .ReturnsAsync(new ConversationResponse("sdoijf", 0));
            _conversationStoreMock
                .Setup(m => m.GetConversations(conversation.Participants[0], null, null, "0"))
                .ReturnsAsync((new List<Conversation>(), null));

            var response = await _httpClientConversation.PostAsync("/api/conversations",
                new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task AddConversation_InvalidParticipants_ReturnsBadRequest()
        {
            var conversation = new ConversationRequest
            (
                Participants: new[] { _profile1.username},
                FirstMessage: new SendMessageRequest
                (
                    SenderUsername: _profile1.username,
                    Text: "Hello, user2!",
                    Id: "msg1"
                )
            );
            var response = await _httpClientConversation.PostAsync("/api/conversations",
                new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task AddConversation_ParticipantNotFound_ReturnsNotFound()
        {
            var conversation = new ConversationRequest
            (
                Participants: new[] { _profile1.username, _profile2.username },
                FirstMessage: new SendMessageRequest
                (
                    SenderUsername: _profile1.username,
                    Text: "Hello, user2!",
                    Id: "msg1"
                )
            );
            
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile2.username))
                .ReturnsAsync(_profile2);

            var response = await _httpClientConversation.PostAsync("/api/conversations",
                new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public async Task AddConversation_ReturnsConflict()
        {
            var conversation = new ConversationRequest
            (
                Participants: new[] { _profile1.username, _profile2.username },
                FirstMessage: new SendMessageRequest
                (
                    SenderUsername: _profile1.username,
                    Text: "Hello, user2!",
                    Id: "msg1"
                )
            );
            var conversationList = new Conversation("sdadf", 0, new[]{_profile1.username, _profile2.username});
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile1.username))
                .ReturnsAsync(_profile1);
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile2.username))
                .ReturnsAsync(_profile2);
            _conversationServiceMock
                .Setup(m => m.AddConversation(conversation))
                .ReturnsAsync(new ConversationResponse("sdoijf", 0));
            _conversationStoreMock
                .Setup(m => m.GetConversations(conversation.Participants[0], null, null, "0"))
                .ReturnsAsync((new List<Conversation>(){conversationList}, null));

            var response = await _httpClientConversation.PostAsync("/api/conversations",
                new StringContent(JsonConvert.SerializeObject(conversation), Encoding.Default, "application/json"));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        public async Task AddMessage()
        {
            var conversation = new Conversation(
                "conversationId",
                0,
                new[] { _profile1.username, _profile2.username }
            );

            var message = new SendMessageRequest(_profile1.username, "Hello", "id1");
            var messageResponse = new SendMessageResponse(0);
            
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile1.username))
                .ReturnsAsync(_profile1);
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile2.username))
                .ReturnsAsync(_profile2);
            _conversationStoreMock
                .Setup(m => m.GetConversation(_profile1.username, "conversationId"))
                .ReturnsAsync(conversation);
            _messageStoreMock
                .Setup(m => m.GetMessages(null, null, "conversationId", "0"))
                .ReturnsAsync((new List<Message>(), null));
            _conversationServiceMock
                .Setup(m => m.AddMessage(message, "conversationId", conversation))
                .ReturnsAsync(messageResponse);

            var response = await _httpClientConversation.PostAsync($"/api/conversations/{conversation.conversationId}/messages",
                new StringContent(JsonConvert.SerializeObject(message), Encoding.Default, "application/json"));

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }
        
        [Fact]
        public async Task AddMessage_ReturnsNotFoundProfile()
        {
            var conversation = new Conversation(
                "conversationId",
                0,
                new[] { _profile1.username, _profile2.username }
            );

            var message = new SendMessageRequest(_profile1.username, "Hello", "id1");
            var messageResponse = new SendMessageResponse(0);
            
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile2.username))
                .ReturnsAsync(_profile2);
            _conversationStoreMock
                .Setup(m => m.GetConversation(_profile1.username, "conversationId"))
                .ReturnsAsync(conversation);
            _messageStoreMock
                .Setup(m => m.GetMessages(null, null, "conversationId", "0"))
                .ReturnsAsync((new List<Message>(), null));
            _conversationServiceMock
                .Setup(m => m.AddMessage(message, "conversationId", conversation))
                .ReturnsAsync(messageResponse);

            var response = await _httpClientConversation.PostAsync($"/api/conversations/{conversation.conversationId}/messages",
                new StringContent(JsonConvert.SerializeObject(message), Encoding.Default, "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public async Task AddMessage_ReturnsNotFound_NoConversation()
        {
            var conversation = new Conversation(
                "conversationId",
                0,
                new[] { _profile1.username, _profile2.username }
            );

            var message = new SendMessageRequest(_profile1.username, "Hello", "id1");
            var messageResponse = new SendMessageResponse(0);
            
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile1.username))
                .ReturnsAsync(_profile1);
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile2.username))
                .ReturnsAsync(_profile2);
            _messageStoreMock
                .Setup(m => m.GetMessages(null, null, "conversationId", "0"))
                .ReturnsAsync((new List<Message>(), null));
            _conversationServiceMock
                .Setup(m => m.AddMessage(message, "conversationId", conversation))
                .ReturnsAsync(messageResponse);

            var response = await _httpClientConversation.PostAsync($"/api/conversations/{conversation.conversationId}/messages",
                new StringContent(JsonConvert.SerializeObject(message), Encoding.Default, "application/json"));

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public async Task AddMessage_ReturnsConflict()
        {
            var conversation = new Conversation(
                "conversationId",
                0,
                new[] { _profile1.username, _profile2.username }
            );

            var message = new SendMessageRequest(_profile1.username, "Hello", "id1");
            var messageResponse = new SendMessageResponse(0);
            
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile1.username))
                .ReturnsAsync(_profile1);
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile2.username))
                .ReturnsAsync(_profile2);
            _conversationStoreMock
                .Setup(m => m.GetConversation(_profile1.username, "conversationId"))
                .ReturnsAsync(conversation);
            _messageStoreMock
                .Setup(m => m.GetMessages(null, null, "conversationId", "0"))
                .ReturnsAsync((new List<Message>(){new Message("id1", "conversationId", _profile1.username, "Hey", 0)}, null));
            _conversationServiceMock
                .Setup(m => m.AddMessage(message, "conversationId", conversation))
                .ReturnsAsync(messageResponse);

            var response = await _httpClientConversation.PostAsync($"/api/conversations/{conversation.conversationId}/messages",
                new StringContent(JsonConvert.SerializeObject(message), Encoding.Default, "application/json"));

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        
        [Fact]
        public async Task AddMessage_ReturnsBadRequest()
        {
            var conversation = new Conversation(
                "conversationId",
                0,
                new[] { _profile1.username, _profile2.username }
            );

            var message = new SendMessageRequest(_profile1.username, "", "id1");
            var messageResponse = new SendMessageResponse(0);
            
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile1.username))
                .ReturnsAsync(_profile1);
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile2.username))
                .ReturnsAsync(_profile2);
            _conversationStoreMock
                .Setup(m => m.GetConversation(_profile1.username, "conversationId"))
                .ReturnsAsync(conversation);
            _messageStoreMock
                .Setup(m => m.GetMessages(null, null, "conversationId", "0"))
                .ReturnsAsync((new List<Message>(), null));
            _conversationServiceMock
                .Setup(m => m.AddMessage(message, "conversationId", conversation))
                .ReturnsAsync(messageResponse);

            var response = await _httpClientConversation.PostAsync($"/api/conversations/{conversation.conversationId}/messages",
                new StringContent(JsonConvert.SerializeObject(message), Encoding.Default, "application/json"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        
        [Fact]
        public async Task GetConversations()
        {
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile1.username))
                .ReturnsAsync(_profile1);
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile2.username))
                .ReturnsAsync(_profile2);
            _conversationStoreMock
                .Setup(m => m.GetConversations(_profile1.username, null, null, "0"))
                .ReturnsAsync((new List<Conversation>(){}, null));
            _conversationServiceMock
                .Setup(m => m.GetConversations(new List<Conversation>() { }, _profile1.username, null, null, 0))
                .ReturnsAsync(new GetConversationResponse(new List<ListConversationsResponseItem>(), null));

            var response = await _httpClientConversation.GetAsync($"/api/conversations?username={_profile1.username}&lastSeenConversationTime=0");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        [Fact]
        public async Task GetConversations_ReturnNotFound()
        {
            _profileStoreMock
                .Setup(x => x.GetProfile(_profile2.username))
                .ReturnsAsync(_profile2);
            _conversationStoreMock
                .Setup(m => m.GetConversations(_profile1.username, null, null, "0"))
                .ReturnsAsync((new List<Conversation>(){}, null));
            _conversationServiceMock
                .Setup(m => m.GetConversations(new List<Conversation>() { }, _profile1.username, null, null, 0))
                .ReturnsAsync(new GetConversationResponse(new List<ListConversationsResponseItem>(), null));

            var response = await _httpClientConversation.GetAsync($"/api/conversations?username={_profile1.username}&lastSeenConversationTime=0");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        
        [Fact]
        public async Task GetMessages()
        {
            var messageResponseList = new List<GetMessageResponse>() {new GetMessageResponse("hi", _profile1.username, 0)};
            var messageResponse = new MessageResponse(messageResponseList, null);
            var message = new Message("msgid", "id1", _profile1.username, "hey", 0);
            _messageStoreMock
                .Setup(m => m.GetMessages(null, null, "id1", "0"))
                .ReturnsAsync((new List<Message>(){message}, null));
            _conversationServiceMock
                .Setup(m => m.GetMessages(new List<Message>(){message}, null, "id1", null, 0))
                .Returns(messageResponse);
            
            var response = await _httpClientConversation.GetAsync($"/api/conversations/id1/messages?lastSeenMessageTime=0");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        
        [Fact]
        public async Task GetMessages_ReturnsNotFound()
        {
            var messageResponseList = new List<GetMessageResponse>() {new GetMessageResponse("hi", _profile1.username, 0)};
            var messageResponse = new MessageResponse(messageResponseList, null);
            var message = new Message("msgid", "id1", _profile1.username, "hey", 0);
            _messageStoreMock
                .Setup(m => m.GetMessages(null, null, "id1", "0"))
                .ReturnsAsync((new List<Message>(), null));
            _conversationServiceMock
                .Setup(m => m.GetMessages(new List<Message>(){message}, null, "id1", null, 0))
                .Returns(messageResponse);
            
            var response = await _httpClientConversation.GetAsync($"/api/conversations/id1/messages?lastSeenMessageTime=0");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
