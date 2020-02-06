using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class MemberApplicationsModule : ModuleBase<SocketCommandContext>
    {
        #region Regular

        [Command("pending", RunMode = RunMode.Async)]
        public async Task Pending()
        {
            using var c = new DatabaseConnection();
            var list = c.GetAllPending();

            if (!list.Any())
            {
                await Context.Channel.SendMessageAsync("There are no pending applications at the moment.");
            }
            else
            {
                var s = list.Aggregate("Pending applications:\n-------------------------------------------\n", 
                    (current, app) => current + $"[{app.AppId}] {app.AuthorName}; {app.AppTime}\n\n");

                await Context.Channel.SendMessageAsync($"```{s}```");
            }
        }
        
        [Command("approved", RunMode = RunMode.Async)]
        public async Task Approved()
        {
            using var c = new DatabaseConnection();
            var list = c.GetLast20Approved();

            if (!list.Any())
            {
                await Context.Channel.SendMessageAsync("You haven't approved any applications yet.");
            }
            else
            {
                var s = list.Aggregate("Last 20 approved applications:\n-------------------------------------------\n", 
                    (current, app) => current + $"[{app.AppId}] {app.AuthorName}; {app.AppTime}\n\n");

                await Context.Channel.SendMessageAsync($"```{s}```");
            }
        }
        
        [Command("rejected", RunMode = RunMode.Async)]
        public async Task Rejected()
        {
            using var c = new DatabaseConnection();
            var list = c.GetLast20Rejected();

            if (!list.Any())
            {
                await Context.Channel.SendMessageAsync("You haven't rejected any applications yet.");
            }
            else
            {
                var s = list.Aggregate("Last 20 rejected applications:\n-------------------------------------------\n", 
                    (current, app) => current + $"[{app.AppId}] {app.AuthorName}; {app.AppTime}\n\n");

                await Context.Channel.SendMessageAsync($"```{s}```");
            }
        }
        
        [Command("view", RunMode = RunMode.Async)]
        public async Task View(int applicationId)
        {
            using var c = new DatabaseConnection();
            var app = c.GetById(applicationId);

            if (app is null)
            {
                await Context.Channel.SendMessageAsync($":x: Application with ID `{applicationId}` does not exist.");
                    
                return;
            }
                
            var b = new EmbedBuilder();
            b.AddField($"{app.AppStatus.ToString().ToUpper()}: Application by {app.AuthorName}", $"Author's Discord ID: {app.AuthorDiscordId}\nApplication ID: {app.AppId}");
            b.AddField("Provided details", app.MessageContent ?? "*No details provided*");
            b.AddField("Link to original message", app.MessageUrl);
            b.WithThumbnailUrl(app.ImageUrl);
            b.WithFooter($"Applied at {app.AppTime}");
                
            if (app.AppStatus == ApplicationStatus.Approved)
            {
                b.WithColor(Color.Green);
            } 
            else if (app.AppStatus == ApplicationStatus.Rejected)
            {
                b.WithColor(Color.Red);
            }
            else
            {
                b.WithColor(Color.Blue);
            }
                
            await Context.Channel.SendMessageAsync("", false, b.Build());
        }

        #endregion
        
        #region Staff Only

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

        #endregion
    }
}