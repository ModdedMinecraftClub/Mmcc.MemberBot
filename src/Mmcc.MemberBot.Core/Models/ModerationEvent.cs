#nullable enable
using System;

namespace Mmcc.MemberBot.Core.Models
{
    public class ModerationEvent
    {
        public int Id { get; set; }
        public ModerationEventType Type { get; set; }
        public bool Active { get; set; }
        public DateTime? ExpiryDateUtc { get; set; }
        public ulong? DiscordId { get; set; }
        public string? Ign { get; set; }
        public string Reason { get; set; } = null!;
    }
}