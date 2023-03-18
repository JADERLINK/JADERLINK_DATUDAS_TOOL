using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JADERLINK_DATUDAS_EXTRACT
{
    internal class Udas
    {
        public int SoundFlag = -1;
        public int DatAmount = 0;
        public string[] DatFiles = null;
        public string DasSndPatch = null;

        public Udas(StreamWriter idxj, Stream readStream, string diretory, string basename) 
        {
            List<uint[]> UdasList = new List<uint[]>();

            uint temp = 0x20;
            for (int i = 0; i < 2; i++)
            {
                readStream.Position = temp;

                byte[] A = new byte[4];
                byte[] B = new byte[4];
                byte[] C = new byte[4];
                byte[] D = new byte[4];
                byte[] E = new byte[4];
                byte[] F = new byte[4];

                readStream.Read(A, 0, 4);
                readStream.Read(B, 0, 4);
                readStream.Read(C, 0, 4);
                readStream.Read(D, 0, 4);
                readStream.Read(E, 0, 4);
                readStream.Read(F, 0, 4);

                uint uA = BitConverter.ToUInt32(A, 0);
                uint uB = BitConverter.ToUInt32(B, 0);
                uint uC = BitConverter.ToUInt32(C, 0);
                uint uD = BitConverter.ToUInt32(D, 0);
                uint uE = BitConverter.ToUInt32(E, 0);
                uint uF = BitConverter.ToUInt32(F, 0);

                uint[] ulist = new uint[] {uA, uB, uC, uD, uE, uF};
                
                temp += 32; 

                if (uA != 0xFFFFFFFF)
                {
                    UdasList.Add(ulist);
                }
            }

            if (UdasList.Count >= 1)
            {
                if (!Directory.Exists(diretory + basename))
                {
                    try
                    {
                        Directory.CreateDirectory(diretory + basename);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failed to create directory: " + diretory + basename);
                    }
                }

                int udasTopLenght = (int)UdasList[0][3];
                byte[] udasTop = new byte[udasTopLenght];

                readStream.Position = 0;
                readStream.Read(udasTop, 0, udasTopLenght);

                string FileFullName = basename + "\\" + basename + "_TOP.HEX";
                idxj.WriteLine("UDAS_TOP:" + FileFullName);

                try
                {
                    File.WriteAllBytes(diretory + FileFullName, udasTop);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(FileFullName + ": " + ex);
                }         

            }

            if (UdasList.Count == 1)
            {
                int lenght = (int)UdasList[0][1];
                int startoffset = (int)UdasList[0][3];
                int maxlenget = (int)readStream.Length;
                int newOffset = startoffset + lenght;
                int newlenght = maxlenget - newOffset;

                if (newlenght > 0)
                {
                    byte[] udasMiddle = new byte[newlenght];

                    readStream.Position = newOffset;
                    readStream.Read(udasMiddle, 0, newlenght);

                    string FileFullName = basename + "\\" + basename + "_MIDDLE.HEX";
                    idxj.WriteLine("UDAS_MIDDLE:" + FileFullName);

                    try
                    {
                        File.WriteAllBytes(diretory + FileFullName, udasMiddle);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(FileFullName + ": " + ex);
                    }
                    
                }
            }


            for (int i = 0; i < UdasList.Count; i++)
            {
                uint type = UdasList[i][0];

                if (type == 0xFFFFFFFF)
                {
                    // none
                }
                else if (type == 0x0)
                {
                    // DAT
                    int lenght = (int)UdasList[i][1];
                    int startoffset = (int)UdasList[i][3];

                    Dat a = new Dat(idxj, readStream, startoffset, lenght, diretory, basename);
                    DatAmount = a.DatAmount;
                    DatFiles = a.DatFiles;
                }
                else if (type != 0x0 && type != 0xFFFFFFFF)
                {
                    //das / snd

                    SoundFlag = (int)type;
                    idxj.WriteLine("UDAS_SOUNDFLAG:" + type);


                    int startoffset = (int)UdasList[i][3];
                    int lenght = (int)(readStream.Length - startoffset);

                    //midle
                    if (i >= 1)
                    {
                        int Mlenght = (int)UdasList[i-1][1];
                        int Mstartoffset = (int)UdasList[i-1][3];
                        int subOffset = Mstartoffset + Mlenght;
                        int sublenght = startoffset - subOffset;

                        if (sublenght > 0)
                        {
                            byte[] udasMiddle = new byte[sublenght];

                            readStream.Position = subOffset;
                            readStream.Read(udasMiddle, 0, sublenght);

                            string FileFullName = basename + "\\" + basename + "_MIDDLE.HEX";
                            idxj.WriteLine("UDAS_MIDDLE:" + FileFullName);

                            try
                            {
                                File.WriteAllBytes(diretory + FileFullName, udasMiddle);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(FileFullName + ": " + ex);
                            }
                        }
                    }

                    if (lenght > 0)
                    {
                        byte[] udasEnd = new byte[lenght];

                        readStream.Position = startoffset;
                        readStream.Read(udasEnd, 0, lenght);

                        string FileFullNameEnd = basename + "\\" + basename + "_END.SND";
                        idxj.WriteLine("UDAS_END:" + FileFullNameEnd);

                        DasSndPatch = FileFullNameEnd;

                        try
                        {
                            File.WriteAllBytes(diretory + FileFullNameEnd, udasEnd);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(FileFullNameEnd + ": " + ex);
                        }
 
                    }
                }

            }


        }


    }
}
