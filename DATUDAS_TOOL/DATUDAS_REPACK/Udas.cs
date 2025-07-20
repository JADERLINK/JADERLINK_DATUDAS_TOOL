using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace DATUDAS_REPACK
{
    internal class Udas
    {
        public Udas(FileStream stream, DatInfo[] dat, UdasInfo udasGroup) 
        {
            byte[] EndBytes = new byte[udasGroup.End.Length];
            byte[] MiddleBytes = new byte[udasGroup.Middle.Length];
            bool hasEnd = false;

            if (udasGroup.End.FileExits)
            {
                try
                {
                    BinaryReader br = new BinaryReader(udasGroup.End.fileInfo.OpenRead());
                    br.BaseStream.Read(EndBytes, 0, (int)udasGroup.End.fileInfo.Length);
                    br.Close();
                    hasEnd = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error to read file: " + udasGroup.End.fileInfo.Name);
                    Console.WriteLine(ex);
                }
            }

            if (udasGroup.Middle.FileExits)
            {
                try
                {
                    BinaryReader br = new BinaryReader(udasGroup.Middle.fileInfo.OpenRead());
                    br.BaseStream.Read(MiddleBytes, 0, (int)udasGroup.Middle.fileInfo.Length);
                    br.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error to read file: " + udasGroup.Middle.fileInfo.Name);
                    Console.WriteLine(ex);
                }
            }

            byte[] TopBytes = MakeUdasTop(udasGroup, hasEnd, dat.Length > 0);

            stream.Write(TopBytes, 0, TopBytes.Length);

            _ = new Dat(stream, dat, TopBytes.Length);

            if (MiddleBytes.Length != 0)
            {
                stream.Position = udasGroup.Middle.Offset;
                stream.Write(MiddleBytes, 0, MiddleBytes.Length);
            }

            if (EndBytes.Length != 0)
            {
                stream.Position = udasGroup.End.Offset;
                stream.Write(EndBytes, 0, EndBytes.Length);
            }

        }

        public static byte[] MakeUdasTop(UdasInfo udasGroup, bool hasEnd, bool hasDat) 
        {
            byte[] TopBytes;

            if (udasGroup.Top.FileExits)
            {
                try
                {
                    TopBytes = new byte[udasGroup.Top.Length];

                    BinaryReader br = new BinaryReader(udasGroup.Top.fileInfo.OpenRead());
                    br.BaseStream.Read(TopBytes, 0, (int)udasGroup.Top.fileInfo.Length);
                    br.Close();

                    if (TopBytes.Length < 0x80)
                    {
                        TopBytes = MakerNewTopBytes(hasEnd, hasDat, udasGroup.SoundFlag);
                        Console.WriteLine("Top file is less than 0x80 in size. It was replaced with a new one.");
                    }
                }
                catch (Exception ex)
                {
                    TopBytes = MakerNewTopBytes(hasEnd, hasDat, udasGroup.SoundFlag);
                    Console.WriteLine("Error to read file: " + udasGroup.Top.fileInfo.Name);
                    Console.WriteLine(ex);
                }

            }
            else
            {
                TopBytes = MakerNewTopBytes(hasEnd, hasDat, udasGroup.SoundFlag);
            }

            //-----

            uint firtPosition = BitConverter.ToUInt32(TopBytes, 0x2C);
            if (firtPosition != TopBytes.Length)
            {
                var b = BitConverter.GetBytes((uint)TopBytes.Length);
                TopBytes[0x2c] = b[0];
                TopBytes[0x2d] = b[1];
                TopBytes[0x2e] = b[2];
                TopBytes[0x2f] = b[3];
                firtPosition = (uint)TopBytes.Length;
            }

            udasGroup.Middle.Offset = firtPosition + udasGroup.DatFileAlignedBytesLength;
            udasGroup.End.Offset = udasGroup.Middle.Offset + (uint)udasGroup.Middle.Length;

            if (hasDat)
            {
                uint firstType = BitConverter.ToUInt32(TopBytes, 0x20);
                if (firstType != 0)
                {
                    TopBytes[0x20] = 0;
                    TopBytes[0x21] = 0;
                    TopBytes[0x22] = 0;
                    TopBytes[0x23] = 0;
                }

                byte[] datlength = BitConverter.GetBytes((uint)udasGroup.DatFileRealBytesLength);
                TopBytes[0x24] = datlength[0];
                TopBytes[0x25] = datlength[1];
                TopBytes[0x26] = datlength[2];
                TopBytes[0x27] = datlength[3];

                if (hasEnd)
                {
                    byte[] endOffset = BitConverter.GetBytes((uint)udasGroup.End.Offset);

                    TopBytes[0x4C] = endOffset[0];
                    TopBytes[0x4D] = endOffset[1];
                    TopBytes[0x4E] = endOffset[2];
                    TopBytes[0x4F] = endOffset[3];



                    TopBytes[0x44] = 0;
                    TopBytes[0x45] = 0;
                    TopBytes[0x46] = 0;
                    TopBytes[0x47] = 0;

                    uint secondType = BitConverter.ToUInt32(TopBytes, 0x40);
                    if (secondType == 0xFFFFFFFF)
                    {
                        byte[] SoundFlag = BitConverter.GetBytes((uint)udasGroup.SoundFlag);

                        TopBytes[0x40] = SoundFlag[0];
                        TopBytes[0x41] = SoundFlag[1];
                        TopBytes[0x42] = SoundFlag[2];
                        TopBytes[0x43] = SoundFlag[3];
                    }

                    TopBytes[0x60] = 0xFF;
                    TopBytes[0x61] = 0xFF;
                    TopBytes[0x62] = 0xFF;
                    TopBytes[0x63] = 0xFF;
                }
                else
                {
                    TopBytes[0x40] = 0xFF;
                    TopBytes[0x41] = 0xFF;
                    TopBytes[0x42] = 0xFF;
                    TopBytes[0x43] = 0xFF;
                }

            }
            else
            {
                if (hasEnd)
                {
                    byte[] endOffset = BitConverter.GetBytes((uint)udasGroup.End.Offset);

                    TopBytes[0x2C] = endOffset[0];
                    TopBytes[0x2D] = endOffset[1];
                    TopBytes[0x2E] = endOffset[2];
                    TopBytes[0x2F] = endOffset[3];



                    TopBytes[0x24] = 0;
                    TopBytes[0x25] = 0;
                    TopBytes[0x26] = 0;
                    TopBytes[0x27] = 0;

                    uint secondType = BitConverter.ToUInt32(TopBytes, 0x20);
                    if (secondType == 0xFFFFFFFF || secondType == 0)
                    {
                        byte[] SoundFlag = BitConverter.GetBytes((uint)udasGroup.SoundFlag);

                        TopBytes[0x20] = SoundFlag[0];
                        TopBytes[0x21] = SoundFlag[1];
                        TopBytes[0x22] = SoundFlag[2];
                        TopBytes[0x23] = SoundFlag[3];
                    }

                    TopBytes[0x40] = 0xFF;
                    TopBytes[0x41] = 0xFF;
                    TopBytes[0x42] = 0xFF;
                    TopBytes[0x43] = 0xFF;
                }
                else
                {
                    TopBytes[0x20] = 0xFF;
                    TopBytes[0x21] = 0xFF;
                    TopBytes[0x22] = 0xFF;
                    TopBytes[0x23] = 0xFF;
                }

            }

            return TopBytes;
        }

        private static byte[] MakerNewTopBytes(bool hasEnd, bool hasDat, int SoundFlag) 
        {
            byte[] top = new byte[0x400];
            int temp = 0;
            for (int i = 0; i < 8; i++)
            {
                top[temp] = 0xCA;
                top[temp+1] = 0xB6;
                top[temp+2] = 0xBE;
                top[temp+3] = 0x20;
                temp += 4;
            }


            top[0x2D] = 0x04; // offset; little endian

            if (hasDat && hasEnd)
            {
                byte[] soundFlag = BitConverter.GetBytes((uint)SoundFlag);

                top[0x40] = soundFlag[0];
                top[0x41] = soundFlag[1];
                top[0x42] = soundFlag[2];
                top[0x43] = soundFlag[3];

                top[0x60] = 0xFF;
                top[0x61] = 0xFF;
                top[0x62] = 0xFF;
                top[0x63] = 0xFF;
            }
            else if (hasDat && !hasEnd)
            {
                top[0x40] = 0xFF;
                top[0x41] = 0xFF;
                top[0x42] = 0xFF;
                top[0x43] = 0xFF;
            }
            else if (!hasDat && hasEnd)
            {
                byte[] soundFlag = BitConverter.GetBytes((uint)SoundFlag);

                top[0x20] = soundFlag[0];
                top[0x21] = soundFlag[1];
                top[0x22] = soundFlag[2];
                top[0x23] = soundFlag[3];

                top[0x40] = 0xFF;
                top[0x41] = 0xFF;
                top[0x42] = 0xFF;
                top[0x43] = 0xFF;
            }
            else 
            {
                top[0x20] = 0xFF;
                top[0x21] = 0xFF;
                top[0x22] = 0xFF;
                top[0x23] = 0xFF;
            }

            return top;
        }

    }


    internal class UdasInfo 
    {
        public uint DatFileAlignedBytesLength = 0;
        public uint DatFileRealBytesLength = 0;
        public int SoundFlag = 4;
        public DatInfo Top = new DatInfo();
        public DatInfo Middle = new DatInfo();
        public DatInfo End = new DatInfo();
    }


}
