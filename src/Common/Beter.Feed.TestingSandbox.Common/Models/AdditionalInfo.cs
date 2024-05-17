namespace Beter.Feed.TestingSandbox.Common.Models
{
    public sealed class AdditionalInfo
    {
        private readonly Dictionary<string, object> _additionalInfo;

        public AdditionalInfo() : this(new Dictionary<string, object>())
        {
        }

        public AdditionalInfo(Dictionary<string, object> additionalInfo)
        {
            _additionalInfo = additionalInfo ?? throw new ArgumentNullException(nameof(additionalInfo));
        }

        public static AdditionalInfo NoInfo() => new();

        public object this[string key]
        {
            get => GetValue(key);
            set => _additionalInfo[key] = value;
        }

        public object GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException($"Value'{nameof(key)}' cannot be null or empty.");

            var foundKey = _additionalInfo.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (foundKey == null)
                throw new KeyNotFoundException($"Value with key '{key}' was not found.");

            return _additionalInfo[foundKey];
        }

        public TValue GetValue<TValue>(string key) => (TValue)GetValue(key);

        public bool TryGetValue(string key, out object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                value = null;
                return false;
            }

            var foundKey = _additionalInfo.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (foundKey == null)
            {
                value = null;
                return false;
            }

            value = _additionalInfo[foundKey];
            return true;
        }

        public bool TryGetValue<TValue>(string key, out TValue value)
        {
            if (TryGetValue(key, out var objValue) && objValue is TValue typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        public Dictionary<string, object> GetInfo() => _additionalInfo;
    }
}
