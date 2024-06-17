using Beter.Feed.TestingSandbox.Emulator.Services.Heartbeats;

namespace Beter.Feed.TestingSandbox.Emulator.UnitTests.Services.Heartbeats
{
    public class HeartbeatCommandTests
    {
        [Fact]
        public void GetAll_ReturnsAllCommands()
        {
            // Act
            var commands = HeartbeatCommand.GetAll();

            // Assert
            Assert.Equal(2, commands.Count());
            Assert.Contains(HeartbeatCommand.Run, commands);
            Assert.Contains(HeartbeatCommand.Stop, commands);
        }

        [Fact]
        public void Get_ReturnsCommandById()
        {
            // Arrange
            var id = 1;

            // Act
            var command = HeartbeatCommand.Get(id);

            // Assert
            Assert.Equal(id, command.Id);
        }

        [Fact]
        public void Get_ThrowsInvalidOperationException_WhenCommandIsNotSupported()
        {
            // Arrange
            var unsupportedId = 3;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => HeartbeatCommand.Get(unsupportedId));
        }

        [Fact]
        public void IsSupport_ReturnsTrue_ForSupportedCommand()
        {
            // Arrange
            var supportedId = 1;

            // Act
            var isSupported = HeartbeatCommand.IsSupport(supportedId);

            // Assert
            Assert.True(isSupported);
        }

        [Fact]
        public void IsSupport_ReturnsFalse_ForUnsupportedCommand()
        {
            // Arrange
            var unsupportedId = 3;

            // Act
            var isSupported = HeartbeatCommand.IsSupport(unsupportedId);

            // Assert
            Assert.False(isSupported);
        }

        [Fact]
        public void IsRunStatus_ReturnsTrue_ForRunCommand()
        {
            // Arrange
            var runCommand = HeartbeatCommand.Run;

            // Act
            var isRunStatus = HeartbeatCommand.IsRunStatus(runCommand);

            // Assert
            Assert.True(isRunStatus);
        }

        [Fact]
        public void IsRunStatus_ReturnsFalse_ForStopCommand()
        {
            // Arrange
            var stopCommand = HeartbeatCommand.Stop;

            // Act
            var isRunStatus = HeartbeatCommand.IsRunStatus(stopCommand);

            // Assert
            Assert.False(isRunStatus);
        }

        [Fact]
        public void EnsureThatIsSupported_ThrowsInvalidOperationException_WhenCommandIsNotSupported()
        {
            // Arrange
            var unsupportedId = 3;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => HeartbeatCommand.EnsureThatIsSupported(unsupportedId));
        }

        [Fact]
        public void EnsureThatIsSupported_DoesNotThrowException_WhenCommandIsSupported()
        {
            // Arrange
            var supportedId = 1;

            // Act
            HeartbeatCommand.EnsureThatIsSupported(supportedId);
        }
    }
}
