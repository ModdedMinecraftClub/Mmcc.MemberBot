using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using Mmcc.MemberBot.Core.Models;
using MySqlConnector;

namespace Mmcc.MemberBot.Infrastructure.Queries.Applications
{
    public class GetLastApplicationsByStatus
    {
        public class Query : IRequest<IEnumerable<Application>>
        {
            public ApplicationStatus ApplicationStatus { get; set; }
        }

        public class Handler : IRequestHandler<Query, IEnumerable<Application>>
        {
            private readonly MySqlConnection _connection;

            public Handler(MySqlConnection connection)
            {
                _connection = connection;
            }

            public async Task<IEnumerable<Application>> Handle(Query request, CancellationToken cancellationToken)
            {
                const string sql =
                    @"SELECT * FROM applications 
                      WHERE AppStatus = @AppStatus 
                      ORDER BY AppId DESC 
                      LIMIT 10";
                var status = Convert.ToInt32(request.ApplicationStatus);
                var res = await _connection.QueryAsync<Application>(sql, new {AppStatus = status});
                return res;
            }
        }
    }
}