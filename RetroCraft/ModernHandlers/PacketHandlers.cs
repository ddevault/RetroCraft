using System;
using Craft.Net.Networking;
using Classic = Craft.Net.Classic.Networking;

namespace RetroCraft.ModernHandlers
{
    public static class PacketHandlers
    {
        public static void Register(Proxy proxy)
        {
            proxy.RegisterPacketHandler(HandshakePacket.PacketId, LoginHandlers.Handshake);
            proxy.RegisterPacketHandler(EncryptionKeyResponsePacket.PacketId, LoginHandlers.EncryptionKeyResponse);
            proxy.RegisterPacketHandler(ClientStatusPacket.PacketId, LoginHandlers.ClientStatus);

            proxy.RegisterPacketHandler(ChatMessagePacket.PacketId, ChatMessage);
            proxy.RegisterPacketHandler(ServerListPingPacket.PacketId, ServerListPing);
        }

        public static void ChatMessage(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            var packet = (ChatMessagePacket)_packet;
            if (packet.Message.StartsWith("//"))
                proxy.HandleCommand(packet.Message, client);
            else
                client.SendClassicPacket(new Classic.ChatMessagePacket(packet.Message, -1));
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

