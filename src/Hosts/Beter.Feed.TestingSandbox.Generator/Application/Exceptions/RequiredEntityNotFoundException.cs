namespace Beter.Feed.TestingSandbox.Generator.Application.Exceptions
{
    public class RequiredEntityNotFoundException : Exception
    {
        public RequiredEntityNotFoundException()
        {
        }

        public RequiredEntityNotFoundException(string message) : base(message)
        {
        }

        public RequiredEntityNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
