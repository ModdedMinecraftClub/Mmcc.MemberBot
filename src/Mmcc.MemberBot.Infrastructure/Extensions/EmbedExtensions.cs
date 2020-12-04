using System;
using System.Collections.Generic;
using Discord;
using Mmcc.MemberBot.Core;
using Mmcc.MemberBot.Core.Models;

namespace Mmcc.MemberBot.Infrastructure.Extensions
{
    public static class EmbedExtensions
    {
        public static EmbedBuilder WithMmccLogo(this EmbedBuilder builder)
        {
            builder.WithThumbnailUrl("https://www.moddedminecraft.club/data/icon.png");
            return builder;
        }

        public static EmbedBuilder WithApplicationFields(this EmbedBuilder builder, IEnumerable<Application> applications)
        {
            foreach (var application in applications)
            {
                builder.AddField($"[{application.AppId}] {application.AuthorName}",
                    $"Submitted at: *{application.AppTime}*");
            }

            return builder;
        }

        public static EmbedBuilder WithApplicationStatusColour(this EmbedBuilder builder, ApplicationStatus status)
        {
            switch (status)
            {
                case ApplicationStatus.Pending:
                    builder.WithColor(Color.Blue);
                    break;
                case ApplicationStatus.Approved:
                    builder.WithColor(Color.Green);
                    break;
                case ApplicationStatus.Rejected:
                    builder.WithColor(Color.Red);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }

            return builder;
        }

        public static Embed ToErrorEmbed<T>(this CommandResult<T> result)
        {
            var eb = new ErrorEmbedBuilder()
                .WithStandardErrorEmbedLayout()
                .WithErrorMessage(result.FailureReason!);

            return eb.Build();
        }
    }
}