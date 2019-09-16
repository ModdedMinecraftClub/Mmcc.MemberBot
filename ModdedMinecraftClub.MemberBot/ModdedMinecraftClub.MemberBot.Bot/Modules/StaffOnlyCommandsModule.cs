using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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
        private readonly CultureInfo _enUs;
        private readonly TimeZoneInfo _est;

        public StaffOnlyCommandsModule()
        {
            _enUs = new CultureInfo("en-US");
            _est = TimeZoneInfo.FindSystemTimeZoneById(Program.Config.EstTimeZoneString);
        }
        
        #region Member Applications
        
        [Command("approve", RunMode = RunMode.Async)]
        [Priority(1)]
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
                var polychatChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.ChannelNames.Polychat));
                var membersChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.ChannelNames.MemberApps));

                var memberRole = roles.FirstOrDefault(role => role.Name.Contains($"[{serverPrefix.ToUpper()}]"));

                if (memberRole is null)
                {
                    await Context.Channel.SendMessageAsync($":x: Prefix `{serverPrefix}` does not exist.");
                    
                    return;
                }
                
                var userToPromote = users.FirstOrDefault(user => user.Id == app.AuthorDiscordId);

                if (userToPromote is null)
                {
                    await Context.Channel.SendMessageAsync($":x: Cannot find a user with ID `{app.AuthorDiscordId}`. Promote manually or reject the application.");

                    return;
                }
                
                await userToPromote.AddRoleAsync(memberRole);
                
                await polychatChannel.SendMessageAsync($"!promote {serverPrefix} {ign}");
                
                c.MarkAsApproved(applicationId);

                await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}> Congratulations, your application has been approved.");
                
                await Context.Channel.SendMessageAsync($":white_check_mark: **Approved** application with ID: `{applicationId}`");
            }
        }
        
        [Command("approve", RunMode = RunMode.Async)]
        [Priority(-1)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Approve(int applicationId, string arg)
        {
            if (arg.Equals("manual"))
            {
                using (var c = new DatabaseConnection())
                {
                    c.MarkAsApproved(applicationId);
                }
                
                await Context.Channel.SendMessageAsync($":white_check_mark: **Marked** application with ID `{applicationId}` as approved but the player still has to be promoted manually.\nRemember to let the player know once you have promoted them manually.");
            } else
            {
                await Context.Channel.SendMessageAsync($":x: Argument {arg} not recognized.");
            }
        }
        
        [Command("reject", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        public async Task Reject(int applicationId, [Remainder]string reason)
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
                var membersChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.ChannelNames.MemberApps));
                
                c.MarkAsRejected(applicationId);
                
                await membersChannel.SendMessageAsync($"<@{app.AuthorDiscordId}> Unfortunately, your application has been rejected.\nReason:\n```{reason}```");
                
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
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.CreatedAt, _est).ToString(_enUs)} EST",
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.EnqueueAt, _est).ToString(_enUs)} EST");
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
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.CreatedAt, _est).ToString(_enUs)} EST",
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.SucceededAt, _est).ToString(_enUs)} EST");
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
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.CreatedAt, _est).ToString(_enUs)} EST");
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
            var polychatChannel = channels.First(channel => channel.Name.Equals(Program.Config.Discord.ChannelNames.Polychat));
            
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

        #region Reminders
        
        [Command("remind", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Priority(1)]
        public async Task Remind(SocketGuildUser whoUser, string estDateTime, [Remainder]string content)
        {
            if (!Context.Channel.Name.Equals(Program.Config.Discord.ChannelNames.Reminders))
            {
                return;
            }
            
            if (estDateTime.Contains("in") && !(estDateTime.Contains("/")))
            {
                var dates = GetInDates(estDateTime);
                
                BackgroundJob.Schedule(() => ReminderJob.ExecuteUser(Context.Guild.Id, Context.Channel.Id, whoUser.Id, content), dates.UtcTime);

                await Context.Channel.SendMessageAsync($":white_check_mark: Created a reminder for <@{whoUser.Id}> for `{dates.EstTime.ToString(_enUs)} EST` (`{dates.UtcTime.ToString(_enUs)} UTC`).");

                return;
            }
            
            // exampleDateString = "05/09/2017 05:05 AM";
            const string format = "MM/dd/yyyy HH:mm tt";
            var parsedEstDate = DateTime.ParseExact(estDateTime, format, _enUs);
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(parsedEstDate, _est);

            BackgroundJob.Schedule(() => ReminderJob.ExecuteUser(Context.Guild.Id, Context.Channel.Id, whoUser.Id, content), utcDate);

            await Context.Channel.SendMessageAsync($":white_check_mark: Created a reminder for <@{whoUser.Id}> for `{parsedEstDate.ToString(_enUs)} EST` (`{utcDate.ToString(_enUs)} UTC`).");
        }
        
        [Command("remind", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [Priority(0)]
        public async Task Remind(SocketRole whoRole, string estDateTime, [Remainder]string content)
        {
            if (!Context.Channel.Name.Equals(Program.Config.Discord.ChannelNames.Reminders))
            {
                return;
            }
            
            if (estDateTime.Contains("in") && !(estDateTime.Contains("/")))
            {
                var dates = GetInDates(estDateTime);
                
                BackgroundJob.Schedule(() => ReminderJob.ExecuteRole(Context.Guild.Id, Context.Channel.Id, whoRole.Id, content), dates.UtcTime);

                await Context.Channel.SendMessageAsync($":white_check_mark: Created a reminder for {whoRole.Mention} for `{dates.EstTime.ToString(_enUs)} EST` (`{dates.UtcTime.ToString(_enUs)} UTC`).");

                return;
            }
            
            // exampleDateString = "05/09/2017 05:05 AM";
            const string format = "MM/dd/yyyy HH:mm tt";
            var parsedEstDate = DateTime.ParseExact(estDateTime, format, _enUs);
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(parsedEstDate, _est);

            BackgroundJob.Schedule(() => ReminderJob.ExecuteRole(Context.Guild.Id, Context.Channel.Id, whoRole.Id, content), utcDate);

            await Context.Channel.SendMessageAsync($":white_check_mark: Created a reminder for {whoRole.Mention} for `{parsedEstDate.ToString(_enUs)} EST` (`{utcDate.ToString(_enUs)} UTC`).");
        }

        private Dates GetInDates(string estDateTime)
        {
            var t = estDateTime[estDateTime.Length - 1];
            estDateTime = estDateTime.Remove(0, estDateTime.IndexOf(' ') + 1);
            estDateTime = estDateTime.Remove(estDateTime.Length - 1);
            var parsed = int.Parse(estDateTime);
            var now = DateTime.UtcNow;
            DateTime time;
                
            if (t.Equals('s'))
            {
                time = now.AddSeconds(parsed);
            }
            else if (t.Equals('m'))
            {
                time = now.AddMinutes(parsed);
            }
            else if (t.Equals('d'))
            {
                time = now.AddDays(parsed);
            }
            else
            {
                throw new ArgumentException("Illegal argument.");
            }

            var timeEst = TimeZoneInfo.ConvertTimeFromUtc(time, _est);

            return new Dates
            {
                UtcTime = time,
                EstTime = timeEst
            };
        }

        private class Dates
        {
            public DateTime UtcTime { get; set; }
            public DateTime EstTime { get; set; }
        }

        #endregion
    }
}