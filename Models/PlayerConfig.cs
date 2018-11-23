using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZtiPlayer.Models
{
    enum PlayerConfig
    {
        /// <summary>
        /// Silent
        /// </summary>
        SoundSilent = 12,

        LogoSettings = 36,

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
