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

        public MainWindow()
        {
            InitializeComponent();           
            Init();        
        }

        private void Init()
        {
            InitializeVideoPlayer();

            showVideoListAnimation = (Storyboard)this.TryFindResource("ShowVideoListAnimation");
            hideVideoListAnimation = (Storyboard)this.TryFindResource("HideVideoListAnimation");
            SetBackground("");
            LoadDemoData();

            InitializeCommands();
            
        }

        private void InitializeVideoPlayer()
        {
            player = new AxAPlayer3Lib.AxPlayer();
            ((System.ComponentModel.ISupportInitialize)(this.player)).BeginInit();
            player.Dock = System.Windows.Forms.DockStyle.Fill;
            player.OnMessage += (a, b) => { if (b.nMessage == 0x0203) ShowOrHideVideoList(); };
            panel.Controls.Add(player);
            ((System.ComponentModel.ISupportInitialize)(this.player)).EndInit();           
        }

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
            if (list_Video.Width == 0)
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

        #region Event
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            if(list_Video.Width == 0)
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
                if (list_Video.Width == 0)
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

        #endregion    
       
        
    }
}
