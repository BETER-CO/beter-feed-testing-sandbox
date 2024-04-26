namespace Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Interfaces;

public interface IEndpointProvider
{
    public static abstract void DefineEndpoints(IEndpointRouteBuilder endpoints);
}