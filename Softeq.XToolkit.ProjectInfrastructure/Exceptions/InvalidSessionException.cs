namespace Softeq.XToolkit.CrossCutting.Exceptions
{
    public class InvalidSessionException : HttpException
    {
        public InvalidSessionException(string message) : base(message)
        {
        }

        public InvalidSessionException(string message, HttpResponse response)
            : base(message, response)
        {
        }
    }
}
