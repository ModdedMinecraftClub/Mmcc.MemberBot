using System.Collections.Generic;
using Newtonsoft.Json;

namespace ModdedMinecraftClub.MemberBot.Bot
{
    public class Job
    {
        protected JobDto Dto;
        public int Id { get; set; }
        public string Method { get; set; }
        public string CreatedAt { get; set; }
        
        public Job(JobDto dto)
        {
            Dto = dto;
        }
        
        protected void ParseBasics()
        {
            Id = Dto.Id;
            Method = DtoToMethod();
            CreatedAt = Dto.CreatedAt;
        }

        protected string ParseJobStatusSpecificDate(string key)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(Dto.Data);

            return Helper.NormalizeDate(data[key]);
        }

        private string DtoToMethod()
        {
            var d = JsonConvert.DeserializeObject<Dictionary<string, string>>(Dto.InvocationData);
            var type = d["Type"];
            var t = type.Substring(0, type.IndexOf(','));

            return $"{t}.{d["Method"]}";
        }
    }
}