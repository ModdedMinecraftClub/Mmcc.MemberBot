using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModdedMinecraftClub.MemberBot.Bot.Models;

namespace ModdedMinecraftClub.MemberBot.Bot.Services.Regular
{
    public interface IDatabaseConnectionService : IDisposable
    {
        Task<bool> DoesTableExistAsync();
        Task CreateTableAsync();
        Task InsertNewApplicationAsync(Application application);
        Task<IEnumerable<Application>> GetAllPendingAsync();
        Task<IEnumerable<Application>> GetLast20ApprovedAsync();
        Task<IEnumerable<Application>> GetLast20RejectedAsync();
        Task<Application> GetByIdAsync(int applicationId);
        Task MarkAsApprovedAsync(int applicationId);
        Task MarkAsRejectedAsync(int applicationId);
    }
}