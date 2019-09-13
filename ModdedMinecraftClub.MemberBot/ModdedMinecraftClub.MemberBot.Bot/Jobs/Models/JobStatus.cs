namespace ModdedMinecraftClub.MemberBot.Bot.Jobs.Models
{
    public enum JobStatus
    {
        Enqueued,
        Scheduled,
        Processing,
        Succeeded,
        Failed,
        Deleted,
        Awaiting
    }
}