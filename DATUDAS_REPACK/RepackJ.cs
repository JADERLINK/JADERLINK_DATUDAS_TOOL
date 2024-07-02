using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JADERLINK_DATUDAS_REPACK
{
    internal class RepackJ
    {
        public RepackJ(FileInfo info) 
        {
            StreamReader idxj = null;
         
            try
            {
                idxj = info.OpenText();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }

            if (idxj != null)
            {
                Dictionary<string, string> pair = new Dictionary<string, string>();


                string endLine = "";
                while (endLine != null)
                {
                    endLine = idxj.ReadLine();

                    if (endLine != null)
                    {
                        endLine = endLine.Trim();

                        if (!(endLine.Length == 0
                            || endLine.StartsWith("#")
                            || endLine.StartsWith("\\")
                            || endLine.StartsWith("/")
                            || endLine.StartsWith(":")
                            || endLine.StartsWith("!")
                            ))
                        {
                            var split = endLine.Split(new char[] { ':' });
                            if (split.Length >= 2)
                            {
                                string key = split[0].ToUpperInvariant().Trim();
                                if (!pair.ContainsKey(key))
                                {
                                    pair.Add(key, split[1].Trim());
                                }
                            }
                        }
                    }

                }

                idxj.Close();

     
                if (pair.ContainsKey("FILE_FORMAT") && pair.ContainsKey("DAT_AMOUNT"))
                {
                    string FileFormat = pair["FILE_FORMAT"].ToUpperInvariant().Trim();

                    if (FileFormat == "UDAS" || FileFormat == "DAT" || FileFormat == "MAP")
                    {
                        if (pair.ContainsKey("TOOL_VERSION"))
                        {
                            Console.WriteLine("TOOL VERSION: " + pair["TOOL_VERSION"].Trim());
                        }
                      
                        FileStream stream = null;

                        try
                        {
                            string EndFileName = info.FullName.Substring(0, info.FullName.Length - info.Extension.Length) + "." + FileFormat.ToLowerInvariant();
                            FileInfo EndFileInfo = new FileInfo(EndFileName);
                            stream = EndFileInfo.Create();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex);
                        }


                        if (stream != null)
                        {

                            int datAmount = 0;
                            try
                            {
                                datAmount = int.Parse(pair["DAT_AMOUNT"].Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("DAT_AMOUNT convert error: " + ex);
                            }

                            DatInfo[] datGroup = new DatInfo[datAmount];
                            
                            int datFileBytesLenght = 0;

                            int datHeaderLenght = 16 + (4 * datAmount * 2);
                            int div = (int)(datHeaderLenght / 32);
                            float rest = (datHeaderLenght % 32.0f);
                            if (rest != 0)
                            {
                                datHeaderLenght = (div + 1) * 32;
                            }
                            datFileBytesLenght += datHeaderLenght;


                            // get files
                            for (int i = 0; i < datAmount; i++)
                            {
                                DatInfo dat = new DatInfo();
                                string key = "DAT_" + i.ToString("D3");
                                if (pair.ContainsKey(key))
                                {
                                    dat.Path = pair[key];
                                }
                                datGroup[i] = dat;
                            }

                            int tempOffset = datHeaderLenght;
                            for (int i = 0; i < datAmount; i++)
                            {
                                FileInfo a = new FileInfo(info.Directory + "\\" + datGroup[i].Path);
                                datGroup[i].fileInfo = a;
                                datGroup[i].Extension = a.Extension.ToUpperInvariant().Replace(".", "").PadRight(4, (char)0x0).Substring(0, 4);
                                datGroup[i].Offset = tempOffset;

                                if (a.Exists)
                                {
                                    int aLength = (int)a.Length;
                                    int aDiv = aLength / 16;
                                    int aRest = aLength % 16;
                                    aDiv += aRest != 0 ? 1 : 0;
                                    aLength = aDiv * 16;

                                    datGroup[i].FileExits = true;
                                    datFileBytesLenght += aLength;
                                    datGroup[i].Length = aLength;
                                    tempOffset += aLength;
                                }
                                else 
                                {
                                    Console.WriteLine("DAT_" + i.ToString("D3") + " - File does not exist: " + datGroup[i].Path);
                                }
                               
                            }

                     
                            if (FileFormat == "DAT" || FileFormat == "MAP")
                            {
                                _ = new Dat(stream, datHeaderLenght, datGroup);
                            }


                            if (FileFormat == "UDAS")
                            {
                                UdasInfo udasGroup = new UdasInfo();
                                udasGroup.datFileBytesLenght = datFileBytesLenght;

                                if (pair.ContainsKey("UDAS_SOUNDFLAG"))
                                {
                                    try
                                    {
                                        udasGroup.SoundFlag = int.Parse(pair["UDAS_SOUNDFLAG"], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                                        if (udasGroup.SoundFlag > 0xFF)
                                        {
                                            udasGroup.SoundFlag = 0xFF;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("UDAS_SOUNDFLAG convert error: " + ex);
                                    }
                                }

                                if (pair.ContainsKey("UDAS_END"))
                                {
                                    udasGroup.End.Path = pair["UDAS_END"];
                                    FileInfo a = new FileInfo(info.Directory + "\\" + udasGroup.End.Path);
                                    udasGroup.End.fileInfo = a;

                                    if (a.Exists)
                                    {
                                        udasGroup.End.FileExits = true;
                                        udasGroup.End.Length = (int)a.Length;
                                    }
                                    else
                                    {
                                        Console.WriteLine("UDAS_END - File does not exist: " + udasGroup.End.Path);
                                    }

                                }

                                if (pair.ContainsKey("UDAS_MIDDLE"))
                                {
                                    udasGroup.Middle.Path = pair["UDAS_MIDDLE"];
                                    FileInfo a = new FileInfo(info.Directory + "\\" + udasGroup.Middle.Path);
                                    udasGroup.Middle.fileInfo = a;

                                    if (a.Exists)
                                    {
                                        udasGroup.Middle.FileExits = true;
                                        udasGroup.Middle.Length = (int)a.Length;
                                    }
                                    else
                                    {
                                        Console.WriteLine("UDAS_MIDDLE - File does not exist: " + udasGroup.Middle.Path);
                                    }
                                }

                                if (pair.ContainsKey("UDAS_TOP"))
                                {
                                    udasGroup.Top.Path = pair["UDAS_TOP"];
                                    FileInfo a = new FileInfo(info.Directory + "\\" + udasGroup.Top.Path);
                                    udasGroup.Top.fileInfo = a;

                                    if (a.Exists)
                                    {
                                        udasGroup.Top.FileExits = true;
                                        udasGroup.Top.Length = (int)a.Length;
                                    }
                                    else
                                    {
                                        Console.WriteLine("UDAS_TOP - File does not exist: " + udasGroup.Top.Path);
                                    }
                                }

                                _ = new Udas(stream, datHeaderLenght, datGroup, udasGroup);

                            }

                            stream.Close();
                        }

                    }
                    else 
                    {
                        Console.WriteLine("Invalid FILE_FORMAT: " + FileFormat);
                    }
                }
                else 
                {
                    Console.WriteLine("Not found FILE_FORMAT or DAT_AMOUNT tag.");
                }

            }

        }

    }
}
