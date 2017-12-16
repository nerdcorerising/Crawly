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
        public Crawler Parent                                   { get; set; }
        public ConcurrentDictionary<string, SiteConfig> Robots  { get; set; }
        public ConcurrentBag<string> Visited                    { get; set; }
        public ConcurrentStack<string> Stack                    { get; set; }
        public int ID                                           { get; set; }
    }

    internal class CrawlerWorker
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(CrawlerWorker));

        private Crawler _parent = null;
        private ConcurrentDictionary<String, SiteConfig> _robots = null;
        private ConcurrentBag<String> _visited = null;
        private ConcurrentStack<string> _stack = null;
        private int _id = -1;

        public void Run(CrawlerWorkerArgs args)
        {
            _robots = args.Robots;
            _visited = args.Visited;
            _stack = args.Stack;
            _id = args.ID;
            _parent = args.Parent;

            
            while (true)
            {
                String next = null;    
                if(!_stack.TryPop(out next))
                {
                    Interlocked.Increment(ref _parent.PausedWorkers);
                    if(_parent.PausedWorkers == _parent.TotalWorkers)
                    {
                        _log.Debug($"Worker {_id}: Queue is empty and all workers paused, terminating.");
                        return;
                    }

                    _log.Debug($"Worker {_id}: No work, sleeping");
                    Thread.Sleep(500);
                    Interlocked.Decrement(ref _parent.PausedWorkers);

                    continue;
                }
                
                VisitOneSite(next);
            }
        }

        private void VisitOneSite(string next)
        {
            _log.Debug($"Worker {_id}:Would have visited site {next} if this actually worked");
            Thread.Sleep(1000);
        }
    }
}
