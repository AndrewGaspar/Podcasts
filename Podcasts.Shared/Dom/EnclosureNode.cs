using System;
using Windows.Data.Xml.Dom;

namespace Podcasts.Dom
{
    public class EnclosureNode : XmlNodeHost
    {
        public IXmlNode Enclosure => Node;

        internal EnclosureNode(IXmlNode node) : base(node)
        {
        }

        private string _type;
        public string Type => LazyLoadStringAttribute(ref _type, "type");

        private Uri _url;
        public Uri Url => LazyLoadUriAttribute(ref _url, "url");

        private ulong? _length;
        public ulong? Length => LazyLoadUlongAttribute(ref _length, "length");
    }
}
