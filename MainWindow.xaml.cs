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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ZtiPlayer.Models;
using ZtiPlayer.Utils;

namespace ZtiPlayer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string playlistConfigFilePath = Environment.CurrentDirectory + "\\config\\playlist.xml";

        Storyboard showVideoListAnimation;
        Storyboard hideVideoListAnimation;

        AxAPlayer3Lib.AxPlayer player;
        System.Windows.Threading.DispatcherTimer timer;
        XmlHelper playlistXmlHelper = new XmlHelper();

        int elapsedTime = 0;
        int videoType = 0;
        string videoPath = "";

        public MainWindow()
        {
            InitializeComponent();           
            Init();        
        }

        private void Init()
        {
            InitTimer();
            InitializeVideoPlayer();
            InitCfg();
            InitCommands();

            showVideoListAnimation = (Storyboard)this.TryFindResource("ShowVideoListAnimation");
            hideVideoListAnimation = (Storyboard)this.TryFindResource("HideVideoListAnimation");
            SetBackground("");
            LoadPlayList();          
            HideFormHost();
        }

        #region Command
        private void InitCommands()
        {
            CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeWindow, CanResizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
            CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, RestoreWindow, CanResizeWindow));
        }

        private void CanResizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode == ResizeMode.CanResize || ResizeMode == ResizeMode.CanResizeWithGrip;
        }

        private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ResizeMode != ResizeMode.NoResize;
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();            
        }

        private void MaximizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void RestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        public void SetBackground(string filePath)
        {
            if(string.IsNullOrEmpty(filePath))
            {
                player.SetCustomLogo(-1);       
                //player.SetConfig((int)ConfigInterface.LogoSettings, "16777215;0;0");       
            }
            else
            {
                player.SetCustomLogo(new System.Drawing.Bitmap(filePath).GetHbitmap().ToInt32());
            }
        }
        #endregion


        #region Function
        private void InitTimer()
        {
            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (a, b) => { UpdateElapsedTime(); };
        }
        private void InitializeVideoPlayer()
        {
            player = new AxAPlayer3Lib.AxPlayer();
            ((System.ComponentModel.ISupportInitialize)(this.player)).BeginInit();
            player.Dock = System.Windows.Forms.DockStyle.Fill;
            player.OnMessage += (a, b) => { MessageHandler(b.nMessage, b.lParam, b.wParam); };
            player.OnBuffer += (a, b) => { Buffering(b.nPercent); };
            player.OnDownloadCodec += (a, b) => { DownloadCodecDialog(b.strCodecPath); };
            player.OnOpenSucceeded += (a, b) => { OpenVideoSuccess(); };
            player.OnStateChanged += (a, b) => { HandleStateChange(b.nOldState, b.nNewState); };
            panel.Controls.Add(player);
            ((System.ComponentModel.ISupportInitialize)(this.player)).EndInit();


            //Volume
            this.slider_Volume.Value = player.GetVolume();
        }

        private void InitCfg()
        {
            try
            {
                playlistXmlHelper.OpenFile(playlistConfigFilePath);
            }
            catch(Exception ex)
            {
                //TODO
            }
        }

        private void LoadDemoData()
        {
            List<VideoItem> listDemoVideo = new List<VideoItem>()
            {
                new VideoItem()
                {
                    Duration = new TimeSpan(0,0,1),
                    Name = "Demo1",
                    Path = "http://www.demo.com/1.mp4"
                },
                new VideoItem()
                {
                    Duration = new TimeSpan(0,0,1),
                    Name = "Demo2",
                    Path = "http://www.demo.com/2.mp4"
                },
                new VideoItem()
                {
                    Duration = new TimeSpan(0,0,1),
                    Name = "Demo3",
                    Path = "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4"
                }
            };

            this.list_Video.ItemsSource = listDemoVideo;
        }

        private void LoadPlayList()
        {
            this.list_Video.ItemsSource = playlistXmlHelper.LoadPlayList();
        }

        private void ShowOrHideVideoList()
        {
            if (this.grid_List.Width == 0)
            {
                if (showVideoListAnimation != null)
                {                               
                    this.WindowState = System.Windows.WindowState.Normal;
                }
            }         
            else
            {
                if (hideVideoListAnimation != null)
                {
                    hideVideoListAnimation.Begin();
                    this.grid_Function.Height = 0;
                    this.WindowState = System.Windows.WindowState.Maximized;
                }
            }
        }

        private void MessageHandler(int nMessage,int lParam,int wParam)
        {
            switch (nMessage)
            {
                case Win32Message.WM_LBUTTONDBLCLK:
                    ShowOrHideVideoList();
                    break;
                case Win32Message.WM_RBUTTONDOWN:
                    //TODO ContextMenu
                    break;
                default:
                    break;
            }
        }

        private void Buffering(int nPercent)
        {
            Dispatcher.Invoke(()=> {
                this.lbl_BufferState.Content = "正在缓冲" + nPercent + "%d";
            });
        }

        private void DownloadCodecDialog(string codec)
        {
            MessageBox.Show(codec);
            player.Close();
        }

        private void OpenLocalFile()
        {
            System.Windows.Forms.OpenFileDialog openDialog = new System.Windows.Forms.OpenFileDialog();
            //openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            openDialog.Filter = "mp4视频文件|*.mp4|所有文件|*.*";
            openDialog.RestoreDirectory = false;
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                videoPath = openDialog.FileName;
                player.Open(videoPath);
                player.Play();
            }           
        }

        private void ShowOpenUrlDialog()
        {
            //TODO
            List<string> list = new List<string>();
            OpenURLDialog openURLDialog = new OpenURLDialog(list);
            if(openURLDialog.ShowDialog().Value == true)
            {
                HideNavigationButton();                
            }
        }

        private void ShowNavigationButton()
        {
            this.formhost.Visibility = Visibility.Hidden;
            this.gridhost.Visibility = Visibility.Visible;
        }

        private void HideNavigationButton()
        {
            this.formhost.Visibility = Visibility.Visible;
            this.gridhost.Visibility = Visibility.Hidden;
        }

        private void HideFormHost()
        {
            this.formhost.Visibility = Visibility.Hidden;
        }


        private void OpenVideoSuccess()
        {
            this.lbl_Elapsed.Content = "00:00:00";
            int durationMillionSeconds = player.GetDuration();
            this.lbl_Duration.Content = GetTimeString(durationMillionSeconds);

            this.slider_Progress.Value = 0;
            this.slider_Progress.Maximum = durationMillionSeconds / 1000;
            timer.IsEnabled = true;

            this.btn_Pause.SetValue(ImageButton.ImageProperty, "../Icon/pause.png");

            HideNavigationButton();

            VideoItem videoItem = new VideoItem();
            videoItem.Name = GetVideoName(videoPath);
            videoItem.Path = videoPath;
            videoItem.Type = videoType;
            videoItem.Duration =TimeSpan.FromMilliseconds( durationMillionSeconds);
            playlistXmlHelper.AddToPlayList(videoItem);
        }

        private string GetTimeString(int millionSeconds)
        {
            int seconds = 0, minutes = 0, hours = 0;
            TimeSpan ts = TimeSpan.FromMilliseconds(millionSeconds);
            seconds = ts.Seconds;
            minutes = ts.Minutes;
            hours = ts.Hours;
            return hours.ToString("00") + ":" +  minutes.ToString("00") +   ":" + seconds.ToString("00");
        }

        private string GetVideoName(string path)
        {
            path = path.ToLower();
            if(videoType == 0)
            {
                return System.IO.Path.GetFileName(path);
            }
            else if(videoType == 1)
            {
                path = path.Substring(path.LastIndexOf("/") + 1);
                return System.Web.HttpUtility.UrlDecode(path);
            }
            else
            {
                return "";
            }
        }

        private void UpdateElapsedTime()
        {         
            this.Dispatcher.Invoke(()=> {
                elapsedTime = this.player.GetPosition();
                this.lbl_Elapsed.Content = GetTimeString(elapsedTime);              
                this.slider_Progress.Value = elapsedTime/1000;
            });
        }

        private void HandleStateChange(int oldState,int newState)
        {
            //TODO
        }

        private void SetProgress(int value)
        {
            slider_Progress.Value = value;

            if(value == 0)
            {
                timer.IsEnabled = false;
                this.lbl_Elapsed.Content = "00:00:00";
            }
        }

        private void StopPlay()
        {
            player.Close();
            SetProgress(0);
            this.btn_Pause.SetValue(ImageButton.ImageProperty, "../Icon/play.png");
            ShowNavigationButton();
        }

        private void InvalidateContextMenu()
        {
            bool isEnabledFlag = true;
            if(this.list_Video.SelectedIndex == -1)
            {
                isEnabledFlag = false;
            }

            MenuItem item = new MenuItem();
            for (int i = 0; i < 4; i++)
            {
                item = this.contextmenu_VideoList.Items[i] as MenuItem;
                item.IsEnabled = isEnabledFlag;
            }
        }
        #endregion

        #region Event
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            if(grid_List.Width == 0)
            {
                showVideoListAnimation.Begin();
            }
            else
            {
                hideVideoListAnimation.Begin();
            }

        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if(this.WindowState == System.Windows.WindowState.Normal)
            {
                this.grid_Function.Height = 120;                
                if (grid_List.Width == 0)
                {
                    if (showVideoListAnimation != null)
                        showVideoListAnimation.Begin();
                }
            }           
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.WindowState = System.Windows.WindowState.Normal;              
            }

        }

        private void list_Video_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(this.list_Video.SelectedIndex != -1)
            {
                VideoItem item = this.list_Video.SelectedItem as VideoItem;
                if(item != null)
                {
                    //TODO Check if resource is available
                    player.Open(item.Path);
                    player.Play();
                }
            }
        }

        private void btn_Open_Click(object sender, RoutedEventArgs e)
        {
            popup_OpenMenu.IsOpen = true;
        }

        private void menu_OpenLocal_Click(object sender, RoutedEventArgs e)
        {
            popup_OpenMenu.IsOpen = false;
            OpenLocalFile();
        }

        private void menu_OpenNetwork_Click(object sender, RoutedEventArgs e)
        {
            popup_OpenMenu.IsOpen = false;
            ShowOpenUrlDialog();
        }

        private void slider_Progress_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            int time = (int)this.slider_Progress.Value * 1000;
            this.player.SetPosition(time);
            this.lbl_Elapsed.Content = GetTimeString(time);
        }

        private void btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            StopPlay();
        }


        private void btn_Pause_Click(object sender, RoutedEventArgs e)
        {
            if(player.GetState() == (int)PlayState.PS_PLAY)
            {
                player.Pause();
                this.btn_Pause.SetValue(ImageButton.ImageProperty, "../Icon/play.png");
            }
            else if(player.GetState() == (int)PlayState.PS_PAUSED)
            {
                player.Play();
                this.btn_Pause.SetValue(ImageButton.ImageProperty, "../Icon/pause.png");
            }
        }

        private void slider_Volume_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            this.player.SetVolume((int)this.slider_Volume.Value);
        }

        private void btn_Volume_Click(object sender, RoutedEventArgs e)
        {
            int volume = this.player.GetVolume();
            if(volume == 0)
            {
                player.SetVolume(50);
                slider_Volume.Value = 50;
                btn_Volume.Image = "../Icon/sound.png";
            }
            else
            {
                player.SetVolume(0);
                slider_Volume.Value = 0;
                btn_Volume.Image = "../Icon/sound_silent.png";
            }
        }

        private void btn_OpenLocalFile_Click(object sender, RoutedEventArgs e)
        {
            OpenLocalFile();
        }

        private void btn_OpenUrl_Click(object sender, RoutedEventArgs e)
        {
            ShowOpenUrlDialog();
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            InvalidateContextMenu();
        }
        #endregion       
    }
}
