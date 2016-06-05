//Title        :  FrmServerConfig.cs
//Package      :  ExcelCloudServer
//Project      :  ExcelCloud
//Description  :  Excel Cloud Server Server Configuration Form
//Created on   :  June 5, 2016
//Author	   :  Prakash Bhandari

using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ExcelCloudServer
{
    /// <summary>
    /// Partial Class FrmServerConfig: Displays a user control
    /// form where user can set the server ip and port number
    /// to listen for job
    /// </summary>
    public partial class FrmServerConfig : Form
    {
        /// <summary>
        /// Connection object for server
        /// </summary>
        private AsyncConnection server;

        public FrmServerConfig()
        {
            InitializeComponent();
        }

        /// <summary>
        /// On click of start listening check if form is valid,
        /// if it is valid load the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Server stops listening and status updated to not listening
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serverStop_Click(object sender, EventArgs e)
        {
            // Close any active connection
            this.server.StopListening();
            this.SetStatus(4);
            Program.CloseServer();
        }

        /// <summary>
        /// Validation for server details
        /// </summary>
        /// <returns>Bool: True if form passes validation else false</returns>
        private bool IsFrmValid()
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

        /// <summary>
        /// Set the notification message based on the status code
        /// </summary>
        /// <param name="status">Status code: 0 - 4</param>
        private void SetStatus(int status)
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
                    this.lblNotification.ForeColor = System.Drawing.Color.Red;
                    this.lblNotification.Text = "Excel Cloud Server stopped";
                    break;
            }
        }
    }
}
