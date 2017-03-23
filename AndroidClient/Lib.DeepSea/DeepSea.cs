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
        public byte options;
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
                    packet = new StreamPacket() { options = packetData[1] };
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

    interface IDeepSeaServer
    {
        bool SendConnectionRequest();
        bool SendTargetDefinition();
        bool SendStream();
    }

    interface IDeepSeaClient
    {
        bool SendClientDefinition();

        bool SendReadyForStream();

    }
}

