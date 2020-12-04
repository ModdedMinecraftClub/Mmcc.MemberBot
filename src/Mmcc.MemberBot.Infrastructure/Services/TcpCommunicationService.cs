using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Mmcc.MemberBot.Core.Interfaces;
using Mmcc.MemberBot.Core.Models.Settings;

namespace Mmcc.MemberBot.Infrastructure.Services
{
    public class TcpCommunicationService : ITcpCommunicationService
    {
        private readonly ILogger<TcpCommunicationService> _logger;
        private readonly PolychatSettings _polychatSettings;
        
        public TcpCommunicationService(ILogger<TcpCommunicationService> logger, PolychatSettings polychatSettings)
        {
            _logger = logger;
            _polychatSettings = polychatSettings;
        }

        public async Task SendProtobufMessage(IMessage protobufMessage)
        {
            using var client = new TcpClient(AddressFamily.InterNetwork);
            await client.ConnectAsync(_polychatSettings.ServerIp, _polychatSettings.Port);
            await using var stream = client.GetStream();
            var packedMsg = Any.Pack(protobufMessage);
            var lengthArrayBuffer = new byte[4];
            var msgBytes = packedMsg.ToByteArray();

            BinaryPrimitives.WriteInt32BigEndian(lengthArrayBuffer.AsSpan(), msgBytes.Length);
            
            await stream.WriteAsync(lengthArrayBuffer);
            await stream.WriteAsync(msgBytes);
        }
    }
}