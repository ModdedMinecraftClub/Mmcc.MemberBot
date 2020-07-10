using Discord.WebSocket;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Extensions
{
    public interface ISocketGuildUserRoleFinder
    {
        SocketGuildUser FindMemberAppAuthor(Application app);
        SocketRole FindMemberRole(string serverPrefix);
    }
}