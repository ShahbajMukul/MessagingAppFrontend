using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Results
{
    public class UserInfo
    {
        public required int UserID { get; init; }
        public required string Username { get; init; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; init; }
    }
}
