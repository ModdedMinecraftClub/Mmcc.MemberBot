using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Mmcc.MemberBot.Core.Extensions;
using Mmcc.MemberBot.Core.Models.Settings;

namespace Mmcc.MemberBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILogger<HelpModule> _logger;
        private readonly char _prefix;
        private readonly CommandService _commandService;
        
        public HelpModule(DiscordSettings config, CommandService commandService, ILogger<HelpModule> logger)
        {
            _commandService = commandService;
            _logger = logger;
            _prefix = config.Prefix;
        }
        
        [Command("help", RunMode = RunMode.Async)]
        public async Task HelpAsync()
        {
            var embedBuilder = new EmbedBuilder()
                .WithTitle("Help")
                .WithDescription("MMCC Member Bot Help")
                .WithColor(Color.Blue)
                .WithMmccLogo();
            
            var commands = _commandService.Commands
                .Where(command => !command.Name.Equals("help") && !string.IsNullOrWhiteSpace(command.Summary));

            foreach (var command in commands)
            {
                var parametersStringBuilder = new StringBuilder();

                foreach (var parameter in command.Parameters)
                {
                    parametersStringBuilder.Append($"<{parameter.Name}> ");
                }
                
                embedBuilder.AddField($"{_prefix}{command.Name} {parametersStringBuilder.ToString()}", command.Summary);
            }

            await Context.Channel.SendEmbedAsync(embedBuilder.Build());
        }
    }
}