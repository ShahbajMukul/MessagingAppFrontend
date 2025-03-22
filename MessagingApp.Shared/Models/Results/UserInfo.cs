using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Results
{
    public class UserInfo
    {
        public required int UserID { get; init; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public required string Username { get; init; }
        public required string Email { get; init; }
    }
}
