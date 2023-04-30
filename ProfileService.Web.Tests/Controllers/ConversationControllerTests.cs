using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProfileService.Web.Controllers;
using ProfileService.Web.Dtos;
using ProfileService.Web.Services;
using ProfileService.Web.Storage;
using Xunit;

namespace ProfileService.Web.Tests.Controllers
{
    public class ConversationControllerTests
    {
        private readonly ConversationController _controller;
        private readonly Mock<IProfileStore> _profileStoreMock;
        private readonly Mock<IConversationStore> _conversationStoreMock;
        private readonly Mock<IMessageStore> _messageStoreMock;
        private readonly Mock<IConversationService> _conversationServiceMock;

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

        public ConversationControllerTests()
        {
            _profileStoreMock = new Mock<IProfileStore>();
            _conversationStoreMock = new Mock<IConversationStore>();
            _messageStoreMock = new Mock<IMessageStore>();
            _conversationServiceMock = new Mock<IConversationService>();

            _controller = new ConversationController(
                _profileStoreMock.Object,
                _conversationStoreMock.Object,
                _messageStoreMock.Object,
                _conversationServiceMock.Object
            );
        }

        [Fact]
        public async Task AddConversation_ValidRequest_ReturnsCreated()
        {
            // Arrange
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
                .Setup(x => x.GetProfile("user1"))
                .ReturnsAsync(_profile1);
            _profileStoreMock
                .Setup(x => x.GetProfile("user2"))
                .ReturnsAsync(_profile2);

            // _conversationStoreMock
            //     .Setup(x => x.GetConversations("user1", null, null, "0"))
            //     .ReturnsAsync(new ConversationResponse();

            // Act
            var result = await _controller.AddConversation(conversation);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result.Result);
            var conversationResponse = Assert.IsType<ConversationResponse>(result.Value);
            //Assert.NotEmpty(conversationResponse.ConversationId);
            //Assert.NotEqual(default, conversationResponse.Timestamp);

            _conversationStoreMock.Verify(x => x.AddConversation(It.IsAny<Conversation>()), Times.Once);
            _messageStoreMock.Verify(x => x.AddMessage(It.IsAny<Message>()), Times.Once);
        }

        [Fact]
        public async Task AddConversation_InvalidParticipants_ReturnsBadRequest()
        {
            // Arrange
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

            // Act
            var result = await _controller.AddConversation(conversation);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Invalid message, please try again.", badRequestResult.Value);
        }

        [Fact]
        public async Task AddConversation_ParticipantNotFound_ReturnsNotFound()
        {
            // Arrange
            var conversation = new ConversationRequest
            (
                Participants: new[] { _profile1.username, _profile2.username},
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
                .ReturnsAsync((Profile)null);

            // Act
            var result = await _controller.AddConversation(conversation);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            //Assert.Equal("One or more participants not found.", notFoundResult.Value);
        }
    }
}