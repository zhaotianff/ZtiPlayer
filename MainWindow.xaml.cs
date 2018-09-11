using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //SetBackground("");
        }

        public void SetBackground(string filePath)
        {
            if(string.IsNullOrEmpty(filePath))
            {
                player.SetCustomLogo(Properties.Resources.background.GetHbitmap().ToInt32());
            }
            else
            {
                player.SetCustomLogo(new System.Drawing.Bitmap(filePath).GetHbitmap().ToInt32());
            }
        }
    }
}
