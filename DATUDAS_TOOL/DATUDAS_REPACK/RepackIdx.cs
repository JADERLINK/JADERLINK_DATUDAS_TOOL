using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DATUDAS_REPACK
{
    internal class RepackIdx
    {
        public RepackIdx(FileInfo info)
        {
            StreamReader idx;

            try
            {
                idx = info.OpenText();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                return;
            }

            // continua só se idx != null

            bool isUdas = false;
            string FileFormat = "dat";
            int SoundFlag = -1;
            uint FileCount = 0;

            Dictionary<string, string> DatFiles = new Dictionary<string, string>();

            while (!idx.EndOfStream)
            {
                string line = idx.ReadLine()?.Trim();

                if (!(string.IsNullOrEmpty(line)
                   || line.StartsWith("#")
                   || line.StartsWith("\\")
                   || line.StartsWith("/")
                   || line.StartsWith(":")
                   || line.StartsWith("!")
                ))
                {
                    var split = line.Split(new char[] { '=' });
                    if (split.Length >= 2)
                    {
                        string key = split[0].ToUpperInvariant().Trim();
                        string value = split[1].Trim().Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

                        if (key.Contains("SOUNDFLAG"))
                        {
                            int.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out SoundFlag);
                            isUdas = true;
                            FileFormat = "udas";
                            if (SoundFlag == 0)
                            {
                                SoundFlag = -1;
                            }
                        }
                        else if (key.Contains("FILECOUNT"))
                        {
                            uint.TryParse(value, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out FileCount);
                        }
                        else if (key.StartsWith("FILE_"))
                        {
                            if (!DatFiles.ContainsKey(key))
                            {
                                DatFiles.Add(key, value);
                            }
                        }
                    }
                }

            }

            idx.Close();

            if (FileCount == 0)
            {
                Console.WriteLine("FileCount cannot be 0!");
                return;
            }

            Console.WriteLine("FileCount = " + FileCount.ToString());

            FileStream stream;

            try
            {
                FileInfo endFileInfo = new FileInfo(Path.ChangeExtension(info.FullName, FileFormat));
                stream = endFileInfo.Create();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                return;
            }

            // continua só se stream != null

            uint datAmount = FileCount;
            if (isUdas && SoundFlag > 0 && datAmount > 0)
            {
                datAmount -= 1;
            }

            DatInfo[] datGroup = new DatInfo[datAmount];

            //calc
            uint fullDatHeaderLength = 16 + (4 * datAmount * 2);
            fullDatHeaderLength = (((fullDatHeaderLength + 31) / 32) * 32);
            uint datFileBytesLength = fullDatHeaderLength;

            // get files
            for (int i = 0; i < datAmount; i++)
            {
                DatInfo dat = new DatInfo();
                string key = "FILE_" + i;
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
            for (int i = 0; i < datAmount; i++)
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

                    Console.WriteLine("File_" + i + " = " + datGroup[i].Path);
                }
                else
                {
                    Console.WriteLine("File_" + i + " = " + datGroup[i].Path + "   (File does not exist!)");
                }

            }

            if (isUdas)
            {
                DatInfo FileSND = new DatInfo();

                if (SoundFlag > 0 && datAmount > 0)
                {
                    string key = "FILE_" + (FileCount - 1);
                    if (DatFiles.ContainsKey(key))
                    {
                        FileSND.Path = DatFiles[key];
                    }

                    FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, FileSND.Path));
                    FileSND.fileInfo = a;

                    if (a.Exists)
                    {
                        int aLength = (int)(((a.Length + 15) / 16) * 16);

                        FileSND.FileExits = true;
                        FileSND.Length = aLength;

                        Console.WriteLine("File_" + (FileCount - 1) + " = " + FileSND.Path);
                    }
                    else
                    {
                        Console.WriteLine("File_" + (FileCount - 1) + " = " + FileSND.Path + "   (File does not exist!)");
                    }
                }

                Console.WriteLine("SoundFlag = " + SoundFlag.ToString());

                UdasInfo udasGroup = new UdasInfo();
                udasGroup.DatFileRealBytesLength = datFileBytesLength;
                udasGroup.DatFileAlignedBytesLength = datFileBytesLength;
                udasGroup.SoundFlag = SoundFlag;
                udasGroup.End = FileSND;

                _ = new Udas(stream, datGroup, udasGroup);
            }
            else
            {
                _ = new Dat(stream, datGroup, 0);
            }

            stream.Close();

        }
    }

}
