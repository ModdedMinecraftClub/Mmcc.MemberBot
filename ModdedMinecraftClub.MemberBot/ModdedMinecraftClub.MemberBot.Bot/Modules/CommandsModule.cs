using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class CommandsModule : ModuleBase<SocketCommandContext>
    {
        [Command("test", RunMode = RunMode.Async)]
        public async Task Test()
        {
            using (var c = new DatabaseConnection())
            {
                await Context.Channel.SendMessageAsync(c.DoesTableExist().ToString());
            }
        }
    }
}