using System.IO;
using ModdedMinecraftClub.MemberBot.Bot.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    public static class Yaml
    {
        private static string LoadConfigFile()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var mainDir = Directory.GetParent(currentDir).ToString();
            var path = Path.Combine(mainDir, "config.yml");
    
            return File.ReadAllText(path);
        }

        public static ConfigRoot GetConfig()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            return deserializer.Deserialize<ConfigRoot>(LoadConfigFile());
        }
    }
}