using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        PlayerSetting playerSetting = new PlayerSetting();
        System.Windows.Threading.DispatcherTimer updateElapsedTimer;
        System.Windows.Threading.DispatcherTimer cursorCheckTimer;
        XmlHelper playlistXmlHelper = new XmlHelper();
        System.Windows.Controls.ContextMenu contextMenu;
        ObservableCollection<VideoItem> playList = new ObservableCollection<VideoItem>();

        bool isActiveStop = false;

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
            CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu));
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

        private void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            if (element == null)
                return;

            var point = WindowState == WindowState.Maximized ? new Point(0, element.ActualHeight)
                : new Point(Left + BorderThickness.Left, element.ActualHeight + Top + BorderThickness.Top);
            point = element.TransformToAncestor(this).Transform(point);
            SystemCommands.ShowSystemMenu(this, point);
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

        private void SetPlayerControlAnimationFlag(bool value)
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

        private void DealWithAplayerKeyDown(int lParam,int wParam)
        {
            //这里不通过键码了，直接判断键名
            StringBuilder sb = new StringBuilder(32);
            WinAPI.GetKeyNameText(lParam, sb, 32);

            if(sb.ToString() == "Space")
            {
                PlayOrPause();
            }

            if(sb.ToString() == "Enter")
            {
                FullScreenOrRestore();
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

        private void LoadPlayList()
        {
            playList =  new ObservableCollection<VideoItem>(playlistXmlHelper.LoadPlayList());
            this.list_Video.ItemsSource = playList;
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
                case Win32Message.WM_KEYDOWN:
                    DealWithAplayerKeyDown(lParam, wParam);
                    break;
                //case Win32Message.WM_LBUTTONDOWN:
                //    PlayOrPause();
                //    break;
                case Win32Message.WM_LBUTTONDBLCLK:
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
            if (string.IsNullOrEmpty(path))
                return;

            VideoItem item = new VideoItem();
            item.Path = path;
            item.Type = 0;
            item.Name = PlayerHelper.GetVideoName(path,item.Type);
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
            item.Name = PlayerHelper.GetVideoName(path,item.Type);
            Open(item);
        }

        private void Open(VideoItem item)
        {
            if(item.Type == 0 && System.IO.File.Exists(item.Path) == false )
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

        private void ShowOpenFolderDialog()
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowserDialog.RootFolder = Environment.SpecialFolder.Desktop;
            if(folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var folder = folderBrowserDialog.SelectedPath;
                var files = FileHelper.GetAllFiles(folder);
                playList.Union(files);
                OpenLocalFile(files.FirstOrDefault());

                //TODO update list
                playlistXmlHelper.UpdatePlayList(playList);
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
            this.lbl_Duration.Content = PlayerHelper.GetTimeString(durationMillionSeconds);

            this.slider_Progress.Value = 0;
            this.slider_Progress.Maximum = durationMillionSeconds / 1000;
            updateElapsedTimer.IsEnabled = true;

            this.btn_Pause.SetValue(ImageButton.ImageProperty, "../Icon/pause.png");

            HideNavigationButton();

            //TODO
            UpdatePlayList();           
        }

        private void ClearPlayList()
        {
            playList.Clear();
            playlistXmlHelper.ClearPlayList();
            CurrentSelectedIndex = -1;
        }

        private void UpdateElapsedTime()
        {         
            this.Dispatcher.Invoke(()=> {
                ElapsedTime = this.player.GetPosition();
                this.lbl_Elapsed.Content = PlayerHelper.GetTimeString(ElapsedTime);              
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
            var existVideoItem = playList?.Where(x => x.Name == CurrentVideoItem.Name && x.Path == CurrentVideoItem.Path).FirstOrDefault();

            if (existVideoItem == null)
            {
                VideoItem videoItem = new VideoItem();
                videoItem.Name = CurrentVideoItem.Name;
                videoItem.Path = CurrentVideoItem.Path;
                videoItem.Type = CurrentVideoItem.Type;
                videoItem.Duration = TimeSpan.FromMilliseconds(player.GetDuration());
                videoItem.DurationStr = PlayerHelper.GetTimeString(videoItem.Duration.TotalMilliseconds);
                playlistXmlHelper.AddToPlayList(videoItem);
                playList.Add(videoItem);
                CurrentSelectedIndex = playList.Count - 1;
                list_Video.SelectedIndex = CurrentSelectedIndex;
            }
            else
            {
                CurrentSelectedIndex = playList.IndexOf(existVideoItem);
                playList[CurrentSelectedIndex].Duration = TimeSpan.FromMilliseconds(player.GetDuration());
                playList[CurrentSelectedIndex].DurationStr = PlayerHelper.GetTimeString(existVideoItem.Duration.TotalMilliseconds);
                list_Video.SelectedIndex = CurrentSelectedIndex;
                playlistXmlHelper.UpdatePlayList(playList[CurrentSelectedIndex], CurrentSelectedIndex);
            }
        }

        private void HandleStateChange(int oldState,int newState)
        {         
            if(oldState == (int)PlayState.PS_PLAY && newState == (int)PlayState.PS_CLOSING)
            {
                StopPlay();

                if (isActiveStop == false)
                {
                    RepeatPlay(playerSetting.RepeatMode);
                }
            }
        }

        private void RepeatPlay(PlayerRepeatMode playerRepeatMode)
        {
            switch(playerRepeatMode)
            {
                case PlayerRepeatMode.PlayNext:
                    PlayNextVideo();
                    break;
                case PlayerRepeatMode.PlayRandom:
                    PlayRandomVideo();
                    break;
                case PlayerRepeatMode.SingleRepeat:
                    PlayCurrentVideo();
                    break;
            }
        }

        private void PlayNextVideo()
        {
            if(list_Video.SelectedIndex < playList.Count)
            {
                list_Video.SelectedIndex++;
            }
            else
            {
                list_Video.SelectedIndex = 0;
            }
           
            list_Video_MouseDoubleClick(null, null);
        }

        private void PlayRandomVideo()
        {

        }

        private void PlayCurrentVideo()
        {

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
                item = playList[CurrentSelectedIndex];
                if(item != null)
                    Open(item);
            }
            else if(CurrentSelectedIndex < playList.Count -1)
            {
                item = playList[CurrentSelectedIndex + 1];
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
                item = playList[CurrentSelectedIndex];
                if (item != null)
                    Open(item);
            }
            else if (CurrentSelectedIndex > 0)
            {
                item = playList[CurrentSelectedIndex - 1];
                if (item != null)
                    Open(item);
            }
        }

        private void EnablePlayerProgress()
        {
            isActiveStop = false;
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

        private void OpenFileLocation(string path)
        {
            //explorer /select, "C:\Users\Dream\Pictures\xx.mp4"
            if(System.IO.File.Exists(path) == false)
            {
                MessageBox.Show("文件不存在");
                return;
            }

            System.Diagnostics.Process.Start("explorer","/select, " + path);
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
                showVideoListAnimation?.Begin();
            }

            if(e.Key == Key.Space)
            {
                PlayOrPause();
            }

            if(e.Key == Key.Enter)
            {
                FullScreenOrRestore();
            }
        }

        private void list_Video_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if(this.list_Video.SelectedIndex != -1)
            {
                VideoItem item = playList[list_Video.SelectedIndex];
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
            this.lbl_Elapsed.Content = PlayerHelper.GetTimeString(time);
        }

        private void btn_Stop_Click(object sender, RoutedEventArgs e)
        {
            isActiveStop = true;
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

        private void menu_OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            ShowOpenFolderDialog();
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
            if (list_Video.SelectedIndex == -1)
                return;

            var videoItem = list_Video.SelectedItem as VideoItem;

            if (videoItem != null)
            {
                WinAPI.ShowFileProperties(videoItem.Path);
            }          
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
            if (list_Video.SelectedIndex == -1)
                return;

            var videoItem = list_Video.SelectedItem as VideoItem;

            if(videoItem != null)
            {
                OpenFileLocation(videoItem.Path);
            }
        }

        private void menu_DeleteChoose_Click(object sender, RoutedEventArgs e)
        {
            var index = list_Video.SelectedIndex;

            if (index == -1)
                return;

            if(playList.Count > 0)
            {
                RemovePlaylistRecord(index);
                playList.RemoveAt(index);
            }
        }

        private void RemovePlaylistRecord(int index)
        {
            playlistXmlHelper.RemoveFromPlayList(index);
        }

        private void menu_PlayChoose_Click(object sender, RoutedEventArgs e)
        {
            list_Video_MouseDoubleClick(null, null);
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

        private void btn_Fullscreen_Click(object sender, RoutedEventArgs e)
        {
            FullScreenOrRestore();
        }
        #endregion
    }
}
