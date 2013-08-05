using System;
using Craft.Net.Networking;

namespace RetroCraft.ModernHandlers
{
    public static class PacketHandlers
    {
        public static void Register(Proxy proxy)
        {
            proxy.RegisterPacketHandler(HandshakePacket.PacketId, LoginHandlers.Handshake);
            proxy.RegisterPacketHandler(EncryptionKeyResponsePacket.PacketId, LoginHandlers.EncryptionKeyResponse);
            proxy.RegisterPacketHandler(ClientStatusPacket.PacketId, LoginHandlers.ClientStatus);

            proxy.RegisterPacketHandler(ServerListPingPacket.PacketId, ServerListPing);
        }

        public static void ServerListPing(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            client.SendPacket(new DisconnectPacket(GetPingValue(proxy)));
        }

        private static string GetPingValue(Proxy proxy)
        {
            return "§1\0" + PacketReader.ProtocolVersion + "\0" +
                PacketReader.FriendlyVersion + "\0RetroCraft Proxy\00\00";
        }
    }
}

