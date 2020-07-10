using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModdedMinecraftClub.MemberBot.Bot.Services.Regular;

namespace ModdedMinecraftClub.MemberBot.Bot.Services.Hosted
{
    public interface IStartupChecksService : IHostedService
    {
        
    }
    
    public class StartupChecksHostedService : IStartupChecksService
    {
        private readonly ILogger<StartupChecksHostedService> _logger;
        private readonly IDatabaseConnectionService _databaseConnection;
        private readonly IHostApplicationLifetime _applicationLifetime;

        public StartupChecksHostedService(
            ILogger<StartupChecksHostedService> logger,
            IDatabaseConnectionService databaseConnection,
            IHostApplicationLifetime applicationLifetime
            )
        {
            _logger = logger;
            _databaseConnection = databaseConnection;
            _applicationLifetime = applicationLifetime;
        }

        private async Task CreateTableAsync()
        {
            _logger.LogWarning("Couldn't find the table. Creating...");

            await _databaseConnection.CreateTableAsync();
            
            _logger.LogInformation("Successfully created the table. Starting the bot...");
        }

        private async Task RunDbCheckAsync()
        {
            _logger.LogInformation("MMCC Member Bot");
            _logger.LogInformation("Checking if \"applications\" table exists...");

            try
            {
                var exists = await _databaseConnection.DoesTableExistAsync();

                if (!exists)
                {
                    await CreateTableAsync();
                }
                else
                {
                    _logger.LogInformation("Found the table. Starting the bot...");
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Database error. Exiting...");
                
                _applicationLifetime.StopApplication();
            }
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
            => await RunDbCheckAsync();

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}