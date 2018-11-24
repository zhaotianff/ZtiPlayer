using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZtiPlayer.Models
{
    public class StartupArgs
    {
        public string Path { get; set; }

        public string[] PlayList { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public bool IsSilent { get; set; }

        public int Volume { get; set; }

        public int ArgsCount { get; set; }
    }
}
