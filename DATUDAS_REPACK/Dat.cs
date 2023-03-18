using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace JADERLINK_DATUDAS_REPACK
{
    internal class Dat
    {

        public Dat(FileStream stream, int DatHeaderLenght, DatInfo[] dat)
        {
            byte[] headerCont = new byte[16];
            byte[] Amount = BitConverter.GetBytes(dat.Length);
            headerCont[0] = Amount[0];
            headerCont[1] = Amount[1];
            headerCont[2] = Amount[2];
            headerCont[3] = Amount[3];
            stream.Write(headerCont, 0, 16);

            for (int i = 0; i < dat.Length; i++)
            {
                byte[] offset = BitConverter.GetBytes(dat[i].Offset);
                stream.Write(offset, 0, 4);
            }

            for (int i = 0; i < dat.Length; i++)
            {
                byte[] name = Encoding.UTF8.GetBytes(dat[i].Extension);
                stream.Write(name, 0, 4);
            }

            byte[] complete = new byte[DatHeaderLenght - (16 + (4 * dat.Length * 2))];

            if (complete.Length > 0)
            {
                stream.Write(complete, 0, complete.Length);
            }

            for (int i = 0; i < dat.Length; i++)
            {
                byte[] archive = new byte[dat[i].Length];
                try
                {
                    if (dat[i].FileExits)
                    {
                        archive = File.ReadAllBytes(dat[i].fileInfo.FullName);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error to read file: " + dat[i].fileInfo.Name + Environment.NewLine + " ex: " + ex);
                }
                stream.Write(archive, 0, archive.Length);
            }

        }

    }


    internal class DatInfo
    {
        public string Path = null;
        public FileInfo fileInfo = null;
        public string Extension = null;
        public int Length = 0;
        public int Offset = 0;
        public bool FileExits = false;
    }


}
