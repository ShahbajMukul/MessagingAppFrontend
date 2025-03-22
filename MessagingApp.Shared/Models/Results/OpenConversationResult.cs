using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Results
{
    public class OpenConversationResult : UserInfo
    {
        public int ConversationID { get; set; }
        public bool ActiveNow { get; set; }
        public DateTime LastActiveTime { get; set; }
        public string? PublicKey { get; set; }

    }
}
