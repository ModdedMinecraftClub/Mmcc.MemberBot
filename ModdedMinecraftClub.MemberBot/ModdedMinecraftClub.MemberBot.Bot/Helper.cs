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
        
        public static string NormalizeDate(string originalJsonDate)
        {
            // 2019-08-16T19:00:00Z UTC
            // to
            // 19:00 16/08/2019
            
            var date = DateTime.Parse(originalJsonDate, null, DateTimeStyles.RoundtripKind);
            var culture = CultureInfo.CreateSpecificCulture(Program.Config.DateCulture);

            return date.ToString(culture);
        }
    }
}