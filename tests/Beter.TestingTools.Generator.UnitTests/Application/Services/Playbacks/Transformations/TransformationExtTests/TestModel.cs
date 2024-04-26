using Beter.TestingTools.Models;

namespace Beter.TestingTools.Generator.UnitTests.Application.Services.Playbacks.Transformations.TransformationExtTests
{
    public class TestModel : IIdentityModel
    {
        public string Id { get; set; }

        public DateTime Date { get; set; }
    }
}
