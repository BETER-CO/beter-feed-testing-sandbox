namespace Beter.Feed.TestingSandbox.Generator.UnitTests.Common
{
    public class AssertInjection
    {
        public static AssertConstructor OfConstructor(Type type) => new(type);
    }

}
