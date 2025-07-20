using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

    
namespace DATUDAS_EXTRACT
{
    internal class Dat
    {
        public int DatAmount = 0;
        public string[] DatFiles = null;

        public Dat(StreamWriter idxj , Stream readStream, uint offsetStart, uint length, string directory, string baseName) 
        {
            readStream.Position = offsetStart;
            BinaryReader br = new BinaryReader(readStream);
            int amount = br.ReadInt32();
            if (amount >= 0x010000)
            {
                Console.WriteLine("Invalid file!");
                return;
            }

            if (!Directory.Exists(Path.Combine(directory, baseName)))
            {
                try
                {
                    Directory.CreateDirectory(Path.Combine(directory, baseName));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to create directory: " + Path.Combine(directory, baseName));
                    Console.WriteLine(ex);
                    return;
                }
            }

            idxj?.WriteLine("DAT_AMOUNT:" + amount);
            DatAmount = amount;

            int blocklength = amount * 4;

            byte[] offsetblock = new byte[blocklength];
            byte[] nameblock = new byte[blocklength];

            readStream.Position = offsetStart + 16;

            readStream.Read(offsetblock, 0, blocklength);
            readStream.Read(nameblock, 0, blocklength);

            KeyValuePair<int, string>[] fileList = new KeyValuePair<int, string>[amount];

            int Temp = 0;
            for (int i = 0; i < amount; i++)
            {
                int offset = BitConverter.ToInt32(offsetblock, Temp);
                string format = Encoding.ASCII.GetString(nameblock, Temp, 4);
                format = ValidateFormat(format).ToUpperInvariant();

                string fullName = Path.Combine(baseName, baseName + "_" + i.ToString("D3"));
                if (format.Length > 0)
                {
                    fullName += "." + format;
                }

                fileList[i] = new KeyValuePair<int, string>(offset, fullName);

                Temp += 4;
            }

            DatFiles = new string[amount];

            for (int i = 0; i < fileList.Length; i++)
            {
                DatFiles[i] = fileList[i].Value;

                int subFileLength;
                if (i < fileList.Length - 1)
                {
                    subFileLength = fileList[i + 1].Key - fileList[i].Key;
                }
                else 
                {
                    subFileLength = (int)(length - fileList[i].Key);
                }

                readStream.Position = offsetStart + fileList[i].Key;

                byte[] endfile = new byte[subFileLength];
                readStream.Read(endfile, 0, subFileLength);
                if (subFileLength > 0)
                {
                    try
                    {
                        File.WriteAllBytes(Path.Combine(directory, fileList[i].Value), endfile);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(fileList[i].Value + ": " + ex);
                    }
                  
                }

                string Line = "DAT_" + i.ToString("D3") + ":" + fileList[i].Value;
                idxj?.WriteLine(Line);
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
