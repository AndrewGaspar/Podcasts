using System;
using Windows.Data.Xml.Dom;

namespace Podcasts.Dom
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
            if (value == null)
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
            if (value == null)
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
            if (value == null)
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
}
