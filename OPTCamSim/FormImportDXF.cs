using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace OPTCAMSim
{
    public partial class FormImportDXF : Form
    {
        public List<netDxf.Vector2> Vertices = new List<netDxf.Vector2> { };
        public double m_HorScale = 0;
        public double m_VerScale = 0;
        public FormImportDXF()
        {
            InitializeComponent();
            nudBulgePrec.Value = (decimal)OPTCAMSim.Properties.Settings.Default.ToolRounding;
        }

        private void bnStart_Click(object sender, EventArgs e)
        {
            if(tbFileName.Text.Length==0 || !tbFileName.Text.EndsWith(".dxf",StringComparison.InvariantCultureIgnoreCase) )
            {
                MessageBox.Show("Provide relevant .dxf file name to start", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;            
            }
            string filename = tbFileName.Text;
            if(!File.Exists(filename))
            {
                MessageBox.Show("File not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int bulgePrecision = (int)nudBulgePrec.Value;
            m_HorScale = chkDoNotRescale.Checked ? 0: (double)nudHScale.Value;
            m_VerScale = chkDoNotRescale.Checked ? 0 : (double)nudVScale.Value;
            string output = "";
            Vertices = DXFLoader.GetDXFPolyVertices(filename, output, bulgePrecision);
            DXFLoader.Normalize(ref Vertices, m_HorScale, m_VerScale);
            
            this.Close();
        }

        private void bnFileOpen_Click(object sender, EventArgs e)
        {
            using
                (
                OpenFileDialog ofd = new OpenFileDialog()
                {
                    DefaultExt = "dxf",
                    Filter = "AUTOCAD DXF files (*.dxf)|*.dxf"
                }
                )
            {
                DialogResult result = ofd.ShowDialog();

                if (result == DialogResult.OK)
                {
                    tbFileName.Text = ofd.FileName;
                }
            }
        }

        private void chkDoNotRescale_CheckedChanged(object sender, EventArgs e)
        {
            nudHScale.Enabled =
            nudVScale.Enabled =
            lbHor.Enabled =
            lbVer.Enabled = (!chkDoNotRescale.Checked);            
        }
    }
}
