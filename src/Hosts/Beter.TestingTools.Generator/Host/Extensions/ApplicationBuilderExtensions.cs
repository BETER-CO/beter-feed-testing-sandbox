using Microsoft.AspNetCore.Http.Features;

namespace Beter.TestingTool.Generator.Host.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder ConfigureMaxRequestBodySize(this IApplicationBuilder app)
    {
        app.Use((context, next) =>
        {
            var feature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
            if (feature != null)
            {
                feature.MaxRequestBodySize = long.MaxValue;

            }

            return next.Invoke();
        });

        return app;
    }
}
