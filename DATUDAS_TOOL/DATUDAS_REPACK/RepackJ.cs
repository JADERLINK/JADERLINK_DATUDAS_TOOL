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
            StreamReader idxj;

            try
            {
                idxj = info.OpenText();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                return;
            }

            // continua só se idxj != null

            string TOOL_VERSION = null;
            string FILE_FORMAT = null;
            uint DAT_AMOUNT = 0;
            Dictionary<string, string> DatFiles = new Dictionary<string, string>();
            string UDAS_TOP = null;
            int UDAS_SOUNDFLAG = -1;
            string UDAS_END = null;
            string UDAS_MIDDLE = null;

            while (!idxj.EndOfStream)
            {
                string line = idxj.ReadLine()?.Trim();

                if (!(string.IsNullOrEmpty(line)
                   || line.StartsWith("#")
                   || line.StartsWith("\\")
                   || line.StartsWith("/")
                   || line.StartsWith(":")
                   || line.StartsWith("!")
                   || line.StartsWith("@")
                ))
                {
                    var split = line.Split(new char[] { ':' });
                    if (split.Length >= 2)
                    {
                        string key = split[0].ToUpperInvariant().Trim();
                        string value = split[1].Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

                        if (key.Contains("FILE_FORMAT"))
                        {
                            FILE_FORMAT = value.ToUpperInvariant();
                        }
                        else if (key.Contains("TOOL_VERSION"))
                        {
                            TOOL_VERSION = value;
                        }
                        else if (key.Contains("UDAS_TOP"))
                        {
                            UDAS_TOP = value;
                        }
                        else if (key.Contains("UDAS_END"))
                        {
                            UDAS_END = value;
                        }
                        else if (key.Contains("UDAS_MIDDLE"))
                        {
                            UDAS_MIDDLE = value;
                        }
                        else if (key.Contains("UDAS_SOUNDFLAG"))
                        {
                            int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out UDAS_SOUNDFLAG);
                        }
                        else if (key.Contains("DAT_AMOUNT"))
                        {
                            uint.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out DAT_AMOUNT);
                        }
                        else if (key.StartsWith("DAT_"))
                        {
                            if (!DatFiles.ContainsKey(key))
                            {
                                DatFiles.Add(key, value);
                            }
                        }
                    }
                }

            }

            idxj.Close();

            if (FILE_FORMAT == null || !(FILE_FORMAT == "UDAS" || FILE_FORMAT == "DAT" || FILE_FORMAT == "MAP"))
            {
                Console.WriteLine("Invalid FILE_FORMAT!");
                return;
            }

            if (DAT_AMOUNT == 0)
            {
                Console.WriteLine("DAT_AMOUNT cannot be 0!");
                return;
            }

            if (TOOL_VERSION != null)
            {
                Console.WriteLine("TOOL_VERSION: " + TOOL_VERSION);
            }

            Console.WriteLine("FILE_FORMAT: " + FILE_FORMAT);
            Console.WriteLine("DAT_AMOUNT: " + DAT_AMOUNT);

            FileStream stream;

            try
            {
                string endFileName = Path.ChangeExtension(info.FullName, FILE_FORMAT.ToLowerInvariant());
                FileInfo endFileInfo = new FileInfo(endFileName);
                stream = endFileInfo.Create();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                return;
            }

            // continua só se stream != null

            DatInfo[] datGroup = new DatInfo[DAT_AMOUNT];

            //calc
            uint fullDatHeaderLength = 16 + (4 * DAT_AMOUNT * 2);
            fullDatHeaderLength = (((fullDatHeaderLength + 31) / 32) * 32);
            uint datFileBytesLength = fullDatHeaderLength;


            // get files
            for (int i = 0; i < DAT_AMOUNT; i++)
            {
                DatInfo dat = new DatInfo();
                string key = "DAT_" + i.ToString("D3");
                if (DatFiles.ContainsKey(key))
                {
                    dat.Path = DatFiles[key];
                }
                else
                {
                    dat.Path = "null";
                }
                datGroup[i] = dat;
            }

            uint tempOffset = fullDatHeaderLength;
            for (int i = 0; i < DAT_AMOUNT; i++)
            {
                FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, datGroup[i].Path));
                datGroup[i].fileInfo = a;
                datGroup[i].Extension = a.Extension.ToUpperInvariant().Replace(".", "").PadRight(4, (char)0x0).Substring(0, 4);
                datGroup[i].Offset = tempOffset;

                if (a.Exists)
                {
                    int aLength = (int)(((a.Length + 15) / 16) * 16);

                    datGroup[i].FileExits = true;
                    datFileBytesLength += (uint)aLength;
                    datGroup[i].Length = aLength;
                    tempOffset += (uint)aLength;

                    Console.WriteLine("DAT_" + i.ToString("D3") + ": " + datGroup[i].Path);
                }
                else
                {
                    Console.WriteLine("DAT_" + i.ToString("D3") + ": " + datGroup[i].Path + "   (File does not exist!)");
                }

            }


            if (FILE_FORMAT == "DAT" || FILE_FORMAT == "MAP")
            {
                _ = new Dat(stream, datGroup, 0);
            }

            else if (FILE_FORMAT == "UDAS")
            {
                UdasInfo udasGroup = new UdasInfo();
                udasGroup.DatFileAlignedBytesLength = datFileBytesLength;
                udasGroup.DatFileRealBytesLength = datFileBytesLength;
                udasGroup.SoundFlag = UDAS_SOUNDFLAG;

                Console.WriteLine("UDAS_SOUNDFLAG: " + UDAS_SOUNDFLAG.ToString("d"));


                if (UDAS_END != null)
                {
                    udasGroup.End.Path = UDAS_END;
                    FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, udasGroup.End.Path));
                    udasGroup.End.fileInfo = a;

                    if (a.Exists)
                    {
                        int aLength = (int)(((a.Length + 15) / 16) * 16);

                        udasGroup.End.FileExits = true;
                        udasGroup.End.Length = aLength;

                        Console.WriteLine("UDAS_END: " + udasGroup.End.Path);
                    }
                    else
                    {
                        Console.WriteLine("UDAS_END: " + udasGroup.End.Path + "   (File does not exist!)");
                    }

                }

                if (UDAS_MIDDLE != null)
                {
                    udasGroup.Middle.Path = UDAS_MIDDLE;
                    FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, udasGroup.Middle.Path));
                    udasGroup.Middle.fileInfo = a;

                    if (a.Exists)
                    {
                        int aLength = (int)(((a.Length + 15) / 16) * 16);

                        udasGroup.Middle.FileExits = true;
                        udasGroup.Middle.Length = aLength;

                        Console.WriteLine("UDAS_MIDDLE: " + udasGroup.Middle.Path);
                    }
                    else
                    {
                        Console.WriteLine("UDAS_MIDDLE: " + udasGroup.Middle.Path + "   (File does not exist!)");
                    }
                }

                if (UDAS_TOP != null)
                {
                    udasGroup.Top.Path = UDAS_TOP;
                    FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, udasGroup.Top.Path));
                    udasGroup.Top.fileInfo = a;

                    if (a.Exists)
                    {
                        int aLength = (int)(((a.Length + 15) / 16) * 16);

                        udasGroup.Top.FileExits = true;
                        udasGroup.Top.Length = aLength;

                        Console.WriteLine("UDAS_TOP: " + udasGroup.Top.Path);
                    }
                    else
                    {
                        Console.WriteLine("UDAS_TOP: " + udasGroup.Top.Path + "   (File does not exist!)");
                    }
                }

                _ = new Udas(stream, datGroup, udasGroup);

            }

            stream.Close();
        }

    }

}
