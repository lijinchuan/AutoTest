using AutoTest.Domain.Entity;
using LJC.FrameWorkV3.Data.EntityDataBase;
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
    public partial class AddTestSiteDlg : SubBaseDlg
    {
        private int _sourceId;
        public AddTestSiteDlg()
        {
            InitializeComponent();
        }

        public AddTestSiteDlg(int sourceId)
        {
            InitializeComponent();

            this._sourceId = sourceId;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            BigEntityTableEngine.LocalEngine.Insert(nameof(TestSite), new TestSite
            {
               Name=TBName.Text,
               CheckLoginCode=TBCode.Text,
               SourceId=_sourceId
            });

            DialogResult = DialogResult.OK;
        }
    }
}
