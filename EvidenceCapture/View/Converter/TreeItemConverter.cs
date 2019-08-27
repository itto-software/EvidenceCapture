using EvidenceCapture.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace EvidenceCapture.View.Converter
{
    class TreeItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SnapTreeItem)
            {
                var obj = value as SnapTreeItem;
                if (obj.NodeFileType == SnapTreeItem.FileType.File)
                {
                    return obj.Name;
                }
                else
                {
                    return $"{obj.Name,3} ({obj.Children.Count,3})";
                }
            }
            return "undefined type";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
