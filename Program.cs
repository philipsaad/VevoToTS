﻿using System.Collections.Generic;
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

                deleteFiles(concatenateFiles(downloadFiles(transportFileList), options.OutputFile));
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
                foreach (string transportStreamFile in transportStreamFiles)
                {
                    string tempFile = Path.GetTempFileName();
                    webClient.DownloadFile(transportStreamFile, tempFile);
                    downloadedTransportStreamFiles.Add(tempFile);
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
