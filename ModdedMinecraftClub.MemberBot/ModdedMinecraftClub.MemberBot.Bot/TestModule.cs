using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    public class TestModule : ModuleBase<SocketCommandContext>
    {
        [Command("hello", RunMode = RunMode.Async)]
        public async Task Hello()
        {
            await Context.Channel.SendMessageAsync("hello");
        }
    }
}