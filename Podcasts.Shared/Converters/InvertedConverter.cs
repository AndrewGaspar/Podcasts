using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;

namespace Podcasts.Converters
{
    [ContentProperty(Name = "Converter")]
    public class InvertedConverter : DependencyObject, IOneToOneConverter
    {
        public IOneToOneConverter Converter
        {
            get { return (IOneToOneConverter)GetValue(ConverterProperty); }
            set { SetValue(ConverterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Converter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConverterProperty =
            DependencyProperty.Register("Converter",
                typeof(IOneToOneConverter),
                typeof(InvertedConverter),
                new PropertyMetadata(0));

        public InvertedConverter()
        {
        }

        public Type From
        {
            get
            {
                return Converter.To;
            }
        }

        public Type To
        {
            get
            {
                return Converter.From;
            }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Debug.Assert(value.GetType().Equals(From) || value == null);
            Debug.Assert(targetType.Equals(To));

            return Converter.ConvertBack(value, targetType, parameter, language);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Debug.Assert(value.GetType().Equals(To) || value == null);
            Debug.Assert(targetType.Equals(From));

            return Converter.Convert(value, targetType, parameter, language);
        }
    }
}