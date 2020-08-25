using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Discord;
using Discord.WebSocket;
using MediatR;
using Mmcc.MemberBot.Core.Models;
using MySqlConnector;

namespace Mmcc.MemberBot.Infrastructure.Commands.Applications
{
    public class CreateNewApplication
    {
        public class Command : IRequest
        {
            public SocketUserMessage UserMessage { get; set; }
        }

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly MySqlConnection _connection;

            public Handler(MySqlConnection connection)
            {
                _connection = connection;
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                // create new Application from the user msg;
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
                
                // insert into db;
                const string sql =
                    @"INSERT INTO applications (AppStatus, AppTime, AuthorName, AuthorDiscordId, MessageContent, MessageUrl, ImageUrl) 
                      VALUES (@AppStatus, @AppTime, @AuthorName, @AuthorDiscordId, @MessageContent, @MessageUrl, @ImageUrl)";
                await _connection.ExecuteAsync(sql, app);
            }
        }
    }
}