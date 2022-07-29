using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

namespace JpgDataEncoder
{
    static class JpegEncodeUtil
    {
        public static void GenerateJpg(FileInfo jpgFile, string text)
        {
            var font = new Font("Arial", 12);

            SizeF textSize;
            using (var img = new Bitmap(1, 1))
            using (var drawing = Graphics.FromImage(img))
                textSize = drawing.MeasureString(text, font);


            //create a new image of the right size
            using (var img = new Bitmap((int)textSize.Width, (int)textSize.Height))
            using (var drawing = Graphics.FromImage(img))
            {

                drawing.Clear(Color.White);

                //create a brush for the text
                using (var textBrush = new SolidBrush(Color.Black))
                {
                    drawing.DrawString(text, font, textBrush, 0, 0);

                    drawing.Save();
                }

                using (var writer = File.Create(jpgFile.FullName))
                    img.Save(writer, ImageFormat.Jpeg);
            }
        }


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
