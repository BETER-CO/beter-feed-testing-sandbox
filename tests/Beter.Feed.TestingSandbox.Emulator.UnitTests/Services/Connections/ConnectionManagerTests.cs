﻿using AutoFixture;
using Beter.Feed.TestingSandbox.Common.Models;
using Beter.Feed.TestingSandbox.Emulator.Services.Connections;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.Services.Connections
{
    public class ConnectionManagerTests
    {
        private static readonly Fixture _fixture = new();

        [Fact]
        public void GetAll_ReturnsAllConnections()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var connection1 = _fixture.Build<FeedConnection>().With(x => x.Id, "1").Create();
            var connection2 = _fixture.Build<FeedConnection>().With(x => x.Id, "2").Create();

            connectionManager.Set(connection1);
            connectionManager.Set(connection2);

            // Act
            var allConnections = connectionManager.GetAll();

            // Assert
            Assert.Equal(2, allConnections.Count());
            Assert.Contains(connection1, allConnections);
            Assert.Contains(connection2, allConnections);
        }

        [Fact]
        public void IsActive_ReturnsTrueForActiveConnection()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var connection = _fixture.Build<FeedConnection>().With(x => x.Id, "1").Create();
            connectionManager.Set(connection);

            // Act
            var isActive = connectionManager.IsActive(connection.Id);

            // Assert
            Assert.True(isActive);
        }

        [Fact]
        public void IsActive_ReturnsFalseForInactiveConnection()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var connection = _fixture.Build<FeedConnection>().With(x => x.Id, "1").Create();

            // Act
            var isActive = connectionManager.IsActive(connection.Id);

            // Assert
            Assert.False(isActive);
        }

        [Fact]
        public void Remove_RemovesConnection()
        {
            // Arrange
            var connectionManager = new ConnectionManager();
            var connection = _fixture.Build<FeedConnection>().With(x => x.Id, "1").Create();
            connectionManager.Set(connection);

            // Act
            connectionManager.Remove(connection.Id);

            // Assert
            Assert.False(connectionManager.IsActive(connection.Id));
        }
    }
}
