using System;
using Windows.Data.Xml.Dom;

namespace Podcasts.Dom
{
    public abstract class XmlNodeHost
    {
        protected IXmlNode Node { get; private set; }

        private XmlNamespace Namespace;

        protected XmlNodeHost(IXmlNode node, XmlNamespace ns)
        {
            Node = node;
            Namespace = ns;
        }

        protected XmlNodeHost(IXmlNode node) : this(node, null)
        {

        }

        private string ns(string key)
        {
            return Namespace == null ? key : $"{Namespace.Namespace}:{key}";
        }

        internal XmlNodeList SelectNodes(string key)
        {
            return Node.SelectNodesNS(ns(key), Namespace?.ToString());
        }

        internal IXmlNode SelectSingleNode(string key)
        {
            return Node.SelectSingleNodeNS(ns(key), Namespace?.ToString());
        }

        protected string LazyLoadString(ref string backing, string key)
        {
            if (backing == null)
            {
                backing = SelectSingleNode(key)?.TryReadString();
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
                backing = SelectSingleNode(key)?.TryReadDateTime();
            }

            return backing;
        }

        protected Uri LazyLoadUri(ref Uri backing, string key)
        {
            if (backing == null)
            {
                backing = SelectSingleNode(key)?.TryReadUri();
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
                backing = SelectSingleNode(key).TryReadUint();
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

        protected YesEnum? LazyLoadYesEnum(ref YesEnum? backing, string key)
        {
            return backing ?? (backing = SelectSingleNode(key)?.TryReadYesEnum());
        }

        protected ExplicitEnum? LazyLoadExplicitEnum(ref ExplicitEnum? backing, string key)
        {
            return backing ?? (backing = SelectSingleNode(key)?.TryReadExplicitEnum());
        }

        protected TimeSpan? LazyLoadITunesDuration(ref TimeSpan? backing, string key)
        {
            return backing ?? (backing = SelectSingleNode(key)?.TryReadITunesDuration());
        }
    }
}
