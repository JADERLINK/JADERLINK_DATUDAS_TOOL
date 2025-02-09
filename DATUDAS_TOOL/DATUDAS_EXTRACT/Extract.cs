using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DATUDAS_EXTRACT
{
    internal class Extract
    {
        public Extract(FileInfo info, FileFormat fileFormat, bool CreateIdx, bool CreateIdxJ) 
        {
            FileStream stream = null;
            StreamWriter idxj = null;
            StreamWriter idx_ = null;

            try
            {
                stream = info.OpenRead();

                if (CreateIdxJ || fileFormat == FileFormat.MAP || fileFormat == FileFormat.DAS)
                {
                    string idxjFileName = Path.ChangeExtension(info.FullName, ".idxJ");
                    FileInfo idxjInfo = new FileInfo(idxjFileName);
                    idxj = idxjInfo.CreateText();
                }

                if (CreateIdx && fileFormat != FileFormat.MAP && fileFormat != FileFormat.DAS)
                {
                    string idx_FileName = Path.ChangeExtension(info.FullName, ".idx");
                    FileInfo idx_Info = new FileInfo(idx_FileName);
                    idx_ = idx_Info.CreateText();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }

            if (stream != null && (idxj != null || idx_ != null))
            {
                idxj?.WriteLine("# github.com/JADERLINK/JADERLINK_DATUDAS_TOOL");
                idxj?.WriteLine("# youtube.com/@JADERLINK");
                idxj?.WriteLine("# JADERLINK DATUDAS TOOL By JADERLINK");
                idxj?.WriteLine("TOOL_VERSION:V03");
                switch (fileFormat)
                {
                    case FileFormat.DAT:
                        idxj?.WriteLine("FILE_FORMAT:DAT");
                        break;
                    case FileFormat.MAP:
                        idxj?.WriteLine("FILE_FORMAT:MAP");
                        break;
                    case FileFormat.UDAS:
                        idxj?.WriteLine("FILE_FORMAT:UDAS");
                        break;
                    case FileFormat.DAS:
                        idxj?.WriteLine("FILE_FORMAT:DAS");
                        break;
                    default:
                        idxj?.WriteLine("FILE_FORMAT:NULL");
                        break;
                }

                string directory = info.Directory.FullName;
                string baseName = Path.GetFileNameWithoutExtension(info.Name);
                if (baseName.Length == 0)
                {
                    baseName = "NULL";
                }

                if (fileFormat == FileFormat.DAT || fileFormat == FileFormat.MAP)
                {
                    try
                    {
                        Dat a = new Dat(idxj, stream, 0, (uint)info.Length, directory, baseName);

                        // .idx
                        idx_?.Write("FileCount = " + a.DatAmount);
                        for (int i = 0; i < a.DatFiles.Length; i++)
                        {
                            idx_?.Write(Environment.NewLine + "File_" + i + " = " + a.DatFiles[i]);
                        }

                        //Console
                        Console.WriteLine("FileCount = " + a.DatAmount);
                        for (int i = 0; i < a.DatFiles.Length; i++)
                        {
                            Console.WriteLine("File_" + i + " = " + a.DatFiles[i]);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex);
                    } 

                }

                else if (fileFormat == FileFormat.UDAS || fileFormat == FileFormat.DAS)
                {
                    try
                    {
                        Udas a = new Udas(idxj, stream, directory, baseName);
                        
                        // .idx
                        int Amount = a.DatAmount;
                        if (a.SndPath != null)
                        {
                            Amount += 1;
                        }
                        idx_?.Write("FileCount = " + Amount);
                        idx_?.Write(Environment.NewLine + "SoundFlag = " + a.SoundFlag);

                        for (int i = 0; i < a.DatFiles.Length; i++)
                        {
                            idx_?.Write(Environment.NewLine + "File_" + i + " = " + a.DatFiles[i]);
                        }

                        if (a.SndPath != null)
                        {
                            idx_?.Write(Environment.NewLine + "File_" + (Amount - 1) + " = " + a.SndPath);
                        }

                        //Console
                        Console.WriteLine("FileCount = " + Amount);
                        Console.WriteLine("SoundFlag = " + a.SoundFlag);
                        for (int i = 0; i < a.DatFiles.Length; i++)
                        {
                            Console.WriteLine("File_" + i + " = " + a.DatFiles[i]);
                        }
                        if (a.SndPath != null)
                        {
                            Console.WriteLine("File_" + (Amount - 1) + " = " + a.SndPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex);
                    }
                    
                }

                stream.Close();
                idxj?.Close();
                idx_?.Close();
            }
        }


    }
}
