using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Payloads
{
    public class MessagePayload
    {
        public required int ConversationID { get; set; }
        public required string Content { get; set; }
        public required string EncryptedKey { get; set; }
        public required string IV { get; set; }
        public required DateTime SentTime { get; set; }

    }
}
