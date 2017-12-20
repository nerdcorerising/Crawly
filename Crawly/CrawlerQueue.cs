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
    internal class SiteInfo
    {
        public Robots Robots { get; set; }
        public ConcurrentQueue<Site> Pending { get; set; }
    }

    internal class CrawlerQueue
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(CrawlerQueue));

        private readonly object AddRobotsLock = new object();

        private Dictionary<string, SiteInfo> _infos = new Dictionary<string, SiteInfo>();
        private ReaderWriterLockSlim _infosLock = new ReaderWriterLockSlim();
        private bool _respectRobots = true;
        private string _userAgent = null;

        public CrawlerQueue(bool respectRobots, string userAgent)
        {
            _respectRobots = respectRobots;
            _userAgent = userAgent;
        }

        public void Enqueue(Site s)
        {
            string domain = new Uri(s.Url).Host.ToLower();
            SiteInfo info;
            _infosLock.EnterReadLock();

            if (!_infos.TryGetValue(domain, out info))
            {
                _infosLock.ExitReadLock();
                SiteInfo newInfo = CreateSiteInfo(domain);

                _infosLock.EnterWriteLock();
                // Check to see if another thread had the lock and updated it
                if (!_infos.TryGetValue(domain, out info))
                {
                    info = newInfo;
                    _infos.Add(domain, info);
                }
                _infosLock.ExitWriteLock();
            }

            if (_infosLock.IsReadLockHeld)
            {
                _infosLock.ExitReadLock();
            }

            info.Pending.Enqueue(s);
        }

        private SiteInfo CreateSiteInfo(string domain)
        {
            SiteInfo info = new SiteInfo()
            {
                Robots = null,
                Pending = new ConcurrentQueue<Site>()
            };

            if (_respectRobots)
            {
                info.Robots = new Robots(domain, _userAgent);
            }

            return info;
        }

        internal bool Empty()
        {
            foreach (String domain in _infos.Keys)
            {
                SiteInfo info;
                if (_infos.TryGetValue(domain, out info))
                {
                    if (info.Pending.Count > 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public bool GetNextAvailableWorker(out Site next)
        {
            try
            {
                _infosLock.EnterReadLock();
                next = null;
                foreach (String domain in _infos.Keys)
                {
                    SiteInfo info;
                    if (_infos.TryGetValue(domain, out info))
                    {
                        if (info.Robots == null || info.Robots.RemainingDelay() <= 0)
                        {
                            Site s;
                            if (info.Pending.TryDequeue(out s))
                            {
                                if (info.Robots != null)
                                {
                                    info.Robots.Visited();
                                }

                                next = s;
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
            finally
            {
                if (_infosLock.IsReadLockHeld)
                {
                    _infosLock.ExitReadLock();
                }
            }
        }
    }
}
