using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace DATUDAS_EXTRACT
{
    internal class Udas
    {
        public int SoundFlag = -1;
        public int DatAmount = 0;
        public string[] DatFiles = null;
        public string SndPath = null;

        public Udas(StreamWriter idxj, Stream readStream, string directory, string baseName) 
        {
            BinaryReader br = new BinaryReader(readStream);

            List<(uint type, uint offset, uint length)> UdasList = new List<(uint type, uint offset, uint length)>();

            uint temp = 0x20;
            for (int i = 0; i < 2; i++)
            {
                br.BaseStream.Position = temp;

                uint u_Type = br.ReadUInt32();
                uint u_DataSize = br.ReadUInt32();
                _ = br.ReadUInt32(); // Unused
                uint u_DataOffset = br.ReadUInt32();

                if (u_Type == 0xFFFFFFFF)
                {
                    break;
                }

                var ulist = (u_Type, u_DataOffset, u_DataSize);
                UdasList.Add(ulist);

                temp += 32;
            }

            if (UdasList.Count == 0 || UdasList[0].offset >= readStream.Length || UdasList[0].offset >= 0x01_00_00)
            {
                Console.WriteLine("Error extracting file, first offset is invalid!");
                return;
            }

            if (UdasList.Count >= 1) // UDAS_TOP
            {
                if (!Directory.Exists(Path.Combine(directory, baseName)))
                {
                    try
                    {
                        Directory.CreateDirectory(Path.Combine(directory, baseName));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Failed to create directory: " + Path.Combine(directory, baseName));
                        Console.WriteLine(ex);
                        return;
                    }
                }

                int udasTopLength = (int)UdasList[0].offset;
                byte[] udasTop = new byte[udasTopLength];

                readStream.Position = 0;
                readStream.Read(udasTop, 0, udasTopLength);

                string fullName = Path.Combine(baseName, baseName + "_TOP.HEX");
                idxj?.WriteLine("!UDAS_TOP:" + fullName);

                try
                {
                    File.WriteAllBytes(Path.Combine(directory, fullName), udasTop);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(fullName + ": " + ex);
                }         

            }

            if (UdasList.Count == 1) // UDAS_MIDDLE
            {
                int length = (int)UdasList[0].length;
                int startoffset = (int)UdasList[0].offset;
                int maxlength = (int)readStream.Length;
                int newOffset = startoffset + length;
                int newlength = maxlength - newOffset;

                if (newlength > 0 && newOffset < readStream.Length)
                {
                    byte[] udasMiddle = new byte[newlength];

                    readStream.Position = newOffset;
                    readStream.Read(udasMiddle, 0, newlength);

                    string fullName = Path.Combine(baseName, baseName + "_MIDDLE.HEX");
                    idxj?.WriteLine("!UDAS_MIDDLE:" + fullName);

                    try
                    {
                        File.WriteAllBytes(Path.Combine(directory, fullName), udasMiddle);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(fullName + ": " + ex);
                    }
                    
                }
            }

            bool readedDat = false;
            bool readedSnd = false;

            for (int i = 0; i < UdasList.Count; i++)
            {
                uint type = UdasList[i].type;

                // type == 0xFFFFFFFF : none

                if (type == 0x0 && !readedDat)
                {
                    // DAT
                    uint length = UdasList[i].length;
                    uint startOffset = UdasList[i].offset;

                    Dat a = new Dat(idxj, readStream, startOffset, length, directory, baseName);
                    DatAmount = a.DatAmount;
                    DatFiles = a.DatFiles;

                    readedDat = true;
                }
                else if (type != 0x0 && type != 0xFFFFFFFF && !readedSnd)
                {
                    // SND  

                    SoundFlag = (int)type;
                    idxj?.WriteLine("UDAS_SOUNDFLAG:" + ((int)type).ToString());

                    int startOffset = (int)UdasList[i].offset;
                    int length = (int)(readStream.Length - startOffset);

                    //middle
                    if (i >= 1)
                    {
                        int M_Length = (int)UdasList[i-1].length;
                        int M_startOffset = (int)UdasList[i-1].offset;
                        int subOffset = M_startOffset + M_Length;
                        int subLength = startOffset - subOffset;

                        if (subLength > 0)
                        {
                            byte[] udasMiddle = new byte[subLength];

                            readStream.Position = subOffset;
                            readStream.Read(udasMiddle, 0, subLength);

                            string fullName = Path.Combine(baseName,  baseName + "_MIDDLE.HEX");
                            idxj?.WriteLine("!UDAS_MIDDLE:" + fullName);

                            try
                            {
                                File.WriteAllBytes(Path.Combine(directory, fullName), udasMiddle);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(fullName + ": " + ex);
                            }
                        }
                    }

                    //end
                    if (length > 0)
                    {
                        byte[] udasEnd = new byte[length];

                        readStream.Position = startOffset;
                        readStream.Read(udasEnd, 0, length);

                        string fullNameSND = Path.Combine(baseName, baseName + "_END.SND");
                        idxj?.WriteLine("UDAS_END:" + fullNameSND);

                        SndPath = fullNameSND;

                        try
                        {
                            File.WriteAllBytes(Path.Combine(directory, fullNameSND), udasEnd);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(fullNameSND + ": " + ex);
                        }
 
                    }

                    readedSnd = true;
                }
                else if (type != 0xFFFFFFFF)
                {
                    idxj?.WriteLine($"# ERROR_FLAG{i:D1}:" + ((int)type).ToString());

                    int startOffset = (int)UdasList[i].offset;
                    int length = (int)(readStream.Length - startOffset);

                    if (length > 0)
                    {
                        byte[] udasError = new byte[length];

                        readStream.Position = startOffset;
                        readStream.Read(udasError, 0, length);

                        string fullName = Path.Combine(baseName, baseName + $"_ERROR{i:D1}.HEX");
                        idxj?.WriteLine($"# ERROR_FILE{i:D1}:" + fullName);

                        try
                        {
                            File.WriteAllBytes(Path.Combine(directory, fullName), udasError);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(fullName + ": " + ex);
                        }

                    }
                }
            }


        }


    }
}
