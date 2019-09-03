using System.Collections.Generic;
using System.IO;
using ModdedMinecraftClub.MemberBot.Core.ConfigModels;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ModdedMinecraftClub.MemberBot.Core
{
    public static class YamlConfiguration
    {
        public static Config Config => GetConfig();

        private static string LoadConfigFile()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var mainDir = Directory.GetParent(currentDir).ToString();
            var path = Path.Combine(mainDir, "config.yml");
    
            return File.ReadAllText(path);
        }

        private static Config GetConfig()
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(new CamelCaseNamingConvention())
                .Build();

            return deserializer.Deserialize<Config>(LoadConfigFile());
        }
    }
}