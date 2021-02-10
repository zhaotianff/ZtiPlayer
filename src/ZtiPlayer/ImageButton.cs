using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZtiPlayer
{
  
    public class ImageButton : Button
    {
        public static readonly DependencyProperty ImageProperty;

        static ImageButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageButton), new FrameworkPropertyMetadata(typeof(ImageButton)));
            ImageProperty = DependencyProperty.Register("Image", typeof(string), typeof(ImageButton));
        }

        public string Image
        {
            set
            {
                SetValue(ImageProperty, value);
            }
            get
            {
                return (string)GetValue(ImageProperty);
            }
        }
    }
}
