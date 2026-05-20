using System;
using System.Windows.Forms;
using HemaLeagueManager.Forms;

namespace HemaLeagueManager
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