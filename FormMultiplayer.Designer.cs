namespace TTTM
{
    partial class FormMultiplayer
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
            this.textBoxChatInput = new System.Windows.Forms.TextBox();
            this.textBoxChat = new System.Windows.Forms.TextBox();
            this.labelCurrentTurn = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.timerRefreshView = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxChatInput
            // 
            this.textBoxChatInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxChatInput.Location = new System.Drawing.Point(337, 309);
            this.textBoxChatInput.Name = "textBoxChatInput";
            this.textBoxChatInput.Size = new System.Drawing.Size(260, 20);
            this.textBoxChatInput.TabIndex = 0;
            this.textBoxChatInput.Enter += new System.EventHandler(this.textBoxChatInput_Enter);
            this.textBoxChatInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxChatInput_KeyPress);
            this.textBoxChatInput.Leave += new System.EventHandler(this.textBoxChatInput_Leave);
            // 
            // textBoxChat
            // 
            this.textBoxChat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxChat.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.textBoxChat.Location = new System.Drawing.Point(337, 31);
            this.textBoxChat.Multiline = true;
            this.textBoxChat.Name = "textBoxChat";
            this.textBoxChat.Size = new System.Drawing.Size(260, 272);
            this.textBoxChat.TabIndex = 1;
            this.textBoxChat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxChat_KeyPress);
            // 
            // labelCurrentTurn
            // 
            this.labelCurrentTurn.AutoSize = true;
            this.labelCurrentTurn.Location = new System.Drawing.Point(418, 12);
            this.labelCurrentTurn.Name = "labelCurrentTurn";
            this.labelCurrentTurn.Size = new System.Drawing.Size(10, 13);
            this.labelCurrentTurn.TabIndex = 2;
            this.labelCurrentTurn.Text = "-";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(334, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(78, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Текущий ход: ";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(317, 317);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            // 
            // timerRefreshView
            // 
            this.timerRefreshView.Interval = 50;
            this.timerRefreshView.Tick += new System.EventHandler(this.timerRefreshView_Tick);
            // 
            // FormMultiplayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 341);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelCurrentTurn);
            this.Controls.Add(this.textBoxChat);
            this.Controls.Add(this.textBoxChatInput);
            this.Name = "FormMultiplayer";
            this.Text = "Мультиплеерная игра";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMultiplayer_FormClosing);
            this.ResizeEnd += new System.EventHandler(this.FormMultiplayer_ResizeEnd_And_SizeChanged);
            this.SizeChanged += new System.EventHandler(this.FormMultiplayer_ResizeEnd_And_SizeChanged);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormMultiplayer_Paint);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxChatInput;
        private System.Windows.Forms.TextBox textBoxChat;
        private System.Windows.Forms.Label labelCurrentTurn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timerRefreshView;
    }
}