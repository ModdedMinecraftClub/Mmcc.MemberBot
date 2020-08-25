using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Mmcc.MemberBot.Core.Models;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Infrastructure.Extensions;

namespace Mmcc.MemberBot.Infrastructure
{
    public static class SimpleCommandsSetup
    {
        public static async Task AddSimpleCommands(this CommandService commandService, IList<SimpleCommand> simpleCommands)
        {
            await commandService.CreateModuleAsync("", builder =>
            {
                builder.WithSummary("Simple commands");

                foreach (var cmd in simpleCommands)
                {
                    builder.AddCommand(cmd.Name, async (context, _, __, ___) =>
                    {
                        var embedBuilder = new EmbedBuilder()
                            .WithTitle(cmd.Title)
                            .WithDescription(cmd.Description)
                            .WithMmccLogo()
                            .WithColor(Color.Blue);

                        if (!string.IsNullOrWhiteSpace(cmd.Link))
                        {
                            embedBuilder.AddField("Link", cmd.Link);
                        }
                        
                        await context.Channel.SendMessageAsync("", false, embedBuilder.Build());
                    }, commandBuilder =>
                    {
                        commandBuilder
                            .WithName(cmd.Name)
                            .WithSummary(cmd.Summary);
                    });
                }
            });
        }
    }
}