using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Raylib_cs;

using DidasUtils.Numerics;
using DidasUtils.Logging;

using Primes.UI.Render;

namespace Primes.UI.Windows
{
    /// <summary>
    /// Temporary, only for testing purposes
    /// </summary>
    internal class SettingsWindow : BaseWindow
    {
        public SettingsWindow()
        {
            Window = new(Vector2i.Zero, "Settings") { Id_Name = "SETTINGS" };

            Window.Add(new FileSelector(Vector2i.Zero, new(400, 250), Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), new string[] { "*.*", "*.primejob", "TumaComisola.*" }, null));
        }
    }
}
