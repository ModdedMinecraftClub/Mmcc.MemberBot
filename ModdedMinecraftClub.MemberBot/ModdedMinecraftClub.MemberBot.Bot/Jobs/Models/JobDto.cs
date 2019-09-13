using System;

namespace ModdedMinecraftClub.MemberBot.Bot.Jobs.Models
{
    public class JobDto
    {
        private DateTime _dateTime;
        public int Id { get; set; }
        public string InvocationData { get; set; }
        public string Arguments { get; set; }

        public DateTime CreatedAt
        {
            get => _dateTime;
            set => _dateTime = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }
        public string Data { get; set; }
    }
}