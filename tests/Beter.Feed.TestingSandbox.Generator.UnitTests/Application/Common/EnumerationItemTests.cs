using Beter.Feed.TestingSandbox.Common;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Common
{
    public class EnumerationItemTests
    {
        [Fact]
        public void Equals_ReturnsTrue_ForEqualItems()
        {
            // Arrange
            var item1 = new TestEnumerationItem(1, "Item1");
            var item2 = new TestEnumerationItem(1, "Item2");

            // Act & Assert
            Assert.Equal(item1, item2);
            Assert.True(item1.Equals(item2));
            Assert.True(item2.Equals(item1));
        }

        [Fact]
        public void Equals_ReturnsFalse_ForDifferentItems()
        {
            // Arrange
            var item1 = new TestEnumerationItem(1, "Item1");
            var item2 = new TestEnumerationItem(2, "Item2");

            // Act & Assert
            Assert.NotEqual(item1, item2);
            Assert.False(item1.Equals(item2));
            Assert.False(item2.Equals(item1));
        }

        [Fact]
        public void GetHashCode_ReturnsSameHashCode_ForEqualItems()
        {
            // Arrange
            var item1 = new TestEnumerationItem(1, "Item1");
            var item2 = new TestEnumerationItem(1, "Item2");

            // Act & Assert
            Assert.Equal(item1.GetHashCode(), item2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_ReturnsUniqueHashCode_ForDifferentItems()
        {
            // Arrange
            var item1 = new TestEnumerationItem(1, "Item1");
            var item2 = new TestEnumerationItem(2, "Item2");

            // Act & Assert
            Assert.NotEqual(item1.GetHashCode(), item2.GetHashCode());
        }

        [Fact]
        public void ImplicitConversionToInt_ReturnsCorrectId()
        {
            // Arrange
            var item = new TestEnumerationItem(1, "Item1");

            // Act
            int id = item;

            // Assert
            Assert.Equal(1, id);
        }

        [Fact]
        public void ImplicitConversionToString_ReturnsCorrectName()
        {
            // Arrange
            var item = new TestEnumerationItem(1, "Item1");

            // Act
            string name = item;

            // Assert
            Assert.Equal("Item1", name);
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenNameIsNull()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(
                () => new TestEnumerationItem(1, null));
        }

        [Fact]
        public void Equals_ReturnsFalse_ForDifferentType()
        {
            // Arrange
            var item1 = new TestEnumerationItem(1, "Item1");
            var item2 = new object();

            // Act & Assert
            Assert.False(item1.Equals(item2));
        }

        private class TestEnumerationItem : EnumerationItem
        {
            public TestEnumerationItem(int id, string name) : base(id, name)
            {
            }
        }
    }
}
