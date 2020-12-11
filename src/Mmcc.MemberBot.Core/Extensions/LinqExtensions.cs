using System.Collections.Generic;
using System.Linq;

namespace Mmcc.MemberBot.Core.Extensions
{
    public static class LinqExtensions
    {
        public static bool IsEmpty<TSource>(this IEnumerable<TSource> source)
            => !source.Any();
    }
}