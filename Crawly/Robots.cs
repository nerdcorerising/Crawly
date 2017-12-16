using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Crawly
{
    internal class Robots
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Robots));

        private string _domain = null;
        private long _waitTime = 0;
        private long _lastAccessTime = 0;
        private string _userAgent = null;
        private List<string> _denyRules = new List<string>();

        public Robots(String domain, String userAgent)
        {
            _domain = domain;
            _userAgent = userAgent;

            ReadRobots();
        }

        public void ReadRobots()
        {
            string url = "http://" + _domain + "/robots.txt";

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                using (StreamReader reader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    while (reader.Peek() != -1)
                    {
                        bool userAgentMatch = ParseUserAgentFields(reader, url);

                        ParseRulesForUserAgent(reader, userAgentMatch, url);
                    }
                }
            }
            catch (Exception e)
            {
                _log.Info($"Error getting {url}. Exception message: {e.Message}.");
            }
        }

        private bool ParseUserAgentFields(StreamReader reader, string url)
        {
            bool matches = false;
            string userAgentString = "user-agent:";

            string line = null;
            while ((line = reader.ReadLine()) != null)
            {
                line = RemoveComment(line);
                line = line.Trim();

                if (String.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }

                if (!line.StartsWith(userAgentString))
                {
                    break;
                }

                // Now we know it's a user agent field, see if it's a match
                int len = userAgentString.Length;

                line = line.Remove(0, len).Trim();

                if (line.Equals("*") || line.Equals(_userAgent))
                {
                    matches = true;

                    // Skip the remaining user agent fields, we already know it's a match
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        if (!(line.StartsWith("#") || line.StartsWith(userAgentString)))
                        {
                            break;
                        }
                    }
                }
            }

            return matches;
        }

        private void ParseRulesForUserAgent(StreamReader reader, bool userAgentMatch, string url)
        {
            string line = null;
            string userAgentString = "user-agent:";
            while ((line = reader.ReadLine()) != null)
            {
                line = RemoveComment(line);
                line = line.Trim();

                if (line.StartsWith(userAgentString))
                {
                    break;
                }

                if (!userAgentMatch || String.IsNullOrEmpty(line) || line.StartsWith("#"))
                {
                    continue;
                }


                string disallow = "Disallow:";
                string crawlDelay = "Crawl-Delay:";

                // Only reads Disallow and Crawl-Delay for now
                if (line.StartsWith(disallow, StringComparison.InvariantCultureIgnoreCase))
                {
                    line = line.Remove(0, disallow.Length).Trim();
                    _denyRules.Add(line);
                }
                else if (line.StartsWith(crawlDelay, StringComparison.InvariantCultureIgnoreCase))
                {
                    long waitSeconds;
                    line = line.Remove(0, crawlDelay.Length).Trim();
                    if (!Int64.TryParse(line, out waitSeconds))
                    {
                        _log.Info($"{url}: Saw Crawl-Delay for site {_domain} but it had an unparseable value of {line}.");
                    }
                    else
                    {
                        if (waitSeconds < 1 || waitSeconds > 30)
                        {
                            _log.Info($"{url}: Crawl-Delay set to invalid value of {waitSeconds}, defaulting to 1 second.");
                            waitSeconds = 1;
                        }

                        _waitTime = TimeSpan.TicksPerSecond * waitSeconds;
                    }
                }
            }
        }

        private string RemoveComment(string line)
        {
            int pos = line.IndexOf("#");

            if (pos > 0 && pos < line.Length)
            {
                return line.Remove(pos);
            }

            return line;
        }

        public bool Allowed(Uri uri)
        {
            if (!uri.Host.Equals(_domain, StringComparison.InvariantCultureIgnoreCase))
            {
                _log.Error($"uri {uri} should not have been passed to this SiteConfig, domain does not match {_domain}");
                return false;
            }

            foreach (string rule in _denyRules)
            {
                if (MatchesRule(uri, rule))
                {
                    return false;
                }
            }

            return true;
        }

        private bool MatchesRule(Uri uri, string rule)
        {
            if (uri.PathAndQuery.StartsWith(rule, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public long RemainingDelay()
        {
            long diff = (_lastAccessTime + _waitTime) - DateTime.Now.Ticks;
            long ret = Math.Max(diff, 0);
            _log.Debug($"{_domain} Remaining time = {ret}");

            return ret;
        }

        public void Visited()
        {
            _lastAccessTime = DateTime.Now.Ticks;
        }
    }
}