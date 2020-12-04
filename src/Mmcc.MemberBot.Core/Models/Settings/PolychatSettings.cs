namespace Mmcc.MemberBot.Core.Models.Settings
{
    public class PolychatSettings
    {
        public string ServerIp { get; set; } = null!;
        public int Port { get; set; }
        public int BufferSize { get; set; }
    }
}