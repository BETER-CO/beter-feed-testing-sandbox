using Beter.TestingTools.Generator.Application.Services.Heartbeats;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.Heartbeats
{
    public class HeartbeatControlServiceTests
    {
        [Fact]
        public void SetCommand_SetsCorrectCurrentCommand()
        {
            // Arrange
            var service = new HeartbeatControlService();
            var expectedCommand = HeartbeatCommand.Stop;

            // Act
            var currentCommand = service.SetCommand(expectedCommand.Id);

            // Assert
            Assert.Equal(expectedCommand, currentCommand);
        }

        [Fact]
        public void GetCurrentCommand_ReturnsCorrectCurrentCommand()
        {
            // Arrange
            var service = new HeartbeatControlService();
            var initialCommand = HeartbeatCommand.Run;
            service.SetCommand(initialCommand.Id);

            // Act
            var currentCommand = service.GetCurrentCommand();

            // Assert
            Assert.Equal(initialCommand, currentCommand);
        }

        [Fact]
        public void SetCommand_ThrowsInvalidOperationException_WhenCommandIsNotSupported()
        {
            // Arrange
            var service = new HeartbeatControlService();
            var unsupportedCommandId = 3;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => service.SetCommand(unsupportedCommandId));
        }
    }
}
