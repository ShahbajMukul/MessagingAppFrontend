using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Results
{
    public class RegiLoginResult : UserInfo
    {
        public required string Token { get; init; }
    }
}
