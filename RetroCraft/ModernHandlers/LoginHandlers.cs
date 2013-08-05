using System;
using Craft.Net.Networking;
using System.Security.Cryptography;
using Craft.Net.Common;

namespace RetroCraft.ModernHandlers
{
    internal static class LoginHandlers
    {
        private const string sessionCheckUri = "http://session.minecraft.net/game/checkserver.jsp?user={0}&serverId={1}";

        public static void Handshake(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            var packet = (HandshakePacket)_packet;
            if (packet.ProtocolVersion < PacketReader.ProtocolVersion)
            {
                client.SendPacket(new DisconnectPacket("Outdated client!"));
                return;
            }
            if (packet.ProtocolVersion > PacketReader.ProtocolVersion)
            {
                client.SendPacket(new DisconnectPacket("Outdated server!"));
                return;
            }
            client.AuthenticationHash = "-";
            client.Username = packet.Username;
            client.SendPacket(CreateEncryptionRequest(client, proxy));
        }

        public static void EncryptionKeyResponse(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            var packet = (EncryptionKeyResponsePacket)_packet;
            var decryptedToken = proxy.CryptoServiceProvider.Decrypt(packet.VerificationToken, false);
            for (int i = 0; i < decryptedToken.Length; i++)
            {
                if (decryptedToken[i] != client.VerificationToken[i])
                {
                    client.Disconnect("Unable to authenticate.");
                    return;
                }
            }
            client.SharedKey = proxy.CryptoServiceProvider.Decrypt(packet.SharedSecret, false);
            client.SendPacket(new EncryptionKeyResponsePacket(new byte[0], new byte[0]));
        }

        public static void ClientStatus(RemoteClient client, Proxy proxy, IPacket _packet)
        {
            var packet = (ClientStatusPacket)_packet;
            if (packet.Status == ClientStatusPacket.ClientStatus.InitialSpawn)
            {
                // Throw them into an empty world and inform them that we'll be connecting them shortly.
                client.SendPacket(new LoginRequestPacket(1, "FLAT", GameMode.Creative, Dimension.Overworld, Difficulty.Normal, 1));
                client.SendPacket(new PlayerAbilitiesPacket(0, 0.05f, 0.1f));
                client.SendPacket(new EntityPropertiesPacket(1, new[] { new EntityProperty("generic.movementSpeed", 0.05f) }));
                client.SendPacket(new PlayerPositionAndLookPacket(0, 1.72, 0, 0.1, 0, 0, false));
                client.SendChat("Welcome to RetroCraft! You'll be connected momentarily.");
                proxy.Connect(client);
            }
            else if (packet.Status == ClientStatusPacket.ClientStatus.Respawn)
            {
                // TODO
            }
        }

        private static EncryptionKeyRequestPacket CreateEncryptionRequest(RemoteClient client, Proxy proxy)
        {
            var verifyToken = new byte[4];
            var csp = new RNGCryptoServiceProvider();
            csp.GetBytes(verifyToken);
            client.VerificationToken = verifyToken;

            var encodedKey = AsnKeyBuilder.PublicKeyToX509(proxy.ServerKey);
            var request = new EncryptionKeyRequestPacket(client.AuthenticationHash, encodedKey.GetBytes(), verifyToken);
            return request;
        }

        private static string CreateHash()
        {
            byte[] hash = BitConverter.GetBytes(MathHelper.Random.Next());
            string response = "";
            foreach (byte b in hash)
                response += b.ToString("x2");
            return response;
        }
    }
}

