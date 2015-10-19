using System;

namespace Podcasts.Converters
{
    public class NullToDefaultConverter<T> : TypedConverter<T?, T> where T : struct
    {
        public override T Convert(T? value, object parameter, string language)
        {
            return value.HasValue ? value.Value : default(T);
        }

        public override T? ConvertBack(T value, object parameter, string language)
        {
            return value;
        }
    }

    public class NullableTimeSpanToTimeSpanConverter : NullToDefaultConverter<TimeSpan> { }
}