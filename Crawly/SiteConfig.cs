using System;
using System.IO;

namespace Crawly
{
    internal class SiteConfig
    {
        private String _domain;

        public SiteConfig(String domain, StreamReader robots)
        {
            _domain = domain;
            throw new NotImplementedException();
        }

        public bool Allowed(String url)
        {
            throw new NotImplementedException();
        }
    }
}