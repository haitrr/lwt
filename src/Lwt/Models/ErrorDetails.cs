using Newtonsoft.Json;

namespace Lwt.Models
{
    public class ErrorDetails
    {
        public string Message { get; }

        public ErrorDetails(string message)
        {
            Message = message;
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}