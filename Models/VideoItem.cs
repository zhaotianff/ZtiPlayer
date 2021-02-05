using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZtiPlayer.Models
{
    public class VideoItem : INotifyPropertyChanged
    {
        private string name;
        private string path;
        private int type;
        private TimeSpan duration;
        private string durationStr;

        public string Name { get => name; set { name = value; RaiseChange("Name"); } }
        public string Path { get => path; set { path = value; RaiseChange("Path"); } }
        public int Type { get => type; set { type = value; RaiseChange("Type"); } }
        public TimeSpan Duration { get => duration; set { duration = value; RaiseChange("Duration"); } }
        public string DurationStr { get => durationStr; set { durationStr = value; RaiseChange("DurationStr"); } }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaiseChange(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
