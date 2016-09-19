namespace TTTM
{
    partial class StartMultiplayerGame
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
            this.radioButtonServer = new System.Windows.Forms.RadioButton();
            this.radioButtonClient = new System.Windows.Forms.RadioButton();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxNick = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.labelConnectedPlayer = new System.Windows.Forms.Label();
            this.labelConnectedPlayerNick = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButtonServer
            // 
            this.radioButtonServer.AutoSize = true;
            this.radioButtonServer.Location = new System.Drawing.Point(12, 12);
            this.radioButtonServer.Name = "radioButtonServer";
            this.radioButtonServer.Size = new System.Drawing.Size(106, 17);
            this.radioButtonServer.TabIndex = 1;
            this.radioButtonServer.TabStop = true;
            this.radioButtonServer.Text = "Создать сервер";
            this.radioButtonServer.UseVisualStyleBackColor = true;
            this.radioButtonServer.CheckedChanged += new System.EventHandler(this.radioButtonServer_CheckedChanged);
            // 
            // radioButtonClient
            // 
            this.radioButtonClient.AutoSize = true;
            this.radioButtonClient.Location = new System.Drawing.Point(12, 35);
            this.radioButtonClient.Name = "radioButtonClient";
            this.radioButtonClient.Size = new System.Drawing.Size(146, 17);
            this.radioButtonClient.TabIndex = 2;
            this.radioButtonClient.TabStop = true;
            this.radioButtonClient.Text = "Подключится к серверу";
            this.radioButtonClient.UseVisualStyleBackColor = true;
            this.radioButtonClient.CheckedChanged += new System.EventHandler(this.radioButtonClient_CheckedChanged);
            // 
            // textBoxIP
            // 
            this.textBoxIP.Location = new System.Drawing.Point(201, 11);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.Size = new System.Drawing.Size(100, 20);
            this.textBoxIP.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(175, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(20, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "IP:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(175, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Порт:";
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(216, 37);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(85, 20);
            this.textBoxPort.TabIndex = 5;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel});
            this.statusStrip1.Location = new System.Drawing.Point(0, 239);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(311, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel
            // 
            this.toolStripStatusLabel.Name = "toolStripStatusLabel";
            this.toolStripStatusLabel.Size = new System.Drawing.Size(106, 17);
            this.toolStripStatusLabel.Text = "Нет подключения";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 77);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Ник:";
            // 
            // textBoxNick
            // 
            this.textBoxNick.Location = new System.Drawing.Point(48, 74);
            this.textBoxNick.Name = "textBoxNick";
            this.textBoxNick.Size = new System.Drawing.Size(100, 20);
            this.textBoxNick.TabIndex = 9;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Black;
            this.panel1.Location = new System.Drawing.Point(159, 74);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(22, 20);
            this.panel1.TabIndex = 10;
            this.panel1.Click += new System.EventHandler(this.panel1_Click);
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(201, 203);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(100, 23);
            this.buttonStart.TabIndex = 11;
            this.buttonStart.Text = "Создать сервер";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(92, 203);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(103, 23);
            this.buttonCancel.TabIndex = 12;
            this.buttonCancel.Text = "Отключит сервер";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Visible = false;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // labelConnectedPlayer
            // 
            this.labelConnectedPlayer.AutoSize = true;
            this.labelConnectedPlayer.Location = new System.Drawing.Point(15, 113);
            this.labelConnectedPlayer.Name = "labelConnectedPlayer";
            this.labelConnectedPlayer.Size = new System.Drawing.Size(59, 13);
            this.labelConnectedPlayer.TabIndex = 13;
            this.labelConnectedPlayer.Text = "Соперник:";
            this.labelConnectedPlayer.Visible = false;
            // 
            // labelConnectedPlayerNick
            // 
            this.labelConnectedPlayerNick.AutoSize = true;
            this.labelConnectedPlayerNick.Location = new System.Drawing.Point(48, 133);
            this.labelConnectedPlayerNick.Name = "labelConnectedPlayerNick";
            this.labelConnectedPlayerNick.Size = new System.Drawing.Size(78, 13);
            this.labelConnectedPlayerNick.TabIndex = 14;
            this.labelConnectedPlayerNick.Text = "Тут будет Ник";
            this.labelConnectedPlayerNick.Visible = false;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(20, 130);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(22, 20);
            this.panel2.TabIndex = 15;
            this.panel2.Visible = false;
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(187, 63);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(114, 21);
            this.comboBox1.TabIndex = 16;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // StartMultiplayerGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(311, 261);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.labelConnectedPlayerNick);
            this.Controls.Add(this.labelConnectedPlayer);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.textBoxNick);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBoxPort);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxIP);
            this.Controls.Add(this.radioButtonClient);
            this.Controls.Add(this.radioButtonServer);
            this.Name = "StartMultiplayerGame";
            this.Text = "Мультиплеер";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StartMultiplayerGame_FormClosed);
            this.Load += new System.EventHandler(this.StartMultiplayerGame_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.RadioButton radioButtonServer;
        private System.Windows.Forms.RadioButton radioButtonClient;
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxNick;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label labelConnectedPlayer;
        private System.Windows.Forms.Label labelConnectedPlayerNick;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ComboBox comboBox1;
    }
}