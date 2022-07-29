using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;

namespace JpgDataEncoder
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            var  rootCommand = new RootCommand("Converts an image file from one format to another.");

            var actionOption = new Option<string>(new[] { "--action", "-a" }, "The action, [E]ncode or [D]ecode.");
            rootCommand.Add(actionOption);

            var pathOption = new Option<string>(new[] { "--path", "-p" }, "The path to the image file that is to be converted.");
            rootCommand.Add(pathOption);
            var extensionOption = new Option<string>(new[] { "--extension", "-e" }, "Combine extension (without .)");
            rootCommand.Add(extensionOption);


            rootCommand.SetHandler((actionOptionValue, pathOptionValue, extensionOptionValue) =>
            {
                if (string.Equals(actionOptionValue, "e", StringComparison.InvariantCultureIgnoreCase))
                {
                    Encode(pathOptionValue, extensionOptionValue);
                }
                else if (string.Equals(actionOptionValue, "d", StringComparison.InvariantCultureIgnoreCase))
                {
                    Decode(pathOptionValue, extensionOptionValue);
                }
                else
                {
                    Console.WriteLine($"Invalid option: {actionOptionValue}");
                }
            }, actionOption, pathOption, extensionOption);
            await rootCommand.InvokeAsync(args);

        }


        private static void Encode(string path, string extension)
        {

            var dataFiles = Directory.GetFiles(path, $"*.{extension}", SearchOption.AllDirectories);

            var extensionLen = extension.Length + 1;
            foreach(var dataFile in dataFiles)
            {
                var dataFileInfo = new FileInfo(dataFile);

                var jpgFile = dataFileInfo.FullName.Substring(0, dataFileInfo.FullName.Length - extensionLen) + ".jpg";
                var jpgFileInfo = new FileInfo(jpgFile);
                if (!jpgFileInfo.Exists)
                {
                    jpgFile = dataFileInfo.FullName.Substring(0, dataFileInfo.FullName.Length - extensionLen) + ".jpeg";
                    jpgFileInfo = new FileInfo(jpgFile);
                }
                if (!jpgFileInfo.Exists)
                {
                    Console.WriteLine($"Unable to find jpg file, skip: {dataFile}");
                    continue;
                }

                var combinedFile = dataFileInfo.FullName.Substring(0, dataFileInfo.FullName.Length - extensionLen) + $".C_{extension}_.jpg";
                var combinedFileInfo = new FileInfo(combinedFile);

                if (combinedFileInfo.Exists)
                {
                    Console.WriteLine($"Combined file exists, skip: {dataFile}");
                    continue;
                }
                try
                {
                    JpegEncodeUtil.Encode(jpgFileInfo, dataFileInfo, combinedFileInfo);
                    Console.WriteLine($"Combined file: {dataFile}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to combine file {dataFile}: {ex.Message}");
                }
            }
        }

        private static void Decode(string path, string extension)
        {
            var combinedFiles = Directory.GetFiles(path, $"*.C_{extension}_.jpg", SearchOption.AllDirectories);

            var extensionLen = $"*.C_{extension}_.jpg".Length;

            foreach (var combinedFile in combinedFiles)
            {
                var combinedFileInfo = new FileInfo(combinedFile);

                var jpgFile = combinedFileInfo.FullName.Substring(0, combinedFileInfo.FullName.Length - extensionLen) + ".jpg";
                var jpgFileInfo = new FileInfo(jpgFile);

                if (jpgFileInfo.Exists)
                {
                    Console.WriteLine($"JPG file exists, skip: {combinedFile}");
                    continue;
                }

                var dataFile = combinedFileInfo.FullName.Substring(0, combinedFileInfo.FullName.Length - extensionLen) + $".{extension}";
                var dataFileInfo = new FileInfo(dataFile);
                if (dataFileInfo.Exists)
                {
                    Console.WriteLine($"Data file exists, skip: {combinedFile}");
                    continue;
                }
                try
                {
                    JpegEncodeUtil.Decode(combinedFileInfo, jpgFileInfo, dataFileInfo);
                    Console.WriteLine($"Decoded file: {dataFile}");
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to decode file {combinedFile}: {ex.Message}");

                }
            }
        }
    }
}
