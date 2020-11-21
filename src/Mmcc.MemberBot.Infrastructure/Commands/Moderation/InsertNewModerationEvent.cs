using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.MemberBot.Core.Models;

namespace Mmcc.MemberBot.Infrastructure.Commands.Moderation
{
    public class InsertNewModerationEvent
    {
        public class Command : IRequest
        {
            public ModerationEvent ModerationEvent { get; set; }
        }

        public class Handler : AsyncRequestHandler<Command>
        {
            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                throw new System.NotImplementedException();
            }
        }
    }
}