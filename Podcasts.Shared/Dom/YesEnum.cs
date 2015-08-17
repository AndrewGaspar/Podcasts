using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace Podcasts.Dom
{
    public enum YesEnum
    {
        Yes,
        No,
    }

    internal static class YesEnumHelpers
    {
        public static YesEnum? TryParseYesEnum(string value)
        {
            if (value == "yes") return YesEnum.Yes;
            if (value == "no") return YesEnum.No;
            return null;
        }

        public static YesEnum? TryReadYesEnum(this IXmlNode node)
        {
            return TryParseYesEnum(node?.InnerText);
        }
    }
}
