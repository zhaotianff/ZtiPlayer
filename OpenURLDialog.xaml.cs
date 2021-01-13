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
using System.Windows.Shapes;

namespace ZtiPlayer
{
    /// <summary>
    /// OpenURLDialog.xaml 的交互逻辑
    /// </summary>
    public partial class OpenURLDialog : Window
    {
        public string Url
        {
            get
            {
                return this.combox_Url.Text;
            }
        }
        public OpenURLDialog(List<string> historyList)
        {
            InitializeComponent();

            Init(historyList);
            InitCommands();
        }

        #region Function
        private void Init(List<string> list)
        {
            this.combox_Url.ItemsSource = list;
        }
        #endregion

        #region Command
        private void InitCommands()
        {
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));       
        }    

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        #endregion

        #region Event
        private void btn_OK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.combox_Url.Focus();
        }
        #endregion


    }
}
