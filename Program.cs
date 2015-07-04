using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

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

            if (vevoRendition.Url.EndsWith("m3u8"))
            {
                VevoHttpStreamingUrl bestVevoHttpStreamingUrl = Vevo.GetBestVevoHttpStreamingUrl(vevoRendition.Url);
                List<string> transportFileList = Vevo.GetVevoTransportFileList(bestVevoHttpStreamingUrl.Url);
                
                deleteFiles(concatenateFiles(downloadFiles(transportFileList), options.OutputFile ?? vevoRendition.FileName));

                Console.Write("\r{0}", "".PadRight(60, ' '));
                Console.WriteLine("\rFile saved as: \"" + (options.OutputFile ?? vevoRendition.FileName) + "\"");
            }
        }

        static List<string> concatenateFiles(List<string> transportStreamFiles, string outputFile)
        {
            using (Stream outputStream = File.OpenWrite(outputFile))
            {
                foreach (string transportStreamFile in transportStreamFiles)
                {
                    using (Stream transportStream = File.OpenRead(transportStreamFile))
                    {
                        transportStream.CopyTo(outputStream);
                    }
                }
            }

            return transportStreamFiles;
        }

        static List<string> downloadFiles(List<string> transportStreamFiles)
        {
            List<string> downloadedTransportStreamFiles = new List<string>();

            using (var webClient = new WebClient())
            {
                int count = 1;
                decimal total = transportStreamFiles.Count;

                foreach (string transportStreamFile in transportStreamFiles)
                {
                    string percent = (count / total).ToString("p");
                    Console.Write("\r{0}", "".PadRight(60, ' '));
                    Console.Write("\rDownloading file " + count + " of " + total + ". (" + percent + ")");

                    string tempFile = Path.GetTempFileName();
                    webClient.DownloadFile(transportStreamFile, tempFile);
                    downloadedTransportStreamFiles.Add(tempFile);

                    count++;
                }
            }

            return downloadedTransportStreamFiles;
        }

        static void deleteFiles(List<string> transportStreamFiles)
        {
            foreach (string transportStreamFile in transportStreamFiles)
            {
                File.Delete(transportStreamFile);
            }
        }
    }
}
