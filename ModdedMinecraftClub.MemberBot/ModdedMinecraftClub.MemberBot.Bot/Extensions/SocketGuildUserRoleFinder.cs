using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Extensions
{
    public interface ISocketGuildUserRoleFinder
    {
        SocketGuildUser FindMemberAppAuthor(Application app);
        SocketRole FindMemberRole(string serverPrefix);
    }
    
    public class SocketGuildUserRoleFinder : ISocketGuildUserRoleFinder
    {
        private readonly IReadOnlyCollection<SocketGuildUser> _users;
        private readonly IReadOnlyCollection<SocketRole> _roles;

        public SocketGuildUserRoleFinder(IReadOnlyCollection<SocketGuildUser> users, IReadOnlyCollection<SocketRole> roles)
        {
            _users = users;
            _roles = roles;
        }

        public SocketGuildUser FindMemberAppAuthor(Application app)
            => _users.FirstOrDefault(user => user.Id == app.AuthorDiscordId);

        public SocketRole FindMemberRole(string serverPrefix)
            => _roles.FirstOrDefault(role => role.Name.Contains($"[{serverPrefix.ToUpper()}]"));
    }
}