using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Crawly.HTML
{
    public class HtmlPage
    {
        public HtmlPage(Uri uri)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                Stream stream = request.GetResponse().GetResponseStream();

                LoadFromStream(stream);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Failed to parse html page from uri.", e);
            }
        }

        public HtmlPage(Stream stream)
        {
            LoadFromStream(stream);
        }

        private void LoadFromStream(Stream stream)
        {
            throw new NotImplementedException();
        }

    }
}
