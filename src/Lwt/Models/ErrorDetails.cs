using Newtonsoft.Json;

namespace Lwt.Models
{
    public class ErrorDetails
    {
        private readonly int _statusCode;
        private readonly string _message;

        public ErrorDetails(int statusCode, string message)
        {
            _statusCode = statusCode;
            _message = message;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}