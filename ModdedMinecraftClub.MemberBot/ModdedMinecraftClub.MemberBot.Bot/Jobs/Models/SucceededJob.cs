using System;

namespace ModdedMinecraftClub.MemberBot.Bot.Jobs.Models
{
    public class SucceededJob : Job
    {
        public DateTime SucceededAt { get; private set; }
        
        public SucceededJob(JobDto dto) : base(dto)
        {
        }
        
        public override void Parse()
        {
            ParseBasics();

            SucceededAt = ParseJobStatusSpecificDate("SucceededAt");
        }
    }
}