namespace Beter.Feed.TestingSandbox.Models;

public sealed record SubscriptionsRemovedModel
{
    public IEnumerable<string> Ids { get; set; }
}