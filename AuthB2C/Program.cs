using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AuthB2C
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Stream myFile = File.Create(@"C:\TestFile.txt");

            TextWriterTraceListener myTextListener = new
            TextWriterTraceListener(myFile);
            Trace.Listeners.Add(myTextListener);
            Trace.AutoFlush = true;

            Trace.Write("Application Started");
            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
