using Beter.Feed.TestingSandbox.Models;

namespace Beter.Feed.TestingSandbox.Models.Incidents;

public interface IIncident : IIdentityModel
{
    public int Index { get; set; }
    public string Type { get; set; }
}
