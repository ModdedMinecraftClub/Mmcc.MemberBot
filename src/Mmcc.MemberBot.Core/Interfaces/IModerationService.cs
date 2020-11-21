using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Mmcc.MemberBot.Core.Models;

namespace Mmcc.MemberBot.Core.Interfaces
{
    public interface IModerationService
    {
        public Task SendInGameCommand(ModerationEvent moderationEvent);
        public Task SendDmNotification(IUser user, ModerationEvent moderationEvent);
        Task SendSuccessStaffNotification(ISocketMessageChannel channel, ModerationEvent moderationEvent);
        Task<TimeSpan?> HandleDuration(ISocketMessageChannel channel, string duration);
    }
}