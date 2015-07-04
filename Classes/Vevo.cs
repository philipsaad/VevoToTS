using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace VevoToTS
{
    class Vevo
    {
        public static List<VevoRendition> GetVevoRenditions(string videoID)
        {
            //TODO Check validity of videoID

            string videoUrl = "https://api.vevo.com/VideoService/AuthenticateVideo?isrc=" + videoID;

            List<VevoRendition> results = new List<VevoRendition>();

            var wc = new WebClient();
            string videoData = wc.DownloadString(videoUrl);

            JObject videoInfo = JObject.Parse(videoData);
            JToken videoVersions = videoInfo["video"]["videoVersions"];

            foreach (JToken videoVersion in videoVersions)
            {
                int version = (int)videoVersion["version"];
                int sourceType = (int)videoVersion["sourceType"];
                string id = (string)videoVersion["id"];
                string name = String.Empty;
                string url = String.Empty;
                string extension = String.Empty;

                string fileName = GetFileName(videoInfo);

                string renditionData = (string)videoVersion["data"];
                if (!String.IsNullOrWhiteSpace(renditionData))
                {
                    XDocument renditionsXml = XDocument.Parse(renditionData);

                    IEnumerable<XElement> renditionsXElement = renditionsXml.Descendants("renditions").Descendants("rendition");

                    foreach (XElement renditionXElement in renditionsXElement)
                    {
                        name = renditionXElement.Attribute("name").Value;
                        url = renditionXElement.Attribute("url").Value;
                        //TODO
                        // totalBitrate="56" videoBitrate="56" audioBitrate="128" frameWidth="176" frameheight="144" videoCodec="H264" audioCodec="AAC" audioSampleRate="44100" />

                        if (url.EndsWith(".mp4"))
                        {
                            extension = ".mp4";
                        }
                        else
                        {
                            extension = ".ts";
                        }

                        VevoRendition vr = new VevoRendition()
                        {
                            FileName = fileName + extension,
                            Version = version,
                            SourceType = sourceType,
                            ID = id,
                            Name = name,
                            Url = url,
                        };

                        results.Add(vr);
                    }
                }

            }

            return results;
        }

        private static string GetFileName(JObject videoInfo)
        {
            string fileName = String.Empty;

            string title = videoInfo["video"]["title"].ToString();
            IEnumerable<string> mainArtists = videoInfo["video"]["mainArtists"].Values<string>("artistName");
            IEnumerable<string> featuredArtists = videoInfo["video"]["featuredArtists"].Values<string>("artistName");

            int commaIndex = -1;

            fileName = string.Join(", ", mainArtists);

            commaIndex = fileName.LastIndexOf(",");
            if (commaIndex > -1)
            {
                fileName = fileName.Substring(0, commaIndex) + " &" + fileName.Substring(commaIndex + 1);
            }

            fileName += " - " + title;

            if (featuredArtists.Count() > 0)
            {
                fileName += " (feat. " + string.Join(", ", featuredArtists) + ")";

                commaIndex = fileName.LastIndexOf(",");
                if (commaIndex > -1)
                {
                    fileName = fileName.Substring(0, commaIndex) + " &" + fileName.Substring(commaIndex + 1);
                }
            }

            return fileName;
        }

        public static VevoRendition GetBestVevoRendition(string videoID)
        {
            List<VevoRendition> vevoRenditions = GetVevoRenditions(videoID).ToList();

            VevoRendition result = vevoRenditions.Where(vr => vr.Name == "HTTP Live Streaming").OrderByDescending(vr => vr.Version).OrderByDescending(vr => vr.SourceType).FirstOrDefault();

            return result;
        }

        public static List<VevoHttpStreamingUrl> GetVevoHttpStreamingUrls(string httpStreamingUrl)
        {
            List<VevoHttpStreamingUrl> result = new List<VevoHttpStreamingUrl>();

            var wc = new WebClient();
            string m3uData = wc.DownloadString(httpStreamingUrl);

            List<string> m3uDataArray = m3uData.Split(new string[] { Environment.NewLine }, StringSplitOptions.None).ToList();

            string rootUrl = httpStreamingUrl.Substring(0, httpStreamingUrl.LastIndexOf("/") + 1);

            string previousM3uLine = String.Empty;

            foreach (string m3uLine in m3uDataArray)
            {
                if (!String.IsNullOrWhiteSpace(m3uLine) && previousM3uLine.StartsWith("#EXT-X-STREAM-INF"))
                {
                    string programID = string.Empty;
                    int bandwidth = 0;
                    string resolution = String.Empty;
                    string videoCodec = String.Empty;
                    string audioCodec = String.Empty;
                    string url = String.Empty;

                    string[] urlInfo = previousM3uLine.Split(',');

                    bandwidth = Int32.Parse(new Regex("BANDWIDTH=([\\d]+)").Match(previousM3uLine).Groups[1].Value);
                    resolution = new Regex("RESOLUTION=([a-z0-9]+)").Match(previousM3uLine).Groups[1].Value;
                    //TODO seperate resolutions, add ProgramID, fix videoCodec and audioCodec when ProgramID exists.
                    videoCodec = urlInfo[2].Substring(urlInfo[2].IndexOf("=") + 2);
                    audioCodec = urlInfo[3].Substring(0, urlInfo[3].Length - 1);

                    url = rootUrl + m3uLine;

                    VevoHttpStreamingUrl vhsu = new VevoHttpStreamingUrl()
                    {
                        Bandwidth = bandwidth,
                        Resolution = resolution,
                        VideoCodec = videoCodec,
                        AudioCodec = audioCodec,
                        Url = url,
                    };

                    result.Add(vhsu);
                }

                previousM3uLine = m3uLine;
            }

            return result;
        }

        public static VevoHttpStreamingUrl GetBestVevoHttpStreamingUrl(string httpStreamingUrl)
        {
            VevoHttpStreamingUrl result = GetVevoHttpStreamingUrls(httpStreamingUrl).OrderByDescending(hsu => hsu.Bandwidth).FirstOrDefault();
            return result;
        }

        public static List<string> GetVevoTransportFileList(string transportFileUrl)
        {
            List<string> result = new List<string>();

            var wc = new WebClient();
            string m3uData = wc.DownloadString(transportFileUrl);

            List<string> m3uDataArray = m3uData.Split(new string[] { "\n" }, StringSplitOptions.None).ToList();

            string rootUrl = transportFileUrl.Substring(0, transportFileUrl.LastIndexOf("/") + 1);

            foreach (string m3uLine in m3uDataArray)
            {
                if (!String.IsNullOrWhiteSpace(m3uLine) && !m3uLine.StartsWith("#"))
                {
                    result.Add(rootUrl + m3uLine);
                }

            }

            return result;
        }
    }

    public class VevoRendition
    {
        public int Version { get; set; }
        public int SourceType { get; set; }
        public string ID { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public int? TotalBitrate { get; set; }
        public int? VideoBitrate { get; set; }
        public int? AudioBitrate { get; set; }
        public int? FrameWidth { get; set; }
        public int? Frameheight { get; set; }
        public string VideoCodec { get; set; }
        public string AudioCodec { get; set; }
        public int? AudioSampleRate { get; set; }
        public string FileName { get; set; }
    }

    public class VevoHttpStreamingUrl
    {
        public string ProgramID { get; set; }
        public int Bandwidth { get; set; }
        public string Resolution { get; set; }
        public string VideoCodec { get; set; }
        public string AudioCodec { get; set; }
        public string Url { get; set; }
    }
}
