using System.Collections.Generic;
using System.Text;
using Discord;
using Mmcc.MemberBot.Infrastructure.Extensions;

namespace Mmcc.MemberBot.Infrastructure
{
    public class IncorrectArgsEmbedBuilder : EmbedBuilder
    {
        public IncorrectArgsEmbedBuilder WithStandardIncorrectArgsEmbedLayout()
        {
            WithTitle(":exclamation: Incorrect arguments")
                .WithColor(Discord.Color.Magenta)
                .WithMmccLogo();
            return this;
        }

        public IncorrectArgsEmbedBuilder WithUsageField(string usage)
        {
            AddField("Usage", $"`{usage}`");
            return this;
        }

        public IncorrectArgsEmbedBuilder WithAlternativeUsage(string altUsage)
        {
            AddField("Alternative usage", $"`{altUsage}`");
            return this;
        }

        public IncorrectArgsEmbedBuilder WithAlternativeUsages(IEnumerable<string> usages)
        {
            var sb = new StringBuilder();

            foreach (var usage in usages)
            {
                sb.AppendLine($"`{usage}`");
            }

            AddField("Alternative usages", sb.ToString());
            return this;
        }
    }
}