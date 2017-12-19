using HtmlAgilityPack;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crawly.HTML
{
    public class LinkExtractor
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(LinkExtractor));

        private IEnumerable<string> _bannedExts = null;
        private IEnumerable<string> _bannedUrls = null;

        public LinkExtractor(IEnumerable<string> bannedExts, IEnumerable<string> bannedUrls)
        {
            _bannedExts = bannedExts;
            _bannedUrls = bannedUrls;
        }

        public IEnumerable<Uri> ExtractLinks(Uri baseUri, HtmlDocument doc)
        {
            List<Uri> found = new List<Uri>();

            foreach (HtmlNode link in doc.DocumentNode.Descendants("a"))
            {
                var content = link.GetAttributeValue("href", "");
                if (!String.IsNullOrEmpty(content) && !content.StartsWith("javascript", StringComparison.InvariantCultureIgnoreCase))
                {
                    Uri temp = new Uri(baseUri, content);
                    _log.Debug($"Created uri {temp} from baseUri {baseUri} and relativeUri {content}");

                    if (IsFromBannedDomain(temp, _bannedUrls) || ContainsBannedExtension(temp, _bannedExts))
                    {
                        continue;
                    }
                    
                    _log.Debug($"Found link {content}");
                    found.Add(temp);
                }
            }

            return found;
        }

        private static bool IsFromBannedDomain(Uri temp, IEnumerable<string> bannedUrls)
        {
            return bannedUrls.Contains(temp.Host, StringComparer.InvariantCultureIgnoreCase);
        }

        private static bool ContainsBannedExtension(Uri temp, IEnumerable<string> bannedExts)
        {
            foreach (string segment in temp.Segments)
            {
                foreach (string ext in bannedExts)
                {
                    if (segment.EndsWith(ext, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _log.Debug($"Skipping url {temp.OriginalString} because it ends with banned extension {ext}.");
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
