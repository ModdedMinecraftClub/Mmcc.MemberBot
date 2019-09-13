using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ModdedMinecraftClub.MemberBot.Bot.Jobs.Models
{
    public abstract class Job
    {
        private readonly JobDto _dto;
        public int Id { get; private set; }
        public string Method { get; private set; }
        public DateTime CreatedAt { get; private set; }

        protected Job(JobDto dto)
        {
            _dto = dto;
        }

        public abstract void Parse();
        
        protected void ParseBasics()
        {
            Id = _dto.Id;
            Method = DtoToMethod();
            CreatedAt = _dto.CreatedAt;
        }

        protected DateTime ParseJobStatusSpecificDate(string key)
        {
            var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(_dto.Data);

            return Helper.NormalizeDate(data[key]);
        }

        private string DtoToMethod()
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(_dto.InvocationData);
            var type = dict["Type"];
            type = type.Substring(0, type.IndexOf(','));
            
            return $"{type}.{dict["Method"]}";
        }
    }
}