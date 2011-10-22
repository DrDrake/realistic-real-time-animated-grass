using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Gras.src
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var sample = new GrasSample())
                sample.Run();
        }
    }
}
