using Beter.Feed.TestingSandbox.Models;

namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Application.Services.Playbacks.Transformations.TransformationExtTests
{
    public class TestModel : IIdentityModel
    {
        public string Id { get; set; }

        public DateTime Date { get; set; }
    }
}
