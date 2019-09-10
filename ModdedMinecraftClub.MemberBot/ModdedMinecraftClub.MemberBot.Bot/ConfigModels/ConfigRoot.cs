namespace ModdedMinecraftClub.MemberBot.Bot.ConfigModels
{
    public class ConfigRoot
    {
        public Mysql Mysql { get; set; }
        public Discord Discord { get; set; }
        public string DateCulture { get; set; }
    }
}