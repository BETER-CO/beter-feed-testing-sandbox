using Beter.TestingTools.Generator.Infrastructure.Services.FeedConnections;

namespace Beter.TestingTools.Generator.UnitTests.Infrastructure.Services.FeedConnections
{
    public class JsonContentDeserializerTests
    {
        private static readonly JsonContentDeserializer Deserializer = new JsonContentDeserializer();

        [Fact]
        public void DeserializeOrThrow_Returns_Deserialized_Object_For_Valid_JSON()
        {
            // Arrange
            var content = "{\"property1\": \"1\", \"property2\": \"2\"}";

            // Act
            var result = Deserializer.DeserializeOrThrow<DeserializableItem>(content);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Property1);
            Assert.Equal("2", result.Property2);
        }

        [Fact]
        public void DeserializeOrThrow_Throws_ArgumentException_For_Invalid_JSON()
        {
            // Arrange
            var deserializer = new JsonContentDeserializer();
            var invalidContent = "invalid json";

            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => Deserializer.DeserializeOrThrow<DeserializableItem>(invalidContent));
        }

        [Fact]
        public void DeserializeOrThrow_Throws_ArgumentNullException_For_Null_Content()
        {
            // Arrange
            var deserializer = new JsonContentDeserializer();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => Deserializer.DeserializeOrThrow<DeserializableItem>(null));
        }

        [Fact]
        public void DeserializeOrThrow_Throws_ArgumentNullException_For_Empty_Content()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(
                () => Deserializer.DeserializeOrThrow<DeserializableItem>(string.Empty));
        }

        private class DeserializableItem
        {
            public string Property1 { get; set; }
            public string Property2 { get; set; }
        }
    }
}
