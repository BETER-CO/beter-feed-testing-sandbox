using Microsoft.AspNetCore.SignalR;

namespace Beter.Feed.TestingSandbox.Emulator.SignalR.Helpers;

public static class ContextInfoHelper
{
    private static string API_KEY_PARAMETER_NAME = "ApiKey";

    private static Guid GetGuidParameter(HubCallerContext hubCallerContext, string parameterKey)
    {
        var param = GetParamImpl(hubCallerContext, parameterKey);

        if (string.IsNullOrEmpty(param) || !Guid.TryParse(param, out var result) || result == Guid.Empty)
        {
            return Guid.Empty;
        }

        return result;
    }

    private static string GetParamImpl(HubCallerContext hubCallerContext, string parameterKey)
    {
        var mvcContext = hubCallerContext?.GetHttpContext();
        if (mvcContext == null)
        {
            throw new ArgumentException(nameof(hubCallerContext));
        }

        string param = mvcContext.Request.Headers[parameterKey];
        if (string.IsNullOrEmpty(param))
        {
            param = mvcContext.Request.Query[parameterKey].FirstOrDefault();
        }

        return param;
    }

    public static Guid GetKey(this HubCallerContext hubCallerContext) => GetGuidParameter(hubCallerContext, API_KEY_PARAMETER_NAME);

    public const string ForwardedHeader = "X-Forwarded-For";
    public const string RealIpHeader = "X-Real-IP";
    public const string CloudFlareHeaderIp = "CF-Connecting-IP";

    public static string GetIp(this HubCallerContext hubCallerContext)
    {
        var mvcContext = hubCallerContext?.GetHttpContext();
        if (mvcContext == null)
        {
            throw new ArgumentException(nameof(hubCallerContext));
        }

        if (mvcContext.Request.Headers.TryGetValue(CloudFlareHeaderIp, out var cloudFlareHeaderIpValue))
        {
            return cloudFlareHeaderIpValue.FirstOrDefault();
        }

        if (mvcContext.Request.Headers.TryGetValue(ForwardedHeader, out var forwardedHeaderValue))
        {
            return forwardedHeaderValue.FirstOrDefault();
        }

        if (mvcContext.Request.Headers.TryGetValue(RealIpHeader, out var realIpHeaderValue))
        {
            return realIpHeaderValue.FirstOrDefault();
        }

        return string.Empty;
    }
}
