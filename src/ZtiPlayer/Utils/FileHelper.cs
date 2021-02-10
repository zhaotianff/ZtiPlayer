using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZtiPlayer.Utils
{
    public class FileHelper
    {
        public static List<string> GetAllFiles(string dir)
        {
            var list = new List<string>();
            var dirInfo = new System.IO.DirectoryInfo(dir);
            var entries = dirInfo.GetFileSystemInfos();

            foreach (var item in entries)
            {
                if(item.Attributes == System.IO.FileAttributes.Directory)
                {
                    list.AddRange(GetAllFiles(item.FullName));
                }
                else
                {
                    var extension = System.IO.Path.GetExtension(item.FullName).ToLower();
                    if (PlayerHelper.IsVideoFormat(extension) == true)
                    {
                        list.Add(item.FullName);
                    }
                }
            }

            return list;
        }

        public static string [] GetFiles(string dir)
        {
            return System.IO.Directory.GetFiles(dir);
        }
    }
}
