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
                Seeds = new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18", "19", "20" }
            };

            Crawler crawler = new Crawler(settings);
            crawler.Crawl();
        }
    }
}
