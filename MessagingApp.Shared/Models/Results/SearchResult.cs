using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Results
{
    public class SearchResult : UserInfo
    {
        public bool ActiveNow { get; set; }
        public DateTime LastActiveTime { get; set; }
        public string? PublicKey { get; set; }

        public string DisplayName { get; set; } = "";

        public SearchResult()
        {
            DisplayName = FirstName + " " + LastName;
        }
    }
}
