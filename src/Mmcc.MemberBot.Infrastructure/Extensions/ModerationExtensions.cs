using System;
using Mmcc.MemberBot.Core.Models;
using Mmcc.MemberBot.Core.Protos;

namespace Mmcc.MemberBot.Infrastructure.Extensions
{
    public static class ModerationExtensions
    {
        public static ModerationCommand ToModerationCommand(this ModerationEvent moderationEvent)
        {
            var type = moderationEvent.Type switch
            {
                ModerationEventType.Warn => ModerationCommand.Types.ModerationCommandType.Warn,
                ModerationEventType.Ban => ModerationCommand.Types.ModerationCommandType.Ban,
                _ => ModerationCommand.Types.ModerationCommandType.Unknown
            };

            return new ModerationCommand
            {
                Type = type,
                Reason = moderationEvent.Reason,
                Username = moderationEvent.Ign
            };
        }
    }
}