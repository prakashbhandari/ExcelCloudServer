using System;
using System.Threading;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ExcelCloudServer
{
    public partial class Form1 : Form
    {
        private AsyncConnection server;
        private Thread asyncConnectionThread;

        public Form1()
        {
            InitializeComponent();
        }

        private void serverStart_Click(object sender, EventArgs e)
        {
            String host = hostIP.Text;
            int port = Convert.ToInt32(Regex.Match(this.servicePort.Value.ToString(), @"\d+").Value);
            IPAddress ipAddress;

            this.server = new AsyncConnection(host, port);

            if (host.Equals(""))
            {
                this.status.ForeColor = System.Drawing.Color.Red;
                this.status.Text = "Host or IP cannot be empty";
            }
            else if(!IPAddress.TryParse(host, out ipAddress))
            {
                this.status.ForeColor = System.Drawing.Color.Red;
                this.status.Text = "Please enter correct IP Address";
            }
            else if(!this.server.isPortAvailable())
            {
                this.status.ForeColor = System.Drawing.Color.Red;
                this.status.Text = "Cannot start server at the supplied host and port";
            }
            else
            {
                // Send status update message
                this.UpdateStatus(1);
                this.asyncConnectionThread = new Thread(new ThreadStart(this.server.StartListening));
                this.asyncConnectionThread.Start();
            }
        }

        public void UpdateStatus(int status)
        {
            switch(status)
            {
                // Server started
                case 1:
                    this.status.ForeColor = System.Drawing.Color.Green;
                    this.status.Text = "Excel Cloud Server started at - " + hostIP.Text + ":" + this.servicePort.Value;
                    this.serverStop.Enabled = true;
                    this.serverStart.Enabled = false;
                    break;
                // Server stopped
                case 2:
                    this.status.ForeColor = System.Drawing.Color.Green;
                    this.status.Text = "Excel Cloud Server stopped";
                    this.serverStart.Enabled = true;
                    this.serverStop.Enabled = false;
                    break;
            }
        }

        private void serverStop_Click(object sender, EventArgs e)
        {
            // Close any active connection
            this.server.StopListening();
            // Stop listening to the port
            this.asyncConnectionThread.Abort();
            this.asyncConnectionThread.Join();
            UpdateStatus(2);
        }
    }
}
