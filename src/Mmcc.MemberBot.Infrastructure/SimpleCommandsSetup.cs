using System.Collections.Generic;
using Discord;
using Discord.Commands;
using Mmcc.MemberBot.Core.Extensions;
using Mmcc.MemberBot.Core.Models;

namespace Mmcc.MemberBot.Infrastructure
{
    public static class SimpleCommandsSetup
    {
        public static void AddSimpleCommands(this CommandService commandService, IList<SimpleCommand> simpleCommands)
        {
            commandService.CreateModuleAsync("", builder =>
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
                            .WithRunMode(RunMode.Async)
                            .WithName(cmd.Name)
                            .WithSummary(cmd.Summary);
                    });
                }
            });
        }
    }
}