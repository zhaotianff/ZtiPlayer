using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using ZtiPlayer.Models;

namespace ZtiPlayer.Utils
{
    public static class CollectionExtension
    {
        public static ObservableCollection<VideoItem> AddRange(this ObservableCollection<VideoItem> list,IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                VideoItem videoItem = new VideoItem();
                videoItem.Path = item;
                videoItem.Type = 0;
                videoItem.Name = PlayerHelper.GetVideoName(item, videoItem.Type);
                list.Add(videoItem);
            }
            return list;
        }

        public static ObservableCollection<VideoItem> Union(this ObservableCollection<VideoItem> list,IEnumerable<string> items)
        {
            foreach (var item in items)
            {
                var tempItem = list.Where(x => x.Path == item).FirstOrDefault();

                if (tempItem != null)
                    continue;

                VideoItem videoItem = new VideoItem();
                videoItem.Path = item;
                videoItem.Type = 0;
                videoItem.Name = PlayerHelper.GetVideoName(item, videoItem.Type);
                list.Add(videoItem);
            }
            return list;
        }
    }
}
