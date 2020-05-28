using System;
using System.Globalization;
using System.IO;
using ModdedMinecraftClub.MemberBot.Bot.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    public static class Helper
    {
        public static string GetMySqlConnectionString()
        {
            var config = Program.Config.Mysql;
            
            return $"Server={config.ServerIp};Port={config.Port};Database={config.DatabaseName};Uid={config.Username};Pwd={config.Password};Allow User Variables=True";
        }
        
        public static ConfigRoot LoadConfigFile()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var mainDir = Directory.GetParent(currentDir).ToString();
            var path = Path.Combine(mainDir, "config.yml");
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            
            return deserializer.Deserialize<ConfigRoot>(File.ReadAllText(path));
        }
        
        public static DateTime NormalizeDate(string originalJsonDate)
        {
            var date = DateTime.Parse(originalJsonDate, null, DateTimeStyles.RoundtripKind);
            var dateSpecified = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            return dateSpecified;
        }
    }
}