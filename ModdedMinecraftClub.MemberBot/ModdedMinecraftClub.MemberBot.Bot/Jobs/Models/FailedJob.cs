namespace ModdedMinecraftClub.MemberBot.Bot.Jobs.Models
{
    public class FailedJob : Job
    {
        public FailedJob(JobDto dto) : base(dto)
        {
        }
        
        public override void Parse()
        {
            ParseBasics();
        }
    }
}