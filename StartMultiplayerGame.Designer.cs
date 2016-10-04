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
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageMainSettings = new System.Windows.Forms.TabPage();
            this.buttonOpenSettings = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panelPlayerColor = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxMyNick = new System.Windows.Forms.TextBox();
            this.tabPageServerList = new System.Windows.Forms.TabPage();
            this.buttonRefresh = new System.Windows.Forms.Button();
            this.buttonConnect = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.ColumnServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnPlayerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnColor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ColumnIP = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageStartServer = new System.Windows.Forms.TabPage();
            this.textBoxServerLog = new System.Windows.Forms.TextBox();
            this.buttonCloseServer = new System.Windows.Forms.Button();
            this.buttonStartServer = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxServerName = new System.Windows.Forms.TextBox();
            this.tabPageStartGame = new System.Windows.Forms.TabPage();
            this.panelPlayer2 = new System.Windows.Forms.Panel();
            this.labelPlayer2Nick = new System.Windows.Forms.Label();
            this.panelPlayer1 = new System.Windows.Forms.Panel();
            this.labelPlayer1Nick = new System.Windows.Forms.Label();
            this.labelServerName = new System.Windows.Forms.Label();
            this.buttonCancelGame = new System.Windows.Forms.Button();
            this.buttonStartGame = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.tabControl.SuspendLayout();
            this.tabPageMainSettings.SuspendLayout();
            this.tabPageServerList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.tabPageStartServer.SuspendLayout();
            this.tabPageStartGame.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl.Controls.Add(this.tabPageMainSettings);
            this.tabControl.Controls.Add(this.tabPageServerList);
            this.tabControl.Controls.Add(this.tabPageStartServer);
            this.tabControl.Controls.Add(this.tabPageStartGame);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.HotTrack = true;
            this.tabControl.Location = new System.Drawing.Point(0, 0);
            this.tabControl.Multiline = true;
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(423, 241);
            this.tabControl.TabIndex = 0;
            this.tabControl.TabStop = false;
            this.tabControl.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.tabControl_Selecting);
            // 
            // tabPageMainSettings
            // 
            this.tabPageMainSettings.Controls.Add(this.buttonOpenSettings);
            this.tabPageMainSettings.Controls.Add(this.label2);
            this.tabPageMainSettings.Controls.Add(this.panelPlayerColor);
            this.tabPageMainSettings.Controls.Add(this.label1);
            this.tabPageMainSettings.Controls.Add(this.textBoxMyNick);
            this.tabPageMainSettings.Location = new System.Drawing.Point(4, 25);
            this.tabPageMainSettings.Name = "tabPageMainSettings";
            this.tabPageMainSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageMainSettings.Size = new System.Drawing.Size(415, 212);
            this.tabPageMainSettings.TabIndex = 0;
            this.tabPageMainSettings.Text = "Общие настройки";
            this.tabPageMainSettings.UseVisualStyleBackColor = true;
            // 
            // buttonOpenSettings
            // 
            this.buttonOpenSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOpenSettings.Location = new System.Drawing.Point(8, 181);
            this.buttonOpenSettings.Name = "buttonOpenSettings";
            this.buttonOpenSettings.Size = new System.Drawing.Size(399, 23);
            this.buttonOpenSettings.TabIndex = 4;
            this.buttonOpenSettings.Text = "Открыть все настройки";
            this.buttonOpenSettings.UseVisualStyleBackColor = true;
            this.buttonOpenSettings.Click += new System.EventHandler(this.buttonOpenSettings_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Цвет:";
            // 
            // panelPlayerColor
            // 
            this.panelPlayerColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPlayerColor.Location = new System.Drawing.Point(21, 68);
            this.panelPlayerColor.Name = "panelPlayerColor";
            this.panelPlayerColor.Size = new System.Drawing.Size(107, 20);
            this.panelPlayerColor.TabIndex = 2;
            this.panelPlayerColor.Click += new System.EventHandler(this.panel1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(30, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Ник:";
            // 
            // textBoxMyNick
            // 
            this.textBoxMyNick.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMyNick.Location = new System.Drawing.Point(21, 29);
            this.textBoxMyNick.Name = "textBoxMyNick";
            this.textBoxMyNick.Size = new System.Drawing.Size(386, 20);
            this.textBoxMyNick.TabIndex = 0;
            this.textBoxMyNick.Text = "Пожалуйста, укажите Ваш ник";
            this.textBoxMyNick.Enter += new System.EventHandler(this.textBoxMyNick_Enter);
            this.textBoxMyNick.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxMyNick_KeyPress);
            this.textBoxMyNick.Leave += new System.EventHandler(this.textBoxMyNick_Leave);
            // 
            // tabPageServerList
            // 
            this.tabPageServerList.Controls.Add(this.buttonRefresh);
            this.tabPageServerList.Controls.Add(this.buttonConnect);
            this.tabPageServerList.Controls.Add(this.dataGridView1);
            this.tabPageServerList.Location = new System.Drawing.Point(4, 25);
            this.tabPageServerList.Name = "tabPageServerList";
            this.tabPageServerList.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageServerList.Size = new System.Drawing.Size(415, 212);
            this.tabPageServerList.TabIndex = 1;
            this.tabPageServerList.Text = "Список серверов";
            this.tabPageServerList.UseVisualStyleBackColor = true;
            // 
            // buttonRefresh
            // 
            this.buttonRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonRefresh.Location = new System.Drawing.Point(8, 186);
            this.buttonRefresh.Name = "buttonRefresh";
            this.buttonRefresh.Size = new System.Drawing.Size(187, 23);
            this.buttonRefresh.TabIndex = 2;
            this.buttonRefresh.Text = "Обновить";
            this.buttonRefresh.UseVisualStyleBackColor = true;
            this.buttonRefresh.Click += new System.EventHandler(this.buttonRefresh_Click);
            // 
            // buttonConnect
            // 
            this.buttonConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConnect.Enabled = false;
            this.buttonConnect.Location = new System.Drawing.Point(220, 186);
            this.buttonConnect.Name = "buttonConnect";
            this.buttonConnect.Size = new System.Drawing.Size(187, 23);
            this.buttonConnect.TabIndex = 1;
            this.buttonConnect.Text = "Подключиться";
            this.buttonConnect.UseVisualStyleBackColor = true;
            this.buttonConnect.Click += new System.EventHandler(this.buttonConnect_Click);
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.dataGridView1.CellBorderStyle = System.Windows.Forms.DataGridViewCellBorderStyle.Sunken;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ColumnServerName,
            this.ColumnPlayerName,
            this.ColumnColor,
            this.ColumnIP});
            this.dataGridView1.EnableHeadersVisualStyles = false;
            this.dataGridView1.Location = new System.Drawing.Point(3, 3);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.None;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(409, 181);
            this.dataGridView1.TabIndex = 0;
            // 
            // ColumnServerName
            // 
            this.ColumnServerName.HeaderText = "Название сервера";
            this.ColumnServerName.Name = "ColumnServerName";
            this.ColumnServerName.ReadOnly = true;
            this.ColumnServerName.Width = 127;
            // 
            // ColumnPlayerName
            // 
            this.ColumnPlayerName.HeaderText = "Имя игрока";
            this.ColumnPlayerName.Name = "ColumnPlayerName";
            this.ColumnPlayerName.ReadOnly = true;
            this.ColumnPlayerName.Width = 127;
            // 
            // ColumnColor
            // 
            this.ColumnColor.HeaderText = "Цвет";
            this.ColumnColor.Name = "ColumnColor";
            this.ColumnColor.ReadOnly = true;
            this.ColumnColor.Width = 38;
            // 
            // ColumnIP
            // 
            this.ColumnIP.HeaderText = "Адрес сервера";
            this.ColumnIP.Name = "ColumnIP";
            this.ColumnIP.ReadOnly = true;
            this.ColumnIP.Width = 127;
            // 
            // tabPageStartServer
            // 
            this.tabPageStartServer.Controls.Add(this.textBoxServerLog);
            this.tabPageStartServer.Controls.Add(this.buttonCloseServer);
            this.tabPageStartServer.Controls.Add(this.buttonStartServer);
            this.tabPageStartServer.Controls.Add(this.label3);
            this.tabPageStartServer.Controls.Add(this.textBoxServerName);
            this.tabPageStartServer.Location = new System.Drawing.Point(4, 25);
            this.tabPageStartServer.Name = "tabPageStartServer";
            this.tabPageStartServer.Size = new System.Drawing.Size(415, 212);
            this.tabPageStartServer.TabIndex = 2;
            this.tabPageStartServer.Text = "Создание сервера";
            this.tabPageStartServer.UseVisualStyleBackColor = true;
            // 
            // textBoxServerLog
            // 
            this.textBoxServerLog.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.textBoxServerLog.Location = new System.Drawing.Point(11, 53);
            this.textBoxServerLog.Multiline = true;
            this.textBoxServerLog.Name = "textBoxServerLog";
            this.textBoxServerLog.Size = new System.Drawing.Size(277, 151);
            this.textBoxServerLog.TabIndex = 10;
            this.textBoxServerLog.Visible = false;
            // 
            // buttonCloseServer
            // 
            this.buttonCloseServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCloseServer.Location = new System.Drawing.Point(294, 181);
            this.buttonCloseServer.Name = "buttonCloseServer";
            this.buttonCloseServer.Size = new System.Drawing.Size(113, 23);
            this.buttonCloseServer.TabIndex = 9;
            this.buttonCloseServer.Text = "Отключить сервер";
            this.buttonCloseServer.UseVisualStyleBackColor = true;
            this.buttonCloseServer.Visible = false;
            this.buttonCloseServer.Click += new System.EventHandler(this.buttonCloseServer_Click);
            // 
            // buttonStartServer
            // 
            this.buttonStartServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStartServer.Enabled = false;
            this.buttonStartServer.Location = new System.Drawing.Point(294, 53);
            this.buttonStartServer.Name = "buttonStartServer";
            this.buttonStartServer.Size = new System.Drawing.Size(113, 23);
            this.buttonStartServer.TabIndex = 8;
            this.buttonStartServer.Text = "Создать сервер";
            this.buttonStartServer.UseVisualStyleBackColor = true;
            this.buttonStartServer.Click += new System.EventHandler(this.buttonStartServer_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Название сервера:";
            // 
            // textBoxServerName
            // 
            this.textBoxServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxServerName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.textBoxServerName.Location = new System.Drawing.Point(21, 27);
            this.textBoxServerName.Name = "textBoxServerName";
            this.textBoxServerName.Size = new System.Drawing.Size(386, 20);
            this.textBoxServerName.TabIndex = 6;
            this.textBoxServerName.Text = "Укажите название сервера";
            this.textBoxServerName.TextChanged += new System.EventHandler(this.textBoxServerName_TextChanged);
            this.textBoxServerName.Enter += new System.EventHandler(this.textBoxServerName_Enter);
            this.textBoxServerName.Leave += new System.EventHandler(this.textBoxServerName_Leave);
            // 
            // tabPageStartGame
            // 
            this.tabPageStartGame.Controls.Add(this.panelPlayer2);
            this.tabPageStartGame.Controls.Add(this.labelPlayer2Nick);
            this.tabPageStartGame.Controls.Add(this.panelPlayer1);
            this.tabPageStartGame.Controls.Add(this.labelPlayer1Nick);
            this.tabPageStartGame.Controls.Add(this.labelServerName);
            this.tabPageStartGame.Controls.Add(this.buttonCancelGame);
            this.tabPageStartGame.Controls.Add(this.buttonStartGame);
            this.tabPageStartGame.Controls.Add(this.label6);
            this.tabPageStartGame.Controls.Add(this.label5);
            this.tabPageStartGame.Controls.Add(this.label4);
            this.tabPageStartGame.Location = new System.Drawing.Point(4, 25);
            this.tabPageStartGame.Name = "tabPageStartGame";
            this.tabPageStartGame.Size = new System.Drawing.Size(415, 212);
            this.tabPageStartGame.TabIndex = 3;
            this.tabPageStartGame.Text = "Подключение";
            this.tabPageStartGame.UseVisualStyleBackColor = true;
            // 
            // panelPlayer2
            // 
            this.panelPlayer2.Location = new System.Drawing.Point(23, 134);
            this.panelPlayer2.Name = "panelPlayer2";
            this.panelPlayer2.Size = new System.Drawing.Size(134, 13);
            this.panelPlayer2.TabIndex = 9;
            // 
            // labelPlayer2Nick
            // 
            this.labelPlayer2Nick.AutoSize = true;
            this.labelPlayer2Nick.Location = new System.Drawing.Point(20, 118);
            this.labelPlayer2Nick.Name = "labelPlayer2Nick";
            this.labelPlayer2Nick.Size = new System.Drawing.Size(16, 13);
            this.labelPlayer2Nick.TabIndex = 8;
            this.labelPlayer2Nick.Text = "...";
            // 
            // panelPlayer1
            // 
            this.panelPlayer1.Location = new System.Drawing.Point(23, 79);
            this.panelPlayer1.Name = "panelPlayer1";
            this.panelPlayer1.Size = new System.Drawing.Size(134, 13);
            this.panelPlayer1.TabIndex = 7;
            // 
            // labelPlayer1Nick
            // 
            this.labelPlayer1Nick.AutoSize = true;
            this.labelPlayer1Nick.Location = new System.Drawing.Point(20, 63);
            this.labelPlayer1Nick.Name = "labelPlayer1Nick";
            this.labelPlayer1Nick.Size = new System.Drawing.Size(16, 13);
            this.labelPlayer1Nick.TabIndex = 6;
            this.labelPlayer1Nick.Text = "...";
            // 
            // labelServerName
            // 
            this.labelServerName.AutoSize = true;
            this.labelServerName.Location = new System.Drawing.Point(20, 27);
            this.labelServerName.Name = "labelServerName";
            this.labelServerName.Size = new System.Drawing.Size(16, 13);
            this.labelServerName.TabIndex = 5;
            this.labelServerName.Text = "...";
            // 
            // buttonCancelGame
            // 
            this.buttonCancelGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonCancelGame.Location = new System.Drawing.Point(8, 181);
            this.buttonCancelGame.Name = "buttonCancelGame";
            this.buttonCancelGame.Size = new System.Drawing.Size(163, 23);
            this.buttonCancelGame.TabIndex = 4;
            this.buttonCancelGame.Text = "Отключить / Отключиться";
            this.buttonCancelGame.UseVisualStyleBackColor = true;
            // 
            // buttonStartGame
            // 
            this.buttonStartGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonStartGame.Location = new System.Drawing.Point(244, 181);
            this.buttonStartGame.Name = "buttonStartGame";
            this.buttonStartGame.Size = new System.Drawing.Size(163, 23);
            this.buttonStartGame.TabIndex = 3;
            this.buttonStartGame.Text = "Начать игру";
            this.buttonStartGame.UseVisualStyleBackColor = true;
            this.buttonStartGame.Click += new System.EventHandler(this.buttonStartGame_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 100);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Игрок 2:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 45);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(50, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Игрок 1:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(47, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Сервер:";
            // 
            // colorDialog1
            // 
            this.colorDialog1.AnyColor = true;
            this.colorDialog1.SolidColorOnly = true;
            // 
            // StartMultiplayerGame
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(423, 241);
            this.Controls.Add(this.tabControl);
            this.MinimumSize = new System.Drawing.Size(439, 280);
            this.Name = "StartMultiplayerGame";
            this.Text = "Мультиплеер";
            this.Load += new System.EventHandler(this.StartMultiplayerGame_Load);
            this.tabControl.ResumeLayout(false);
            this.tabPageMainSettings.ResumeLayout(false);
            this.tabPageMainSettings.PerformLayout();
            this.tabPageServerList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.tabPageStartServer.ResumeLayout(false);
            this.tabPageStartServer.PerformLayout();
            this.tabPageStartGame.ResumeLayout(false);
            this.tabPageStartGame.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageMainSettings;
        private System.Windows.Forms.TabPage tabPageServerList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panelPlayerColor;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxMyNick;
        private System.Windows.Forms.Button buttonRefresh;
        private System.Windows.Forms.Button buttonConnect;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.TabPage tabPageStartServer;
        private System.Windows.Forms.Button buttonStartServer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxServerName;
        private System.Windows.Forms.TabPage tabPageStartGame;
        private System.Windows.Forms.Button buttonOpenSettings;
        private System.Windows.Forms.Button buttonCancelGame;
        private System.Windows.Forms.Button buttonStartGame;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnServerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnPlayerName;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnColor;
        private System.Windows.Forms.DataGridViewTextBoxColumn ColumnIP;
        private System.Windows.Forms.Button buttonCloseServer;
        private System.Windows.Forms.Label labelServerName;
        private System.Windows.Forms.TextBox textBoxServerLog;
        private System.Windows.Forms.Panel panelPlayer2;
        private System.Windows.Forms.Label labelPlayer2Nick;
        private System.Windows.Forms.Panel panelPlayer1;
        private System.Windows.Forms.Label labelPlayer1Nick;
    }
}