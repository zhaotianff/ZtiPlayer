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
using ZtiPlayer.Utils;

namespace ZtiPlayer
{
    /// <summary>
    /// Downloader.xaml 的交互逻辑
    /// </summary>
    public partial class Downloader : Window
    {
        private const string CodecDirName = "codecs";
        private const string DecodePackUrl = "http://aplayer.open.xunlei.com/codecs.zip";
        private static readonly string Temp = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "temp");
        private static readonly string TempFileName = System.IO.Path.Combine(Temp, "dd.zip");
        private static readonly string CodecDirPath = System.IO.Path.Combine(Environment.CurrentDirectory, CodecDirName);

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
                WebClient webClient = new WebClient();
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                webClient.DownloadFileAsync(new Uri(DecodePackUrl), TempFileName);
            }
            catch
            {
                MessageBox.Show("下载错误，请稍后重试");
            }
            
        }

        private void ExtractPackFile()
        {
            try
            {
                FileHelper.CreateDirectory(CodecDirPath);

                if(FileHelper.GetFiles(CodecDirPath).Length > 0)
                        return;
                    
                System.IO.Compression.ZipFile.ExtractToDirectory(TempFileName, CodecDirPath);
            }
            catch
            {

            }
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            ExtractPackFile();
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
