using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.IO;

namespace OPTCAMSim
{
    public partial class FormTools : Form
    {
        //All existing tool objs
        List<Tool> Tools = new List<Tool> { };
        //current tool
        Tool currentTool;
        
        float max_Z = 10;
        string path = Application.StartupPath + "\\UDT\\";
        string selected = "";
        bool b_new_tool = false;
        /// <summary>
        /// Accessible tool names from OPTCAM
        /// </summary>
        List<string> acc_tools = new List<string> { };
        public FormTools()
        {
            InitializeComponent();
        }

        private void bnAddNewTool_Click(object sender, EventArgs e)
        {
            if(acc_tools.Count==0)
            {
                MessageBox.Show("All of OPTCAM tools are already defined.");
                return;
            }

            string new_tool_name = Prompt.ShowDialog("Select new tool name:", "New tool name", acc_tools );
            if (new_tool_name.Length > 0)
            {
                LbTools.Items.Add(new_tool_name);
                LbTools.SelectedIndex = LbTools.Items.Count - 1;
                int ID = 1;
                if (Tools != null)
                    ID = Tools.Max(x => x.ocID) + 1;
                currentTool = new Tool(ToolType.Custom, new Dictionary<string, double> { }) { ToolName = new_tool_name, ocID= ID,  tool_contour = new List<Net3dBool.Vector3> { } };
                RefreshParams(currentTool);
                pnlToolParams.Enabled = true;
                tbToolName.Enabled = false;
                cmbToolType.Enabled = true;
                pnlToolList.Enabled = false;                
                b_new_tool = true;
            }
            
        }

        private void bnEditTool_Click(object sender, EventArgs e)
        {
            if (currentTool != null && currentTool.ToolName != "")
            {
                pnlToolParams.Enabled = true;
                tbToolName.Enabled = false;
                cmbToolType.Enabled = false;
                pnlToolList.Enabled = false;
                if(currentTool.myType == ToolType.Custom)
                {
                    dvParams.RowHeadersVisible = true;
                }
            }
            else
            {
                MessageBox.Show("Select tool to edit.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bnCopy_Click(object sender, EventArgs e)
        {
            if (acc_tools.Count == 0)
            {
                MessageBox.Show("All of OPTCAM tools are already defined.");
                return;
            }
            if (currentTool != null && currentTool.ToolName != "")
            {
                DialogResult res = MessageBox.Show(@"Do you want to copy tool?",
                    "Copy tool " + currentTool.ToolName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    string new_tool_name = Prompt.ShowDialog("Select new tool name:","New tool name", acc_tools);
                    if (new_tool_name.Length > 0)
                    {
                        try
                        {
                            File.Copy(path + currentTool.ToolName + ".txt", path + new_tool_name + ".txt");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                        selected = new_tool_name;
                        ReloadAllTools();
                    }
                }
            }
        }

        private void bnDeleteTool_Click(object sender, EventArgs e)
        {
            if (currentTool != null && currentTool.ToolName != "")
            {
                DialogResult res = MessageBox.Show(@"Are you sure? 
The tool definition file will be deleted permanently.",
                    "Delete tool " + currentTool.ToolName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (res == DialogResult.Yes)
                {
                    File.Delete(path + currentTool.ToolName + ".txt");
                    selected = "";
                    ReloadAllTools();
                }                    
            }
        }

        private void bnSave_Click(object sender, EventArgs e)
        {
            ToolType tt = currentTool.myType;
            bool dok = true;
            if (tt != ToolType.Custom)
            {
                currentTool.ToolParams.Clear();
                foreach (DataGridViewRow r in dvParams.Rows)
                {
                    if (r.IsNewRow)
                        continue;
                    string key = r.Cells[Property.Index].Value.ToString().Split(' ')[0];
                    string value = r.Cells[Value.Index].Value.ToString();
                    float val = 0;
                    dok = dok && float.TryParse(value, out val) && val >= 0;
                    if (!dok)
                    {
                        r.ErrorText = "Wrong value! Should be positive and numeric.";
                        return;
                    }
                    else
                    {
                        r.ErrorText = "";
                    }
                    currentTool.ToolParams.Add(key, key != "A" ? val / 10f : val);
                }
            }
            else
            {
                currentTool.tool_contour.Clear();
                foreach (DataGridViewRow r in dvParams.Rows)
                {
                    if (r.Cells[Property.Index].Value is null)
                        continue;
                    float x_ = 0, y_ = 0;
                    string x = r.Cells[Property.Index].Value.ToString();
                    string y = r.Cells[Value.Index].Value.ToString();
                    
                    dok = dok && float.TryParse(x, out x_) && x_ >= 0;
                    dok = dok && float.TryParse(y, out y_) && y_ >= 0;
                    if (!dok)
                    {
                        r.ErrorText = "Wrong value! Should be positive and numeric.";
                        return;
                    }
                    else
                    {
                        r.ErrorText = "";
                    }
                    currentTool.tool_contour.Add(new Net3dBool.Vector3(x_, 0, y_));
                }
                if(currentTool.tool_contour.Count<2)
                {
                    MessageBox.Show("Number of points should be greater than 1.","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }
            using (StreamWriter sw = new StreamWriter(path +"T"+ currentTool.ocID + ".txt",false))
            {
                sw.WriteLine(currentTool.ToString());
            }
            if (b_new_tool)
                selected = currentTool.ToolName;
            ReloadAllTools();
            pnlToolParams.Enabled = false;
            pnlToolList.Enabled = true;
            dvParams.RowHeadersVisible = false;
            b_new_tool = false;
        }

        private void bnCancel_Click(object sender, EventArgs e)
        {
            pnlToolParams.Enabled = false;
            pnlToolList.Enabled = true;
            b_new_tool = false;
            dvParams.RowHeadersVisible = false;
            ReloadAllTools();
        }

        private void cmbToolType_SelectedIndexChanged(object sender, EventArgs e)
        {
            dvParams.Rows.Clear();
            
            if (cmbToolType.Text == "Flat")
            {
                dvParams.Rows.Add("D (tool diameter):", "6");
                dvParams.Rows.Add("L (Tool height):", 100);
            }
            else if (cmbToolType.Text == "V-shape" || cmbToolType.Text=="Drill")
            {
                dvParams.Rows.Add("D (tool diameter):", "6");
                dvParams.Rows.Add("A (Tool angle):", "60");
                dvParams.Rows.Add("L (Tool height):", 100);
            }
            else if (cmbToolType.Text == "Ball")
            {
                dvParams.Rows.Add("D (tool diameter):", "6");
                dvParams.Rows.Add("L (Tool height):", 100);
            }
            else if (cmbToolType.Text == "Bull")
            {
                dvParams.Rows.Add("D (tool diameter):", "6");
                dvParams.Rows.Add("R (Rounding radius):", "1");
                dvParams.Rows.Add("L (Tool height):", 100);
            }
            else if (cmbToolType.Text == "VBit1")
            {
                double D1 = 14; // head diam
                double S = 25.5;    // head to edge
                double S1 = 19;    //edge height
                double R = 3;     // small rounding rad
                double L = 105;    // total height
                double d = 20;      // upper diam
                double h = 11.8;    // rounding start height
                double RR = 65; // big rounding rad
                dvParams.Rows.Add("D1 (head diam):", D1);
                dvParams.Rows.Add("S (head to edge):", S);
                dvParams.Rows.Add("S1 (head height):", S1);
                dvParams.Rows.Add("R (round. radius):", R);
                dvParams.Rows.Add("H (round. height):", h);
                dvParams.Rows.Add("RR (Big round. radius):", RR);
                dvParams.Rows.Add("L (Tool height):", L);
                dvParams.Rows.Add("D (tail diam):", d);
            }
            else if (cmbToolType.Text == "VBit2" || cmbToolType.Text == "VBit3")
            {
                double D1 = 14; // head diam
                double S1 = 19;    //edge height
                double L = 85;    // total height
                double d = 20;      // upper diam                
                dvParams.Rows.Add("D1 (head diam):", D1);
                dvParams.Rows.Add("S1 (head height):", S1);
                dvParams.Rows.Add("L (Tool height):", L);
                dvParams.Rows.Add("D (tail diam):", d);
            }
            else if (cmbToolType.Text == "VBit4")
            {
                double D1 = 12; // head diam
                double S1 = 19;    //edge height
                double L = 85;    // total height
                double d = 20;      // upper diam 
                double S = 11;    // head to edge
                double h = 5;    // rounding start height
                double R = 10;     // rounding rad
                dvParams.Rows.Add("D1 (head diam):", D1);
                dvParams.Rows.Add("S1 (head height):", S1);
                dvParams.Rows.Add("S (head to edge):", S);
                dvParams.Rows.Add("H (round. height):", h);
                dvParams.Rows.Add("R (round. radius):", R);
                dvParams.Rows.Add("L (Tool height):", L);
                dvParams.Rows.Add("D (tail diam):", d);
            }
            else if (cmbToolType.Text == "VBit5")
            {
                double D = 20; // head diam
                double S1 = 5;    //edge height
                double L = 85;    // total height
                double d = 14;      // upper diam 
                double R = 12.5;     // rounding rad
                dvParams.Rows.Add("D (head diam):", D);
                dvParams.Rows.Add("S1 (head height):", S1);
                dvParams.Rows.Add("R (round. radius):", R);
                dvParams.Rows.Add("L (Tool height):", L);
                dvParams.Rows.Add("D2 (tail diam):", d);
            }
            else if (cmbToolType.Text == "VBit6")
            {
                double D1 = 12; // head diam
                double D = 32;    //edge height
                double L = 85;    // total height
                double d = 14;      // upper diam 
                double A = 45; // angle
                dvParams.Rows.Add("D1 (head diam):", D1);
                dvParams.Rows.Add("D (Tool diam):", D);
                dvParams.Rows.Add("A (Edge angle):", A);
                dvParams.Rows.Add("L (Tool height):", L);
                dvParams.Rows.Add("D2 (tail diam):", d);
            }
            else if (cmbToolType.Text == "VBit7")
            {
                double D1 = 12.5; // hole diam
                double D = 30;    //head diam
                double L = 70;    // total height
                double d = 12;      // upper diam 
                double A = 45; // angle
                double h = 5; // straight part height
                double S1 = 9;    //slot height
                dvParams.Rows.Add("D (head diam):", D);
                dvParams.Rows.Add("D1 (hole diam):", D1);

                dvParams.Rows.Add("S1 (slot height):", S1);
                dvParams.Rows.Add("H (straight part height):", h);
                dvParams.Rows.Add("A (edge angle):", A);
                dvParams.Rows.Add("L (Tool height):", L);
                dvParams.Rows.Add("D2 (tail diam):", d);
            }
            else if (cmbToolType.Text == "VBit8")
            {
                double D1 = 12.5; // hole diam
                double D = 30;    //head diam
                double L = 80;    // total height
                double d = 20;      // upper diam 
                double S1 = 9;    //slot height
                double H0 = 18; // cutter height
                dvParams.Rows.Add("D (head diam):", D);
                dvParams.Rows.Add("D1 (hole diam):", D1);

                dvParams.Rows.Add("S1 (slot height):", S1);
                dvParams.Rows.Add("H0 (cutter height):", H0);
                dvParams.Rows.Add("L (Tool height):", L);
                dvParams.Rows.Add("D2 (tail diam):", d);
            }
            else if (cmbToolType.Text == "VBit9")
            {
                double D1 = 24; // hole diam
                double D = 12;    //head diam
                double L = 85;    // total height
                double d = 20;      // upper diam 
                double S1 = 14;    //head height
                double A = 45; // angle
                dvParams.Rows.Add("D1 (hole diam):", D1);
                dvParams.Rows.Add("D (head diam):", D);
                dvParams.Rows.Add("S1 (slot height):", S1);
                dvParams.Rows.Add("A (edge angle):", A);
                dvParams.Rows.Add("L (Tool height):", L);
                dvParams.Rows.Add("D2 (tail diam):", d);
            }
            if(b_new_tool)
            {
                currentTool.myType = Tool.GetTypeByString(cmbToolType.Text);
                if (currentTool.myType != ToolType.Custom)
                {
                    lblParams.Text = "Parameters:";                    
                    dvParams.AllowUserToAddRows = false;
                    Value.HeaderText = "Value";
                    Property.HeaderText = "Property";
                }
                else
                {
                    lblParams.Text = "Tool shape coordinates:";
                    dvParams.AllowUserToAddRows = true;
                    Value.HeaderText = "Y-coordinate, mm";
                    Property.HeaderText = "X-coordinate, mm";                    
                }
            }
        }

        private void FormTools_Load(object sender, EventArgs e)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            ReloadAllTools();            
        }

        private void ReloadAllTools()
        {
            LbTools.Items.Clear();
            Tools.Clear();
            string errors = "";
            foreach (string f in Directory.GetFiles(path))
            {
                Tool t = Tool.GetToolFromFile(f, ref errors);
                if (t != null)
                    Tools.Add(t);
            }
            if (Tools.Count > 0)
            {
                LbTools.Items.AddRange(Tools.Select(r => r.ToolName).OrderBy(x=>x).ToArray());
                if (selected == "")
                    LbTools.SetSelected(0, true);
                else
                    if (LbTools.Items.IndexOf(selected) != -1)
                    {
                        LbTools.SetSelected(LbTools.Items.IndexOf(selected), true);                        
                    }
            }
            //Read Accessible tools - except already existing
            try
            {
                acc_tools = File.ReadAllLines(Application.StartupPath + "//OPTCAM_Tools.txt").ToList()
                .Where(x=> !Tools.Select(r => r.ToolName).Contains(x) ).ToList();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Error reading OPTCAM_Tools", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

       
        private void glControl1_Load(object sender, EventArgs e)
        {
            IGraphicsContext control2Context = new GraphicsContext(GraphicsMode.Default, glControl1.WindowInfo);
            glControl1.MakeCurrent();
            glControl1.oGLCamera = new OGLCamera();
           
            
            GL.ClearColor(Color.SkyBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Normalize);

            //Matrix4 p = Matrix4.CreatePerspectiveFieldOfView((float)(80 * Math.PI / 180), (float)glControl1.Width / (float)glControl1.Height, 1, 1000);
            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadMatrix(ref p);

            ResetView(max_Z);
            glControl1.oGLCamera.Reset(null);

            float[] mat_diffuse = { 1.0f, 0.0f, 0.0f };
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.AmbientAndDiffuse, mat_diffuse);
            GL.Light(LightName.Light0, LightParameter.Position, new float[] { 10.0f, 0.0f, 5.0f, -1.0f });

            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.DepthTest);
            GL.ShadeModel(ShadingModel.Smooth);

            glControl1.loaded = true;
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(glControl1.ClientRectangle.X, glControl1.ClientRectangle.Y, glControl1.ClientRectangle.Width, glControl1.ClientRectangle.Height);

            Matrix4 p = Matrix4.CreatePerspectiveFieldOfView((float)(80 * Math.PI / 180), (float)glControl1.Width / (float)glControl1.Height, 1, 1000);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref p);

            GL.MatrixMode(MatrixMode.Modelview);

        }
        private void ResetView(float scale_XYZ)
        {
            glControl1.oGLCamera.eyePos_def = glControl1.oGLCamera.eyePos = new Vector3(scale_XYZ, scale_XYZ, scale_XYZ / 2);
            glControl1.oGLCamera.targetPos_def = glControl1.oGLCamera.targetPos = new Vector3(0, 0, scale_XYZ / 2);
            glControl1.oGLCamera.cur_zoom = 1;
            glControl1.oGLCamera.eyeMatrix = Matrix4.LookAt(glControl1.oGLCamera.eyePos, glControl1.oGLCamera.targetPos,
                OGLCamera.OZ * (float)Math.Pow(-1, glControl1.oGLCamera.times_changed_sign))
                 * Matrix4.CreateScale(glControl1.oGLCamera.cur_zoom, glControl1.oGLCamera.cur_zoom, 1);
            glControl1.oGLCamera.RefreshCameraPos();
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref glControl1.oGLCamera.eyeMatrix);
            glControl1.Invalidate();
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            RenderMesh();

            GL.Light(LightName.Light0, LightParameter.Position, new float[] { 10.0f, 0.0f, 5.0f, -1.0f });

            glControl1.SwapBuffers();
        }
        public void RenderMesh()
        {
            if (currentTool == null || currentTool.ToolObj == null)
                return;
            GL.Begin(PrimitiveType.Triangles);

            Color col = currentTool.ToolObj.color;

            var verts = currentTool.ToolObj.GetVertices();
            int[] ind = currentTool.ToolObj.GetIndices();
            max_Z = -999;
            for (var i = 0; i < ind.Length; i = i + 3)
            {
                GL.Normal3(DemoForm.MainForm.GetNormal(verts[ind[i]], verts[ind[i + 1]], verts[ind[i + 2]]));
                var p = verts[ind[i]];
                GL.Color3(col);
                GL.Vertex3(new Vector3d(p.X, p.Y, p.Z));

                p = verts[ind[i + 1]];
                GL.Color3(col);
                GL.Vertex3(new Vector3d(p.X, p.Y, p.Z));

                p = verts[ind[i + 2]];
                GL.Color3(col);
                GL.Vertex3(new Vector3d(p.X, p.Y, p.Z));

                if (max_Z < p.Z)
                    max_Z = (float)p.Z;
            }

            GL.End();
        }

        private void LbTools_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Tools.Where(r => r.ToolName == LbTools.SelectedItem.ToString()).Count() > 0)
            {
                Tool current = Tools.Where(r => r.ToolName == LbTools.SelectedItem.ToString()).ToList()[0];
                currentTool = current;
                glControl1.Refresh();
                ResetView(max_Z);
                RefreshParams(current);
                selected = current.ToolName;
            }
        }
        private void RefreshParams(Tool current)
        {
            dvParams.Rows.Clear();
            tbToolName.Text = current.ToolName;
            cmbToolType.SelectedIndex = (int)current.myType - 1;
            if (dvParams.Rows.Count == 0)
                cmbToolType_SelectedIndexChanged(null, null);
            if (current.myType != ToolType.Custom)
            {
                lblParams.Text = "Parameters:";
                for (int k = 0; k < dvParams.Rows.Count; k++)
                {
                    DataGridViewRow dr = dvParams.Rows[k];
                    if (dr.Cells[Property.Index].Value != null)
                    {
                        string key = dr.Cells[Property.Index].Value.ToString().Split(' ')[0];
                        dr.Cells[Value.Index].Value = current.ToolParams[key] * (key == "A" ? 1 : 10);
                    }
                }
                dvParams.AllowUserToAddRows = false;
                Value.HeaderText = "Value";
                Property.HeaderText = "Property";
                bnImportDXF.Visible = false;
            }
            else
            {
                lblParams.Text = "Tool shape coordinates:";
                dvParams.AllowUserToAddRows = true;
                dvParams.AllowUserToDeleteRows = true;
                Value.HeaderText = "Y-coordinate, mm";
                Property.HeaderText = "X-coordinate, mm";
                for (int k = 0; k < current.tool_contour.Count; k++)
                {
                    Net3dBool.Vector3 v = current.tool_contour[k];
                    dvParams.Rows.Add(Math.Round( v.X * 10,5), Math.Round(v.Z * 10, 5));
                    if (v == Net3dBool.Vector3.Zero)
                        break;
                }
                bnImportDXF.Visible = true;
            }
        }

        private void bnImportDXF_Click(object sender, EventArgs e)
        {
            OPTCAMSim.FormImportDXF f = new FormImportDXF();
            try
            {
                f.ShowDialog();
                if (f.Vertices != null && f.Vertices.Count>0)
                {
                    dvParams.Rows.Clear();
                    foreach (netDxf.Vector2 v in f.Vertices)
                        dvParams.Rows.Add( Math.Round(v.X,5), Math.Round(v.Y, 5));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        private void FormTools_FormClosing(object sender, FormClosingEventArgs e)
        {
            StoreLocationAndSize();
        }
        private void FormTools_Shown(object sender, EventArgs e)
        {
            RestoreLocationAndSize();
        }
        private void RestoreLocationAndSize()
        {
            if (Properties.Settings.Default.ToolsFormMaximized)
            {
                WindowState = FormWindowState.Maximized;
                Location = Properties.Settings.Default.ToolsFormLocation;
                Size = Properties.Settings.Default.ToolsFormSize;
            }
            else if (Properties.Settings.Default.ToolsFormMinimized)
            {
                WindowState = FormWindowState.Minimized;
                Location = Properties.Settings.Default.ToolsFormLocation;
                Size = Properties.Settings.Default.ToolsFormSize;
            }
            else
            {
                Location = Properties.Settings.Default.ToolsFormLocation;
                Size = Properties.Settings.Default.ToolsFormSize;
            }
        }
        private void StoreLocationAndSize()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                Properties.Settings.Default.ToolsFormLocation = RestoreBounds.Location;
                Properties.Settings.Default.ToolsFormSize = RestoreBounds.Size;
                Properties.Settings.Default.ToolsFormMaximized = true;
                Properties.Settings.Default.ToolsFormMinimized = false;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.ToolsFormLocation = Location;
                Properties.Settings.Default.ToolsFormSize = Size;
                Properties.Settings.Default.ToolsFormMaximized = false;
                Properties.Settings.Default.ToolsFormMinimized = false;
            }
            else
            {
                Properties.Settings.Default.ToolsFormLocation = RestoreBounds.Location;
                Properties.Settings.Default.ToolsFormSize = RestoreBounds.Size;
                Properties.Settings.Default.ToolsFormMaximized = false;
                Properties.Settings.Default.ToolsFormMinimized = true;
            }
            Properties.Settings.Default.Save();
        }

        
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, List<string> combo_items)
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 150,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen
            };
            Label textLabel = new Label() { Left = 50, Top = 20, Text = text, AutoSize = true};
            ComboBox comboBox = new ComboBox() { Left = 50, Top = 50, Width = 400, DropDownStyle = ComboBoxStyle.DropDownList};
            Button confirmation = new Button() { Text = "OK", Left = 350, Width = 100, Top = 75, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(comboBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            comboBox.Items.AddRange(combo_items.ToArray());
            if (comboBox.Items.Count > 0)
                comboBox.SelectedIndex = 0;
            return prompt.ShowDialog() == DialogResult.OK ? comboBox.Text : "";
        }
    }
}
