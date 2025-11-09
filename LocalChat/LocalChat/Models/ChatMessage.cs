using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalChat.Models
{
    internal class ChatMessage
    {
        public string Sender { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public MessageType Type { get; set; }
        public string? TargetUserId { get; set; }
    }

    public enum MessageType
    {
        Text,
        Command,
        System,
        UserList
    }
}
