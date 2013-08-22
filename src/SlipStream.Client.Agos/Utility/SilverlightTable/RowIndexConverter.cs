using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Collections.Generic;
using System.Globalization;

namespace SilverlightTable
{
    public sealed class RowIndexConverter : IValueConverter
    {
        private IValueConverter _valueConverter;

        public RowIndexConverter()
        {
        }

        public RowIndexConverter(IValueConverter realValueConverter)
        {
            this._valueConverter = realValueConverter;
        }

        /// <summary>
        /// A value converter for formatting this value
        /// </summary>
        public IValueConverter ValueConverter
        {
            set
            {
                _valueConverter = value;
            }
        }

        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            // obtain the 'bound' property via the Row string indexer
            Row row = (Row)value;
            string index = parameter as string;
            object propertyValue = row[index];

            // convert if required
            if (_valueConverter != null)
            {
                propertyValue = _valueConverter.Convert(propertyValue, targetType, parameter, culture);
            }

            return propertyValue;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            object valueToConvert = value;

            // convert if required
            if (_valueConverter != null)
            {
                valueToConvert = _valueConverter.ConvertBack(valueToConvert, targetType, parameter, culture);
            }

            // inform the bound Row instance of the property value change
            return new PropertyValueChange(parameter as string, valueToConvert);
        }
    }

}
