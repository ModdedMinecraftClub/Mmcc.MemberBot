using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace ModdedMinecraftClub.MemberBot.Bot.Services.Hosted
{
    public interface IBotService : IHostedService, IAsyncDisposable
    {       
        public DiscordSocketClient DiscordClient { get; }
        public void ExecuteHandlerAsyncronously<TReturn>(Func<DiscordSocketClient, Task<TReturn>> handler, Action<TReturn> callback);

    }
}