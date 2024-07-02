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
        static void Main(string[] args)
        {
            Console.WriteLine("# JADERLINK DATUDAS EXTRACT TOOL");
            Console.WriteLine("# VERSION 1.0.2");
            Console.WriteLine("# youtube.com/@JADERLINK");

            if (args.Length == 0)
            {
                Console.WriteLine("For more information read:");
                Console.WriteLine("https://github.com/JADERLINK/JADERLINK_DATUDAS_TOOL");
                Console.WriteLine("Press any key to close the console.");
                Console.ReadKey();
            }
            else if (args.Length > 0 && File.Exists(args[0]))
            {
                string file = args[0];
                FileInfo info = null;

                try
                {
                    info = new FileInfo(file);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in the path: " + file + Environment.NewLine + ex);
                }

                if (info != null)
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
                            try
                            {
                                _ = new Extract(info, fileFormat);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error: " + ex);
                            }
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
            }
            else
            {
                Console.WriteLine("File specified does not exist.");
            }

            Console.WriteLine("Finished!!!");
            Console.WriteLine("");

        }
    }
}
