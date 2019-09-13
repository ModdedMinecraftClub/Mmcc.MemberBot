using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using ConsoleTables;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hangfire;
using ModdedMinecraftClub.MemberBot.Bot.Jobs;
using ModdedMinecraftClub.MemberBot.Bot.Jobs.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class StaffOnlyCommandsModule : ModuleBase<SocketCommandContext>
    {
        #region Member Applications
        
        [Command("approve", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Approve(int applicationId, string serverPrefix, string ign)
        {
            using (var c = new DatabaseConnection())
            {
                var app = c.GetById(applicationId);

                if (app is null)
                {
                    await Context.Channel.SendMessageAsync(
                        $":x: Application with ID `{applicationId}` does not exist.");

                    return;
                }

                var channels = Context.Guild.TextChannels;
                var roles = Context.Guild.Roles;
                var users = Context.Guild.Users;
                var polychatChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.PolychatInteractionChannel));
                var membersChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.MemberAppsChannelName));
                var memberRole = roles.First(role => role.Name.Contains($"[{serverPrefix.ToUpper()}]"));
                var userToPromote = users.First(user => user.Id.Equals(app.AuthorDiscordId));

                await userToPromote.AddRoleAsync(memberRole);
                
                await polychatChannel.SendMessageAsync($"!promote {serverPrefix} {ign}");
                
                c.MarkAsApproved(applicationId, serverPrefix);

                await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}> Congratulations, your application has been approved.");
                
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
                
                var channels = Context.Guild.TextChannels;
                var membersChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.MemberAppsChannelName));
                
                c.MarkAsRejected(applicationId);
                
                await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}> Unfortunately, your application has been rejected.");
                
                await Context.Channel.SendMessageAsync($":white_check_mark: **Rejected** application with ID `{applicationId}`");
            }
        }
        
        #endregion

        #region Hangfire

        [Command("jobs", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Jobs(string arg)
        {
            arg = arg.ToLower();
            
            var easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            
            if (arg.Equals(JobStatus.Scheduled.ToString().ToLower()))
            {
                using (var c = new DatabaseConnection())
                {
                    var table = new ConsoleTable("Id", "Method", "Created At", "Scheduled For");
                    var jobDtos = c.GetJobs(JobStatus.Scheduled);
                    var jobs = new List<ScheduledJob>();

                    foreach (var jobDto in jobDtos)
                    {
                        var job = new ScheduledJob(jobDto);
                        job.Parse();
                        jobs.Add(job);
                    }

                    foreach (var job in jobs)
                    {
                        table.AddRow(job.Id, job.Method,
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.CreatedAt, easternZone)} EST",
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.EnqueueAt, easternZone)} EST");
                    }

                    await Context.Channel.SendMessageAsync($"```json\n{table.ToMinimalString()}\n```");
                }
            }
            else if (arg.Equals(JobStatus.Succeeded.ToString().ToLower()) || arg.Equals("completed"))
            {
                using (var c = new DatabaseConnection())
                {
                    var table = new ConsoleTable("Id", "Method", "Created At", "Completed At");
                    var jobDtos = c.GetJobs(JobStatus.Succeeded);
                    var jobs = new List<SucceededJob>();

                    foreach (var jobDto in jobDtos)
                    {
                        var job = new SucceededJob(jobDto);
                        job.Parse();
                        jobs.Add(job);
                    }

                    foreach (var job in jobs)
                    {
                        table.AddRow(job.Id, job.Method,
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.CreatedAt, easternZone)} EST",
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.SucceededAt, easternZone)} EST");
                    }

                    await Context.Channel.SendMessageAsync($"```json\n{table.ToMinimalString()}\n```");
                }
            }
            else if (arg.Equals(JobStatus.Failed.ToString().ToLower()))
            {
                using (var c = new DatabaseConnection())
                {
                    var table = new ConsoleTable("Id", "Method", "Created At");
                    var jobDtos = c.GetJobs(JobStatus.Succeeded);
                    var jobs = new List<FailedJob>();

                    foreach (var jobDto in jobDtos)
                    {
                        var job = new FailedJob(jobDto);
                        job.Parse();
                        jobs.Add(job);
                    }

                    foreach (var job in jobs)
                    {
                        table.AddRow(job.Id, job.Method,
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.CreatedAt, easternZone)} EST");
                    }

                    await Context.Channel.SendMessageAsync($"```json\n{table.ToMinimalString()}\n```");
                }
            }
        }

        [Command("deletejob", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task DeleteJob(string jobId)
        {
            BackgroundJob.Delete(jobId);

            await Context.Channel.SendMessageAsync($":white_check_mark: Successfully deleted the job with id `{jobId}`");
        }

        #endregion

        #region Vip

        [Command("vip", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Vip(string ign, SocketGuildUser guildUser)
        {
            var channels = Context.Guild.TextChannels;
            var polychatChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.PolychatInteractionChannel));
            
            // Give VIP on all servers
            await polychatChannel.SendMessageAsync($"!exec <all> ranks set {ign} vip");
            
            // Give VIP role on Discord
            var roles = Context.Guild.Roles;
            var vipRole = roles.First(r => r.Name.Equals("Vip"));
            await guildUser.AddRoleAsync(vipRole);

            var dateNow = DateTime.UtcNow;
            var vipExpiry = dateNow.AddMonths(1);
            
            // Schedule a job that will take VIP away
            BackgroundJob.Schedule(() => TakeVipAwayJob.Execute(guildUser.Id, Context.Guild.Id, ign), vipExpiry);
            
            // Info message
            await Context.Channel.SendMessageAsync($":white_check_mark: Gave VIP to {ign} and scheduled a VIP job for next month.");
        }
        
        #endregion
    }
}