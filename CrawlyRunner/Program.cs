using Crawly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrawlyRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            CrawlerSettings settings = new CrawlerSettings()
            {
                OutputPath = "Sample.txt",
                RespectRobots = true,
                Seeds = new string[] { @"http://5by5.tv/hypercritical" },
                MaxDepth = 8,
                WorkerCount = 64,
                BannedExtensions = new string[] {
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
                }
            };

            Crawler crawler = new Crawler(settings);
            crawler.Crawl();
        }
    }
}
