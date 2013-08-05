using System;
using Craft.Net.Classic.Networking;
using Craft.Net.Common;

namespace RetroCraft.ClassicHandlers
{
    internal static class PacketHandlers
    {
        public static void Register(Proxy proxy)
        {
            proxy.RegisterClassicPacketHandler(LevelInitializePacket.PacketId, LevelHandlers.LevelInitialize);
            proxy.RegisterClassicPacketHandler(LevelDataPacket.PacketId, LevelHandlers.LevelData);
            proxy.RegisterClassicPacketHandler(LevelFinalizePacket.PacketId, LevelHandlers.LevelFinalize);

            proxy.RegisterClassicPacketHandler(ChatMessagePacket.PacketId, ChatMessage);
            proxy.RegisterClassicPacketHandler(HandshakePacket.PacketId, Handshake);
        }

        public static void Handshake(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            var packet = (HandshakePacket)_packet;
            client.SendChat(ChatColors.Yellow + "Logged into " + packet.PartyName);
            client.SendChat(ChatColors.Yellow + "Message of the Day: " + packet.KeyOrMOTD);
        }

        public static void ChatMessage(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            var packet = (ChatMessagePacket)_packet;
            client.SendChat(packet.Message);
        }
    }
}

