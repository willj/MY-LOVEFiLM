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
using System.Windows.Media.Imaging;

namespace WPLovefilm.Helpers
{
    public class VisibilityValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((bool)value == true)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HoursMinutesValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string timeString = "";

            if ((int)value > 0)
            {
                TimeSpan t = TimeSpan.FromMinutes((int)value);

                if (t.Hours > 0)
                {
                    if (t.Hours == 1)
                    {
                        timeString += t.Hours + " hr ";
                    }
                    else
                    {
                        timeString += t.Hours + " hrs ";
                    }
                    
                }

                if (t.Minutes > 0)
                {
                    timeString += t.Minutes + " mins";
                }
                
            }

            return timeString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class YearValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if((int)value == 0){
                return string.Empty;
            } else {
                return value;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ShipDateFormatValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime d;
            string s = "";

            if (DateTime.TryParse((string)value, out d))
            {
                s = d.ToString("dd MMM");
            }

            if (s == "")
            {
                s = "Shipping soon!";
            } else {
                s = "Shipped " + s;
            }

            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FullShipDateFormatValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime d;
            string s = "";

            if (DateTime.TryParse((string)value, out d))
            {
                s = d.ToString("dd MMMM yyyy");
            }

            if (s == "")
            {
                s = "Shipping soon!";
            }
            else
            {
                s = "Shipped " + s;
            }

            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class FullDateFormatValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DateTime d;
            string s = "";

            if (DateTime.TryParse((string)value, out d))
            {
                s = d.ToString("dd MMMM yyyy");
            }
            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class QueuePriorityColourValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush b = new SolidColorBrush(Color.FromArgb(255,255,255,255));

            switch ((int)value)
            {
                case 1:
                    b.Color = Color.FromArgb(255, 229, 20, 0);  //Red - High Priority
                    break;
                case 2:
                    b.Color = Color.FromArgb(255, 240, 150, 9); //Orange - Medium Priority
                    break;
                case 3:
                    b.Color = Color.FromArgb(255, 51, 153, 51);    //Green - Low Priority
                    break;
            }

            return b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PlayerStringValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = "";

            if (!string.IsNullOrEmpty((string)value))
            {
                if((string)value == "1"){
                    s = value + " player";
                }else{
                    s = value + " players";
                }
            }

            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class StringVisibilityValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrEmpty((string)value))
            {
                return Visibility.Collapsed;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PriorityBoolValueConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return value;
            }

            return value.ToString() == parameter.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return value;
            }

            return parameter;
        }
    }

}