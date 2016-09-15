using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TTTM
{
    public partial class FormAbout : Form
    {
        List<Bitmap> HelpPictures = new List<Bitmap>();
        int CurrentPicture = 0;

        public FormAbout()
        {
            InitializeComponent();
            HelpPictures.Add(Properties.Resources.help1);
            HelpPictures.Add(Properties.Resources.help2);
            HelpPictures.Add(Properties.Resources.help3);
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            CurrentPicture++;
            if (CurrentPicture == HelpPictures.Count)
                CurrentPicture = 0;

            pictureBox1.Image = HelpPictures[CurrentPicture];
        }
    }
}
