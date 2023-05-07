using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using ProfileService.Web.Dtos;
using ProfileService.Web.Services;
using ProfileService.Web.Storage;
using Xunit;

namespace ProfileService.Web.Tests.Services
{
    public class ConversationServiceTests
    {
        private readonly Mock<IProfileStore> _profileStoreMock;
        private readonly Mock<IConversationStore> _conversationStoreMock;
        private readonly Mock<IMessageStore> _messageStoreMock;
        private readonly ConversationService _conversationService;

        private readonly Profile _profile1 = new(
            username: "user1",
            firstName: "User 1",
            lastName: "Bar",
            ProfilePictureId: Guid.NewGuid().ToString()
        );

        private readonly Profile _profile2 = new(
            username: "user2",
            firstName: "User 2",
            lastName: "Bar",
            ProfilePictureId: Guid.NewGuid().ToString()
        );

        private readonly Profile _profile3 = new(
            username: "user3",
            firstName: "User 3",
            lastName: "Bar",
            ProfilePictureId: Guid.NewGuid().ToString()
        );

        private readonly Profile _profile4 = new(
            username: "user4",
            firstName: "User 4",
            lastName: "Bar",
            ProfilePictureId: Guid.NewGuid().ToString()
        );

        public ConversationServiceTests()
        {
            _profileStoreMock = new Mock<IProfileStore>();
            _conversationStoreMock = new Mock<IConversationStore>();
            _messageStoreMock = new Mock<IMessageStore>();
            _conversationService = new ConversationService(
                _profileStoreMock.Object,
                _conversationStoreMock.Object,
                _messageStoreMock.Object
            );
        }

        [Fact]
        public void GetMessages_ReturnsCorrectMessageResponse()
        {
            // Arrange
            var messages = new List<Message>
            {
                new Message("1", "1", _profile1.username, "Hello", 1620649861),
                new Message("2", "1", _profile2.username, "Hi", 1620649862),
                new Message("3", "1", _profile1.username, "How are you?", 1620649863)
            };
            var continuationToken = "abc";
            var conversationId = "1";
            var limit = 10;
            var lastseenmessagetime = 1620649861;

            // Act
            var result = _conversationService.GetMessages(
                messages,
                continuationToken,
                conversationId,
                limit,
                lastseenmessagetime
            );

            // Asser
            Assert.Equal(_profile1.username, result.Messages[0].SenderUsername);
            Assert.Equal("Hello", result.Messages[0].Text);
            Assert.Equal(1620649861, result.Messages[0].UnixTime);
            Assert.Equal("/api/conversations/1/messages?limit=10&continuationToken=abc&lastSeenMessageTime=1620649861",
                result.NextUri);
        }

        [Fact]
        public async Task GetConversations_ReturnsCorrectConversationResponse()
        {
            // Arrange
            var conversations = new List<Conversation>
            {
                new Conversation("1", 1620649861, new string[] { _profile1.username, _profile2.username }),
                new Conversation("2", 1620649862, new string[] { _profile3.username, _profile4.username })
            };
            var username = _profile1.username;
            var limit = 10;
            var continuationToken = "abc";
            var lastSeenConversationTime = 1620649861;
            _profileStoreMock.Setup(p => p.GetProfile(_profile2.username))
                .ReturnsAsync(_profile2);
            _profileStoreMock.Setup(p => p.GetProfile(_profile1.username))
                .ReturnsAsync(_profile1);

            // Act
            var result = await _conversationService.GetConversations(
                conversations,
                username,
                limit,
                continuationToken,
                lastSeenConversationTime
            );

            // Assert

            Assert.Equal(1620649861, result.Conversations[0].LastModifiedUnixTime);
            Assert.Equal(_profile2.username, result.Conversations[0].Recipient.username);
            Assert.Equal(2, result.Conversations.Count);
        }
    }
}