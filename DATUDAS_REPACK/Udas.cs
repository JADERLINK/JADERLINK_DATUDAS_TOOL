using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JADERLINK_DATUDAS_REPACK
{
    internal class Udas
    {
        public Udas(FileStream stream, int DatHeaderLenght, DatInfo[] dat, UdasInfo udasGroup) 
        {
            byte[] TopBytes = new byte[0];
            byte[] EndBytes = new byte[0];
            byte[] MiddleBytes = new byte[0];
            bool asEnd = false;

            if (udasGroup.End.FileExits)
            {
                try
                {
                    EndBytes = File.ReadAllBytes(udasGroup.End.fileInfo.FullName);
                    asEnd = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error to read file: " + udasGroup.End.fileInfo.Name + Environment.NewLine + " ex: " + ex);
                }
            }

            if (udasGroup.Middle.FileExits)
            {
                try
                {
                    MiddleBytes = File.ReadAllBytes(udasGroup.Middle.fileInfo.FullName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error to read file: " + udasGroup.Middle.fileInfo.Name + Environment.NewLine + " ex: " + ex);
                }
            }


            if (udasGroup.Top.FileExits)
            {
                try
                {
                    TopBytes = File.ReadAllBytes(udasGroup.Top.fileInfo.FullName);

                    if (TopBytes.Length < 0x80)
                    {
                        TopBytes = MakerTopBytes(asEnd, dat.Length > 0, udasGroup.SoundFlag);
                        Console.WriteLine("Top file is less than 0x80 in size.");
                    }
                }
                catch (Exception ex)
                {
                    TopBytes = MakerTopBytes(asEnd, dat.Length > 0, udasGroup.SoundFlag);
                    Console.WriteLine("Error to read file: " + udasGroup.Top.fileInfo.Name + Environment.NewLine + " ex: " + ex);
                }

            }
            else 
            {
                TopBytes = MakerTopBytes(asEnd, dat.Length > 0, udasGroup.SoundFlag);
            }


            // // // // 

            uint firtPosition = BitConverter.ToUInt32(TopBytes, 0x2c);
            if (firtPosition != TopBytes.Length)
            {
                var b = BitConverter.GetBytes((uint)TopBytes.Length);
                TopBytes[0x2c] = b[0];
                TopBytes[0x2d] = b[1];
                TopBytes[0x2e] = b[2];
                TopBytes[0x2f] = b[3];
                firtPosition = (uint)TopBytes.Length;
            }

            udasGroup.Middle.Offset = (int)firtPosition + udasGroup.datFileBytesLenght;
            udasGroup.End.Offset = udasGroup.Middle.Offset + MiddleBytes.Length;


            if (dat.Length > 0)
            {
                uint firstType = BitConverter.ToUInt32(TopBytes, 0x20);
                if (firstType != 0)
                {
                    TopBytes[0x20] = 0;
                    TopBytes[0x21] = 0;
                    TopBytes[0x22] = 0;
                    TopBytes[0x23] = 0;
                }

                byte[] datlenght = BitConverter.GetBytes((uint)udasGroup.datFileBytesLenght);
                TopBytes[0x24] = datlenght[0];
                TopBytes[0x25] = datlenght[1];
                TopBytes[0x26] = datlenght[2];
                TopBytes[0x27] = datlenght[3];

                if (asEnd)
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
                        TopBytes[0x40] = (byte)udasGroup.SoundFlag;
                        TopBytes[0x41] = 0x00;
                        TopBytes[0x42] = 0x00;
                        TopBytes[0x43] = 0x00;
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
                if (asEnd)
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
                        TopBytes[0x20] = (byte)udasGroup.SoundFlag;
                        TopBytes[0x21] = 0x00;
                        TopBytes[0x22] = 0x00;
                        TopBytes[0x23] = 0x00;
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

            stream.Write(TopBytes, 0, TopBytes.Length);

            _ = new Dat(stream, DatHeaderLenght, dat);

            if (MiddleBytes.Length != 0)
            {
                stream.Write(MiddleBytes, 0, MiddleBytes.Length);
            }

            if (EndBytes.Length != 0)
            {
                stream.Write(EndBytes, 0, EndBytes.Length);
            }

        }




        private byte[] MakerTopBytes(bool asEnd, bool asDat, int SoundFlag) 
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

            top[0x2D] = 0x04; // offfset

            if (asDat && asEnd)
            {
                top[0x40] = (byte)SoundFlag;//0x04;

                top[0x60] = 0xFF;
                top[0x61] = 0xFF;
                top[0x62] = 0xFF;
                top[0x63] = 0xFF;
            }
            else if (asDat && !asEnd)
            {
                top[0x40] = 0xFF;
                top[0x41] = 0xFF;
                top[0x42] = 0xFF;
                top[0x43] = 0xFF;
            }
            else if (!asDat && asEnd)
            {
                top[0x20] = (byte)SoundFlag;//0x04;

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
        public int datFileBytesLenght = 0;
        public int SoundFlag = 4;
        public DatInfo Top = new DatInfo();
        public DatInfo Middle = new DatInfo();
        public DatInfo End = new DatInfo();
    }





}
