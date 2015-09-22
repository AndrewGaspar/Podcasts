using System;
using Windows.Data.Xml.Dom;

namespace Podcasts.Dom
{
    public class ImageNode : XmlNodeHost
    {
        public IXmlNode Image => Node;

        public ImageNode(IXmlNode node) : base(node)
        {
        }

        private Uri _url;
        public Uri Url => LazyLoadUri(ref _url, "url");

        private string _title;
        public string Title => LazyLoadString(ref _title, "title");

        private Uri _link;
        public Uri Link => LazyLoadUri(ref _link, "link");

        private uint? _width;
        public uint? Width => LazyLoadUint(ref _width, "width");

        private uint? _height;
        public uint? Height => LazyLoadUint(ref _height, "height");

        private string _description;
        public string Description => LazyLoadString(ref _description, "description");
    }
}