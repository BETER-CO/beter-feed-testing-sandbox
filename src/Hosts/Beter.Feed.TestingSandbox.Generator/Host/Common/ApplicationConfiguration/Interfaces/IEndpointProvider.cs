namespace Beter.Feed.TestingSandbox.Generator.Host.Common.ApplicationConfiguration.Interfaces;

public interface IEndpointProvider
{
    public static abstract void DefineEndpoints(IEndpointRouteBuilder endpoints);
}