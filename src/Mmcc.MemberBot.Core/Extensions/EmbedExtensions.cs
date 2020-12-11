﻿using System;
using System.Collections.Generic;
using Discord;
using Mmcc.MemberBot.Core.Embeds;
using Mmcc.MemberBot.Core.Models;

namespace Mmcc.MemberBot.Core.Extensions
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
    }
}