using System;

namespace ModdedMinecraftClub.MemberBot.Bot.Jobs.Models
{
    public class ScheduledJob : Job
    {
        public DateTime EnqueueAt { get; private set; }
        
        public ScheduledJob(JobDto dto) : base(dto)
        {
        }
        
        public override void Parse()
        {
            ParseBasics();

            EnqueueAt = ParseJobStatusSpecificDate("EnqueueAt");
        }
    }
}