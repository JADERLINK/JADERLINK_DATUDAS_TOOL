using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DATUDAS_REPACK
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
                    string FileFormat = pair["FILE_FORMAT"].ToUpperInvariant();

                    if (FileFormat == "UDAS" || FileFormat == "DAT" || FileFormat == "MAP" || FileFormat == "DAS")
                    {
                        if (pair.ContainsKey("TOOL_VERSION"))
                        {
                            Console.WriteLine("TOOL_VERSION: " + pair["TOOL_VERSION"]);
                        }

                        Console.WriteLine("FILE_FORMAT: " + FileFormat);

                        FileStream stream = null;

                        try
                        {
                            string EndFileName = Path.ChangeExtension(info.FullName, FileFormat.ToLowerInvariant());
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
                                datAmount = int.Parse(pair["DAT_AMOUNT"], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);

                                Console.WriteLine("DAT_AMOUNT: " + datAmount.ToString());
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("DAT_AMOUNT convert error: " + ex);
                            }

                            DatInfo[] datGroup = new DatInfo[datAmount];

                            //calc
                            int datHeaderLength = 16 + (4 * datAmount * 2);
                            int div = datHeaderLength / 32;
                            float rest = (datHeaderLength % 32.0f);
                            if (rest != 0) { datHeaderLength = (div + 1) * 32; }
                            int datFileBytesLength = datHeaderLength;


                            // get files
                            for (int i = 0; i < datAmount; i++)
                            {
                                DatInfo dat = new DatInfo();
                                string key = "DAT_" + i.ToString("D3");
                                if (pair.ContainsKey(key))
                                {
                                    dat.Path = pair[key];
                                }
                                else
                                {
                                    dat.Path = "null";
                                }
                                datGroup[i] = dat;
                            }

                            int tempOffset = datHeaderLength;
                            for (int i = 0; i < datAmount; i++)
                            {
                                FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, datGroup[i].Path));
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
                                    datFileBytesLength += aLength;
                                    datGroup[i].Length = aLength;
                                    tempOffset += aLength;

                                    Console.WriteLine("DAT_" + i.ToString("D3") + ": " + datGroup[i].Path);
                                }
                                else 
                                {
                                    Console.WriteLine("DAT_" + i.ToString("D3") + ": " + datGroup[i].Path + "   (File does not exist!)");
                                }
                               
                            }

                     
                            if (FileFormat == "DAT" || FileFormat == "MAP")
                            {
                                _ = new Dat(stream, datHeaderLength, datGroup);
                            }


                            if (FileFormat == "UDAS" || FileFormat == "DAS")
                            {


                                UdasInfo udasGroup = new UdasInfo();
                                udasGroup.datFileBytesLength = datFileBytesLength;

                                if (pair.ContainsKey("UDAS_SOUNDFLAG"))
                                {
                                    try
                                    {
                                        udasGroup.SoundFlag = int.Parse(pair["UDAS_SOUNDFLAG"], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                                        if (udasGroup.SoundFlag > 0xFF)
                                        {
                                            udasGroup.SoundFlag = 0xFF;
                                        }

                                        Console.WriteLine("UDAS_SOUNDFLAG: " + udasGroup.SoundFlag.ToString());
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("UDAS_SOUNDFLAG convert error: " + ex);
                                    }
                                }

                                if (pair.ContainsKey("UDAS_END"))
                                {
                                    udasGroup.End.Path = pair["UDAS_END"];
                                    FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, udasGroup.End.Path));
                                    udasGroup.End.fileInfo = a;

                                    if (a.Exists)
                                    {
                                        int aLength = (int)a.Length;
                                        int aDiv = aLength / 16;
                                        int aRest = aLength % 16;
                                        aDiv += aRest != 0 ? 1 : 0;
                                        aLength = aDiv * 16;

                                        udasGroup.End.FileExits = true;
                                        udasGroup.End.Length = aLength;

                                        Console.WriteLine("UDAS_END: " + udasGroup.End.Path);
                                    }
                                    else
                                    {
                                        Console.WriteLine("UDAS_END: " + udasGroup.End.Path + "   (File does not exist!)");
                                    }

                                }

                                if (pair.ContainsKey("UDAS_MIDDLE"))
                                {
                                    udasGroup.Middle.Path = pair["UDAS_MIDDLE"];
                                    FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, udasGroup.Middle.Path));
                                    udasGroup.Middle.fileInfo = a;

                                    if (a.Exists)
                                    {
                                        int aLength = (int)a.Length;
                                        int aDiv = aLength / 16;
                                        int aRest = aLength % 16;
                                        aDiv += aRest != 0 ? 1 : 0;
                                        aLength = aDiv * 16;

                                        udasGroup.Middle.FileExits = true;
                                        udasGroup.Middle.Length = aLength;

                                        Console.WriteLine("UDAS_MIDDLE: " + udasGroup.Middle.Path);
                                    }
                                    else
                                    {
                                        Console.WriteLine("UDAS_MIDDLE: " + udasGroup.Middle.Path + "   (File does not exist!)");
                                    }
                                }

                                if (pair.ContainsKey("UDAS_TOP"))
                                {
                                    udasGroup.Top.Path = pair["UDAS_TOP"];
                                    FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, udasGroup.Top.Path));
                                    udasGroup.Top.fileInfo = a;

                                    if (a.Exists)
                                    {
                                        int aLength = (int)a.Length;
                                        int aDiv = aLength / 16;
                                        int aRest = aLength % 16;
                                        aDiv += aRest != 0 ? 1 : 0;
                                        aLength = aDiv * 16;

                                        udasGroup.Top.FileExits = true;
                                        udasGroup.Top.Length = aLength;

                                        Console.WriteLine("UDAS_TOP: " + udasGroup.Top.Path);
                                    }
                                    else
                                    {
                                        Console.WriteLine("UDAS_TOP: " + udasGroup.Top.Path + "   (File does not exist!)");
                                    }
                                }

                                _ = new Udas(stream, datHeaderLength, datGroup, udasGroup);

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
