﻿using log4net;
using log4net.Appender;
using log4net.Layout;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
namespace Crawly
{
    public class Crawler
    {
        static Crawler()
        {
            //log4net.Config.XmlConfigurator.Configure();
            Console.WriteLine("Enabling log4net");
            var appender = new ConsoleAppender();
            appender.Layout = new SimpleLayout();
            //log4net.Config.BasicConfigurator.Configure(appender);
        }

        internal int TotalWorkers = 0;
        internal int PausedWorkers = 0;

        private static readonly ILog _log = LogManager.GetLogger(typeof(CrawlerWorker));

        private CrawlerSettings _settings;
        private ConcurrentDictionary<String, Robots> _robots = new ConcurrentDictionary<string, Robots>();
        private ConcurrentBag<String> _visited = new ConcurrentBag<string>();
        private ConcurrentQueue<Site> _sites = new ConcurrentQueue<Site>();

        public Crawler(CrawlerSettings settings)
        {
            _settings = settings;
            TotalWorkers = _settings.WorkerCount;

            foreach (String str in _settings.Seeds)
            {
                Site s = new Site()
                {
                    Url = str,
                    Depth = 0
                };

                _sites.Enqueue(s);
            }
        }

        public void Crawl()
        {
            List<Thread> workerThreads = new List<Thread>();

            for (int i = 0; i < _settings.WorkerCount; ++i)
            {
                ParameterizedThreadStart ts = new ParameterizedThreadStart(RunWorker);

                Thread temp = new Thread(ts);
                CrawlerWorkerArgs args = new CrawlerWorkerArgs()
                {
                    Parent = this,
                    Robots = _robots,
                    Visited = _visited,
                    Sites = _sites,
                    RespectRobots = _settings.RespectRobots,
                    UserAgent = _settings.UserAgent,
                    MaxDepth = _settings.MaxDepth,
                    ID = i,
                };

                var functionArgs = Tuple.Create(args, new CrawlerWorker());
                _log.Debug($"Starting worker {i}");
                temp.Start(functionArgs);

                workerThreads.Add(temp);
            }

            foreach (Thread t in workerThreads)
            {
                t.Join();
            }
        }

        private void RunWorker(object obj)
        {
            if (obj.GetType() != typeof(Tuple<CrawlerWorkerArgs, CrawlerWorker>))
            {
                String error = "Incorrect arguments provided to RunWorker, can't run crawler.";
                _log.Fatal(error);
                throw new InvalidOperationException(error);
            }

            var tuple = (Tuple<CrawlerWorkerArgs, CrawlerWorker>)obj;
            CrawlerWorkerArgs args = tuple.Item1;
            CrawlerWorker worker = tuple.Item2;

            worker.Run(args);
        }
    }
}
