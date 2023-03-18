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
                    Console.WriteLine("Error in the directory: " + Environment.NewLine + ex);
                }

                if (info != null)
                {
                    if (info.Exists)
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