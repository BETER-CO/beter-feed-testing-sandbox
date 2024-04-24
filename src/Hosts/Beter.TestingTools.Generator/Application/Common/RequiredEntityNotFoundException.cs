namespace Beter.TestingTools.Generator.Application.Common
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
