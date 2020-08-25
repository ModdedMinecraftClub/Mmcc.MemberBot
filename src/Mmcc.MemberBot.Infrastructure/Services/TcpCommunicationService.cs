using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Mmcc.MemberBot.Core.Interfaces;
using Mmcc.MemberBot.Core.Protos;

namespace Mmcc.MemberBot.Infrastructure.Services
{
    public class TcpCommunicationService : ITcpCommunicationService
    {
        private readonly ILogger<TcpCommunicationService> _logger;
        private readonly TcpClient _tcpClient;

        public TcpCommunicationService(ILogger<TcpCommunicationService> logger, TcpClient tcpClient)
        {
            _logger = logger;
            _tcpClient = tcpClient;
        }

        public async Task SendPromoteMemberCommand(PromoteMemberCommand command)
        {
            var stream = _tcpClient.GetStream();
            var packedMsg = Any.Pack(command);
            var lengthArrayBuffer = new byte[4];
            var msgBytes = packedMsg.ToByteArray();

            BinaryPrimitives.WriteInt32BigEndian(lengthArrayBuffer.AsSpan(), msgBytes.Length);
            
            var arr = CombineBytes(lengthArrayBuffer, msgBytes);
            
            await stream.WriteAsync(arr);
        }
        
        private byte[] CombineBytes(IEnumerable<byte> a, IEnumerable<byte> b)
        {
            var list = new List<byte>();
            list.AddRange(a);
            list.AddRange(b);
            return list.ToArray();
        }
    }
}