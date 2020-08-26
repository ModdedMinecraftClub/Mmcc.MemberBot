using System;
using Discord;
using Mmcc.MemberBot.Infrastructure.Extensions;

namespace Mmcc.MemberBot.Infrastructure
{
    public class ErrorEmbedBuilder : EmbedBuilder
    {
        public ErrorEmbedBuilder WithStandardErrorEmbedLayout()
        {
            WithTitle(":x: Error")
                .WithDescription("An error has occurred while executing the command.")
                .WithColor(Discord.Color.Red)
                .WithMmccLogo();
            return this;
        }

        public ErrorEmbedBuilder WithErrorMessage(string message)
        {
            AddField(":information_source: Message", message);
            return this;
        }

        public ErrorEmbedBuilder WithException(Exception e)
        {
            AddField(":exclamation: Exception", $"{e.GetType().Name}: {e.Message}");
            return this;
        }

        public ErrorEmbedBuilder WithHowToDealWithThisErrorField(string instructions)
        {
            AddField(":grey_question: How to deal with this error?", instructions);
            return this;
        }
    }
}