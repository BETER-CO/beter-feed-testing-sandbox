namespace Beter.Feed.TestingSandbox.Generator.Application.Common;

public abstract class EnumerationItem
{
    public int Id { get; private set; }

    public string Name { get; private set; }

    protected EnumerationItem(int id, string name)
    {
        Id = id;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public override string ToString() => Name;

    public override bool Equals(object obj)
    {
        if (obj is not EnumerationItem otherValue)
        {
            return false;
        }

        var typeMatches = GetType().Equals(obj.GetType());
        var valueMatches = Id.Equals(otherValue.Id);

        return typeMatches && valueMatches;
    }

    public override int GetHashCode() => HashCode.Combine(GetType().GetHashCode(), Id.GetHashCode());

    public static implicit operator int(EnumerationItem item) => item.Id;

    public static implicit operator string(EnumerationItem item) => item.Name;
}
