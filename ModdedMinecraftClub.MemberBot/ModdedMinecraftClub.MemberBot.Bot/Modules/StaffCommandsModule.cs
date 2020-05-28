using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    public class StaffCommandsModule : ModuleBase<SocketCommandContext>
    {
        [Command("approve", RunMode = RunMode.Async)]
        [Priority(1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Approve(int applicationId, string serverPrefix, string ign)
        {
            using var c = new DatabaseConnection();
            var app = c.GetById(applicationId);

            if (app is null)
            {
                await Context.Channel.SendMessageAsync(
                    $":x: Application with ID `{applicationId}` does not exist.");

                return;
            }

            var channels = Context.Guild.TextChannels;
            var roles = Context.Guild.Roles;
            var users = Context.Guild.Users;
            var polychatChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.ChannelNames.Polychat));
            var membersChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.ChannelNames.MemberApps));

            var memberRole = roles.FirstOrDefault(role => role.Name.Contains($"[{serverPrefix.ToUpper()}]"));

            if (memberRole is null)
            {
                await Context.Channel.SendMessageAsync($":x: Prefix `{serverPrefix}` does not exist.");
                    
                return;
            }
                
            var userToPromote = users.FirstOrDefault(user => user.Id == app.AuthorDiscordId);

            if (userToPromote is null)
            {
                await Context.Channel.SendMessageAsync($":x: Cannot find a user with ID `{app.AuthorDiscordId}`. Promote manually or reject the application.");

                return;
            }
                
            await userToPromote.AddRoleAsync(memberRole);
                
            await polychatChannel.SendMessageAsync($"!promote {serverPrefix} {ign}");
                
            c.MarkAsApproved(applicationId);

            await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}> Congratulations, your application has been approved.");
                
            await Context.Channel.SendMessageAsync($":white_check_mark: **Approved** application with ID: `{applicationId}`");
        }
        
        [Command("approve", RunMode = RunMode.Async)]
        [Priority(-1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Approve(int applicationId, string arg)
        {
            if (arg.Equals("manual"))
            {
                using (var c = new DatabaseConnection())
                {
                    c.MarkAsApproved(applicationId);
                }
                
                await Context.Channel.SendMessageAsync($":white_check_mark: **Marked** application with ID `{applicationId}` as approved but the player still has to be promoted manually.\nRemember to let the player know once you have promoted them manually.");
            } else
            {
                await Context.Channel.SendMessageAsync($":x: Argument {arg} not recognized.");
            }
        }
        
        [Command("reject", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Reject(int applicationId, [Remainder]string reason)
        {
            using var c = new DatabaseConnection();
            var app = c.GetById(applicationId);

            if (app is null)
            {
                await Context.Channel.SendMessageAsync($":x: Application with ID `{applicationId}` does not exist.");
                    
                return;
            }
                
            var channels = Context.Guild.TextChannels;
            var membersChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.ChannelNames.MemberApps));
                
            c.MarkAsRejected(applicationId);
                
            await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}> Unfortunately, your application has been rejected.\nReason:\n```{reason}```");
                
            await Context.Channel.SendMessageAsync($":white_check_mark: **Rejected** application with ID `{applicationId}`");
        }
    }
}