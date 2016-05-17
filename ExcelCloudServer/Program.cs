using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;

namespace ExcelCloudServer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 

        static Thread asyncConnectionThread;
        static AsyncConnection server;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmServerConfig());
        }

        public static void LoadServer(AsyncConnection connection)
        {
            server = connection;
            asyncConnectionThread = new Thread(new ThreadStart(server.StartListening));
            asyncConnectionThread.Start();
        }

        public static void CloseServer()
        {
            asyncConnectionThread.Join();
            Debug.WriteLine("Port is now released");
        }
    }
}
