using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZtiPlayer.Utils
{
    enum PLAY_STATE
    {
        PS_READY = 0,  // 准备就绪
        PS_OPENING = 1,  // 正在打开
        PS_PAUSING = 2,  // 正在暂停
        PS_PAUSED = 3,  // 暂停中
        PS_PLAYING = 4,  // 正在开始播放
        PS_PLAY = 5,  // 播放中
        PS_CLOSING = 6,  // 正在开始关闭
    };
}
