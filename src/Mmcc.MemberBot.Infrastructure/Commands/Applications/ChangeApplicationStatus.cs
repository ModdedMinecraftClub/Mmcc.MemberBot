using System;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Mmcc.MemberBot.Core.Models;
using MySqlConnector;

namespace Mmcc.MemberBot.Infrastructure.Commands.Applications
{
    public class ChangeApplicationStatus
    {
        public class Command : IRequest
        {
            public int ApplicationId { get; set; }
            public ApplicationStatus ApplicationStatus { get; set; }
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
                    @"UPDATE applications 
                      SET AppStatus = @AppStatus
                      WHERE AppId = @AppId";
                var appStatus = Convert.ToInt32(request.ApplicationStatus);
                await _connection.ExecuteAsync(sql, new
                {
                    AppId = request.ApplicationId,
                    AppStatus = appStatus
                });
            }
        }
    }
}