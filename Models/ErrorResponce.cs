using System.Net;

namespace TransactionAPI.Models
{
    public class ErrorResponse
    {
        public HttpStatusCode Status { get; set; }
        public string Message { get; set; }
    }
}
