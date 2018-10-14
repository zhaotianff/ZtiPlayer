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
        Storyboard showVideoListAnimation;
        Storyboard hideVideoListAnimation;

        AxAPlayer3Lib.AxPlayer player;
        System.Windows.Threading.DispatcherTimer timer;

        private int elapsedTime = 0;

        public MainWindow()
        {
            InitializeComponent();           
            Init();        
        }

        private void Init()
        {
            InitTimer();
            InitializeVideoPlayer();

            showVideoListAnimation = (Storyboard)this.TryFindResource("ShowVideoListAnimation");
            hideVideoListAnimation = (Storyboard)this.TryFindResource("HideVideoListAnimation");
            SetBackground("");
            LoadDemoData();

            InitializeCommands();
            
        }

        #region Command
        private void InitializeCommands()
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
        }
        private void LoadDemoData()
        {
            List<VideoItem> listDemoVideo = new List<VideoItem>()
            {
                new VideoItem()
                {
                    Durationtime = new TimeSpan(0,0,1),
                    Name = "Demo1",
                    Url = "http://www.demo.com/1.mp4"
                },
                new VideoItem()
                {
                    Durationtime = new TimeSpan(0,0,1),
                    Name = "Demo2",
                    Url = "http://www.demo.com/2.mp4"
                },
                new VideoItem()
                {
                    Durationtime = new TimeSpan(0,0,1),
                    Name = "Demo3",
                    Url = "http://clips.vorwaerts-gmbh.de/big_buck_bunny.mp4"
                }
            };

            this.list_Video.ItemsSource = listDemoVideo;
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
        }

        private void OpenVideoSuccess()
        {
            elapsedTime = 0;
            this.lbl_Elapsed.Content = "00:00:00";
            int durationMillionSeconds = player.GetDuration();
            this.lbl_Duration.Content = GetTimeString(durationMillionSeconds);

            this.slider_Progress.Value = 0;
            this.slider_Progress.Maximum = durationMillionSeconds / 1000;
            timer.IsEnabled = true;   
        }

        private string GetTimeString(int millionSeconds)
        {
            int seconds = 0, minutes = 0, hours = 0;
            TimeSpan ts = TimeSpan.FromMilliseconds(millionSeconds);
            seconds = ts.Seconds;
            minutes = ts.Minutes;
            hours = ts.Hours;
            return hours.ToString("00:") + minutes.ToString("00:") + minutes.ToString("00");
        }

        private void UpdateElapsedTime()
        {
            elapsedTime++;
            TimeSpan ts = TimeSpan.FromSeconds(elapsedTime);
            this.Dispatcher.Invoke(()=> {
                this.lbl_Elapsed.Content = ts.Hours.ToString("00:") + ts.Minutes.ToString("00:") + ts.Seconds.ToString("00");
                this.slider_Progress.Value = elapsedTime;
            });
        }

        private void HandleStateChange(int oldState,int newState)
        {
            //TODO
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
                    player.Open(item.Url);
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
            System.Windows.Forms.OpenFileDialog openDialog = new System.Windows.Forms.OpenFileDialog();
            openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            openDialog.Filter = "mp4视频文件|*.mp4";
            openDialog.RestoreDirectory = true;
            if(openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                player.Open(openDialog.FileName);
                player.Play();
            }
        }

        private void menu_OpenNetwork_Click(object sender, RoutedEventArgs e)
        {
            popup_OpenMenu.IsOpen = false;
            //TODO
        }

        private void slider_Progress_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            timer.IsEnabled = false;
            this.player.SetPosition((int)this.slider_Progress.Value * 1000);
        }
        #endregion
    }
}
