﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Mmcc.MemberBot.Core.Models;

namespace Mmcc.MemberBot.Infrastructure.Extensions
{
    /// <summary>
    /// Extension methods for Discord.NET
    /// </summary>
    public static class DiscordNetExtensions
    {
        /// <summary>
        /// Converts Discord.LogSeverity to Microsoft.Extensions.Logging.LogLevel
        /// </summary>
        /// <param name="severity">LogSeverity to convert</param>
        /// <returns>Converted LogLevel</returns>
        public static LogLevel ToLogLevel(this LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Debug,
                _ => LogLevel.Debug
            };
        }
        
        /// <summary>
        /// Finds an author of an application.
        ///
        /// Returns null if not found, throws an exception if more than one element is found.
        /// </summary>
        /// <param name="users">IEnumerable of users</param>
        /// <param name="app">Member application</param>
        /// <returns>Author of an application</returns>
        public static SocketGuildUser FindMemberAppAuthor(this IEnumerable<SocketGuildUser> users, ulong authorDiscordId)
            => users.SingleOrDefault(user => user.Id == authorDiscordId);
        
        /// <summary>
        /// Finds member role for a given MC server.
        ///
        /// Returns null if not found, throws an exception if more than one element is found.
        /// </summary>
        /// <param name="roles">IEnumerable of roles</param>
        /// <param name="serverPrefix">Prefix of the MC server</param>
        /// <returns>Member role for a given MC server</returns>
        public static SocketRole FindMemberRole(this IEnumerable<SocketRole> roles, string serverPrefix)
            => roles.SingleOrDefault(role => role.Name.Contains($"[{serverPrefix.ToUpper()}]"));

        /// <summary>
        /// Finds a channel by name.
        ///
        /// Throws an exception if no elements are found.
        /// </summary>
        /// <param name="textChannels">IEnumerable of text channels</param>
        /// <param name="channelName">Channel name</param>
        /// <returns>First channel with a given name</returns>
        public static SocketTextChannel FindChannel(this IEnumerable<SocketTextChannel> textChannels, string channelName)
            => textChannels.First(channel => channel.Name.Equals(channelName));
        
        /// <summary>
        /// Sends an Embed to this message channel.
        /// </summary>
        /// <param name="messageChannel">This message channel</param>
        /// <param name="embed">Embed to be sent</param>
        /// <returns>Sent message</returns>
        public static async Task<RestUserMessage> SendEmbedAsync(this ISocketMessageChannel messageChannel, Embed embed)
            => await messageChannel.SendMessageAsync("", false, embed);
    }
}