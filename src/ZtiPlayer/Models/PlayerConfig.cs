using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZtiPlayer.Models
{
    enum PlayerConfig
    {
        EngineLibVersion = 1,

        /// <summary>
        /// Silent
        /// </summary>
        SoundSilent = 12,

        LogoSettings = 36,


        /// <summary>
        /// play speed，100-normal，>100 fast，<100 slow
        /// </summary>
        PlaySpeed = 104,

        /// <summary>
        /// Aspect ratio(4:3)
        /// </summary>
        AspectRatioCustom = 204,

        /// <summary>
        /// Video black border
        /// </summary>
        ClipBlackbandEnable = 207,

        /// <summary>
        /// horrizontal flip 1-flip , 0-do not flip
        /// </summary>
        ImageFlip_H = 302,

        /// <summary>
        /// vertical flip 1-flip, 0-do not flip
        /// </summary>
        ImageFlip_V = 303,

        /// <summary>
        /// rotate (0-360)
        /// </summary>
        ImageRotate = 304,

        /// <summary>
        /// Picture alpha(0-255)
        /// </summary>
        PictureAlpha = 608,

        /// <summary>
        /// Screenshot
        /// </summary>
        SnapshotImage = 702,

        /// <summary>
        /// Screenshot image width
        /// </summary>
        SnapshotWidth = 703,

        /// <summary>
        /// Screenshot image height
        /// </summary>
        SnapshotHeight = 704,

        /// <summary>
        /// Screenshot image format(1-bmp, 2-jpg, 3-png, 4-gif, default 1)
        /// </summary>
        SnapshotFormat = 707,
    }
}
