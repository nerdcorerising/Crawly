using Crawly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Crawly.HTML;

namespace CrawlyRunner
{
    class Program
    {
        private static LinkExtractor s_extractor;

        static void Main(string[] args)
        {
            CrawlerSettings settings = new CrawlerSettings()
            {
                Function = MyFunction,
                OutputPath = "Sample.txt",
                RespectRobots = true,
                Seeds = new string[] { @"http://5by5.tv/hypercritical" },
                MaxDepth = 8,
                WorkerCount = 64
            };

            IEnumerable<string> banedExts = new string[] 
            {
                // images
                ".mng", ".pct", ".bmp", ".gif", ".jpg", ".jpeg", ".png", ".pst", ".psp", ".tif",
                ".tiff", ".ai", ".drw", ".dxf", ".eps", ".ps", ".svg",

                // audio
                ".mp3", ".wma", ".ogg", ".wav", ".ra", ".aac", ".mid", ".au", ".aiff",

                // video
                ".3gp", ".asf", ".asx", ".avi", ".mov", ".mp4", ".mpg", ".qt", ".rm", ".swf", ".wmv",
                ".m4a",

                //other
                ".css", ".pdf", ".doc", ".exe", ".bin", ".rss", ".zip", ".rar"
            };

            IEnumerable<string> bannedUrls = new string[]
            {
                "twitter.com",
                "youtube.com",
                "reddit.com",
                "facebook.com",
                "amazon.com",
                "itunes.apple.com",
                "firstpost.com",
                "wikipedia.org",
                "play.google.com",
                "pinterest.com"
            };

            s_extractor = new LinkExtractor(banedExts, bannedUrls);

            Crawler crawler = new Crawler(settings);
            crawler.Crawl();
        }

        private static void MyFunction(HtmlDocument doc, Uri docUri, out List<string> foundItems, out List<Uri> newUris)
        {
            foundItems = new List<string>();
            newUris = new List<Uri>();

            HtmlNodeCollection rssNodes = doc.DocumentNode.SelectNodes("//link[(@type='application/rss+xml' or @type='application/atom+xml') and @rel='alternate']");
            if (rssNodes != null)
            {
                foreach (var rss in rssNodes)
                {
                    try
                    {
                        Uri rssUri = new Uri(docUri, rss.Attributes["href"].Value);
                        foundItems.Add(rssUri.ToString());
                    }
                    catch
                    {

                    }
                }
            }

            foreach (Uri link in s_extractor.ExtractLinks(docUri, doc))
            {
                newUris.Add(link);
            }
        }
    }
}
