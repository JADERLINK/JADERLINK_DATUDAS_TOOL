using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JADERLINK_DATUDAS_REPACK
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("# JADERLINK DATUDAS REPACK TOOL");
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
                    Console.WriteLine("File: " + info.Name);

                    if (info.Extension.ToUpperInvariant() == ".IDXJ")
                    {
                        try
                        {
                            _ = new RepackJ(info);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex);
                        }

                    }
                    else if (info.Extension.ToUpperInvariant() == ".IDX")
                    {
                        try
                        {
                            _ = new RepackIdx(info);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error: " + ex);
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