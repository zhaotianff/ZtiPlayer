using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
        private static readonly string ScreenShotSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        private static readonly string ScreenShotExtension = ".jpg";
        private const string PlaySpeedSlow = "0.5";
        private const string PlaySpeedNormal = "1";
        private const string PlaySpeedFastOnePointFive = "1.5";
        private const string PlaySpeedFastTwice = "2";
        private const string PlaySpeedFastFourth = "4";

        Storyboard showVideoListAnimation;
        Storyboard hideVideoListAnimation;
        Storyboard showPlayerControlAnimation;
        Storyboard hidePlayerControlAnimation;

        AxAPlayer3Lib.AxPlayer player;
        System.Windows.Threading.DispatcherTimer updateElapsedTimer;
        System.Windows.Threading.DispatcherTimer cursorCheckTimer;
        XmlHelper playlistXmlHelper = new XmlHelper();
        System.Windows.Controls.ContextMenu contextMenu;

        private int ElapsedTime { get; set; } = 0;
        private int MousePointCheckTick { get; set; } = 0;
        private Point LastMousePoint { get; set; }
        private int CurrentSelectedIndex { get; set; } = -1;
        private VideoItem CurrentVideoItem { get; set; }
        private bool PlayerControlAnimationFlag { get; set; } = false;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        public MainWindow(StartupArgs args)
        {
            InitializeComponent();
            Init();
            ParseArgs(args);
        }

        private void Init()
        {
            InitTimer();
            InitializeVideoPlayer();
            InitCfg();
            InitCommands();
            InitContextMenu();

            showVideoListAnimation = (Storyboard)this.TryFindResource("ShowVideoListAnimation");
            hideVideoListAnimation = (Storyboard)this.TryFindResource("HideVideoListAnimation");
            showPlayerControlAnimation = (Storyboard)this.TryFindResource("ShowPlayerControlAnimation");
            hidePlayerControlAnimation = (Storyboard)this.TryFindResource("HidePlayerControlAnimation");
            hidePlayerControlAnimation.Completed += async (a, b) => { await Task.Delay(500); SetPlayerControlAnimationFlag(false); };
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
                //player.SetConfig((int)PlayerConfig.LogoSettings, "16777215;0;0");       
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
            updateElapsedTimer = new System.Windows.Threading.DispatcherTimer();
            updateElapsedTimer.Interval = TimeSpan.FromSeconds(1);
            updateElapsedTimer.Tick += (a, b) => { UpdateElapsedTime(); };

            cursorCheckTimer = new System.Windows.Threading.DispatcherTimer();
            cursorCheckTimer.Interval = TimeSpan.FromSeconds(1);
            cursorCheckTimer.Tick += (a, b) => { CheckCursorPoint(); };
        }

        private async void SetPlayerControlAnimationFlag(bool value)
        {
            PlayerControlAnimationFlag = value;
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

            DisablePlayerProgress();
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

        private void InitContextMenu()
        {
            try
            {
                contextMenu = this.TryFindResource("contextMenu") as System.Windows.Controls.ContextMenu;
                this.formhost.ContextMenu = contextMenu;
            }
            catch
            {

            }
        }

        private void ShowContextMenu()
        {
            if(contextMenu != null)
            {
                //ContextMenu Rule
                this.formhost.ContextMenu.IsOpen = true;
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
            if (grid_List.Width == 0)
            {
                showVideoListAnimation?.Begin();
            }
            else
            {
                hideVideoListAnimation?.Begin();
            }
        }

        private void MessageHandler(int nMessage,int lParam,int wParam)
        {
            switch (nMessage)
            {
                case Win32Message.WM_LBUTTONDBLCLK:
                    //TODO 使用单击判断 增加播放暂停 
                    FullScreenOrRestore();
                    break;
                case Win32Message.WM_RBUTTONDOWN:
                    //TODO ContextMenu
                    ShowContextMenu();
                    break;
                case Win32Message.WM_MOUSEMOVE:
                    ShowFullscreenPlayerControl();
                    break;
                default:
                    break;
            }
        }

        private void Buffering(int nPercent)
        {
            Dispatcher.Invoke(()=> {
                if(nPercent == 100)
                {
                    this.lbl_BufferState.Content = "";
                }
                else
                {
                    this.lbl_BufferState.Content = "正在缓冲" + nPercent + "%";
                }             
            });
        }

        private void DownloadCodecDialog(string codec)
        {
            MessageBox.Show(codec);
            player.Close();
        }

        private void BrowseLocalFile()
        {
            System.Windows.Forms.OpenFileDialog openDialog = new System.Windows.Forms.OpenFileDialog();
            //openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            openDialog.Filter = "mp4视频文件|*.mp4|所有文件|*.*";
            openDialog.RestoreDirectory = false;
            if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenLocalFile(openDialog.FileName);
            }
        }

        private void OpenLocalFile(string path)
        {
            VideoItem item = new VideoItem();
            item.Path = path;
            item.Type = 0;
            item.Name = GetVideoName(path,item.Type);
            Open(item);
        }

        private void OpenNetworkFile()
        {
            
        }

        private void OpenNetworkFile(string path)
        {
            VideoItem item = new VideoItem();
            if(!(path.StartsWith("https") || path.StartsWith("http") || path.StartsWith("ftp")))
            {
                path = "http://" + path;
            }
            item.Path = path;
            item.Type = 1;
            item.Name = GetVideoName(path,item.Type);
            Open(item);
        }

        private void Open(VideoItem item)
        {
            if(System.IO.File.Exists(item.Path) == false )
            {
                MessageBox.Show("文件不存在");
                return;
            }

            CurrentVideoItem = item;

            this.player.Open(item.Path);
            this.player.Play();
            EnablePlayerProgress();
        }

        private void ShowOpenUrlDialog()
        {
            //TODO
            List<string> list = new List<string>();
            OpenURLDialog openURLDialog = new OpenURLDialog(list);
            if(openURLDialog.ShowDialog().Value == true)
            {
                HideNavigationButton();
                OpenNetworkFile(openURLDialog.Url);              
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
            updateElapsedTimer.IsEnabled = true;

            this.btn_Pause.SetValue(ImageButton.ImageProperty, "../Icon/pause.png");

            HideNavigationButton();
            UpdatePlayList();           
        }

        private void ClearPlayList()
        {
            this.list_Video.ItemsSource = null;
            playlistXmlHelper.ClearPlayList();
            CurrentSelectedIndex = -1;
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

        private string GetVideoName(string path,int videoType)
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
                ElapsedTime = this.player.GetPosition();
                this.lbl_Elapsed.Content = GetTimeString(ElapsedTime);              
                this.slider_Progress.Value = ElapsedTime/1000;
            });
        }

        private void CheckCursorPoint()
        {
            MousePointCheckTick++;

            if (MousePointCheckTick == 1)
            {
                LastMousePoint = Mouse.GetPosition(this);
                return;
            }

            var currentPoint = Mouse.GetPosition(this);

            if(LastMousePoint != currentPoint)
            {
                MousePointCheckTick = 0;
                ShowFullscreenPlayerControl();
                return;
            }

            if(MousePointCheckTick == 3)
            {
                HideFullscreenPlayerControl();
            }
        }

        private void UpdatePlayList()
        {
            var list = list_Video.ItemsSource as List<VideoItem>;
            var existVideoItem = list?.Where(x => x.Name == CurrentVideoItem.Name && x.Path == CurrentVideoItem.Path).FirstOrDefault();

            if (existVideoItem == null)
            {
                VideoItem videoItem = new VideoItem();
                videoItem.Name = CurrentVideoItem.Name;
                videoItem.Path = CurrentVideoItem.Path;
                videoItem.Type = CurrentVideoItem.Type;
                videoItem.Duration = TimeSpan.FromMilliseconds(player.GetDuration());
                var playlist = playlistXmlHelper.AddToPlayList(videoItem);
                this.list_Video.ItemsSource = playlist;
                CurrentSelectedIndex = playlist.Count - 1;
                list_Video.SelectedIndex = CurrentSelectedIndex;
            }
            else
            {
                CurrentSelectedIndex = list.IndexOf(existVideoItem);
                list_Video.SelectedIndex = CurrentSelectedIndex;
            }
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
                updateElapsedTimer.IsEnabled = false;
                this.lbl_Elapsed.Content = "00:00:00";
            }
        }

        private void StopPlay()
        {
            player.Close();
            SetProgress(0);
            this.btn_Pause.SetValue(ImageButton.ImageProperty, "../Icon/play.png");
            ShowNavigationButton();
            DisablePlayerProgress();
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

        private void ScreenShot()
        {
            var fileName = System.IO.Path.Combine(ScreenShotSavePath,
                System.IO.Path.GetFileNameWithoutExtension(CurrentVideoItem.Path) 
                + DateTime.Now.ToString("HH_mm_ss") 
                + ScreenShotExtension);
            player.SetConfig((int)PlayerConfig.SnapshotImage, fileName);
        }

        private void PlayNext()
        {
            if (list_Video.Items.Count == 0)
                return;

            VideoItem item = new VideoItem();
            if(CurrentSelectedIndex == -1)
            {
                CurrentSelectedIndex = 0;
                list_Video.SelectedIndex = 0;
                item = list_Video.Items[CurrentSelectedIndex] as VideoItem;
                if(item != null)
                    Open(item);
            }
            else if(CurrentSelectedIndex < list_Video.Items.Count -1)
            {
                item = list_Video.Items[CurrentSelectedIndex + 1] as VideoItem;
                if (item != null)
                    Open(item);
            }

            //TODO Disable next
        }

        private void PlayPrevious()
        {
            VideoItem item = new VideoItem();
            if (CurrentSelectedIndex == -1)
            {
                CurrentSelectedIndex = 0;
                list_Video.SelectedIndex = 0;
                item = list_Video.Items[CurrentSelectedIndex] as VideoItem;
                if (item != null)
                    Open(item);
            }
            else if (CurrentSelectedIndex > 0)
            {
                item = list_Video.Items[CurrentSelectedIndex - 1] as VideoItem;
                if (item != null)
                    Open(item);
            }
        }

        private void EnablePlayerProgress()
        {
            slider_Progress.IsEnabled = true;
        }

        private void DisablePlayerProgress()
        {
            slider_Progress.IsEnabled = false;
        }

        private void ParseArgs(StartupArgs args)
        {
            if(args.ArgsCount > 0)
            {
                //TODO 
                try
                {
                    if (args.Width > 500 && args.Width < SystemParameters.PrimaryScreenWidth)
                    {
                        this.Width = args.Width;
                    }
                    if (args.Height > 300 && args.Height < SystemParameters.PrimaryScreenHeight)
                    {
                        this.Height = args.Height;
                    }
                    if (args.Volume >= 0 && args.Volume <= 100)
                    {
                        this.player.SetVolume(args.Volume);
                    }
                    if(!string.IsNullOrEmpty(args.Path))
                    {
                        var tempPath = args.Path.ToLower();
                        if (tempPath.StartsWith("http") || tempPath.StartsWith("https") || tempPath.StartsWith("ftp"))
                            OpenNetworkFile(tempPath);
                        else
                            OpenLocalFile(tempPath);
                    }
                }
                catch (Exception ex)
                {
                    //TODO
                }
            }
        }

        private void ShowVersionInfo()
        {        
            var versionStr = $"程序版本:{Assembly.GetExecutingAssembly().GetName().Version.ToString()}" +
                $"\nAplayer engine 版本:{player.GetVersion()}";

            MessageBox.Show(versionStr);
        }

        private void ShowDetailVersionInfo()
        {
            var detailVersionStr = $"{player.GetConfig((int)PlayerConfig.EngineLibVersion)}";

            MessageBox.Show(detailVersionStr);
        }

        private void FullScreenOrRestore()
        {
            ShowOrHideVideoList();
            if(this.WindowState == WindowState.Maximized)
            {
                Restore();
            }
            else
            {
                FullScreen();
            }         
        }

        private void FullScreen()
        {
            hidePlayerControlAnimation?.Begin();
            this.WindowStyle = WindowStyle.None;
            this.WindowState = System.Windows.WindowState.Maximized;
            StartCursorCheck();
        }

        private void Restore()
        {
            showPlayerControlAnimation?.Begin();
            this.WindowStyle = WindowStyle.SingleBorderWindow;
            this.WindowState = System.Windows.WindowState.Normal;
            StopCursorCheck();
        }

        private void StartCursorCheck()
        {
            MousePointCheckTick = 0;
            cursorCheckTimer.IsEnabled = true;
        }

        private void StopCursorCheck()
        {
            cursorCheckTimer.IsEnabled = false;
        }

        private void PlayOrPause()
        {
            if (player.GetState() == (int)PlayState.PS_PLAY)
            {
                player.Pause();
                this.btn_Pause.SetValue(ImageButton.ImageProperty, "../Icon/play.png");
            }
            else if (player.GetState() == (int)PlayState.PS_PAUSED)
            {
                player.Play();
                this.btn_Pause.SetValue(ImageButton.ImageProperty, "../Icon/pause.png");
            }
        }

        private void ShowFullscreenPlayerControl()
        {
            if (this.WindowStyle == WindowStyle.None && this.WindowState == WindowState.Maximized && grid_Function.Height == 0 && PlayerControlAnimationFlag == false)
            {
                StartCursorCheck();
                showPlayerControlAnimation?.Begin();
            }           
        }

        private void HideFullscreenPlayerControl()
        {
            StopCursorCheck();
            SetPlayerControlAnimationFlag(true);
            hidePlayerControlAnimation?.Begin();
        }
        int MyHookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            //LPKBDLLHOOKSTRUCT hookStruct = (LPKBDLLHOOKSTRUCT)lParam;
            //if (code == HC_ACTION)
            //{
            //    switch (wParam)
            //    {
            //        case WM_KEYDOWN:
            //        case WM_SYSKEYDOWN:
            //        case WM_KEYUP:
            //        case WM_SYSKEYUP:
            //            if (hookStruct->vkCode == VK_LWIN || hookStruct->vkCode == VK_RWIN ||
            //                  (hookStruct->vkCode == VK_ESCAPE) && ((GetKeyState(VK_CONTROL) & 0x8000) != 0))
            //            {
            //                return 1;
            //            }
            //            break;
            //        default:
            //            break;
            //    }
            //}
            //return LRESULT();

            return 0;
        }
        #endregion

        #region Event
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideVideoList();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Restore();              
            }

        }

        private void list_Video_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(this.list_Video.SelectedIndex != -1)
            {
                VideoItem item = this.list_Video.SelectedItem as VideoItem;
                if(item != null)
                {
                    Open(item);
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
            BrowseLocalFile();
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
            PlayOrPause();
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
            BrowseLocalFile();
        }

        private void btn_OpenUrl_Click(object sender, RoutedEventArgs e)
        {
            ShowOpenUrlDialog();
        }

        private void menu_Screenshot_Click(object sender, RoutedEventArgs e)
        {
            ScreenShot();
        }

        private void ContextMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            InvalidateContextMenu();
        }

        private void menu_ClearPlayList_Click(object sender, RoutedEventArgs e)
        {
            ClearPlayList();
        }

        private void btn_PlayNext_Click(object sender, RoutedEventArgs e)
        {
            PlayNext();
        }

        private void btn_PlayPrevious_Click(object sender, RoutedEventArgs e)
        {
            PlayPrevious();
        }

        private void menu_Property_Click(object sender, RoutedEventArgs e)
        {
            WinAPI.ShowFileProperties(CurrentVideoItem.Path);
        }

        private void menu_Next_Click(object sender, RoutedEventArgs e)
        {
            PlayNext();
        }

        private void menu_Previous_Click(object sender, RoutedEventArgs e)
        {
            PlayPrevious();
        }

        private void menu_Stop_Click(object sender, RoutedEventArgs e)
        {
            StopPlay();
        }

        private void menu_Pause_Click(object sender, RoutedEventArgs e)
        {
            player.Pause();
            this.btn_Pause.SetValue(ImageButton.ImageProperty, "../Icon/play.png");
        }

        private void menu_Play_Click(object sender, RoutedEventArgs e)
        {
            player.Play();
            this.btn_Pause.SetValue(ImageButton.ImageProperty, "../Icon/pause.png");
        }

        private void menu_Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            FullScreenOrRestore();
        }


        private void menu_OpenFileLocation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menu_DeleteChoose_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menu_PlayChoose_Click(object sender, RoutedEventArgs e)
        {

        }

        private void menu_Version_Click(object sender, RoutedEventArgs e)
        {
            ShowVersionInfo();
        }

        private void menu_VersionDetail_Click(object sender, RoutedEventArgs e)
        {
            ShowDetailVersionInfo();
        }

        private void menu_PlaySpeed_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;

            switch(menuItem.Header)
            {
                case PlaySpeedSlow:
                    player.SetConfig((int)PlayerConfig.PlaySpeed, "50");
                    break;
                case PlaySpeedNormal:
                    player.SetConfig((int)PlayerConfig.PlaySpeed, "100");
                    break;
                case PlaySpeedFastOnePointFive:
                    player.SetConfig((int)PlayerConfig.PlaySpeed, "150");
                    break;
                case PlaySpeedFastTwice:
                    player.SetConfig((int)PlayerConfig.PlaySpeed, "200");
                    break;
                case PlaySpeedFastFourth:
                    player.SetConfig((int)PlayerConfig.PlaySpeed, "400");
                    break;
            }
        }
    
        private void menu_Close_Click(object sender, RoutedEventArgs e)
        {
            StopPlay();
        }
        private void menu_ShowOrHidePlaylist_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideVideoList();
        }       
        #endregion

    }
}
