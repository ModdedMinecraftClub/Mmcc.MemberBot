using System.Threading.Tasks;
using Mmcc.MemberBot.Core.Protos;

namespace Mmcc.MemberBot.Core.Interfaces
{
    public interface ITcpCommunicationService
    {
        Task SendPromoteMemberCommand(PromoteMemberCommand command);
    }
}