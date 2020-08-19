using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using MySqlConnector;

namespace Mmcc.MemberBot.Infrastructure.Queries
{
    public class DoesTableExist
    {
        public class Query : IRequest<bool>
        {
            public string Name { get; set; }
        }

        public class Handler : IRequestHandler<Query, bool>
        {
            private readonly MySqlConnection _connection;

            public Handler(MySqlConnection connection)
            {
                _connection = connection;
            }

            public async Task<bool> Handle(Query request, CancellationToken cancellationToken)
            {
                const string sql =
                    @"SELECT count(*) 
                      FROM information_schema.TABLES 
                      WHERE (TABLE_SCHEMA = @name) 
                        AND (TABLE_NAME = 'applications');";
                var q = await _connection.QueryFirstOrDefaultAsync<int>(sql, new {name = request.Name});
                return q != 0;
            }
        }
    }
}