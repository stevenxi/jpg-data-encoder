using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace JpgDataEncoder
{
    static class JpegEncodeUtil
    {
        public static void Encode(FileInfo jpgFile, FileInfo dataFile, FileInfo outputFile)
        {
            using (var writer = File.Create(outputFile.FullName))
            {
                long jpgFileLength;
                using (var input = File.OpenRead(jpgFile.FullName))
                {
                    jpgFileLength = input.Length;
                    input.CopyTo(writer);
                }

                writer.Flush();

                using (var input = File.OpenRead(dataFile.FullName))
                    input.CopyTo(writer);

                writer.Flush();

                byte[] jpgFileLengthByte = BitConverter.GetBytes(jpgFileLength); //8bytes
                writer.Write(jpgFileLengthByte, 0, jpgFileLengthByte.Length);
            }
        }

        public static void Decode(FileInfo combinedFile, FileInfo jpgFile, FileInfo dataFile)
        {
            using (var reader = File.OpenRead(combinedFile.FullName))
            {
                reader.Seek(-8, SeekOrigin.End);
                var jpgFileLengthBytes = new byte[8];
                reader.Read(jpgFileLengthBytes, 0, 8);

                var jpgLength = BitConverter.ToInt64(jpgFileLengthBytes, 0);
                var length = jpgLength;

                reader.Seek(0, SeekOrigin.Begin);


                var buffer = new byte[32768];
                using (var writer = File.Create(jpgFile.FullName))
                {
                    int read;
                    while ((read = reader.Read(buffer, 0, (int)Math.Min(buffer.Length, length))) > 0)
                    {
                        writer.Write(buffer, 0, read);
                        length -= read;
                    }
                }

                length = combinedFile.Length - jpgLength - 8;

                using (var writer = File.Create(dataFile.FullName))
                {
                    int read;
                    while ((read = reader.Read(buffer, 0, (int)Math.Min(buffer.Length, length))) > 0)
                    {
                        writer.Write(buffer, 0, read);
                        length -= read;
                    }
                }

            }
        }

    }
}
