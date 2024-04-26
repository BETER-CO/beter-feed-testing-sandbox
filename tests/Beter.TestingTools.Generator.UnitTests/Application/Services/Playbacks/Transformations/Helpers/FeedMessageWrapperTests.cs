using AutoFixture;
using Beter.TestingTools.Generator.Application.Services.Playbacks.Transformations.Helpers;
using System.Text.Json.Nodes;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.Playbacks.Transformations.Helpers
{
    public class FeedMessageWrapperTests
    {
        private static readonly Fixture Fixture = new();
        private readonly FeedMessageWrapper _wrapper;

        public FeedMessageWrapperTests()
        {
            var message = JsonNode.Parse("{\"Id\":\"1\"}");
            _wrapper = new FeedMessageWrapper(message);
        }

        [Fact]
        public void Constructor_Throws_Exception_When_Message_Is_Null()
        {
            // Arrange
            JsonNode message = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => new FeedMessageWrapper(message));
        }

        [Fact]
        public void Id_Get_And_Set_Work_Correctly()
        {
            // Arrange
            var expected = Fixture.Create<string>();

            // Act
            _wrapper.Id = expected;
            var actual = _wrapper.Id;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void MsgType_Get_And_Set_Work_Correctly()
        {
            // Arrange
            var expected = Fixture.Create<int>();

            // Act
            _wrapper.MsgType = expected;
            var actual = _wrapper.MsgType;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Offset_Get_And_Set_Work_Correctly()
        {
            // Arrange
            var expected = Fixture.Create<int>();

            // Act
            _wrapper.Offset = expected;
            var actual = _wrapper.Offset;

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SportId_Get_And_Set_Work_Correctly()
        {
            // Arrange
            var expected = Fixture.Create<int>();

            // Act
            _wrapper.SportId = expected;
            var actual = _wrapper.SportId;

            // Assert
            Assert.Equal(expected, actual);
        }
    }
}
