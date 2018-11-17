using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZtiPlayer.Models
{
    class VideoItem
    {
        public string Name { get; set; }

        public string Path { get; set; }

        /// <summary>
        /// 0-Local 1-Web 2-Other
        /// </summary>
        public int Type { get; set; }

        public TimeSpan Durationtime { get; set; }


    }
}
