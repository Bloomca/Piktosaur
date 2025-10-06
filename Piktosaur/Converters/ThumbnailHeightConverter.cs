using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

namespace Piktosaur.Converters
{
    public class ThumbnailHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // If thumbnail is not loaded (null), return the default height
            // It uses square dimensions, which is not perfect (most images are 16:9 today),
            // but it helps because GridView virtualization is a little bit too aggressive
            if (value == null)
            {
                return 200.0;
            }
            // If thumbnail is loaded, return NaN to use auto-sizing
            return double.NaN;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
