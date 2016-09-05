namespace TTTM
{
    partial class FormSingle
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.labelCurrentTurn = new System.Windows.Forms.Label();
            this.buttonSettings = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.buttonNewGame = new System.Windows.Forms.Button();
            this.buttonLoadGame = new System.Windows.Forms.Button();
            this.buttonSaveGame = new System.Windows.Forms.Button();
            this.timerBotTurn = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Текущий ход: ";
            // 
            // labelCurrentTurn
            // 
            this.labelCurrentTurn.AutoSize = true;
            this.labelCurrentTurn.Location = new System.Drawing.Point(93, 9);
            this.labelCurrentTurn.Name = "labelCurrentTurn";
            this.labelCurrentTurn.Size = new System.Drawing.Size(10, 13);
            this.labelCurrentTurn.TabIndex = 2;
            this.labelCurrentTurn.Text = "-";
            // 
            // buttonSettings
            // 
            this.buttonSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSettings.Location = new System.Drawing.Point(255, 344);
            this.buttonSettings.Name = "buttonSettings";
            this.buttonSettings.Size = new System.Drawing.Size(75, 23);
            this.buttonSettings.TabIndex = 0;
            this.buttonSettings.Text = "Настройки";
            this.buttonSettings.UseVisualStyleBackColor = true;
            this.buttonSettings.Click += new System.EventHandler(this.buttonSettings_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(12, 34);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(317, 294);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            // 
            // buttonNewGame
            // 
            this.buttonNewGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonNewGame.Location = new System.Drawing.Point(12, 344);
            this.buttonNewGame.Name = "buttonNewGame";
            this.buttonNewGame.Size = new System.Drawing.Size(75, 23);
            this.buttonNewGame.TabIndex = 4;
            this.buttonNewGame.Text = "Новая игра";
            this.buttonNewGame.UseVisualStyleBackColor = true;
            this.buttonNewGame.Click += new System.EventHandler(this.buttonNewGame_Click);
            // 
            // buttonLoadGame
            // 
            this.buttonLoadGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonLoadGame.Location = new System.Drawing.Point(93, 344);
            this.buttonLoadGame.Name = "buttonLoadGame";
            this.buttonLoadGame.Size = new System.Drawing.Size(75, 23);
            this.buttonLoadGame.TabIndex = 5;
            this.buttonLoadGame.Text = "Загрузить";
            this.buttonLoadGame.UseVisualStyleBackColor = true;
            this.buttonLoadGame.Click += new System.EventHandler(this.buttonLoadGame_Click);
            // 
            // buttonSaveGame
            // 
            this.buttonSaveGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.buttonSaveGame.Enabled = false;
            this.buttonSaveGame.Location = new System.Drawing.Point(174, 344);
            this.buttonSaveGame.Name = "buttonSaveGame";
            this.buttonSaveGame.Size = new System.Drawing.Size(75, 23);
            this.buttonSaveGame.TabIndex = 6;
            this.buttonSaveGame.Text = "Сохранить";
            this.buttonSaveGame.UseVisualStyleBackColor = true;
            this.buttonSaveGame.Click += new System.EventHandler(this.buttonSaveGame_Click);
            // 
            // timerBotTurn
            // 
            this.timerBotTurn.Interval = 500;
            this.timerBotTurn.Tick += new System.EventHandler(this.timerBotTurn_Tick);
            // 
            // FormSingle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(341, 379);
            this.Controls.Add(this.buttonSaveGame);
            this.Controls.Add(this.buttonLoadGame);
            this.Controls.Add(this.buttonNewGame);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonSettings);
            this.Controls.Add(this.labelCurrentTurn);
            this.Controls.Add(this.label1);
            this.MinimumSize = new System.Drawing.Size(357, 418);
            this.Name = "FormSingle";
            this.Text = "Одиночная игра";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormSingle_FormClosing);
            this.ResizeEnd += new System.EventHandler(this.FormSingle_ResizeEnd_And_SizeChanged);
            this.SizeChanged += new System.EventHandler(this.FormSingle_ResizeEnd_And_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormSingle_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelCurrentTurn;
        private System.Windows.Forms.Button buttonSettings;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button buttonNewGame;
        private System.Windows.Forms.Button buttonLoadGame;
        private System.Windows.Forms.Button buttonSaveGame;
        private System.Windows.Forms.Timer timerBotTurn;
    }
}