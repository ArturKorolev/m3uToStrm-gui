using System;
using System.Windows.Forms;

namespace m3uToStrm.Gui
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
