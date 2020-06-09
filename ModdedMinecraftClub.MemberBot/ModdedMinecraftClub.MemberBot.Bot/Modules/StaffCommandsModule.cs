using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ModdedMinecraftClub.MemberBot.Bot.Database;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    public class StaffCommandsModule : ModuleBase<SocketCommandContext>
    {
        private readonly DatabaseConnection _db;
        private readonly ConfigRoot _config;

        public StaffCommandsModule(ConfigRoot configRoot, DatabaseConnection db)
        {
            _db = db;
            _config = configRoot;
        }
        
        [Command("approve", RunMode = RunMode.Async)]
        [Priority(1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Approve(int applicationId, string serverPrefix, string ign)
        {
            var app = await _db.GetByIdAsync(applicationId);

            if (app is null)
            {
                await Context.Channel.SendMessageAsync(
                    $":x: Application with ID `{applicationId}` does not exist.");

                return;
            }
            
            var channelFinder = new ChannelFinder(Context, _config);
            var userRoleFinder = new UserRoleFinder(Context);
            var polychatChannel = channelFinder.FindPolychatChannel();
            var membersChannel = channelFinder.FindMemberAppsChannel();
            var memberRole = userRoleFinder.FindMemberRole(serverPrefix);

            if (memberRole is null)
            {
                await Context.Channel.SendMessageAsync($":x: Prefix `{serverPrefix}` does not exist.");
                    
                return;
            }

            var userToPromote = userRoleFinder.FindMemberAppAuthor(app);

            if (userToPromote is null)
            {
                await Context.Channel.SendMessageAsync($":x: Cannot find a user with ID `{app.AuthorDiscordId}`. Promote manually or reject the application.");

                return;
            }
                
            await userToPromote.AddRoleAsync(memberRole);
            await polychatChannel.SendMessageAsync($"!promote {serverPrefix} {ign}");
            await _db.MarkAsApprovedAsync(applicationId);

            var embed = BuildApprovedEmbed();

            await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}>", false, embed);
            await Context.Channel.SendMessageAsync($":white_check_mark: **Approved** application with ID: `{applicationId}`");
        }
        
        [Command("approve", RunMode = RunMode.Async)]
        [Priority(-1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Approve(int applicationId, string arg)
        {
            if (arg.Equals("manual"))
            {
                await _db.MarkAsApprovedAsync(applicationId);
                await Context.Channel.SendMessageAsync($":white_check_mark: **Marked** application with ID `{applicationId}` as approved but the player still has to be promoted manually.\nRemember to let the player know once you have promoted them manually.");
            } 
            else
            {
                await Context.Channel.SendMessageAsync($":x: Argument {arg} not recognized.");
            }
        }
        
        [Command("reject", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Reject(int applicationId, [Remainder]string reason)
        {
            var app = await _db.GetByIdAsync(applicationId);

            if (app is null)
            {
                await Context.Channel.SendMessageAsync($":x: Application with ID `{applicationId}` does not exist.");
                    
                return;
            }
            
            var channelFinder = new ChannelFinder(Context, _config);
            var membersChannel = channelFinder.FindMemberAppsChannel();
                
            await _db.MarkAsRejectedAsync(applicationId);

            var resultEmbed = BuildRejectedEmbed(reason);

            await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}>", false, resultEmbed);
            await Context.Channel.SendMessageAsync($":white_check_mark: **Rejected** application with ID `{applicationId}`");
        }
        
        /// <summary>
        /// Builds an embed that lets the player know their application has been approved
        /// </summary>
        /// <returns>Embed letting the player know their application has been approved</returns>
        private Embed BuildApprovedEmbed()
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle($"Application approved");
            embedBuilder.WithColor(Color.Green);
            embedBuilder.WithThumbnailUrl("https://www.moddedminecraft.club/data/icon.png");
            embedBuilder.WithDescription("Congratulations, your application has been approved.");
            embedBuilder.AddField("Approved by", Context.Message.Author);

            return embedBuilder.Build();
        }

        /// <summary>
        /// Builds an embed that lets the player know their application has been rejected
        /// </summary>
        /// <param name="reason">Reason for rejection</param>
        /// <returns>Embed letting the player know their application has been rejected</returns>
        private Embed BuildRejectedEmbed(string reason)
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.WithTitle($"Application rejected");
            embedBuilder.WithColor(Color.Red);
            embedBuilder.WithDescription("Unfortunately, your application has been rejected.");
            embedBuilder.WithThumbnailUrl("https://www.moddedminecraft.club/data/icon.png");
            embedBuilder.AddField("Reason", reason);
            embedBuilder.AddField("Rejected by", Context.Message.Author);

            return embedBuilder.Build();
        }
    }
}