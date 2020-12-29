using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using MediatR;
using Mmcc.MemberBot.Core;
using Mmcc.MemberBot.Core.Extensions;
using Mmcc.MemberBot.Core.Interfaces;
using Mmcc.MemberBot.Core.Models;
using Mmcc.MemberBot.Core.Models.Settings;
using Mmcc.MemberBot.Core.Protos;

namespace Mmcc.MemberBot.Infrastructure.Commands.Applications
{
    public class ApproveAutomatically
    {
        public class Command : IRequest<CommandResult<Application>>
        {
            public DiscordRestClient RestClient { get; set; } = null!;
            public SocketGuild Guild { get; set; } = null!;
            public int ApplicationId { get; set; }
            public string ServerPrefix { get; set; } = null!;
            public IList<string> Igns { get; set; } = null!;
        }

        public class Handler : IRequestHandler<Command, CommandResult<Application>>
        {
            private readonly MemberBotContext _context;
            private readonly DiscordSettings _config;
            private readonly ITcpCommunicationService _tcpCommunicationService;

            public Handler(
                MemberBotContext context,
                DiscordSettings config,
                ITcpCommunicationService tcpCommunicationService
                )
            {
                _context = context;
                _config = config;
                _tcpCommunicationService = tcpCommunicationService;
            }

            public async Task<CommandResult<Application>> Handle(Command req, CancellationToken cancellationToken)
            {
                var app = await _context.Applications
                    .FirstOrDefaultAsync(a => a.AppId == req.ApplicationId, cancellationToken);
                if (app is null)
                    return new CommandResult<Application>($"Application with ID `{req.ApplicationId}` does not exist.");

                var memberRole = req.Guild.Roles.FindMemberRole(req.ServerPrefix);
                if (memberRole is null)
                    return new CommandResult<Application>($"Cannot find a corresponding role for server prefix `{req.ServerPrefix}`.");

                var userToPromote = await req.RestClient.GetGuildUserAsync(req.Guild.Id, app.AuthorDiscordId);
                if (userToPromote is null)
                    return new CommandResult<Application>($"Cannot find a user with ID `{app.AuthorDiscordId}`.");

                foreach (var ign in req.Igns)
                {
                    var polychatCommand = new PromoteMemberCommand
                    {
                        ServerId = req.ServerPrefix,
                        Username = ign
                    };

                    try
                    {
                        await _tcpCommunicationService.SendProtobufMessage(polychatCommand);
                    }
                    catch (SocketException e)
                    {
                        return new CommandResult<Application>($"Failed to connect to Polychat Server.\n`{e.GetType()}: {e.Message}`");
                    }
                }
                
                await userToPromote.AddRoleAsync(memberRole);

                app.AppStatus = ApplicationStatus.Approved;
                await _context.SaveChangesAsync(cancellationToken);
                return new CommandResult<Application>(app);
            }
        }
    }
}