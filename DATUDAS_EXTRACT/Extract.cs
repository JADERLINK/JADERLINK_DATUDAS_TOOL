using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JADERLINK_DATUDAS_EXTRACT
{
    internal class Extract
    {
        public Extract(FileInfo info, FileFormat fileFormat) 
        {
            FileStream stream = null;
            StreamWriter idxj = null;
            StreamWriter idx_ = null;

            try
            {
                string idxjFileName = info.FullName.Substring(0, info.FullName.Length - info.Extension.Length) + ".idxJ";
                string idx_FileName = info.FullName.Substring(0, info.FullName.Length - info.Extension.Length) + ".idx";
                FileInfo idxjInfo = new FileInfo(idxjFileName);
                FileInfo idx_Info = new FileInfo(idx_FileName);

                stream = info.OpenRead();
                idxj = idxjInfo.CreateText();
                if (fileFormat != FileFormat.MAP)
                {
                    idx_ = idx_Info.CreateText();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
            }

            if (stream != null && idxj != null)
            {
                idxj.WriteLine("# github.com/JADERLINK/JADERLINK_DATUDAS_TOOL");
                idxj.WriteLine("# youtube.com/@JADERLINK");
                idxj.WriteLine("# JADERLINK DATUDAS TOOL By JADERLINK");
                idxj.WriteLine("TOOL_VERSION:V02");
                switch (fileFormat)
                {
                    case FileFormat.DAT:
                        idxj.WriteLine("FILE_FORMAT:DAT");
                        break;
                    case FileFormat.MAP:
                        idxj.WriteLine("FILE_FORMAT:MAP");
                        break;
                    case FileFormat.UDAS:
                        idxj.WriteLine("FILE_FORMAT:UDAS");
                        break;
                    default:
                        idxj.WriteLine("FILE_FORMAT:NULL");
                        break;
                }

                string diretory = info.Directory.FullName + "\\";
                string basename = info.Name.Substring(0, info.Name.Length - info.Extension.Length);
                if (basename.Length == 0)
                {
                    basename = "NULL";
                }

                if (fileFormat == FileFormat.DAT || fileFormat == FileFormat.MAP)
                {
                    try
                    {
                        Dat a = new Dat(idxj, stream, 0, (int)info.Length, diretory, basename);

                        if (fileFormat != FileFormat.MAP)
                        {
                            // .idx
                            idx_.Write("FileCount = " + a.DatAmount);
                            for (int i = 0; i < a.DatFiles.Length; i++)
                            {
                                idx_.Write(Environment.NewLine + "File_" + i + " = " + a.DatFiles[i]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex);
                    } 

                }

                else if (fileFormat == FileFormat.UDAS)
                {
                    try
                    {
                        Udas a = new Udas(idxj, stream, diretory, basename);
                        
                        // .idx
                        int Amount = a.DatAmount;
                        if (a.DasSndPatch != null)
                        {
                            Amount += 1;
                        }
                        idx_.Write("FileCount = " + Amount);
                        idx_.Write(Environment.NewLine + "SoundFlag = " + a.SoundFlag);

                        for (int i = 0; i < a.DatFiles.Length; i++)
                        {
                            idx_.Write(Environment.NewLine + "File_" + i + " = " + a.DatFiles[i]);
                        }

                        if (a.DasSndPatch != null)
                        {
                            idx_.Write(Environment.NewLine + "File_" + (Amount - 1) + " = " + a.DasSndPatch);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + ex);
                    }
                    
                }

                stream.Close();
                idxj.WriteLine("# END_FILE");
                idxj.Close();
                if (fileFormat != FileFormat.MAP)
                {
                    idx_.Close();
                }

            }
        }


    }
}
