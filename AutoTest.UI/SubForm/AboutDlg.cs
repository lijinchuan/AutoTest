using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoTest.UI.SubForm
{
    public partial class AboutDlg : SubBaseDlg
    {
        public AboutDlg()
        {
            InitializeComponent();

            PBWx.SizeMode = PictureBoxSizeMode.StretchImage;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.PBWx.Image = Resources.Resource1.wx;
        }
    }
}
