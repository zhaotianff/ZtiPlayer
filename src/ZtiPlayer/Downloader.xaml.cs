using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    /// Downloader.xaml 的交互逻辑
    /// </summary>
    public partial class Downloader : Window
    {
        private const string DecodePackUrl = "http://aplayer.open.xunlei.com/codecs.zip";


        public Downloader()
        {
            InitializeComponent();
            InitCommands();
        }

       
        #region Command
        private void InitCommands()
        {
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if(MessageBox.Show("是否取消下载?","提示信息",MessageBoxButton.YesNo,MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        #endregion

        #region Function
        public async void StartDownloadDecodePack()
        {
            var checkResult = await CheckConnection();

            if(checkResult == false)
            {
                MessageBox.Show("请检查网络连接","提示信息");
                return;
            }

            try
            {
                var temp = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"temp");
                var tempFileName = System.IO.Path.Combine(temp, "dd.zip");
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                webClient.DownloadFileAsync(new Uri(DecodePackUrl), tempFileName);
            }
            catch
            {
                MessageBox.Show("下载错误，请稍后重试");
            }
            
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            this.Close();
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            this.Dispatcher.Invoke(()=> {
                bar_Progress.Value = e.ProgressPercentage;
            });
        }

        private async Task<bool> CheckConnection()
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                var response = await httpClient.GetAsync(DecodePackUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        #endregion
    }
}
