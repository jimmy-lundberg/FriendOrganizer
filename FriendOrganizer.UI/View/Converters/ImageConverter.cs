using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace FriendOrganizer.UI.View.Converters
{
    public class ImageConverter : IValueConverter
    {
        public int DecodeWidth { get; set; }
        public int DecodeHeight { get; set; }

        public ImageConverter()
        {
            DecodeWidth = -1;
            DecodeHeight = -1;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var uri = new Uri((string)value);

            if (uri != null)
            {
                var source = new BitmapImage();
                source.BeginInit();
                source.UriSource = uri;

                if (DecodeWidth >= 0)
                {
                    source.DecodePixelWidth = DecodeWidth; 
                }

                if (DecodeHeight >= 0)
                {
                    source.DecodePixelHeight = DecodeHeight; 
                }

                source.EndInit();
                return source;
            }

            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
