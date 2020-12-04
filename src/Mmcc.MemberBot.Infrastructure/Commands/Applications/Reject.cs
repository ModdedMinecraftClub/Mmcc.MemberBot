using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Mmcc.MemberBot.Core;
using Mmcc.MemberBot.Core.Models;

namespace Mmcc.MemberBot.Infrastructure.Commands.Applications
{
    public class Reject
    {
        public class Command : IRequest<CommandResult<Application>>
        {
            public int ApplicationId { get; set; }
        }
        
        public class Handler : IRequestHandler<Command, CommandResult<Application>>
        {
            private readonly MemberBotContext _context;

            public Handler(MemberBotContext context)
            {
                _context = context;
            }

            public async Task<CommandResult<Application>> Handle(Command req, CancellationToken cancellationToken)
            {
                var app = await _context.Applications.FirstOrDefaultAsync(a => a.AppId == req.ApplicationId,
                    cancellationToken);
                if (app is null)
                    return new CommandResult<Application>($"Application with ID `{req.ApplicationId}` does not exist.");

                app.AppStatus = ApplicationStatus.Approved;
                await _context.SaveChangesAsync(cancellationToken);
                return new CommandResult<Application>(app);
            }
        }
    }
}