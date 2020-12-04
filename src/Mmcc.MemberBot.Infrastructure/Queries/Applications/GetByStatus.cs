using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.MemberBot.Core;
using Mmcc.MemberBot.Core.Models;

namespace Mmcc.MemberBot.Infrastructure.Queries.Applications
{
    public class GetByStatus
    {
        public class Query : IRequest<IList<Application>>
        {
            public ApplicationStatus Status { get; set; }
            public int? Limit { get; set; }
        }
        
        public class Handler : IRequestHandler<Query, IList<Application>>
        {
            private readonly MemberBotContext _context;

            public Handler(MemberBotContext context)
            {
                _context = context;
            }

            public async Task<IList<Application>> Handle(Query request, CancellationToken cancellationToken)
            {
                var data = _context.Applications
                    .AsQueryable()
                    .Where(a => a.AppStatus == request.Status)
                    .AsNoTracking();

                if (request.Limit is not null)
                {
                    return await data
                        .OrderByDescending(x => x.AppId)
                        .Take((int)request.Limit)
                        .ToListAsync(cancellationToken);
                }

                return await data.ToListAsync(cancellationToken);
            }
        }
    }
}