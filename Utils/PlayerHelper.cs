﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZtiPlayer.Utils
{
    public class PlayerHelper
    {
        /// <summary>
        /// TODO add full format
        /// 
        /// [".csf"] = "",
        /// [".ac3"] = "",
        /// [".gif"] = "",
        /// [".jpg"] = "",
        /// [".png"] = "",
        /// [".bmp"] = "",
        /// </summary>
        private static readonly Dictionary<string, string> VideoExtensions = new Dictionary<string, string>()
        {         
            [".mp4"] = "",
            [".avi"] = "",
            [".flv"] = "",
            [".mpg"] = "",
            [".mpeg"] = "",
            [".rmvb"] = ""
        };


        public static string GetVideoName(string path, int videoType)
        {
            path = path.ToLower();
            if (videoType == 0)
            {
                return System.IO.Path.GetFileName(path);
            }
            else if (videoType == 1)
            {
                path = path.Substring(path.LastIndexOf("/") + 1);
                return System.Web.HttpUtility.UrlDecode(path);
            }
            else
            {
                return "";
            }
        }

        public static bool IsVideoFormat(string extension)
        {
            return VideoExtensions.Keys.Contains(extension);
        }

        public static string GetTimeString(double millionSeconds)
        {
            int seconds = 0, minutes = 0, hours = 0;
            TimeSpan ts = TimeSpan.FromMilliseconds(millionSeconds);
            seconds = ts.Seconds;
            minutes = ts.Minutes;
            hours = ts.Hours;
            return hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");
        }
    }
}
