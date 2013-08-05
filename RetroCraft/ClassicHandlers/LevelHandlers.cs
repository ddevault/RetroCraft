using System;
using Craft.Net.Classic.Networking;
using Craft.Net.Anvil;
using Craft.Net.Common;
using Modern = Craft.Net.Networking;

namespace RetroCraft
{
    internal static class LevelHandlers
    {
        public static void LevelInitialize(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            client.Level = new Level("Classic World");
            client.Level.AddWorld(new World("world"));
            client.ClassicLevelData = new byte[0];
            client.LevelDownloaded = 0;
        }

        public static void LevelData(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            var packet = (LevelDataPacket)_packet;
            if ((int)packet.Complete % 10 > (int)client.LevelDownloaded % 10)
            {
                client.SendChat(string.Format(ChatColors.Yellow + "Downloading level: {0}% complete", (int)(packet.Complete * 100)));
                proxy.FlushClient(client);
            }
            client.LevelDownloaded = packet.Complete;
            int before = client.ClassicLevelData.Length;
            Array.Resize(ref client.ClassicLevelData, client.ClassicLevelData.Length + packet.Data.Length);
            Array.Copy(packet.Data, 0, client.ClassicLevelData, before, packet.Data.Length);
        }

        public static void LevelFinalize(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            var packet = (LevelFinalizePacket)_packet;
            client.SendChat(ChatColors.Yellow + "Download complete. Converting world to Anvil.");
            if (packet.YSize > 256)
            {
                client.SendChat(ChatColors.Red + "Warning! Level is taller than maximum 1.6.2 world height.");
                client.SendChat(ChatColors.Red + "The top of the world will be cut off.");
            }
            proxy.FlushClient(client);
            WorldConverter.PopulateWorld(client.Level.DefaultWorld, client.ClassicLevelData, packet.XSize, packet.YSize, packet.ZSize);
            client.SendPacket(new Modern.RespawnPacket(Dimension.Overworld, Difficulty.Normal, GameMode.Creative, 256, "FLAT"));
            for (int x = 0; x < packet.XSize / 16; x++)
            {
                for (int z = 0; z < packet.ZSize / 16; z++)
                {
                    client.SendPacket(ChunkHelper.CreatePacket(client.Level.DefaultWorld.GetChunk(
                        new Coordinates2D(x, z))));
                }
            }
            client.SendPacket(new Modern.PlayerPositionAndLookPacket(
                client.Position.X, client.Position.Y + 1.72, client.Position.Z, client.Position.Y + 0.1, 0, 0, false));
            client.SendChat(ChatColors.Yellow + "Conversion complete. Welcome to RetroCraft.");
        }
    }
}

