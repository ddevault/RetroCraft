using System;
using Craft.Net.Classic.Networking;
using Craft.Net.Common;
using Modern = Craft.Net.Networking;

namespace RetroCraft.ClassicHandlers
{
    internal static class PlayerHandlers
    {
        public static void PositionAndOrientation(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            var packet = (PositionAndOrientationPacket)_packet;
            if (packet.PlayerID < 0)
            {
                client.Position = new Vector3(packet.X / 32, packet.Y / 32, packet.Z / 32);
                client.Yaw = packet.Yaw;
                client.Pitch = packet.Pitch;
                client.SendPacket(new Modern.PlayerPositionAndLookPacket(
                    client.Position.X, client.Position.Y + 1.72, client.Position.Z, client.Position.Y + 0.1, 0, 0, false));
            }
            else
            {
            }
        }

        public static void SpawnPlayer(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            var packet = (SpawnPlayerPacket)_packet;
            if (packet.PlayerID < 0)
            {
                client.Position = new Vector3(packet.X / 32.0, packet.Y / 32.0, packet.Z / 32.0);
                client.Yaw = packet.Yaw;
                client.Pitch = packet.Pitch;
                client.SendPacket(new Modern.PlayerPositionAndLookPacket(
                    client.Position.X, client.Position.Y + 1.72, client.Position.Z, client.Position.Y + 0.1, 0, 0, false));
            }
            else
            {
                var dictionary = new MetadataDictionary();
                dictionary[0] = new MetadataByte(0);
                dictionary[1] = new MetadataShort(300);
                client.SendPacket(new Modern.SpawnPlayerPacket(packet.PlayerID, packet.Username,
                    MathHelper.CreateAbsoluteInt(packet.X / 32.0),
                    MathHelper.CreateAbsoluteInt(packet.Y / 32.0),
                    MathHelper.CreateAbsoluteInt(packet.Z / 32.0), packet.Yaw, packet.Pitch, 0, dictionary));
            }
            client.SendPacket(new Modern.PlayerListItemPacket(packet.Username, true, 0));
        }
    }
}

