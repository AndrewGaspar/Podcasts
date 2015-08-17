using System;
using Windows.Data.Xml.Dom;

namespace Podcasts.Dom
{
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
            if (node == null)
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

}
