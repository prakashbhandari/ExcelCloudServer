namespace ExcelCloudServer
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.hostIP = new System.Windows.Forms.TextBox();
            this.serverStart = new System.Windows.Forms.Button();
            this.status = new System.Windows.Forms.Label();
            this.servicePort = new System.Windows.Forms.NumericUpDown();
            this.serverStop = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.servicePort)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 88);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Host or IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 144);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Service Port";
            // 
            // hostIP
            // 
            this.hostIP.Location = new System.Drawing.Point(100, 81);
            this.hostIP.Name = "hostIP";
            this.hostIP.Size = new System.Drawing.Size(121, 20);
            this.hostIP.TabIndex = 2;
            // 
            // serverStart
            // 
            this.serverStart.Location = new System.Drawing.Point(44, 191);
            this.serverStart.Name = "serverStart";
            this.serverStart.Size = new System.Drawing.Size(75, 23);
            this.serverStart.TabIndex = 4;
            this.serverStart.Text = "Start";
            this.serverStart.UseVisualStyleBackColor = true;
            this.serverStart.Click += new System.EventHandler(this.serverStart_Click);
            // 
            // status
            // 
            this.status.AutoSize = true;
            this.status.Location = new System.Drawing.Point(12, 39);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(134, 13);
            this.status.TabIndex = 6;
            this.status.Text = "Please enter Host and Port";
            // 
            // servicePort
            // 
            this.servicePort.Location = new System.Drawing.Point(100, 136);
            this.servicePort.Maximum = new decimal(new int[] {
            65536,
            0,
            0,
            0});
            this.servicePort.Minimum = new decimal(new int[] {
            1025,
            0,
            0,
            0});
            this.servicePort.Name = "servicePort";
            this.servicePort.Size = new System.Drawing.Size(120, 20);
            this.servicePort.TabIndex = 7;
            this.servicePort.Value = new decimal(new int[] {
            9990,
            0,
            0,
            0});
            // 
            // serverStop
            // 
            this.serverStop.Enabled = false;
            this.serverStop.Location = new System.Drawing.Point(145, 190);
            this.serverStop.Name = "serverStop";
            this.serverStop.Size = new System.Drawing.Size(75, 23);
            this.serverStop.TabIndex = 8;
            this.serverStop.Text = "Stop";
            this.serverStop.UseVisualStyleBackColor = true;
            this.serverStop.Click += new System.EventHandler(this.serverStop_Click);
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.serverStop);
            this.Controls.Add(this.servicePort);
            this.Controls.Add(this.status);
            this.Controls.Add(this.serverStart);
            this.Controls.Add(this.hostIP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Form1";
            this.Text = "Excel Cloud Server";
            ((System.ComponentModel.ISupportInitialize)(this.servicePort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox hostIP;
        private System.Windows.Forms.Button serverStart;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.NumericUpDown servicePort;
        private System.Windows.Forms.Button serverStop;
    }
}

