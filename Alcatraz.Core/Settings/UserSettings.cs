using System;
using System.Drawing;

namespace Alcatraz.Core.Settings
{
    [Serializable]
    public sealed class UserSettings
    {
        internal static readonly Color DefaultTraceLevelColor = Color.Gray;
        internal static readonly Color DefaultDebugLevelColor = Color.Black;
        internal static readonly Color DefaultInfoLevelColor = Color.Green;
        internal static readonly Color DefaultWarnLevelColor = Color.Orange;
        internal static readonly Color DefaultErrorLevelColor = Color.Red;
        internal static readonly Color DefaultFatalLevelColor = Color.Purple;
    }
}