using AutoFixture;
using Beter.Feed.TestingSandbox.Generator.Application.Services.Playbacks.Transformations;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.Playbacks.Transformations.TransformationExtTests
{
    public class UpdateModelIdTests
    {
        private static readonly Fixture Fixture = new();

        [Fact]
        public void UpdateModelId_Updates_Model_Id_Using_Context_NewId()
        {
            // Arrange
            var oldId = Fixture.Create<string>();
            var newId = Fixture.Create<string>();
            var model = Fixture.Build<TestModel>()
                .With(x => x.Id, oldId)
                .Create();

            var profile = Fixture.Build<MessagesTransformationContext.MatchIdProfile>()
                .With(x => x.Id, oldId)
                .With(x => x.NewId, newId)
                .Create();
            var context = Fixture.Build<MessagesTransformationContext>()
                .With(x => x.Matches, new Dictionary<string, MessagesTransformationContext.MatchIdProfile> { { oldId, profile } })
                .Create();

            // Act
            TransformationsExt.UpdateModelId(model, context);

            // Assert
            Assert.Equal(model.Id, newId);
        }
    }
}
