using System;
using System.Collections.Generic;

using Raylib_cs;

using Primes.UI.Render;

namespace Primes.UI.Windows
{
    internal abstract class BaseWindow
    {
        #region Palette
        public readonly static Color Background = new(51, 51, 51, 255), Mid = new(77, 77, 77, 255), Foreground = new(102, 102, 102, 255), Text = new(0, 0, 0, 255), Highlights = new(0, 206, 255, 255);
        #endregion


        public Holder Window { get; protected set; }
    }
}
