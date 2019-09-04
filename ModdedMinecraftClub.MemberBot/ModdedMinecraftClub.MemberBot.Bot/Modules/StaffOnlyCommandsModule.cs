using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class StaffOnlyCommandsModule : ModuleBase<SocketCommandContext>
    {
        [Command("approve", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Approve(int applicationId)
        {
            using (var c = new DatabaseConnection())
            {
                var app = c.GetById(applicationId);

                if (app is null)
                {
                    await Context.Channel.SendMessageAsync($":x: Application with ID `{applicationId}` does not exist.");
                    
                    return;
                }
                
                c.MarkAsApproved(applicationId);

                await Context.Channel.SendMessageAsync($":white_check_mark: **Approved** application with ID: `{applicationId}`");
            }
        }
        
        [Command("reject", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Reject(int applicationId)
        {
            using (var c = new DatabaseConnection())
            {
                var app = c.GetById(applicationId);

                if (app is null)
                {
                    await Context.Channel.SendMessageAsync($":x: Application with ID `{applicationId}` does not exist.");
                    
                    return;
                }
                
                c.MarkAsRejected(applicationId);

                await Context.Channel.SendMessageAsync($":white_check_mark: **Rejected** application with ID `{applicationId}`");
            }
        }
    }
}