using MessagePack;

namespace Beter.TestingTools.Models.Incidents;

[MessagePackObject]
public class IncidentParameterModel
{
    public IncidentParameterModel()
    {

    }
    [Key("key")] public string Key { get; set; }
    [Key("value")] public string Value { get; set; }
    [Key("variableValue")] public string VariableValue { get; set; }
}
