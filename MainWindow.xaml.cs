﻿using System;
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

        public MainWindow()
        {
            InitializeComponent();           
            Init();        
        }

        private void Init()
        {
            showVideoListAnimation = (Storyboard)this.TryFindResource("ShowVideoListAnimation");
            hideVideoListAnimation = (Storyboard)this.TryFindResource("HideVideoListAnimation");
            //SetBackground("");
            LoadDemoData();
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
                }
            };

            this.list_Video.ItemsSource = listDemoVideo;
        }

        private void ShowOrHideVideoList(bool isHide = false)
        {
            if(isHide == true)
            {
                if (hideVideoListAnimation != null)
                    hideVideoListAnimation.Begin();
                return;
            }
            if(list_Video.Width == 0)
            {
                if (showVideoListAnimation != null)
                    showVideoListAnimation.Begin();
            }
            else
            {
                if (hideVideoListAnimation != null)
                    hideVideoListAnimation.Begin();
            }
        }

        #region Event
        private void ImageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHideVideoList();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if(this.WindowState == System.Windows.WindowState.Normal)
            {
                this.grid_Function.Height = 120;
                this.WindowStyle = System.Windows.WindowStyle.SingleBorderWindow;
            }
            else if(this.WindowState == System.Windows.WindowState.Maximized)
            {
                this.grid_Function.Height = 0;
                ShowOrHideVideoList(true);
                this.WindowStyle = System.Windows.WindowStyle.None;                          
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.WindowState = System.Windows.WindowState.Normal;
                ShowOrHideVideoList();
            }

        }

        #endregion    
        
    }
}
