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
                Seeds = new string[] { @"http://maximumfun.org/" },
                MaxDepth = 8
            };

            Crawler crawler = new Crawler(settings);
            crawler.Crawl();
        }
    }
}
