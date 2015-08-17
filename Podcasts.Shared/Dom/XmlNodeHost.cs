using System;
using Windows.Data.Xml.Dom;

namespace Podcasts.Dom
{
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
            if (backing == null) backing = Node.Attributes.GetNamedItem(attr)?.InnerText;

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
            if (backing == null)
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
            if (backing == null)
            {
                backing = Helpers.TryParseUlong(Node.Attributes.GetNamedItem(attr)?.InnerText);
            }

            return backing;
        }
    }
}
