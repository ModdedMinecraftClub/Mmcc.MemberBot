using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Infrastructure.Extensions;

namespace Mmcc.MemberBot.Modules
{
    public class CoreModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<CoreModule> _logger;
        private readonly DiscordSettings _config;

        public CoreModule(ILogger<CoreModule> logger, DiscordSettings config)
        {
            _logger = logger;
            _config = config;
        }

        [Command("discord", RunMode = RunMode.Async)]
        [Summary("Gives an invite link to our server")]
        public async Task DiscordAsync()
        {
            await Context.Channel.SendMessageAsync(_config.InviteUrl);
        }

        [Command("member", RunMode = RunMode.Async)]
        [Summary("Gives info about the member role")]
        public async Task MemberAsync()
        {
            var membersChannel = Context.Guild.TextChannels.FindChannel(_config.ChannelNames.MemberApps);
            var staffRole = Context.Guild.Roles.FindRole(_config.RoleNames.Staff);
            
            if (staffRole is null)
            {
                _logger.LogError("Staff role not found");
                await Context.Channel.SendMessageAsync(":x: Error. Staff role can not be `null`.");
                return;
            }
            
            var embed = new EmbedBuilder()
                .WithTitle("How to obtain the Member role")
                .WithDescription(
                    "When first joining the server, players are given the Guest rank. Member is a rank that can be accessed for free by all players. To apply for the rank, complete the following steps:")
                .WithMmccLogo()
                .WithColor(Color.Blue)
                .AddField(
                    ":one: Read the requirements",
                    "Requirements for the Member role are available on our wiki:\nhttps://wiki.moddedminecraft.club/index.php?title=How_to_earn_the_Member_rank"
                    )
                .AddField(
                    ":two: Read the application format",
                    "When applying, to ensure that your application is processed swiftly, please follow the following application message format:\n" +
                    "```" +
                    "IGN: john01dav\nServer: Enigmatica 2: Expert" +
                    "```\n" +
                    "In addition to the above, please include a screenshot of the required setup with your message. " +
                    "The screenshot should be sent directly via Discord. Do not link a screenshot uploaded to a 3rd party service like gyazo or imgur. " +
                    "Both the information (in the required format), as well as the screenshot should be sent as a single Discord message, not as two separate messages. "
                    )
                .AddField(
                    ":three: Apply",
                    $"After you've familiarized yourself with the requirements and are reasonably sure you meet them, head over to <#{membersChannel.Id}> and apply! Remember about the correct format :wink:."
                    )
                .AddField(
                    ":four: Wait for reply",
                    "As soon as you post your application, the bot will let you know that it has been submitted (if it doesn't then most likely you didn't adhere to the application format). " +
                    $"Now all you have to do is wait for a <@&{staffRole.Id}> member to process your application. You will be pinged by the bot once it has been processed. You can track your application via this bot's commands. To obtain the ID do `{_config.Prefix}pending`. You can then view its status at any time by doing `{_config.Prefix}view <applicationId>`. You can see other available commands by doing `{_config.Prefix}help`.\n\n" +
                    $"*We try to process applications as quickly as possible. If you feel like your application has been missed (defined as pending for over 48h), please ping a <@&{staffRole.Id}> member.*"
                    )
                .Build();

            await Context.Channel.SendEmbedAsync(embed);
        }
    }
}