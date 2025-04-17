using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessagingApp.Shared.Models.Results
{
    public class MessageResult
    {
        public int MessageID { get; set; }
        public int ConversationID { get; set; }
        public required string Content { get; set; }
        public required string EncryptedKey { get; set; }
        public required string IV { get; set; }
        public int SenderUserID { get; set; }
        public DateTime SentTime { get; set; }
        public bool IsRead { get; set; }
    }
}
