using Windows.UI.Xaml;

namespace Podcasts.Converters
{
    public class BooleanToVisibilityConverter : TypedConverter<bool, Visibility>
    {
        public override Visibility Convert(bool value, object parameter, string language) => value ? Visibility.Visible : Visibility.Collapsed;

        public override bool ConvertBack(Visibility value, object parameter, string language) => value == Visibility.Visible;
    }
}