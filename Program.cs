using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VevoToTS
{
    class Program
    {
        static void Main()
        {
            //Retrieve the arguments being passed into the application
            List<string> args = Environment.GetCommandLineArgs().Parse();

            //Show usage if the user did not pass any arguments or specifically passed in the usage argument
            if ((args.Count == 1) || (args.Contains("/?"))) { ArgumentHandler.showUsage(); return; }

            //Show version information if the user passed in the correct argument
            if (args.Contains("/V")) { ArgumentHandler.showVersion(); return; }
            
            VevoRendition vevoRendition = Vevo.GetBestVevoRendition("USUV71501692");

            if (vevoRendition.Url.EndsWith("m3u8"))
            {
                VevoHttpStreamingUrl bestVevoHttpStreamingUrl = Vevo.GetBestVevoHttpStreamingUrl(vevoRendition.Url);
                List<string> transportFileList = Vevo.GetVevoTransportFileList(bestVevoHttpStreamingUrl.Url);

                deleteFiles(concatenateFiles(downloadFiles(transportFileList), "Alright.ts"));
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
