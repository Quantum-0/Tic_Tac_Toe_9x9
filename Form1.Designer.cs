namespace TTTM
{
    partial class FormMainMenu
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonSingle = new System.Windows.Forms.Button();
            this.buttonMulti = new System.Windows.Forms.Button();
            this.buttonExit = new System.Windows.Forms.Button();
            this.buttonSettings = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonSingle
            // 
            this.buttonSingle.Location = new System.Drawing.Point(183, 135);
            this.buttonSingle.Name = "buttonSingle";
            this.buttonSingle.Size = new System.Drawing.Size(89, 23);
            this.buttonSingle.TabIndex = 0;
            this.buttonSingle.Text = "Синглплеер";
            this.buttonSingle.UseVisualStyleBackColor = true;
            this.buttonSingle.Click += new System.EventHandler(this.buttonSingle_Click);
            // 
            // buttonMulti
            // 
            this.buttonMulti.Location = new System.Drawing.Point(183, 165);
            this.buttonMulti.Name = "buttonMulti";
            this.buttonMulti.Size = new System.Drawing.Size(89, 23);
            this.buttonMulti.TabIndex = 1;
            this.buttonMulti.Text = "Мультиплеер";
            this.buttonMulti.UseVisualStyleBackColor = true;
            this.buttonMulti.Click += new System.EventHandler(this.buttonMulti_Click);
            // 
            // buttonExit
            // 
            this.buttonExit.Location = new System.Drawing.Point(183, 223);
            this.buttonExit.Name = "buttonExit";
            this.buttonExit.Size = new System.Drawing.Size(89, 23);
            this.buttonExit.TabIndex = 2;
            this.buttonExit.Text = "Выход";
            this.buttonExit.UseVisualStyleBackColor = true;
            this.buttonExit.Click += new System.EventHandler(this.buttonExit_Click);
            // 
            // buttonSettings
            // 
            this.buttonSettings.Location = new System.Drawing.Point(183, 194);
            this.buttonSettings.Name = "buttonSettings";
            this.buttonSettings.Size = new System.Drawing.Size(89, 23);
            this.buttonSettings.TabIndex = 3;
            this.buttonSettings.Text = "Настройки";
            this.buttonSettings.UseVisualStyleBackColor = true;
            this.buttonSettings.Click += new System.EventHandler(this.buttonSettings_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(260, 237);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // FormMainMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.buttonSettings);
            this.Controls.Add(this.buttonExit);
            this.Controls.Add(this.buttonMulti);
            this.Controls.Add(this.buttonSingle);
            this.Controls.Add(this.pictureBox1);
            this.Name = "FormMainMenu";
            this.Text = "Менюшк";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMainMenu_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonSingle;
        private System.Windows.Forms.Button buttonMulti;
        private System.Windows.Forms.Button buttonExit;
        private System.Windows.Forms.Button buttonSettings;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}

