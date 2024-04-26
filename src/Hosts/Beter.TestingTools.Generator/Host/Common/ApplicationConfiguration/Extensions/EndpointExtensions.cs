using Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Extensions;
using Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Interfaces;
using System.Reflection;

namespace Beter.TestingTools.Generator.Host.Common.ApplicationConfiguration.Extensions;

public static class EndpointExtensions
{
    public static void UseEndpoints<TMarker>(this IApplicationBuilder app)
    {
        app.UseEndpoints(typeof(TMarker));
    }

    private static void UseEndpoints(this IApplicationBuilder app, Type typeMarker)
    {
        var endpointTypes = GetEndpointTypesFromAssemblyContaining(typeMarker);
        foreach (var endpointType in endpointTypes)
        {
            app.UseEndpoints(endpoints =>
            {
                endpointType.GetMethod(nameof(IEndpointProvider.DefineEndpoints))!
                .Invoke(null, new object[] { endpoints });
            });
        }
    }

    private static IEnumerable<TypeInfo> GetEndpointTypesFromAssemblyContaining(Type typeMarker)
    {
        var endpointTypes = typeMarker.Assembly.DefinedTypes
            .Where(x => x is { IsAbstract: false, IsInterface: false } && typeof(IEndpointProvider).IsAssignableFrom(x));
        return endpointTypes;
    }
}
