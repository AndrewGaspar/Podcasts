using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;

namespace Podcasts
{
    /// <summary>
    /// http://cyber.law.harvard.edu/rss/rss.html
    /// </summary>
    namespace Dom
    {
        internal static class Helpers
        {
            public static DateTime? TryGetDateTime(IXmlNode parentNode, string subNode)
            {
                var node = parentNode.SelectSingleNode(subNode);
                if (node == null)
                {
                    return null;
                }

                DateTime actualDate;
                if (DateTime.TryParse(node.InnerText, out actualDate))
                {
                    return actualDate;
                }

                return null;
            }

            public static string TryGetString(IXmlNode parentNode, string key)
            {
                var node = parentNode.SelectSingleNode(key);
                if (node == null)
                {
                    return null;
                }

                return node.InnerText;
            }

            public static Uri TryParseUri(string value)
            {
                if(value == null)
                {
                    return null;
                }

                Uri result;
                if (Uri.TryCreate(value, UriKind.Absolute, out result))
                {
                    return result;
                }

                return result;
            }

            public static Uri TryGetUri(IXmlNode parentNode, string key)
            {
                var node = parentNode.SelectSingleNode(key);
                if (node == null)
                {
                    return null;
                }

                return TryParseUri(node.InnerText);
            }

            public static uint? TryParseUint(string value)
            {
                if(value == null)
                {
                    return null;
                }

                uint result;
                if (uint.TryParse(value, out result))
                {
                    return result;
                }

                return null;
            }

            public static uint? TryGetUint(IXmlNode parentNode, string key)
            {
                var node = parentNode.SelectSingleNode(key);
                if (node == null)
                {
                    return null;
                }

                return TryParseUint(parentNode.InnerText);
            }

            public static ulong? TryParseUlong(string value)
            {
                if(value == null)
                {
                    return null;
                }

                ulong result;
                if (ulong.TryParse(value, out result))
                {
                    return result;
                }

                return null;
            }
        }

        public class PodcastFeedParseException : Exception
        {

        }

        public abstract class XmlNodeHost
        {
            protected IXmlNode Node { get; private set; }

            protected XmlNodeHost(IXmlNode node)
            {
                Node = node;
            }

            protected string LazyLoadString(ref string backing, string key)
            {
                if (backing == null)
                {
                    backing = Helpers.TryGetString(Node, key);
                }

                return backing;
            }

            protected string LazyLoadStringAttribute(ref string backing, string attr)
            {
                if(backing == null) backing = Node.Attributes.GetNamedItem(attr)?.InnerText;

                return backing;
            }

            protected DateTime? LazyLoadDateTime(ref DateTime? backing, string key)
            {
                if (backing == null)
                {
                    backing = Helpers.TryGetDateTime(Node, key);
                }

                return backing;
            }

            protected Uri LazyLoadUri(ref Uri backing, string key)
            {
                if (backing == null)
                {
                    backing = Helpers.TryGetUri(Node, key);
                }

                return backing;
            }

            protected Uri LazyLoadUriAttribute(ref Uri backing, string attr)
            {
                if(backing == null)
                {
                    backing = Helpers.TryParseUri(Node.Attributes.GetNamedItem(attr)?.InnerText);
                }

                return backing;
            }

            protected uint? LazyLoadUint(ref uint? backing, string key)
            {
                if (backing == null)
                {
                    backing = Helpers.TryGetUint(Node, key);
                }

                return backing;
            }

            protected ulong? LazyLoadUlongAttribute(ref ulong? backing, string attr)
            {
                if(backing == null)
                {
                    backing = Helpers.TryParseUlong(Node.Attributes.GetNamedItem(attr)?.InnerText);
                }

                return backing;
            }
        }

        public class EnclosureNode : XmlNodeHost
        {
            public IXmlNode Enclosure => Node;

            internal EnclosureNode(IXmlNode node) :base (node)
            {
            }

            private string _type;
            public string Type => LazyLoadStringAttribute(ref _type, "type");

            private Uri _url;
            public Uri Url => LazyLoadUriAttribute(ref _url, "url");

            private ulong? _length;
            public ulong? Length => LazyLoadUlongAttribute(ref _length, "length");
        }

        /// <summary>
        /// http://cyber.law.harvard.edu/rss/rss.html#hrelementsOfLtitemgt
        /// </summary>
        public class PodcastFeedItem : XmlNodeHost
        {
            public IXmlNode Item => Node;

            internal PodcastFeedItem(IXmlNode node) : base(node)
            {
            }

            private string _title;
            public string Title => LazyLoadString(ref _title, "title");

            private Uri _link;
            public Uri Link => LazyLoadUri(ref _link, "link");

            private string _description;
            public string Description => LazyLoadString(ref _description, "description");

            private string _author;
            public string Author => LazyLoadString(ref _author, "author");

            private string _category;
            public string Category => LazyLoadString(ref _category, "category");

            private Uri _comments;
            public Uri Comments => LazyLoadUri(ref _comments, "comments");

            private EnclosureNode TryGetEnclosure()
            {
                var node = Item.SelectSingleNode("enclosure");
                if(node == null)
                {
                    return null;
                }

                return new EnclosureNode(node);
            }

            private EnclosureNode _enclosure;
            public EnclosureNode Enclosure => _enclosure ?? (_enclosure = TryGetEnclosure());

            private string _guid;
            public string Guid => LazyLoadString(ref _guid, "guid");

            private DateTime? _pubDate;
            public DateTime? PubDate => LazyLoadDateTime(ref _pubDate, "pubDate");

            private string _source;
            public string Source => LazyLoadString(ref _source, "source");
        }

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

        public class PodcastFeed : XmlNodeHost
        {
            private XmlDocument Document;

            public IXmlNode Channel => Node;

            public PodcastFeed(XmlDocument dom) : base(dom.SelectSingleNode("/rss/channel"))
            {
                Document = dom;
            }

            private string _title;
            public string Title => LazyLoadString(ref _title, "title");

            private string _description;
            public string Description => LazyLoadString(ref _description, "description");

            private Uri _link;
            public Uri Link => LazyLoadUri(ref _link, "link");

            private string _language;
            public string Language => LazyLoadString(ref _language, "language");

            private string _copyright;
            public string Copyright => LazyLoadString(ref _copyright, "copyright");

            private string _managingEditor;
            public string ManagingEditor => LazyLoadString(ref _managingEditor, "managingEditor");

            private string _webMaster;
            public string WebMaster => LazyLoadString(ref _webMaster, "webMaster");

            private DateTime? _pubDate;
            public DateTime? PubDate => LazyLoadDateTime(ref _pubDate, "pubDate");

            private DateTime? _lastBuildDate;
            public DateTime? LastBuildDate => LazyLoadDateTime(ref _lastBuildDate, "lastBuildDate");

            private string _category;
            public string Category => LazyLoadString(ref _category, "category");

            private string _generator;
            public string Generator => LazyLoadString(ref _generator, "generator");

            private Uri _docs;
            public Uri Docs => LazyLoadUri(ref _docs, "docs");

            private uint? _ttl;
            public uint? TimeToLive => LazyLoadUint(ref _ttl, "ttl");

            private IEnumerable<PodcastFeedItem> _items;
            public IEnumerable<PodcastFeedItem> Items => 
                _items ?? (_items = Channel.SelectNodes("item").Select(node => new PodcastFeedItem(node)).CacheResults());

            private ImageNode TryGetImageNode()
            {
                var node = Channel.SelectSingleNode("image");
                if(node == null)
                {
                    return null;
                }

                return new ImageNode(node);
            }

            private ImageNode _image;
            public ImageNode Image => _image ?? (_image = TryGetImageNode());

            public static async Task<PodcastFeed> LoadFeedAsync(Uri location)
            {
                XmlDocument document;
                // XmlDocument.LoadFromUriAsync doesn't support files, so branch on ms-appx:/// for testing
                if (location.Scheme == "ms-appx")
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(location).AsTask().ConfigureAwait(false);
                    document = await XmlDocument.LoadFromFileAsync(file).AsTask().ConfigureAwait(false);
                }
                else
                {
                    document = await XmlDocument.LoadFromUriAsync(location).AsTask().ConfigureAwait(false);
                }

                return new PodcastFeed(document);
            }
        }
    }
}
