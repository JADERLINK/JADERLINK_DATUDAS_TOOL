using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JADERLINK_DATUDAS_EXTRACT
{
    internal class Dat
    {
        public int DatAmount = 0;
        public string[] DatFiles = null;

        public Dat(StreamWriter idxj , Stream readStream, int offsetStart, int length, string diretory, string basename) 
        {
            readStream.Position = offsetStart;
            byte[] amountB = new byte[4];
            readStream.Read(amountB, 0 , 4);
            int amount = BitConverter.ToInt32(amountB, 0);
            Console.WriteLine("Dat Amount: " + amount);
            idxj.WriteLine("DAT_AMOUNT:" + amount);
            DatAmount = amount;

            int blocklenght = amount * 4;

            byte[] offsetblock = new byte[blocklenght];
            byte[] nameblock = new byte[blocklenght];

            readStream.Position = offsetStart + 16;

            readStream.Read(offsetblock, 0, blocklenght);
            readStream.Read(nameblock, 0, blocklenght);

           
            KeyValuePair<int, string>[] fileList = new KeyValuePair<int, string>[amount];

            int Temp = 0;
            for (int i = 0; i < amount; i++)
            {
                int offset = BitConverter.ToInt32(offsetblock, Temp);
                string format = Encoding.ASCII.GetString(nameblock, Temp, 4);
                format = ValidateFormat(format).ToUpperInvariant();

                string FileFullName = basename + "\\" + basename + "_" + i.ToString("D3");
                if (format.Length > 0)
                {
                    FileFullName += "." + format;
                }

                fileList[i] = new KeyValuePair<int, string>(offset, FileFullName);

                Temp += 4;
            }

            if (!Directory.Exists(diretory + basename))
            {
                try
                {
                    Directory.CreateDirectory(diretory + basename);
                }
                catch (Exception)
                {
                    Console.WriteLine("Failed to create directory: " + diretory + basename);
                }
               
            }

            DatFiles = new string[amount];

            for (int i = 0; i < fileList.Length; i++)
            {
                DatFiles[i] = fileList[i].Value;

                int subFileLenght = 0;
                if (i < fileList.Length - 1)
                {
                    subFileLenght = fileList[i + 1].Key - fileList[i].Key;
                }
                else 
                {
                    subFileLenght = length - fileList[i].Key;
                }

                readStream.Position = offsetStart + fileList[i].Key;

                byte[] endfile = new byte[subFileLenght];
                readStream.Read(endfile, 0, subFileLenght);
                if (subFileLenght > 0)
                {
                    try
                    {
                        File.WriteAllBytes(diretory + fileList[i].Value, endfile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(fileList[i].Value + ": " + ex);
                    }
                  
                }

                string Line = "DAT_" + i.ToString("D3") + ":" + fileList[i].Value;
                idxj.WriteLine(Line);
            }

        }

        private string ValidateFormat(string source) 
        {
            string res = "";
            for (int i = 0; i < source.Length; i++)
            {
                if ((source[i] >= 65 && source[i] <= 90)
                 || (source[i] >= 97 && source[i] <= 122)
                 || (source[i] >= 48 && source[i] <= 57))
                {
                    res += source[i];
                }
            }
            return res;
        }

    }
}
