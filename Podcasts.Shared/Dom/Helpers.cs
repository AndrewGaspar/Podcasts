using System;
using System.Linq;
using Windows.Data.Xml.Dom;

namespace Podcasts.Dom
{
    internal static class Helpers
    {
        public static string TryReadString(this IXmlNode node)
        {
            return node?.InnerText;
        }

        public static DateTime? TryReadDateTime(this IXmlNode node)
        {
            DateTime actualDate;
            if (DateTime.TryParse(node.InnerText, out actualDate))
            {
                return actualDate;
            }

            return null;
        }

        public static Uri TryReadUri(this IXmlNode node)
        {
            return TryParseUri(node?.InnerText);
        }

        public static uint? TryReadUint(this IXmlNode node)
        {
            return TryParseUint(node?.InnerText);
        }

        public static ulong? TryReadUlong(this IXmlNode node)
        {
            return TryParseUlong(node?.InnerText);
        }

        public static DateTime? TryGetDateTime(IXmlNode parentNode, string subNode, string ns = null)
        {
            return parentNode.SelectSingleNodeNS(subNode, ns)?.TryReadDateTime();
        }

        public static string TryGetString(IXmlNode parentNode, string key, string ns = null)
        {
            return parentNode.SelectSingleNodeNS(key, ns)?.TryReadString();
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

            return null;
        }

        public static Uri TryGetUri(IXmlNode parentNode, string key, string ns = null)
        {
            return parentNode.SelectSingleNodeNS(key, ns)?.TryReadUri();
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

        public static uint? TryGetUint(IXmlNode parentNode, string key, string ns = null)
        {
            return parentNode.SelectSingleNodeNS(key, ns)?.TryReadUint();
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

        public static TimeSpan? TryParseITunesDuration(string value)
        {
            if(value == null)
            {
                return null;
            }

            var maybeParts = value.Split(':').Select(TryParseUint).ToList();

            var allParsed = maybeParts.Aggregate(true, (isParsed, num) => isParsed && num.HasValue);
            if (!allParsed) return null;

            var parts = maybeParts.Select(num => (int)num.Value).ToList();

            // SS
            if(parts.Count == 1)
            {
                return TimeSpan.FromSeconds(parts[0]);
            }

            // MM:SS
            if(parts.Count == 2)
            {
                return new TimeSpan(0, parts[0], parts[1]);
            }

            // HH:MM:SS
            if(parts.Count == 3)
            {
                return new TimeSpan(parts[0], parts[1], parts[2]);
            }

            return null;
        }

        public static TimeSpan? TryReadITunesDuration(this IXmlNode node)
        {
            return TryParseITunesDuration(node?.InnerText);
        }
    }
}
