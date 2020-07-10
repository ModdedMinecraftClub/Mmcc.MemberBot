using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ModdedMinecraftClub.MemberBot.Bot.Services.Hosted
{
    public interface IStartupChecksService : IHostedService
    {
        
    }
    
    public class StartupChecksChecksHostedService : IStartupChecksService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}