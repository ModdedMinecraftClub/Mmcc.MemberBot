using System;
using System.Globalization;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    public static class Helper
    {
        public static string GetMySqlConnectionString()
        {
            var config = Program.Config.Mysql;
            
            return $"Server={config.ServerIp};Port={config.Port};Database={config.DatabaseName};Uid={config.Username};Pwd={config.Password};Allow User Variables=True";
        }
        
        public static DateTime NormalizeDate(string originalJsonDate)
        {
            var date = DateTime.Parse(originalJsonDate, null, DateTimeStyles.RoundtripKind);
            var dateSpecified = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            return dateSpecified;
        }
    }
}