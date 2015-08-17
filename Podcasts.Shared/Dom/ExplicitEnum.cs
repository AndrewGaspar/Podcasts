using Windows.Data.Xml.Dom;

namespace Podcasts.Dom
{
    public enum ExplicitEnum
    {
        Yes,
        Clean,
    }

    internal static class ExplicitEnumHelper
    {
        public static ExplicitEnum? TryParseExplicitEnum(string text)
        {
            if (text == "yes") return ExplicitEnum.Yes;
            if (text == "clean") return ExplicitEnum.Clean;
            return null;
        }

        public static ExplicitEnum? TryReadExplicitEnum(this IXmlNode node)
        {
            return TryParseExplicitEnum(node?.InnerText);
        }
    }
}
