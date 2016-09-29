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
            this.components = new System.ComponentModel.Container();
            this.timerOpacity = new System.Windows.Forms.Timer(this.components);
            this.timerClosing = new System.Windows.Forms.Timer(this.components);
            this.elementHost = new System.Windows.Forms.Integration.ElementHost();
            this.mainMenuControl = new TTTM.MainMenuControl();
            this.SuspendLayout();
            // 
            // timerOpacity
            // 
            this.timerOpacity.Interval = 20;
            this.timerOpacity.Tick += new System.EventHandler(this.timerOpacity_Tick);
            // 
            // timerClosing
            // 
            this.timerClosing.Interval = 20;
            this.timerClosing.Tick += new System.EventHandler(this.timerClosing_Tick);
            // 
            // elementHost
            // 
            this.elementHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost.Location = new System.Drawing.Point(0, 0);
            this.elementHost.Name = "elementHost";
            this.elementHost.Size = new System.Drawing.Size(600, 347);
            this.elementHost.TabIndex = 6;
            this.elementHost.Text = "elementHost";
            this.elementHost.Child = this.mainMenuControl;
            // 
            // FormMainMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 347);
            this.Controls.Add(this.elementHost);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMainMenu";
            this.Opacity = 0D;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Менюшк";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMainMenu_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Integration.ElementHost elementHost;
        private MainMenuControl mainMenuControl;
        private System.Windows.Forms.Timer timerOpacity;
        private System.Windows.Forms.Timer timerClosing;
    }
}

