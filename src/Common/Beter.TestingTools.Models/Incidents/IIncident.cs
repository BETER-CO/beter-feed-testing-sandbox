using Beter.TestingTools.Models;

namespace Beter.TestingTools.Models.Incidents;

public interface IIncident : IIdentityModel
{
    public int Index { get; set; }
    public string Type { get; set; }
}
