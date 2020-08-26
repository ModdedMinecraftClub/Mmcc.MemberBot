namespace Mmcc.MemberBot.Core.Models
{
    public class Application
    {
        public int AppId { get; set; }
        public ApplicationStatus AppStatus { get; set; }
        public string AuthorName { get; set; }
        public ulong AuthorDiscordId { get; set; }
        public string MessageContent { get; set; }
        public string MessageUrl { get; set; }
        public string ImageUrl { get; set; }
        public string AppTime { get; set; }
    }
}