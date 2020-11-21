using System.Threading.Tasks;
using Google.Protobuf;

namespace Mmcc.MemberBot.Core.Interfaces
{
    public interface ITcpCommunicationService
    {
        Task SendMessage(IMessage message);
    }
}