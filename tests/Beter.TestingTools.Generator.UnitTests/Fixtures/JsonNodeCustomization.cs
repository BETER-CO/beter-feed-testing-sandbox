using AutoFixture;
using AutoFixture.Kernel;
using Beter.TestingTools.Models.Scoreboards;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Beter.TestingTools.Generator.UnitTests.Fixtures
{
    public class JsonNodeBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            var type = request as Type;
            if (type == null || type != typeof(JsonNode))
            {
                return new NoSpecimen();
            }

            var value = JsonSerializer.Serialize(context.CreateMany<ScoreBoardModel>());

            return JsonNode.Parse(value);
        }
    }
}
