namespace Beter.Feed.TestingSandbox.Logging.WebApi;

public static class CorrelationIdManager
{
    private static readonly AsyncLocal<string> _correlationId = new();

    public static string? CorrelationId
    {
        get
        {
            if (string.IsNullOrEmpty(_correlationId.Value))
            {
                SetCorrelationId(Guid.NewGuid().ToString("D"));
            }

            return _correlationId.Value;
        }
    }

    public static void SetCorrelationId(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentNullException(nameof(correlationId));
        }

        if (correlationId.Length >= 1024)
        {
            throw new ArgumentException("Argument is too long.", nameof(correlationId));
        }

        _correlationId.Value = correlationId;
    }
}
