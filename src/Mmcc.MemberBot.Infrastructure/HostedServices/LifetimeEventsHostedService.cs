using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Mmcc.MemberBot.Infrastructure.HostedServices
{
    public class LifetimeEventsHostedService : IHostedService
    {
        private readonly ILogger<LifetimeEventsHostedService> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly TcpClient _tcpClient;

        public LifetimeEventsHostedService(
            ILogger<LifetimeEventsHostedService> logger,
            IHostApplicationLifetime applicationLifetime,
            TcpClient tcpClient
            )
        {
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _tcpClient = tcpClient;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _applicationLifetime.ApplicationStopped.Register(OnStopped);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnStopped()
        {
            _logger.LogInformation("TcpClient: Disconnecting");
            _tcpClient.Dispose();
            _logger.LogInformation("TcpClient: Disconnected");
        }
    }
}