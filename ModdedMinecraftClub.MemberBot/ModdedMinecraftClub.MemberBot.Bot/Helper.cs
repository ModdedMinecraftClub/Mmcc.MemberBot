using System;
using System.Globalization;
using System.IO;
using ModdedMinecraftClub.MemberBot.Bot.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    /// <summary>
    /// Helper utility methods
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Loads config from yml config file
        /// </summary>
        /// <returns>Deserialized config</returns>
        public static BotSettings LoadConfigFile()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var mainDir = Directory.GetParent(currentDir).ToString();
            var path = Path.Combine(mainDir, "config.yml");
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();
            
            return deserializer.Deserialize<BotSettings>(File.ReadAllText(path));
        }
        
        /// <summary>
        /// Converts JSON date string into C# DateTime
        /// </summary>
        /// <param name="originalJsonDate">JSON date string</param>
        /// <returns>DateTime for that JSON date string</returns>
        public static DateTime NormalizeDate(string originalJsonDate)
        {
            var date = DateTime.Parse(originalJsonDate, null, DateTimeStyles.RoundtripKind);
            var dateSpecified = DateTime.SpecifyKind(date, DateTimeKind.Utc);

            return dateSpecified;
        }
    }
}