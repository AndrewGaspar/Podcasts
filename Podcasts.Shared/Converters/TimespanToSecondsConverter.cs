using System;

namespace Podcasts.Converters
{
    public class TimeSpanToSecondsConverter : TypedConverter<TimeSpan, int>
    {
        public override int Convert(TimeSpan value, object parameter, string language)
        {
            return (int)value.TotalSeconds;
        }

        public override TimeSpan ConvertBack(int value, object parameter, string language)
        {
            return new TimeSpan(hours: 0, minutes: 0, seconds: value);
        }
    }
}