using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using MediatR;
using Mmcc.MemberBot.Core;
using Mmcc.MemberBot.Core.Models;

namespace Mmcc.MemberBot.Infrastructure.Commands.Applications
{
    public class CreateFromUserMsg
    {
        public class Command : IRequest
        {
            public IUserMessage UserMessage { get; set; } = null!;
        }
    
        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly MemberBotContext _context;

            public Handler(MemberBotContext context)
            {
                _context = context;
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var app = new Application
                {
                    AppStatus = ApplicationStatus.Pending,
                    AppTime = request.UserMessage.Timestamp.ToString(),
                    AuthorName = request.UserMessage.Author.ToString(),
                    AuthorDiscordId = request.UserMessage.Author.Id,
                    MessageContent = request.UserMessage.Content,
                    MessageUrl = request.UserMessage.GetJumpUrl(),
                    ImageUrl = request.UserMessage.Attachments.First().Url
                };

                await _context.Applications.AddAsync(app, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}