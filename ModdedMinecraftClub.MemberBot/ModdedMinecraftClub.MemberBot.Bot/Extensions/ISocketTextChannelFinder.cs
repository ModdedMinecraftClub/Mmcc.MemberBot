using Discord.WebSocket;

namespace ModdedMinecraftClub.MemberBot.Bot.Extensions
{
    public interface ISocketTextChannelFinder
    {
        SocketTextChannel FindPolychatChannel();
        SocketTextChannel FindMemberAppsChannel();
    }
}