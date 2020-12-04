namespace Mmcc.MemberBot.Core.Models.Settings
{
    public class DiscordSettings
    {
        public char Prefix { get; set; }
        public string Token { get; set; } = null!;
        public string InviteUrl { get; set; } = null!;
        public ChannelNamesSettings ChannelNames { get; set; } = null!;
        public RoleNamesSettings RoleNames { get; set; } = null!;
    }
}