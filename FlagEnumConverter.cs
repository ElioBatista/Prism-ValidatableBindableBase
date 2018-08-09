using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace Apollo.Infrastructure.Presentation
{
    public class FlagEnumConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentValue = new List<object>();
            if (value != null && (int)Enum.Parse(value.GetType(), value.ToString()) != 0)
            {
                var newValue = value as Enum;
                var enumValues = Enum.GetValues(value.GetType());
                foreach (object enumvalue in enumValues)
                {
                    if ((int)enumvalue != 0 && newValue.HasFlag((Enum)enumvalue))
                        currentValue.Add(enumvalue);
                }
            }
            return currentValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                object result = null;
                var selection = (List<object>)value;
                foreach (object item in selection)
                {
                    result = System.Convert.ToInt32(result) | System.Convert.ToInt32(item);
                }
                return Enum.Parse(targetType, result?.ToString() ?? "0");
            }
            return Enum.Parse(targetType, "0");
        }
    }
}
