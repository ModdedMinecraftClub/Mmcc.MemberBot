using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Google.Protobuf.WellKnownTypes;
using Mmcc.MemberBot.Core.Interfaces;
using Mmcc.MemberBot.Core.Models;
using Mmcc.MemberBot.Infrastructure.Extensions;

namespace Mmcc.MemberBot.Infrastructure.Services
{
    public class ModerationService : IModerationService
    {
        private readonly ITcpCommunicationService _tcpCommunicationService;

        public ModerationService(ITcpCommunicationService tcpCommunicationService)
        {
            _tcpCommunicationService = tcpCommunicationService;
        }
        
        public async Task<TimeSpan?> HandleDuration(ISocketMessageChannel channel, string duration)
        {
            var durationParseable = DurationConverter.TryParseDurationString(duration, out var timeSpan);
            
            // ReSharper disable once InvertIf
            if (!durationParseable)
            {
                var eb = new ErrorEmbedBuilder()
                    .WithStandardErrorEmbedLayout()
                    .WithErrorMessage("Cannot parse duration.")
                    .WithHowToDealWithThisErrorField(
                        "Use correct duration string, for example `1s`, `1m`, `1h`, `10d`");
                await channel.SendEmbedAsync(eb.Build());
            }

            return timeSpan;
        }

        public async Task SendInGameCommand(ModerationEvent moderationEvent)
        {
            var command = moderationEvent.ToModerationCommand();
            var packed = Any.Pack(command);
            
            await _tcpCommunicationService.SendMessage(packed);
        }

        public async Task SendDmNotification(IUser user, ModerationEvent moderationEvent)
        {
            var eb = new EmbedBuilder()
                .WithColor(Color.Red);

            switch (moderationEvent.Type)
            {
                case ModerationEventType.Ban:
                    eb.WithTitle("You have been banned from Modded Minecraft Club");
                    break;
                case ModerationEventType.Mute:
                    eb.WithTitle("You have been muted in Modded Minecraft Club");
                    break;
                case ModerationEventType.Warn:
                    eb.WithTitle("You have been warned in Modded Minecraft Club");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(moderationEvent));
            }

            eb.AddField("Reason", moderationEvent.Reason);
            eb.AddField("Expiry date",
                moderationEvent.ExpiryDateUtc is null ? "Permanent" : moderationEvent.ExpiryDateUtc.ToString());

            await user.SendMessageAsync("", false, eb.Build());
        }

        public async Task SendSuccessStaffNotification(ISocketMessageChannel channel, ModerationEvent moderationEvent)
        {
            var eb = new EmbedBuilder()
                .WithTitle("Moderation operation succeeded")
                .WithColor(Color.Green)
                .AddField("Type", moderationEvent.Type.ToString().ToUpper())
                .AddField("Expiry date",
                    moderationEvent.ExpiryDateUtc is null ? "Permanent" : moderationEvent.ExpiryDateUtc.ToString());

            if (moderationEvent.DiscordId is not null)
            {
                eb.AddField("Discord ID", moderationEvent.DiscordId);
            }

            if (moderationEvent.Ign is not null)
            {
                eb.AddField("IGN", moderationEvent.Ign);
            }

            eb.AddField("Reason", moderationEvent.Reason);

            await channel.SendEmbedAsync(eb.Build());
        }
    }
}