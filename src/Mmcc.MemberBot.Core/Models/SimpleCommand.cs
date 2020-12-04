namespace Mmcc.MemberBot.Core.Models
{
    public class SimpleCommand
    {
        public string Name { get; set; } = null!;
        public string Summary { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? Link { get; set; }
    }
}