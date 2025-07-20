using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DATUDAS_TOOL
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("# JADERLINK_DATUDAS_TOOL");
            Console.WriteLine("# By: JADERLINK");
            Console.WriteLine("# youtube.com/@JADERLINK");
            Console.WriteLine("# github.com/JADERLINK");
            Console.WriteLine("# VERSION 1.0.4 (2025-07-20)");


            bool usingBatFile = false;
            bool CreateIdx = false;
            bool CreateIdxJ = true;
            int start = 0;
            for (int i = 0; i < args.Length && i < 2; i++)
            {
                if (args[i].ToLowerInvariant() == "-bat")
                {
                    usingBatFile = true;
                    start++;
                }
                else if (args[i].ToLowerInvariant() == "-idx")
                {
                    CreateIdx = true;
                    CreateIdxJ = false;
                    start++;
                }
                else if (args[i].ToLowerInvariant() == "-all")
                {
                    CreateIdxJ = true;
                    CreateIdx = true;
                    start++;
                }
            }

          
            for (int i = start; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                {
                    try
                    {
                        Action(args[i], CreateIdx, CreateIdxJ);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error: " + args[i]);
                        Console.WriteLine(ex);
                    }
                }
                else
                {
                    Console.WriteLine("File specified does not exist: " + args[i]);
                }

            }


            if (args.Length == 0)
            {
                Console.WriteLine("How to use: drag the file to the executable.");
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/JADERLINK_DATUDAS_TOOL");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("Finished!!!");
                if (!usingBatFile)
                {
                    Console.WriteLine("Press any key to close the console.");
                    Console.ReadKey();
                }
            }

        }

        private static void Action(string file, bool CreateIdx, bool CreateIdxJ)
        {
            var fileInfo = new FileInfo(file);
            Console.WriteLine();
            Console.WriteLine("File: " + fileInfo.Name);
            var Extension = fileInfo.Extension.ToUpperInvariant();

            if (Extension == ".DAT" || Extension == ".MAP" || Extension == ".UDAS")
            {
                Console.WriteLine("Extract Mode!");

                DATUDAS_EXTRACT.FileFormat fileFormat = DATUDAS_EXTRACT.FileFormat.Null;
                switch (Extension)
                {
                    case ".DAT": fileFormat = DATUDAS_EXTRACT.FileFormat.DAT; break;
                    case ".MAP": fileFormat = DATUDAS_EXTRACT.FileFormat.MAP; break;
                    case ".UDAS": fileFormat = DATUDAS_EXTRACT.FileFormat.UDAS; break;
                }

                if (fileFormat != DATUDAS_EXTRACT.FileFormat.Null)
                {
                    _ = new DATUDAS_EXTRACT.Extract(fileInfo, fileFormat, CreateIdx, CreateIdxJ);
                }
            }
            else if (Extension == ".IDXJ")
            {
                Console.WriteLine("Repack Mode!");

                _ = new DATUDAS_REPACK.RepackJ(fileInfo);

            }
            else if (Extension == ".IDX")
            {
                Console.WriteLine("Repack Mode!");

                _ = new DATUDAS_REPACK.RepackIdx(fileInfo);
            }
            else
            {
                Console.WriteLine("The extension is not valid: " + Extension);
            }
        }


    }
}
