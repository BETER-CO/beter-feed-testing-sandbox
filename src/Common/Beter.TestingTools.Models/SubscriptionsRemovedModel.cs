namespace Beter.TestingTools.Models;

public sealed record SubscriptionsRemovedModel
{
    public IEnumerable<string> Ids { get; set; }
}