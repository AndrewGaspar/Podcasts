using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml.Data;

namespace Podcasts.Converters
{
    public class ArregateConverter : List<OneToOneConverter>, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Debug.Assert(this[this.Count - 1].To.Equals(targetType));

            return this.Aggregate(value, (current, converter) => converter.Convert(current, converter.To, parameter, language));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Debug.Assert(this[0].From.Equals(targetType));

            return (this as IEnumerable<OneToOneConverter>)
                .Reverse()
                .Aggregate(value, (current, converter) => converter.Convert(current, converter.From, parameter, language));
        }
    }
}