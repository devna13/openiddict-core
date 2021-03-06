﻿/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace OpenIddict.Server.Tests
{
    public class OpenIddictServerEventDispatcherTests
    {
        [Fact]
        public async Task DispatchAsync_ThrowsAnExceptionForNullNotification()
        {
            // Arrange
            var provider = Mock.Of<IServiceProvider>();
            var dispatcher = new OpenIddictServerEventDispatcher(provider);

            // Act and assert
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(()
                => dispatcher.DispatchAsync<Event>(notification: null));

            Assert.Equal("notification", exception.ParamName);
        }

        [Fact]
        public async Task DispatchAsync_InvokesHandlers()
        {
            // Arrange
            var handlers = new List<IOpenIddictServerEventHandler<Event>>
            {
                Mock.Of<IOpenIddictServerEventHandler<Event>>(),
                Mock.Of<IOpenIddictServerEventHandler<Event>>()
            };

            var provider = new Mock<IServiceProvider>();
            provider.Setup(mock => mock.GetService(typeof(IEnumerable<IOpenIddictServerEventHandler<Event>>)))
                .Returns(handlers);

            var dispatcher = new OpenIddictServerEventDispatcher(provider.Object);

            var notification = new Event();

            // Act
            await dispatcher.DispatchAsync(notification);

            // Assert
            Mock.Get(handlers[0]).Verify(mock => mock.HandleAsync(notification), Times.Once());
            Mock.Get(handlers[1]).Verify(mock => mock.HandleAsync(notification), Times.Once());
        }

        [Fact]
        public async Task DispatchAsync_StopsInvokingHandlersWhenHandledIsReturned()
        {
            // Arrange
            var handlers = new List<IOpenIddictServerEventHandler<Event>>
            {
                Mock.Of<IOpenIddictServerEventHandler<Event>>(
                    mock => mock.HandleAsync(It.IsAny<Event>()) == Task.FromResult(OpenIddictServerEventState.Unhandled)),
                Mock.Of<IOpenIddictServerEventHandler<Event>>(
                    mock => mock.HandleAsync(It.IsAny<Event>()) == Task.FromResult(OpenIddictServerEventState.Unhandled)),
                Mock.Of<IOpenIddictServerEventHandler<Event>>(
                    mock => mock.HandleAsync(It.IsAny<Event>()) == Task.FromResult(OpenIddictServerEventState.Handled)),
                Mock.Of<IOpenIddictServerEventHandler<Event>>()
            };

            var provider = new Mock<IServiceProvider>();
            provider.Setup(mock => mock.GetService(typeof(IEnumerable<IOpenIddictServerEventHandler<Event>>)))
                .Returns(handlers);

            var dispatcher = new OpenIddictServerEventDispatcher(provider.Object);

            var notification = new Event();

            // Act
            await dispatcher.DispatchAsync(notification);

            // Assert
            Mock.Get(handlers[0]).Verify(mock => mock.HandleAsync(notification), Times.Once());
            Mock.Get(handlers[1]).Verify(mock => mock.HandleAsync(notification), Times.Once());
            Mock.Get(handlers[2]).Verify(mock => mock.HandleAsync(notification), Times.Once());
            Mock.Get(handlers[3]).Verify(mock => mock.HandleAsync(notification), Times.Never());
        }

        public class Event : IOpenIddictServerEvent { }
    }
}
