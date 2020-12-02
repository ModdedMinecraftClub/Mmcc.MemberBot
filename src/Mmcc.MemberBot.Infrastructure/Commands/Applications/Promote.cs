using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using MediatR;
using Mmcc.MemberBot.Core.Interfaces;
using Mmcc.MemberBot.Core.Models;
using Mmcc.MemberBot.Core.Protos;

namespace Mmcc.MemberBot.Infrastructure.Commands.Applications
{
    public class Promote
    {
        public class Command : IRequest
        {
            public string ServerPrefix { get; set; }
            public string Ign { get; set; }
            public int ApplicationId { get; set; }
            public IGuildUser UserToPromote { get; set; }
            public IRole MemberRole { get; set; }
        }

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly IMediator _mediator;
            private readonly ITcpCommunicationService _tcpService;

            public Handler(IMediator mediator, ITcpCommunicationService tcpService)
            {
                _mediator = mediator;
                _tcpService = tcpService;
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                // tell Polychat to promote in-game;
                var polychatCommand = new PromoteMemberCommand
                {
                    ServerId = request.ServerPrefix,
                    Username = request.Ign
                };
                await _tcpService.SendPromoteMemberCommand(polychatCommand);
            
                // add role;
                await request.UserToPromote.AddRoleAsync(request.MemberRole);
            
                // mark as approved;
                var dbCommand = new ChangeApplicationStatus.Command
                {
                    ApplicationId = request.ApplicationId,
                    ApplicationStatus = ApplicationStatus.Approved
                };
                await _mediator.Send(dbCommand, cancellationToken);
            }
        }
    }
}