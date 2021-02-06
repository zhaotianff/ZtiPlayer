using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZtiPlayer.Models
{
    public enum PlayerRepeatMode
    {
        SingleRepeat = 0,
        PlayNext = 1,
        PlayRandom = 2
    }
    public class PlayerSetting
    {
        public PlayerRepeatMode RepeatMode { get; set; } = PlayerRepeatMode.PlayNext;
    }
}
