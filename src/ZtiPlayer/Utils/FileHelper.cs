using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZtiPlayer.Utils
{
    public class FileHelper
    {
        public static readonly string CodecDirName = "codecs";
        public static readonly string CodecDirPath = Environment.CurrentDirectory;      

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

        public static bool CreateDirectory(string dir)
        {
            try
            {
                if (System.IO.Directory.Exists(dir) == false)
                {
                    System.IO.Directory.CreateDirectory(dir);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
