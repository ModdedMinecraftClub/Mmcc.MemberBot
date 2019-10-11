using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hangfire;
using ModdedMinecraftClub.MemberBot.Bot.Jobs;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class VipModule : ModuleBase<SocketCommandContext>
    {
        #region Staff Only

        [Command("vip", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Vip(string ign, SocketGuildUser guildUser)
        {
            var channels = Context.Guild.TextChannels;
            var polychatChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.ChannelNames.Polychat));
            
            // Give VIP on all servers
            await polychatChannel.SendMessageAsync($"!exec <all> ranks set {ign} vip");
            
            // Give VIP role on Discord
            var roles = Context.Guild.Roles;
            var vipRole = roles.First(r => r.Name.Equals("Vip"));
            await guildUser.AddRoleAsync(vipRole);

            var dateNow = DateTime.UtcNow;
            var vipExpiry = dateNow.AddMonths(1);
            
            // Schedule a job that will take VIP away
            BackgroundJob.Schedule(() => TakeVipAwayJob.Execute(guildUser.Id, Context.Guild.Id, ign), vipExpiry);
            
            // Info message
            await Context.Channel.SendMessageAsync($":white_check_mark: Gave VIP to {ign} and scheduled a VIP job for next month.");
        }

        #endregion
    }
}