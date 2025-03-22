using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Payloads
{
    public class LoginPayload
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string PublicKey { get; set; }
    }
}
