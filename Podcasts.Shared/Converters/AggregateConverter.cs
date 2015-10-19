using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Podcasts.Converters
{
    public class AggregateConverter : List<IOneToOneConverter>, IOneToOneConverter
    {
        private IEnumerable<IOneToOneConverter> Backwards()
        {
            for (var i = this.Count - 1; i >= 0; i--)
            {
                yield return this[i];
            }
        }

        public Type From => this[0].From;

        public Type To => this[this.Count - 1].To;

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Debug.Assert(this[this.Count - 1].To.Equals(targetType));

            return this.Aggregate(value, (current, converter) => converter.Convert(current, converter.To, parameter, language));
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Debug.Assert(this[0].From.Equals(targetType) || targetType.Equals(typeof(object)));

            return this
                .Backwards()
                .Aggregate(value, (current, converter) => converter.ConvertBack(current, converter.From, parameter, language));
        }
    }
}