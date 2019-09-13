using System.Threading.Tasks;

namespace ModdedMinecraftClub.MemberBot.Bot.Jobs
{
    public static class ReminderJob
    {
        public static async Task ExecuteUser(ulong guildId, ulong channelId, ulong userId, string content)
        {
            var guild = Program.Client.GetGuild(guildId);
            var channel = guild.GetTextChannel(channelId);

            await channel.SendMessageAsync($"<@{userId}> {content}");
        }
        
        public static async Task ExecuteRole(ulong guildId, ulong channelId, ulong roleId, string content)
        {
            var guild = Program.Client.GetGuild(guildId);
            var channel = guild.GetTextChannel(channelId);
            var role = guild.GetRole(roleId);

            await channel.SendMessageAsync($"{role.Mention} {content}");
        }
    }
}