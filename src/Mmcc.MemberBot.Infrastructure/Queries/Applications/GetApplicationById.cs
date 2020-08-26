using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Mmcc.MemberBot.Core.Models;
using MySqlConnector;

namespace Mmcc.MemberBot.Infrastructure.Queries.Applications
{
    public class GetApplicationById
    {
        public class Query : IRequest<Application>
        {
            public int Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Application>
        {
            private readonly MySqlConnection _connection;

            public Handler(MySqlConnection connection)
            {
                _connection = connection;
            }

            public async Task<Application> Handle(Query request, CancellationToken cancellationToken)
            {
                const string sql =
                    @"SELECT * FROM applications 
                      WHERE AppId = @AppId";
                var applicationId = request.Id;
                var res = await _connection.QuerySingleOrDefaultAsync<Application>(sql, new {AppId = applicationId});
                return res;
            }
        }
    }
}