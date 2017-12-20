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
                Seeds = new string[] { @"http://5by5.tv/", @"http://maximumfun.org/", @"https://www.relay.fm/" },
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

            IEnumerable<HtmlNode> nodes = doc.DocumentNode.Descendants();// SelectNodes("//link[(@type='application/rss+xml' or @type='application/atom+xml') and @rel='alternate']");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    if (node.HasAttributes)
                    {
                        var href = node.Attributes["href"];
                        var type = node.Attributes["type"];
                        var rel = node.Attributes["rel"];

                        if (href != null && type != null 
                            && rel != null && IsRss(type, rel))
                        {
                            try
                            {
                                Uri rssUri = new Uri(docUri, href.Value);
                                foundItems.Add(rssUri.ToString());
                            }
                            catch
                            {

                            }

                        }
                    }
                }
            }

            foreach (Uri link in s_extractor.ExtractLinks(docUri, doc))
            {
                newUris.Add(link);
            }
        }

        private static bool IsRss(HtmlAttribute type, HtmlAttribute rel)
        {
            //application/rss+xml' or @type='application/atom+xml
            bool isRss = rel.Value.Equals("alternate", StringComparison.InvariantCultureIgnoreCase)
                && (type.Value.Equals("application/rss+xml", StringComparison.InvariantCultureIgnoreCase)
                    || type.Value.Equals("application /atom+xml", StringComparison.InvariantCultureIgnoreCase));

            return isRss;
        }
    }
}
