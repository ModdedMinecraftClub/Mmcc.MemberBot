using Discord;
using Mmcc.MemberBot.Core.Embeds;

namespace Mmcc.MemberBot.Core.Extensions
{
    public static class CommandResultExtensions
    {
        public static Embed ToErrorEmbed<T>(this CommandResult<T> result)
        {
            var eb = new ErrorEmbedBuilder()
                .WithErrorMessage(result.FailureReason!);

            return eb.Build();
        }
    }
}