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
            StreamReader idx = null;

            try
            {
                idx = info.OpenText();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }

            if (idx != null)
            {

                Dictionary<string, string> pair = new Dictionary<string, string>();

                string endLine = "";
                while (endLine != null)
                {
                    endLine = idx.ReadLine();

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
                            var split = endLine.Split(new char[] { '=' });
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

                idx.Close();

                //SoundFlag
                
                bool isUdas = false;
                string FileFormat = "dat";
                int SoundFlag = -1;
                int FileCount = 0;
                if (pair.ContainsKey("SOUNDFLAG"))
                {
                    isUdas = true;
                    FileFormat = "udas";
                    try
                    {
                        SoundFlag = int.Parse(pair["SOUNDFLAG"], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                        if (SoundFlag > 0xFF)
                        {
                            SoundFlag = 0xFF;
                        }

                        Console.WriteLine("SoundFlag = " + SoundFlag.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("SoundFlag convert error: " + ex);
                    }
                }

                FileStream stream = null;

                try
                {
                    string EndFileName = Path.ChangeExtension(info.FullName, FileFormat);
                    FileInfo EndFileInfo = new FileInfo(EndFileName);
                    stream = EndFileInfo.Create();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                }

                if (stream != null)
                {

                    //FileCount
                    if (pair.ContainsKey("FILECOUNT"))
                    {
                        try
                        {
                            FileCount = int.Parse(pair["FILECOUNT"], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);

                            Console.WriteLine("FileCount = " + FileCount.ToString());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("FileCount convert error: " + ex);
                        }

                    }
                    else
                    {
                        Console.WriteLine("FileCount does not exist.");
                    }

                    //---------------

                    int datAmount = FileCount;
                    if (isUdas && SoundFlag > 0 && datAmount > 0)
                    {
                        datAmount -= 1;
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
                        string key = "FILE_" + i;
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
                            string key = "FILE_" + (FileCount -1);
                            if (pair.ContainsKey(key))
                            {
                                FileSND.Path = pair[key];
                            }

                            FileInfo a = new FileInfo(Path.Combine(info.Directory.FullName, FileSND.Path));
                            FileSND.fileInfo = a;

                            if (a.Exists)
                            {
                                int aLength = (int)a.Length;
                                int aDiv = aLength / 16;
                                int aRest = aLength % 16;
                                aDiv += aRest != 0 ? 1 : 0;
                                aLength = aDiv * 16;

                                FileSND.FileExits = true;
                                FileSND.Length = aLength;

                                Console.WriteLine("File_" + (FileCount - 1) + " = " + FileSND.Path);
                            }
                            else
                            {
                                Console.WriteLine("File_" + (FileCount - 1) + " = " + FileSND.Path + "   (File does not exist!)");
                            }
                        }

                        UdasInfo udasGroup = new UdasInfo();
                        udasGroup.datFileBytesLength = datFileBytesLength;
                        udasGroup.SoundFlag = SoundFlag;
                        udasGroup.End = FileSND;

                        _ = new Udas(stream, datHeaderLength, datGroup, udasGroup);
                    }
                    else
                    {
                        _ = new Dat(stream, datHeaderLength, datGroup);
                    }


                    stream.Close();

                    //end


                }
            }

        }
    }

}