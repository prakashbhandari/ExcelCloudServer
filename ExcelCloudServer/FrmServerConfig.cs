using System;
using System.Threading;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ExcelCloudServer
{
    public partial class FrmServerConfig : Form
    {
        private AsyncConnection server;
        private Thread asyncConnectionThread;

        public FrmServerConfig()
        {
            InitializeComponent();
        }

        private void serverStart_Click(object sender, EventArgs e)
        {
            int port = Convert.ToInt32(Regex.Match(this.servicePort.Value.ToString(), @"\d+").Value);

            this.server = new AsyncConnection(this.hostIP.Text, port);

            if(this.isFrmValid())
            {
                // Send status update message
                this.updateStatus(3);
                this.asyncConnectionThread = new Thread(new ThreadStart(this.server.StartListening));
                this.asyncConnectionThread.Start();
            }
        }

        private void serverStop_Click(object sender, EventArgs e)
        {
            // Close any active connection
            this.server.StopListening();
            // Stop listening to the port
            this.asyncConnectionThread.Abort();
            this.asyncConnectionThread.Join();
            this.updateStatus(4);
        }

        public bool isFrmValid()
        {
            IPAddress ipAddress;
            if (this.hostIP.Text == String.Empty)
            {
                updateStatus(0);
            }
            else if (!IPAddress.TryParse(this.hostIP.Text, out ipAddress))
            {
                updateStatus(1);
            }
            else if (!this.server.isPortAvailable())
            {
                updateStatus(2);
            }
            return true;
        }

        public void updateStatus(int status)
        {
            switch (status)
            {
                case 0:
                    this.lblNotification.ForeColor = System.Drawing.Color.Red;
                    this.lblNotification.Text = "Host or IP cannot be empty";
                    break;
                case 1:
                    this.lblNotification.ForeColor = System.Drawing.Color.Red;
                    this.lblNotification.Text = "Please enter correct IP Address";
                    break;
                case 2:
                    this.lblNotification.ForeColor = System.Drawing.Color.Red;
                    this.lblNotification.Text = "Cannot start server at the supplied host and port";
                    break;
                case 3:
                    this.serverStop.Enabled = true;
                    this.serverStart.Enabled = false;
                    this.lblNotification.ForeColor = System.Drawing.Color.Green;
                    this.lblNotification.Text = "Excel Cloud Server started at - " + hostIP.Text + ":" + this.servicePort.Value;
                    break;
                case 4:
                    this.serverStart.Enabled = true;
                    this.serverStop.Enabled = false;
                    this.lblNotification.ForeColor = System.Drawing.Color.Green;
                    this.lblNotification.Text = "Excel Cloud Server stopped";
                    break;
            }
        }
    }
}
