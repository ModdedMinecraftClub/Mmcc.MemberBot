using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using ConsoleTables;
using Discord;
using Discord.Commands;
using Hangfire;
using ModdedMinecraftClub.MemberBot.Bot.Jobs.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class HangfireModule : ModuleBase<SocketCommandContext>
    {
        private static CultureInfo EnUs => new CultureInfo("en-US");
        private static TimeZoneInfo Est => TimeZoneInfo.FindSystemTimeZoneById(Program.Config.EstTimeZoneString);

        #region Staff Only

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
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.CreatedAt, Est).ToString(EnUs)} EST",
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.EnqueueAt, Est).ToString(EnUs)} EST");
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
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.CreatedAt, Est).ToString(EnUs)} EST",
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.SucceededAt, Est).ToString(EnUs)} EST");
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
                            $"{TimeZoneInfo.ConvertTimeFromUtc(job.CreatedAt, Est).ToString(EnUs)} EST");
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
    }
}