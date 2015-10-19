namespace Podcasts.Converters
{
    public class IntToDoubleConverter : TypedConverter<int, double>
    {
        public override double Convert(int value, object parameter, string language)
        {
            return value;
        }

        public override int ConvertBack(double value, object parameter, string language)
        {
            return (int)value;
        }
    }
}