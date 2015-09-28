using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Podcasts.Converters
{
    public class PodcastDurationConverter : TypedConverter<TimeSpan?, string>
    {
        public override string Convert(TimeSpan? duration, object parameter, string language)
        {
            if (!duration.HasValue)
            {
                return "--";
            }
            else if (duration.Value.TotalHours >= 1.0)
            {
                return duration?.ToString(@"h\:mm\:ss");
            }
            else
            {
                return duration?.ToString(@"m\:ss");
            }
        }

        public override TimeSpan? ConvertBack(string value, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}