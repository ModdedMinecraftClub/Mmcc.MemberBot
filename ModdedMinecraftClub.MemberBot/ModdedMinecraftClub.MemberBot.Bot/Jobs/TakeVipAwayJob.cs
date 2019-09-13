using System.Linq;
using System.Threading.Tasks;

namespace ModdedMinecraftClub.MemberBot.Bot.Jobs
{
    public static class TakeVipAwayJob
    {
        public static async Task Execute(ulong userId, ulong guildId, string ign)
        {
            var guild = Program.Client.GetGuild(guildId);
            var user = guild.GetUser(userId);
            var roles = user.Roles;
            var channels = guild.TextChannels;
            
            // Remove rank on Discord
            var vipRole = roles.First(r => r.Name.Equals("Vip"));
            
            await user.RemoveRoleAsync(vipRole);
            
            // Set appropriate rank on each server
            var polychatChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.ChannelNames.Polychat));

            await polychatChannel.SendMessageAsync($"!exec <all> ranks set {ign} guest");
            
            var prefixes = from r in roles where r.Name.Contains('[') && r.Name.Contains(']') select r.Name.Substring(1, r.Name.IndexOf(']') - 1);

            foreach (var prefix in prefixes)
            {
                await polychatChannel.SendMessageAsync($"!promote {prefix} {ign}");
            }
            
            // Send info messages
            var membersChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.ChannelNames.MemberApps));
            var serverRoles = guild.Roles;
            var staffRole = serverRoles.First(r => r.Name.Equals("Staff"));
            
            await membersChannel.SendMessageAsync(
                $"<@{userId}> Your VIP has expired. If you have already renewed it, the role will be regranted to you shortly.");
            await membersChannel.SendMessageAsync($"{staffRole.Mention} Check if the above user has renewed their VIP.");
        }
    }
}