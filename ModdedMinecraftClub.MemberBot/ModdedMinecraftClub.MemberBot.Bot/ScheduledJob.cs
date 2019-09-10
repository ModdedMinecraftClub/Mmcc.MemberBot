using System.Collections.Generic;
using Newtonsoft.Json;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    public class ScheduledJob : Job
    {
        public string EnqueueAt { get; set; }
        
        public ScheduledJob(JobDto dto) : base(dto)
        {
        }
        
        public void Parse()
        {
            ParseBasics();

            EnqueueAt = ParseJobStatusSpecificDate("EnqueueAt");
        }
    }
}