using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    public class ChannelFinder
    {
        private readonly ConfigRoot _config;
        private readonly IReadOnlyCollection<SocketTextChannel> _textChannels;

        public ChannelFinder(SocketCommandContext context, ConfigRoot config)
        {
            _config = config;
            _textChannels = context.Guild.TextChannels;
        }

        public SocketTextChannel FindPolychatChannel()
        {
            var polychatChannel = _textChannels.First(channel => channel.Name.Equals(_config.Discord.ChannelNames.Polychat));

            return polychatChannel;
        }

        public SocketTextChannel FindMemberAppsChannel()
        {
            var membersChannel = _textChannels.First(channel => channel.Name.Equals(_config.Discord.ChannelNames.MemberApps));

            return membersChannel;
        }
    }
}