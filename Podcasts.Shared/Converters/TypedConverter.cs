﻿using System;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace Podcasts.Converters
{
    public interface IOneToOneConverter : IValueConverter
    {
        Type From { get; }
        Type To { get; }
    }

    public abstract class OneToOneConverter : IOneToOneConverter
    {
        public Type From { get; private set; }
        public Type To { get; private set; }

        protected OneToOneConverter(Type from, Type to)
        {
            From = from;
            To = to;
        }

        public abstract object Convert(object value, Type targetType, object parameter, string language);

        public abstract object ConvertBack(object value, Type targetType, object parameter, string language);
    }

    public abstract class TypedConverter<From, To> : OneToOneConverter
    {
        protected TypedConverter() : base(typeof(From), typeof(To))
        {
        }

        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            Debug.Assert(value is From || value == null);
            Debug.Assert(targetType.Equals(typeof(To)));

            return Convert((From)value, parameter, language);
        }

        public abstract To Convert(From value, object parameter, string language);

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            Debug.Assert(value is To || value == null);
            Debug.Assert(targetType.Equals(typeof(From)));

            return ConvertBack((To)value, parameter, language);
        }

        public abstract From ConvertBack(To value, object parameter, string language);
    }
}