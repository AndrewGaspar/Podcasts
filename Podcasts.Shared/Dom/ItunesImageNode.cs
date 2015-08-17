using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace Podcasts.Dom
{
    public class ITunesImageNode : XmlNodeHost
    {
        public ITunesImageNode(IXmlNode node) : base(node, Constants.ITunesNamespace)
        {

        }

        public static ITunesImageNode TryCreate(XmlNodeHost host)
        {
            var node = host.SelectSingleNode("image");
            return node == null ? null : new ITunesImageNode(node);
        }

        private Uri _href;
        public Uri Href => LazyLoadUriAttribute(ref _href, "href");
    }
}
