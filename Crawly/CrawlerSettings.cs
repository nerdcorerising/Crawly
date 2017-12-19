using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace Crawly
{
    public delegate void WorkerFunction(HtmlDocument doc, Uri docUri, out List<string> foundItems, out List<Uri> newUris);

    public class CrawlerSettings
    {
        public WorkerFunction Function                  { get; set; } = null;
        public IEnumerable<String> Seeds                { get; set; } = new List<String>();
        public IEnumerable<String> BannedExtensions     { get; set; } = new List<String>();
        public String OutputPath                        { get; set; } = "output.txt";
        public bool RespectRobots                       { get; set; } = true;
        public int WorkerCount                          { get; set; } = 8;
        public int MaxDepth                             { get; set; } = int.MaxValue;
        public string UserAgent                         { get; set; }
    }
}
