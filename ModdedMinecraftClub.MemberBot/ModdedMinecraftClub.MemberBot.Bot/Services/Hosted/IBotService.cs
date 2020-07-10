using System;
using Microsoft.Extensions.Hosting;

namespace ModdedMinecraftClub.MemberBot.Bot.Services.Hosted
{
    public interface IBotService : IHostedService, IAsyncDisposable
    {
        
    }
}