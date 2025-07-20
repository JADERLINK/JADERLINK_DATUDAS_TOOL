using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace DATUDAS_REPACK
{
    internal class Dat
    {

        public Dat(Stream stream, DatInfo[] dat, long StartOffset)
        {
            stream.Position = StartOffset;
            byte[] headerCont = new byte[16];
            BitConverter.GetBytes((uint)dat.Length).CopyTo(headerCont, 0); //Amount
            stream.Write(headerCont, 0, headerCont.Length);

            for (int i = 0; i < dat.Length; i++)
            {
                byte[] offset = BitConverter.GetBytes(dat[i].Offset);
                stream.Write(offset, 0, 4);
            }

            for (int i = 0; i < dat.Length; i++)
            {
                byte[] name = Encoding.ASCII.GetBytes(dat[i].Extension);
                stream.Write(name, 0, 4);
            }

            for (int i = 0; i < dat.Length; i++)
            {
                stream.Position = StartOffset + dat[i].Offset;

                try
                {
                    if (dat[i].FileExits)
                    {
                        var reader = dat[i].fileInfo.OpenRead();
                        reader.CopyTo(stream);
                        reader.Close();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error to read file: " + dat[i].fileInfo.Name);
                    Console.WriteLine(ex);
                }
            }

        }

    }


    internal class DatInfo
    {
        public string Path = null;
        public FileInfo fileInfo = null;
        public string Extension = null;
        public int Length = 0;
        public uint Offset = 0;
        public bool FileExits = false;
    }


}
