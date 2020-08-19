using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Infrastructure.Commands;
using Mmcc.MemberBot.Infrastructure.Queries;

namespace Mmcc.MemberBot.Infrastructure.HostedServices
{
    public class StartupChecksHostedService : IHostedService
    {
        private readonly ILogger<StartupChecksHostedService> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IMediator _mediator;
        private readonly MySqlSettings _settings;

        public StartupChecksHostedService(
            ILogger<StartupChecksHostedService> logger,
            IHostApplicationLifetime applicationLifetime,
            IMediator mediator,
            MySqlSettings settings
            )
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _mediator = mediator;
            _settings = settings;
        }

        private async Task RunDbCheckAsync()
        {
            _logger.LogInformation("MMCC Member Bot");
            _logger.LogInformation("Checking if \"applications\" table exists...");

            try
            {
                var exists = await _mediator.Send(new DoesTableExist.Query {Name = _settings.DatabaseName});

                if (!exists)
                {
                    _logger.LogWarning("Couldn't find the table. Creating...");
                    await _mediator.Send(new CreateTable.Command());
                    _logger.LogInformation("Successfully created the table. Starting the bot...");
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