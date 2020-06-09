using System.Collections.Generic;
using System.Linq;
using Discord.Commands;
using Discord.WebSocket;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    public class UserRoleFinder
    {
        private readonly IReadOnlyCollection<SocketGuildUser> _users;
        private readonly IReadOnlyCollection<SocketRole> _roles;

        public UserRoleFinder(SocketCommandContext context)
        {
            _users = context.Guild.Users;
            _roles = context.Guild.Roles;
        }

        public SocketGuildUser FindMemberAppAuthor(Application app)
        {
            var userToPromote = _users.FirstOrDefault(user => user.Id == app.AuthorDiscordId);

            return userToPromote;
        }

        public SocketRole FindMemberRole(string serverPrefix)
        {
            var memberRole = _roles.FirstOrDefault(role => role.Name.Contains($"[{serverPrefix.ToUpper()}]"));

            return memberRole;
        }
    }
}