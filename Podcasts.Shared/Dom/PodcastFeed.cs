using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;
using Windows.Storage;

namespace Podcasts.Dom
{
    /// <summary>
    /// http://cyber.law.harvard.edu/rss/rss.html
    /// </summary>
    public class PodcastFeed : XmlNodeHost
    {
        private XmlDocument Document;

        public class ITunesNS : XmlNodeHost
        {
            public class CategoryNode : XmlNodeHost
            {
                public IXmlNode Category => Node;

                public CategoryNode(IXmlNode node) : base(node, Constants.ITunesNamespace)
                {
                }

                internal static CategoryNode TryCreate(XmlNodeHost host)
                {
                    var categoryNode = host.SelectSingleNode("category");

                    return categoryNode == null ? null : new CategoryNode(categoryNode);
                }

                private CategoryNode TryCreate()
                {
                    return TryCreate(this);
                }

                private CategoryNode _category;
                public CategoryNode SubCategories => _category ?? (_category = TryCreate());

                private string _text;
                public string Text => LazyLoadStringAttribute(ref _text, "text");
            }

            public class OwnerNode : XmlNodeHost
            {
                public OwnerNode(IXmlNode node) : base(node, Constants.ITunesNamespace)
                {
                }

                private string _email;
                public string Email => LazyLoadString(ref _email, "email");

                private string _name;
                public string Name => LazyLoadString(ref _name, "name");
            }

            public ITunesNS(IXmlNode node) : base(node, Constants.ITunesNamespace)
            {
            }

            private string _author;
            public string Author => LazyLoadString(ref _author, "author");

            private YesEnum? _block;
            public YesEnum? Block => LazyLoadYesEnum(ref _block, "block");

            private IEnumerable<CategoryNode> _categories;

            public IEnumerable<CategoryNode> Categories =>
                _categories ?? (_categories = SelectNodes("category").Select(node => new CategoryNode(node)).CacheResults());

            private ITunesImageNode _image;
            public ITunesImageNode Image => _image ?? (_image = ITunesImageNode.TryCreate(this));

            private ExplicitEnum? _explicit;
            public ExplicitEnum? Explicit => LazyLoadExplicitEnum(ref _explicit, "explicit");

            private YesEnum? _complete;
            public YesEnum? Complete => LazyLoadYesEnum(ref _complete, "complete");

            private Uri _newFeedUrl;
            public Uri NewFeedUrl => LazyLoadUri(ref _newFeedUrl, "new-feed-url");

            private OwnerNode _owner;

            public OwnerNode Owner
            {
                get
                {
                    if (_owner == null)
                    {
                        var node = SelectSingleNode("owner");

                        if (node != null) _owner = new OwnerNode(node);
                    }

                    return _owner;
                }
            }

            private string _subtitle;
            public string Subtitle => LazyLoadString(ref _subtitle, "subtitle");

            private string _summary;
            public string Summary => LazyLoadString(ref _summary, "summary");
        }

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
            if (node == null)
            {
                return null;
            }

            return new ImageNode(node);
        }

        private ImageNode _image;
        public ImageNode Image => _image ?? (_image = TryGetImageNode());

        private ITunesNS _itunes;
        public ITunesNS ITunes => _itunes ?? (_itunes = new ITunesNS(Channel));

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