using Crawly.HTML;
using HtmlAgilityPack;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crawly
{
    internal class CrawlerWorkerArgs
    {
        public Crawler Parent                                       { get; set; }
        public WorkerFunction Function                              { get; set; }
        public ConcurrentDictionary<string, Robots> Robots          { get; set; }
        public IEnumerable<string> BannedExtensions                 { get; set; }
        public ConcurrentBag<string> Visited                        { get; set; }
        public CrawlerQueue Sites                                   { get; set; }
        public bool RespectRobots                                   { get; set; }
        public string UserAgent                                     { get; set; }
        public int MaxDepth                                         { get; set; }
        public int ID                                               { get; set; }
    }

    internal class CrawlerWorker
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(CrawlerWorker));

        private Crawler _parent = null;
        private WorkerFunction _callback = null;
        private ConcurrentDictionary<String, Robots> _robots = null;
        // TODO: use something like hashset if perf matters here
        private IEnumerable<string> _bannedExts;
        private ConcurrentBag<String> _visited = null;
        private CrawlerQueue _sites = null;
        private bool _respectRobots = true;
        private string _userAgent = null;
        private int _maxDepth = -1;
        private int _id = -1;
        private HtmlWeb _web;

        public void Run(CrawlerWorkerArgs args)
        {
            _parent = args.Parent;
            _callback = args.Function;
            if (_callback == null)
            {
                throw new ArgumentException("No callback was provided so no work would be done.");
            }

            _robots = args.Robots;
            _bannedExts = args.BannedExtensions;
            _visited = args.Visited;
            _sites = args.Sites;
            _respectRobots = args.RespectRobots;
            _userAgent = args.UserAgent;
            _maxDepth = args.MaxDepth;
            _id = args.ID;

            _web = new HtmlWeb();
            _web.UserAgent = _userAgent;

            while (true)
            {
                Site next = null;    
                if(!_sites.GetNextAvailableWorker(out next))
                {
                    Interlocked.Increment(ref _parent.PausedWorkers);
                    if(_sites.Empty() && _parent.PausedWorkers == _parent.TotalWorkers)
                    {
                        Log("Queue is empty and all workers paused, terminating.");
                        return;
                    }

                    Log("No work, sleeping");
                    Thread.Sleep(500);
                    Interlocked.Decrement(ref _parent.PausedWorkers);

                    continue;
                }

                String url = next.Url;
                if (next.Depth < _maxDepth && !_visited.Contains(url))
                {
                    VisitOneSite(next);
                }
                else
                {
                    Log($"Skipping {url}.");
                }
            }
        }

        private void VisitOneSite(Site next)
        {
            _log.Debug($"Visiting site {next.Url}.");
            Uri uri = new Uri(next.Url);
            string host = uri.Host;

            if (_respectRobots)
            {
                Robots config;
                if (!_robots.TryGetValue(host, out config))
                {
                    // TODO: actually get the robots.txt
                    config = new Robots(host, _userAgent);

                    _robots.TryAdd(host, config);
                }

                if (!config.Allowed(uri))
                {
                    Log($"Skipping link {uri} because it was banned by robots.txt.");
                    return;
                }
            }

            _visited.Add(next.Url);

            Log($"Visiting site {uri}.");

            try
            {
                HtmlDocument doc = _web.Load(uri);
                List<string> found;
                List<Uri> nextSites;
                _callback(doc, uri, out found, out nextSites);

                foreach (string f in found)
                {
                    _parent.UrlFound(f);
                }

                foreach(Uri link in nextSites)
                {
                    Site temp = new Site()
                    {
                        Url = link.AbsoluteUri,
                        Depth = next.Depth + 1
                    };

                    _sites.Enqueue(temp);
                }
            }
            catch (Exception e)
            {
                Log($"Error visiting site {uri.AbsolutePath}, exception message {e.Message}.");
            }
        }

        private void Log(string message)
        {
            _log.Debug($"Worker {_id}: {message}");
        }
    }
}
