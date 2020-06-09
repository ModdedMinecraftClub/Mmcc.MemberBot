using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ModdedMinecraftClub.MemberBot.Bot.Database;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    public class RegularCommandsModule : ModuleBase<SocketCommandContext>
    {
        private readonly DatabaseConnection _db;

        public RegularCommandsModule(DatabaseConnection db)
        {
            _db = db;
        }
        
        [Command("pending", RunMode = RunMode.Async)]
        public async Task PendingAsync()
        {
            var list = (await _db.GetAllPendingAsync()).ToList();

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
        public async Task ApprovedAsync()
        {
            var list = (await _db.GetLast20ApprovedAsync()).ToList();

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
        public async Task RejectedAsync()
        {
            var list = (await _db.GetLast20RejectedAsync()).ToList();

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
        public async Task ViewAsync(int applicationId)
        {
            var app = await _db.GetByIdAsync(applicationId);

            if (app is null)
            {
                await Context.Channel.SendMessageAsync($":x: Application with ID `{applicationId}` does not exist.");
                    
                return;
            }

            var embed = BuildApplicationEmbed(app);
            
            await Context.Channel.SendMessageAsync("", false, embed);
        }
        
        /// <summary>
        /// Builds an embed for an application
        /// </summary>
        /// <param name="app">Application</param>
        /// <returns>Embed representing an application</returns>
        private Embed BuildApplicationEmbed(Application app)
        {
            var embedBuilder = new EmbedBuilder();
            embedBuilder.AddField($"{app.AppStatus.ToString().ToUpper()}: Application by {app.AuthorName}", $"Author's Discord ID: {app.AuthorDiscordId}\nApplication ID: {app.AppId}");
            
            if (app.MessageContent is null || app.MessageContent.Equals(""))
            {
                embedBuilder.AddField("Provided details","*Player did not provide any details.*");
            }
            else
            {
                embedBuilder.AddField("Provided details", app.MessageContent);
            }

            embedBuilder.AddField("Link to original message", app.MessageUrl);
            embedBuilder.WithThumbnailUrl(app.ImageUrl);
            embedBuilder.WithFooter($"Applied at {app.AppTime}");
                
            switch (app.AppStatus)
            {
                case ApplicationStatus.Approved:
                    embedBuilder.WithColor(Color.Green);
                    break;
                case ApplicationStatus.Rejected:
                    embedBuilder.WithColor(Color.Red);
                    break;
                default:
                    embedBuilder.WithColor(Color.Blue);
                    break;
            }

            return embedBuilder.Build();
        }
    }
}