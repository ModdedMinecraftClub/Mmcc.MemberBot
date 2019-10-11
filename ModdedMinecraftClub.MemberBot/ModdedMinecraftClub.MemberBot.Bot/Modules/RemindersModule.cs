using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Hangfire;
using ModdedMinecraftClub.MemberBot.Bot.Jobs;

namespace ModdedMinecraftClub.MemberBot.Bot.Modules
{
    public class RemindersModule : ModuleBase<SocketCommandContext>
    {
        private static CultureInfo EnUs => new CultureInfo("en-US");
        private static TimeZoneInfo Est => TimeZoneInfo.FindSystemTimeZoneById(Program.Config.EstTimeZoneString);

        #region Staff Only

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

                await Context.Channel.SendMessageAsync($":white_check_mark: Created a reminder for <@{whoUser.Id}> for `{dates.EstTime.ToString(EnUs)} EST` (`{dates.UtcTime.ToString(EnUs)} UTC`).");

                return;
            }
            
            // exampleDateString = "05/09/2017 05:05 AM";
            const string format = "MM/dd/yyyy HH:mm tt";
            var parsedEstDate = DateTime.ParseExact(estDateTime, format, EnUs);
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(parsedEstDate, Est);

            BackgroundJob.Schedule(() => ReminderJob.ExecuteUser(Context.Guild.Id, Context.Channel.Id, whoUser.Id, content), utcDate);

            await Context.Channel.SendMessageAsync($":white_check_mark: Created a reminder for <@{whoUser.Id}> for `{parsedEstDate.ToString(EnUs)} EST` (`{utcDate.ToString(EnUs)} UTC`).");
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

                await Context.Channel.SendMessageAsync($":white_check_mark: Created a reminder for {whoRole.Mention} for `{dates.EstTime.ToString(EnUs)} EST` (`{dates.UtcTime.ToString(EnUs)} UTC`).");

                return;
            }
            
            // exampleDateString = "05/09/2017 05:05 AM";
            const string format = "MM/dd/yyyy HH:mm tt";
            var parsedEstDate = DateTime.ParseExact(estDateTime, format, EnUs);
            var utcDate = TimeZoneInfo.ConvertTimeToUtc(parsedEstDate, Est);

            BackgroundJob.Schedule(() => ReminderJob.ExecuteRole(Context.Guild.Id, Context.Channel.Id, whoRole.Id, content), utcDate);

            await Context.Channel.SendMessageAsync($":white_check_mark: Created a reminder for {whoRole.Mention} for `{parsedEstDate.ToString(EnUs)} EST` (`{utcDate.ToString(EnUs)} UTC`).");
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

            var timeEst = TimeZoneInfo.ConvertTimeFromUtc(time, Est);

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