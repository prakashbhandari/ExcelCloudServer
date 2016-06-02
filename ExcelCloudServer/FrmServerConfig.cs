using System;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ExcelCloudServer
{
    public partial class FrmServerConfig : Form
    {
        private AsyncConnection server;

        public FrmServerConfig()
        {
            InitializeComponent();
        }

        private void serverStart_Click(object sender, EventArgs e)
        {
            int port = Convert.ToInt32(Regex.Match(this.servicePort.Value.ToString(), @"\d+").Value);

            this.server = new AsyncConnection(this.hostIP.Text, port);

            if (this.IsFrmValid())
            {
                // Set status update message to server started
                this.SetStatus(3);
                Program.LoadServer(this.server);
            }
        }

        private void serverStop_Click(object sender, EventArgs e)
        {
            // Close any active connection
            this.server.StopListening();
            this.SetStatus(4);
            Program.CloseServer();
        }

        public bool IsFrmValid()
        {
            IPAddress ipAddress;
            if (this.hostIP.Text == String.Empty)
            {
                SetStatus(0);
                return false;
            }
            else if (!IPAddress.TryParse(this.hostIP.Text, out ipAddress))
            {
                SetStatus(1);
                return false;
            }
            else if (!this.server.IsPortAvailable())
            {
                SetStatus(2);
                return false;
            }
            return true;
        }

        public void SetStatus(int status)
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
