using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lib.DeepSea
{
    public enum PacketType
    {
        //Init
        ConnectionRequest = 1,
        ClientDefinition = 2,

        //Definitions
        TargetDefinition = 3,
        StreamRequest = 4,

        //Stream
        Stream = 0,

        //Misc
        Unknown = 255
    }

    public struct ConnectionRequestPacket
    {
        public byte options;
    }

    public struct ClientDefinitionPacket
    {
        public ushort width;
        public ushort height;
    }

    public struct TargetDefinitionPacket
    {
        public ushort width;
        public ushort height;
    }

    public struct StreamRequestPacket
    {
        public byte options;
    }

    public struct StreamPacket
    {
    }

    public class DeepSea
    {
        private DeepSea() { }

        public static PacketType GetPacketType(byte[] packetData)
        {
            return (PacketType)packetData[0];
        }

        public static PacketType GetPacket(byte[] packetData, int byteCount, out object packet)
        {
            PacketType type = (PacketType)packetData[0];

            switch (type)
            {
                case PacketType.ConnectionRequest:
                    packet = new ConnectionRequestPacket() { options = packetData[1] };
                    break;
                case PacketType.ClientDefinition:
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(packetData, 1, 2);
                        Array.Reverse(packetData, 3, 2);
                    }
                    packet = new ClientDefinitionPacket() { width = BitConverter.ToUInt16(packetData, 1), height = BitConverter.ToUInt16(packetData, 3) };
                    break;
                case PacketType.TargetDefinition:
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(packetData, 1, 2);
                        Array.Reverse(packetData, 3, 2);
                    }
                    packet = new TargetDefinitionPacket() { width = BitConverter.ToUInt16(packetData, 1), height = BitConverter.ToUInt16(packetData, 3) };
                    break;
                case PacketType.StreamRequest:
                    packet = new StreamRequestPacket() { options = packetData[1] };
                    break;
                case PacketType.Stream:
                    //TODO: Do the packet shit
                    packet = new StreamPacket() { };
                    break;
                default:
                    type = PacketType.Unknown;
                    packet = null;
                    break;
            }

            return type;
        }

        public static T GetPacket<T>(byte[] packetData, int byteCount)
        {
            object packet = null;
            GetPacket(packetData, byteCount, out packet);

            return (T) packet;
        }
    }

    public interface ICommunicationProvider
    {
        bool SendPayload(byte[] payload);
    }

    public class DeepSeaServer
    {
        private ICommunicationProvider provider;

        public bool Send(ConnectionRequestPacket packet)
        {
            if (provider.SendPayload(new byte[] {Convert.ToByte(PacketType.ConnectionRequest), packet.options}))
                return true;
            return false;
        }

        public bool Send(TargetDefinitionPacket packet)
        {
            byte[] width = BitConverter.GetBytes(packet.width);
            byte[] height = BitConverter.GetBytes(packet.height);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(width);
                Array.Reverse(height);
            }

            byte[] payload = new[]
            {
                Convert.ToByte(PacketType.TargetDefinition),
                width[0],
                width[1],
                height[0],
                height[1]
            };

            if (provider.SendPayload(payload))
                return true;
            return false;
        }

        public bool Send(StreamPacket packet)
        {
            return false;
        }

        public DeepSeaServer(ICommunicationProvider provider)
        {
            this.provider = provider;
        }
    }

    public class DeepSeaClient
    {
        private ICommunicationProvider provider;

        public bool Send(ClientDefinitionPacket packet)
        {
            byte[] width = BitConverter.GetBytes(packet.width);
            byte[] height = BitConverter.GetBytes(packet.height);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(width);
                Array.Reverse(height);
            }

            byte[] payload = new[]
            {
                Convert.ToByte(PacketType.ClientDefinition),
                width[0],
                width[1],
                height[0],
                height[1]
            };

            if (provider.SendPayload(payload))
                return true;
            return false;
        }

        public bool Send(StreamRequestPacket packet)
        {
            if (provider.SendPayload(new byte[] { Convert.ToByte(PacketType.StreamRequest), packet.options }))
                return true;
            return false;
        }

        public DeepSeaClient(ICommunicationProvider provider)
        {
            this.provider = provider;
        }
    }
}

