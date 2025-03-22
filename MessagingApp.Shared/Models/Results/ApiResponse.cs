using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Results
{
    public class ApiResponse<T>
    {
        public T? Data { get; init; }
        public HttpStatusCode StatusCode { get; init; }
        public string? ErrorMessage { get; init; }

        public bool IsSuccess => StatusCode == HttpStatusCode.OK;
    }
}
