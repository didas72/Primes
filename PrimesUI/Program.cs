using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace PrimesUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        static Timer UIUpdateTimer = new Timer();


        [STAThread]
        static void Main()
        {
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            System.Windows.Forms.Application.Run(new PrimesUI());

            UIUpdateTimer.Interval = 200;
            UIUpdateTimer.Elapsed += PrimesProgram.OnUITimer;
            UIUpdateTimer.Start();
        }
    }
}
