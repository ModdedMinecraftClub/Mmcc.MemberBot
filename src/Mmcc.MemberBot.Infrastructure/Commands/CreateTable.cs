using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using MySqlConnector;

namespace Mmcc.MemberBot.Infrastructure.Commands
{
    public class CreateTable
    {
        public class Command : IRequest
        {
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
                const string sql =
                    "create table applications (\n\tAppId int not null auto_increment,\n\tAppStatus int not null,\n\tAppTime varchar(50) null,\n\tAuthorName varchar(50) not null,\n\tAuthorDiscordId long not null,\n\tMessageContent varchar(800) null,\n    MessageUrl varchar(250) not null,\n\tImageUrl varchar(250) null,\n\tprimary key (AppId)\n);";
                await _connection.ExecuteAsync(sql);
            }
        }
    }
}