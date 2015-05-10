using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RetroCraft
{
    public class ClassicLevel
    {
        public byte[] Blocks { get; set; }
        public byte[] Metadata { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public short Depth { get; set; }

        public ClassicLevel(byte[] classicData, short width, short height, short depth)
        {
            Blocks = new byte[width * depth * height];
            Metadata = new byte[width * depth * height];

            this.Width = width;
            this.Height = height;
            this.Depth = depth;

            for (int i = 4; i < Blocks.Length; i++)
            {
                Blocks[i] = WorldConverter.TranslateId(classicData[i])[0];
                Metadata[i] = WorldConverter.TranslateId(classicData[i])[1];
            }
        }

        public void SetBlock(short X, short Y, short Z, byte ID, byte Metadata)
        {
            int index = X + (Z * Width) + (Y * Width * Depth);
            if (index >= this.Blocks.Length)
                return;
            this.Blocks[index] = ID;
            this.Metadata[index] = Metadata;
        }

        public byte[] GetBlock(short X, short Y, short Z)
        {
            int index = X + (Z * Width) + (Y * Width * Depth);
            if (index >= this.Blocks.Length)
                return new byte[] { 0, 0 };
            return new byte[] { this.Blocks[index], this.Metadata[index] };
        }

        public byte[][] GetModernSection(short cX, short cY, short cZ)
        {
            byte[][] cData = new byte[4][] { new byte[4096], new byte[2048], new byte[2048], new byte[2048], };
            bool isAir = true;
            for (short x = 0; x < 16; x++)
            {
                for (short z = 0; z < 16; z++)
                {
                    for (short y = 0; y < 16; y++)
                    {
                        // Add blocks and metadata
                        byte[] blockData = GetBlock((short)(cX + x), (short)(cY + y), (short)(cZ + z));
                        cData[0][x + (z * 16) + (y * 16 * 16)] = blockData[0];
                        if ((x + (z * Width) + (y * Width * Depth)) % 2 == 1)
                            blockData[1] <<= 4;

                        cData[1][(x + (z * 16) + (y * 16 * 16)) / 2] |= blockData[1];
                        if (blockData[0] != 0)
                            isAir = false;
                    }
                }
            }
            if (isAir)
                return null;

            for (int i = 0; i < 2048; i++) // Fake lighting
            {
                cData[2][i] = (byte)0xFF;
                cData[3][i] = (byte)0xFF;
            }

            return cData;
        }
    }
}
