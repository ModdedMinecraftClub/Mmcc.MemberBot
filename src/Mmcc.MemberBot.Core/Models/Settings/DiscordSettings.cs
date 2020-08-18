namespace Mmcc.MemberBot.Core.Models.Settings
{
    public class DiscordSettings
    {
        public char Prefix { get; set; }
        public string Token { get; set; }
        public ChannelNamesSettings ChannelNames { get; set; }
    }
}