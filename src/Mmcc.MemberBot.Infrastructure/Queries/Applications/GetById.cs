using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Mmcc.MemberBot.Core;
using Mmcc.MemberBot.Core.Models;

namespace Mmcc.MemberBot.Infrastructure.Queries.Applications
{
    public class GetById
    {
        public class Query : IRequest<Application?>
        {
            public int Id { get; set; }
        }

        public class Handler : IRequestHandler<Query, Application?>
        {
            private readonly MemberBotContext _context;

            public Handler(MemberBotContext context)
            {
                _context = context;
            }

            public async Task<Application?> Handle(Query request, CancellationToken cancellationToken)
            {
                var res = await _context.Applications
                    .AsNoTracking()
                    .Where(x => x.AppId == request.Id)
                    .FirstOrDefaultAsync(cancellationToken);
                return res;
            }
        }
    }
}