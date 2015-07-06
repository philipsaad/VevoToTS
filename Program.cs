using System;
using System.Collections.Generic;

namespace VevoToTS
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new Options();
            if (!CommandLine.Parser.Default.ParseArguments(args, options))
            {
                return;
            }

            VevoRendition vevoRendition = Vevo.GetBestVevoRendition(options.VideoId);

            int downloadThreads = options.DownloadThreads ?? 1;
            if (downloadThreads == 0) { downloadThreads = -1; }

            if (vevoRendition.Url.EndsWith("m3u8"))
            {
                FileOperations.DeleteFiles(FileOperations.ConcatenateFiles(FileOperations.DownloadFiles(Vevo.GetVevoTransportFileList(Vevo.GetBestVevoHttpStreamingUrl(vevoRendition.Url).Url), downloadThreads), options.OutputFile ?? vevoRendition.FileName));

                Console.Write("\r{0}", "".PadRight(60, ' '));
                Console.WriteLine("\rFile saved as: \"" + (options.OutputFile ?? vevoRendition.FileName) + "\"");
            }
        }
    }
}
