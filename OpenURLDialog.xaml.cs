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
        public OpenURLDialog(List<string> historyList)
        {
            InitializeComponent();

            Init(historyList);
        }

        #region Function
        private void Init(List<string> list)
        {
            //TODO
            InitializeCommands();
        }
        #endregion

        #region Command
        private void InitializeCommands()
        {
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));       
        }    

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
      
        #endregion
    }
}
