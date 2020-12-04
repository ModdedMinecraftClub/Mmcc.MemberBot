namespace Mmcc.MemberBot.Core.Models.Settings
{
    public class MySqlSettings
    {
        public string ServerIp { get; set; } = null!;
        public int Port { get; set; }
        public string DatabaseName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}