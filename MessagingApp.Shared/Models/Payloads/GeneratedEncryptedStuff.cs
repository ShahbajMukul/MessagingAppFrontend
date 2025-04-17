using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Payloads
{
    public class GeneratedEncryptedStuff
    {
        public int ConversationID { get; set; }
        public string? Content { get; set; }
        public string? EncryptionKey { get; set; }
        public string? IV { get; set; }
        public DateTime SentTime { get; set; }
    }
}
