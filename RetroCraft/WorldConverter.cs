using System;
using Craft.Net.Anvil;
using System.IO;
using System.IO.Compression;

namespace RetroCraft
{
    public static class WorldConverter
    {
        public static void PopulateWorld(World anvil, byte[] classic, short width, short height, short depth)
        {
            var stream = new GZipStream(new MemoryStream(classic), CompressionMode.Decompress, false);
            var output = new MemoryStream();
            stream.CopyTo(output);
            stream.Close();
            classic = output.ToArray();
            output.Close();

            var level = new ClassicLevel(classic, width, height, depth);

            for (short x = 0; x < width; x += 16)
            {
                for (short z = 0; z < depth; z += 16)
                {
                    var chunk = new Chunk();
                    for (byte y = 0; y < 16 && y < (height / 16); y++)
                    {
                        var section = new Section(y);
                        var data = level.GetModernSection(x, (short)(y * 16), z);
                        if (data == null) // Chunk is entirely air
                            continue;
                        section.Blocks = data[0];
                        section.Metadata.Data = data[1];
                        section.BlockLight.Data = data[2];
                        section.SkyLight.Data = data[3];
                        section.ProcessSection();
                        chunk.Sections[y] = section;
                        Console.WriteLine("Converted <{0}, {1}, {2}>", x / 16, y, z / 16);
                    }
                    anvil.SetChunk(new Coordinates2D(x / 16, z /16), chunk);
                }
            }
        }

        private static byte[][] blockDictionary = new byte[][]
        {
            // new byte[] { ID, Metadata }
            new byte[]{ 0, 0 }, // Air
            new byte[]{ 1, 0 }, // Stone
            new byte[]{ 2, 0 }, // Grass
            new byte[]{ 3, 0 }, // Dirt
            new byte[]{ 4, 0 }, // Cobble
            new byte[]{ 5, 0 }, // Wooden Plank
            new byte[]{ 6, 0 }, // Sapling
            new byte[]{ 7, 0 }, // Bedrock
            new byte[]{ 8, 0 }, // Water
            new byte[]{ 9, 0 }, // Water (stationary)
            new byte[]{ 10, 0 }, // Lava
            new byte[]{ 11, 0 }, // Lava (stationary)
            new byte[]{ 12, 0 }, // Sand
            new byte[]{ 13, 0 }, // Gravel
            new byte[]{ 14, 0 }, // Gold Ore
            new byte[]{ 15, 0 }, // Iron Ore
            new byte[]{ 16, 0 }, // Coal Ore
            new byte[]{ 17, 0 }, // Wood
            new byte[]{ 18, 0 }, // Leaves
            new byte[]{ 19, 0 }, // Sponge
            new byte[]{ 20, 0 }, // Glass
            new byte[]{ 35, 14 }, // Wool
            new byte[]{ 35, 1 }, // Wool
            new byte[]{ 35, 4 }, // Wool
            new byte[]{ 35, 5 }, // Wool
            new byte[]{ 35, 13 }, // Wool
            new byte[]{ 35, 9 }, // Wool
            new byte[]{ 35, 3 }, // Wool
            new byte[]{ 35, 11 }, // Wool
            new byte[]{ 35, 10 }, // Wool
            new byte[]{ 35, 10 }, // Wool
            new byte[]{ 35, 10 }, // Wool
            new byte[]{ 35, 2 }, // Wool
            new byte[]{ 35, 6 }, // Wool
            new byte[]{ 35, 7 }, // Wool
            new byte[]{ 35, 8 }, // Wool
            new byte[]{ 35, 0 }, // Wool
            new byte[]{ 37, 0 }, // Dandilion
            new byte[]{ 38, 0 }, // Rose
            new byte[]{ 39, 0 }, // Brown Mushroom
            new byte[]{ 40, 0 }, // Red Mushroom
            new byte[]{ 41, 0 }, // Gold Block
            new byte[]{ 42, 0 }, // Iron Block
            new byte[]{ 43, 6 }, // Double slab
            new byte[]{ 44, 0 }, // Slab
            new byte[]{ 45, 0 }, // Brick Block
            new byte[]{ 46, 0 }, // TNT
            new byte[]{ 47, 0 }, // Bookshelf
            new byte[]{ 48, 0 }, // Moss Stone
            new byte[]{ 49, 0 }, // Obsidian
        };

        public static byte[] TranslateId(byte classicId)
        {
            return blockDictionary[classicId];
        }
    }
}

