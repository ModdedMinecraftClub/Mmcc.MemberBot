using System.Collections.Generic;
using System.Linq;
using Discord.WebSocket;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Extensions
{
    public interface ISocketTextChannelsFinder
    {
        SocketTextChannel FindPolychatChannel();
        SocketTextChannel FindMemberAppsChannel();
    }
    
    public class SocketTextChannelsFinder : ISocketTextChannelsFinder
    {
        private readonly BotSettings _config;
        private readonly IReadOnlyCollection<SocketTextChannel> _textChannels;

        public SocketTextChannelsFinder(IReadOnlyCollection<SocketTextChannel> textChannels, BotSettings config)
        {
            _config = config;
            _textChannels = textChannels;
        }

        public SocketTextChannel FindPolychatChannel() 
            => _textChannels.First(channel => channel.Name.Equals(_config.Discord.ChannelNames.Polychat));

        public SocketTextChannel FindMemberAppsChannel()
            => _textChannels.First(channel => channel.Name.Equals(_config.Discord.ChannelNames.MemberApps));
    }
}