namespace Podcasts.Converters
{
    public class InvertBooleanConverter : TypedConverter<bool, bool>
    {
        public override bool Convert(bool value, object parameter, string language) => !value;

        public override bool ConvertBack(bool value, object parameter, string language) => !value;
    }
}