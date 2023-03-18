using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JADERLINK_DATUDAS_EXTRACT
{
    class Program
    {
        /*
        program.exe
        "Caminho do arquivo"
        */

        static void Main(string[] args)
        {
            Console.WriteLine("##############################");
            Console.WriteLine("### JADERLINK DATUDAS TOOL ###");
            Console.WriteLine("### VERSION 1.0.0.1        ###");
            Console.WriteLine("##############################");

            if (args.Length > 0)
            {
                string file = args[0];
                FileInfo info = null;

                try
                {
                    info = new FileInfo(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in the directory: " +Environment.NewLine + ex);
                }

                if (info != null)
                {
                    if (info.Exists)
                    {
                        if (info.Extension.ToUpperInvariant() == ".DAT"
                         || info.Extension.ToUpperInvariant() == ".MAP"
                         || info.Extension.ToUpperInvariant() == ".UDAS"
                         )
                        {
                            FileFormat fileFormat = FileFormat.Null;
                            switch (info.Extension.ToUpperInvariant())
                            {
                                case ".DAT": fileFormat = FileFormat.DAT; break;
                                case ".MAP": fileFormat = FileFormat.MAP; break;
                                case ".UDAS": fileFormat = FileFormat.UDAS; break;
                                default:
                                    break;
                            }

                            Console.WriteLine("File: " + info.Name);


                            if (fileFormat != FileFormat.Null)
                            {
                                _ = new Extract(info, fileFormat);
                            }
                            else
                            {
                                Console.WriteLine("The extension was not detected: " + info.Extension);
                            }
                        }
                        else
                        {
                            Console.WriteLine("The extension is not valid: " + info.Extension);
                        }
                    }
                    else
                    {
                        Console.WriteLine("File specified does not exist.");
                    }
                }
            }
            else 
            {
                Console.WriteLine("Unspecified file directory.");
            }

            Console.WriteLine("Finished!!!");
            Console.WriteLine("");

            //Console.ReadKey();//remover no final
        }
    }
}
