namespace ModdedMinecraftClub.MemberBot.Bot
{
    public static class Helper
    {
        public static string GetMySqlConnectionString()
        {
            var config = Program.Config.Mysql;
            
            return $"Server={config.ServerIp};Port={config.Port};Database={config.DatabaseName};Uid={config.Username};Pwd={config.Password}";
        }
    }
}