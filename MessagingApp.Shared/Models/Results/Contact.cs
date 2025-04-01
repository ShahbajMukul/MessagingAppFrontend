using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Results
{
    public class Contact
    {
        public string UserId { get; set; } = "";
        public string DisplayName { get; set; } = "";
        public string PublicKey { get; set; } = ""; // Important for sending messages
                                                    // Add other properties like presence status, etc.
        public bool IsExternal { get; set; } = false; // Flag if it came from external search
    }
}
