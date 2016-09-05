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
            this.textBoxChatInput = new System.Windows.Forms.TextBox();
            this.textBoxChat = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBoxChatInput
            // 
            this.textBoxChatInput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxChatInput.Location = new System.Drawing.Point(337, 248);
            this.textBoxChatInput.Name = "textBoxChatInput";
            this.textBoxChatInput.Size = new System.Drawing.Size(260, 20);
            this.textBoxChatInput.TabIndex = 0;
            this.textBoxChatInput.TextChanged += new System.EventHandler(this.textBoxChatInput_TextChanged);
            this.textBoxChatInput.Enter += new System.EventHandler(this.textBoxChatInput_Enter);
            this.textBoxChatInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxChatInput_KeyPress);
            this.textBoxChatInput.Leave += new System.EventHandler(this.textBoxChatInput_Leave);
            // 
            // textBoxChat
            // 
            this.textBoxChat.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxChat.Location = new System.Drawing.Point(337, 12);
            this.textBoxChat.Multiline = true;
            this.textBoxChat.Name = "textBoxChat";
            this.textBoxChat.Size = new System.Drawing.Size(260, 230);
            this.textBoxChat.TabIndex = 1;
            this.textBoxChat.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxChat_KeyPress);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(101, 128);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "А где-то тут будет игра";
            // 
            // FormMultiplayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(609, 280);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxChat);
            this.Controls.Add(this.textBoxChatInput);
            this.Name = "FormMultiplayer";
            this.Text = "Мультиплеерная игра";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMultiplayer_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxChatInput;
        private System.Windows.Forms.TextBox textBoxChat;
        private System.Windows.Forms.Label label1;
    }
}