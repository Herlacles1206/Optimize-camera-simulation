using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.IO;
using System.Xml.Serialization;
using System.Threading;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Linq;
using OPTCAMSim;

namespace DemoForm
{
    
    public partial class MainForm : Form
    {
        public List<Net3dBool.Solid> MeshList = new List<Net3dBool.Solid> { };
        List<int> changed_indices = new List<int> { };
        
        public CSGPanel cSGPanel;
        
        Tool myTool;

        DateTime last_upd = DateTime.UtcNow;
        DateTime start = DateTime.UtcNow;

        ToolProgram toolProgram = new ToolProgram();
        private static System.Timers.Timer aTimer;
        BackgroundWorker BGDW = new BackgroundWorker();
        
        /// <summary>
        ///  tolerance value to test equalities
        /// </summary>
        double EQ_TOL = 5e-6f;

        /// <summary>
        /// user changed tool speed
        /// </summary>
        bool speed_changed = false;
        /// <summary>
        /// user changed panel size
        /// </summary>
        bool size_changed = false;

        
        //Debug only!
        bool render_cuts = false;

        bool pause_thread = false;
        int MAX_NUM_ITERS = 2000;

        bool PlayM6 = false;
        bool PlayStep = false;

        //Shaders
        ShaderProgram main_shader;
        int _vao;
        int _glbuf;
        
        float[] _vertices;         
        int last_buffer_size = 0;

        //Light source position
        Vector3 LightPos = new Vector3(500, 500, 500);

        bool redraw_panel = false;
        bool clicked_speed = false;        
               
        //Code display vars       
        public int pnl_index = 0;
        private int prev_index = -1;

        //Nested panels from JSON
        List<NestedPanels> panelList = new List<NestedPanels> { };
        
        //Panel Texture
        int PNLTexID = -1;
        bool usePNLTex = false; // for later use...

        /// <summary>
        /// Draw shadows
        /// </summary>
        bool showShadows = true;
        int depthMapFBO;
        int SHADOW_WIDTH = 8192, SHADOW_HEIGHT = 8192;
        int depthMap;
        ShaderProgram shadows_shader;

        public MainForm()
        {
            InitializeComponent();

            BGDW.DoWork += BGDW_DoWork;
            BGDW.ProgressChanged += BGDW_ProgressChanged;
            BGDW.RunWorkerCompleted += BGDW_RunWorkerCompleted;
            BGDW.WorkerReportsProgress = true;
            BGDW.WorkerSupportsCancellation = true;

            this.FormClosing += Demo_FormClosing;          

            tbToolSpeed.MouseDown += (s,e) =>{ clicked_speed = true; };
            tbToolSpeed.MouseUp += (s, e) => { clicked_speed = false; tbToolSpeed_Scroll(s, e); };
            tbToolSpeed.MouseWheel += (sender, e) => ((HandledMouseEventArgs)e).Handled = true;
            nudSpeed.MouseWheel += (sender, e) => ((HandledMouseEventArgs)e).Handled = true;

            //SetDoubleBuffered(dvCode);    
        }
        /// <summary>
        /// Better drawing of datagridview
        /// </summary>
        /// <param name="myDataGridViewObject"></param>
        private void SetDoubleBuffered(DataGridView myDataGridViewObject)
        {
            typeof(DataGridView).InvokeMember(
           "DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.SetProperty,
           null,
           myDataGridViewObject,
           new object[] { true });
        }
        private void SetTimer()
        {
            if (aTimer != null)
                aTimer.Stop();
            // Create a timer with a 1/fps second interval.
            aTimer = new System.Timers.Timer(1000.0 / OPTCAMSim.Properties.Settings.Default.FPS);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += ATimer_Elapsed;
            aTimer.AutoReset = true;
            //aTimer.Enabled = true;
        }
        /// <summary>
        /// Constantly redraws the form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ATimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //if (!pause_thread)
            //{
                glControl1.Invalidate();
            //}
        }

        private void Demo_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (BGDW.IsBusy)
                BGDW.CancelAsync();

            FreeShader();
            FreeVao();

            StoreSettings();
        }

        private void BGDW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            aTimer.Enabled = false;
            TimeSpan time = (DateTime.UtcNow - start);
            this.glControl1.Invalidate();

            MessageBox.Show("Simulation finished!" + Environment.NewLine + "Total time: " + ToReadableString(time));

            bnStart.Enabled = true;
            bnPause.Enabled = false;
            bnStop.Enabled = false;
            bnPlayM6.Enabled = true;
            bnStep.Enabled = true;
            cmbPanels.Enabled = true;

            pause_thread = false;
            bnPause.Text = "Pause";
            
            progressBar1.Value = 0;
            lblPrc.Text = "";
            changed_indices = new List<int> { };
            SetCurLine(toolProgram.steps[0].line_number);
        }
        public static string ToReadableString(TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }
        private void BGDW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            RefillVBO();
            progressBar1.Value = e.ProgressPercentage;
            lblPrc.Text = progressBar1.Value.ToString() + "%";
            lblPrc.Invalidate();
            lblPrc.Refresh();


        }
        [HandleProcessCorruptedStateExceptions]
        [SecurityCritical]
        private void BGDW_DoWork(object sender, DoWorkEventArgs e)
        {
            aTimer.Enabled = true;
            
            if (toolProgram.steps != null && toolProgram.steps.Count > 0)
            {
                int cur_line = -1;
                for (int step_index = 0; step_index < toolProgram.steps.Count; step_index++)
                {                    
                    try
                    {
                        cur_line = toolProgram.steps[step_index].line_number;
                        if (BGDW.CancellationPending)
                        {
                            e.Cancel = true;
                            return;
                        }
                        //Pause sim
                        while (pause_thread)
                        {
                            Thread.Sleep(50);
                            if (BGDW.CancellationPending)
                            {
                                e.Cancel = true;
                                return;
                            }
                        }
                        //Wait data refill
                        //refill_VBO.WaitOne(5000);

                        //Prepare program in case of speed change
                        if (speed_changed)
                        {
                            if (step_index == 0)
                            {
                                OPTCAMSim.Properties.Settings.Default.StepLength = (double)nudSpeed.Value / 3.0;
                                OPTCAMSim.Properties.Settings.Default.Save();
                                toolProgram.SplitStepsInSmallerSteps((double)nudSpeed.Value / 3.0);
                                speed_changed = false;
                                toolProgram.CombineStepsIntoLargerSteps(OPTCAMSim.Properties.Settings.Default.SmallSteps);
                            }
                            else
                            {
                                int ref_step = toolProgram.steps[step_index - 1].G != Gcode.TOOL_CHANGE ? step_index - 1 : step_index - 2;
                                if (ref_step < 0)
                                    ref_step = 0;
                                int new_index = PrepareProgramForNewSpeed(toolProgram.steps[ref_step].base_step_index, toolProgram.steps[ref_step].pos);
                                step_index = new_index >= 0 ? new_index : step_index;
                                speed_changed = false;
                            }
                        }
                        //show current progress on code form
                        if(step_index< toolProgram.steps.Count-1)
                            dvCode.Invoke(new Action(() => SetCurLine(toolProgram.steps[step_index+1].line_number)));

                        if (toolProgram.steps[step_index].G == Gcode.QUICK_MOVE)
                        {
                            if (toolProgram.steps[step_index].action == null)
                            {
                                float dx = toolProgram.steps[step_index].step.X;
                                float dy = toolProgram.steps[step_index].step.Y;
                                float dz = toolProgram.steps[step_index].step.Z;
                                myTool.ToolObj.Translate(dx, dy, dz);
                            }
                            else
                            {
                                if (toolProgram.steps[step_index].action.actType == SpecialActionType.Stop)
                                {
                                    bnPause.Invoke(new Action(() =>
                                    {
                                        this.bnPause_Click(null, null);
                                    }));
                                }
                                else if (toolProgram.steps[step_index].action.actType == SpecialActionType.Pause)
                                {                                    
                                    Thread.Sleep((int)(toolProgram.steps[step_index].action.pars["t"] * 1000));                                    
                                }
                            }
                        }
                        else if (toolProgram.steps[step_index].G == Gcode.TOOL_CHANGE)
                        {
                            float dx = toolProgram.steps[step_index].pos.X;
                            float dy = toolProgram.steps[step_index].pos.Y;
                            float dz = toolProgram.steps[step_index].pos.Z;

                            //Remove old, create new tool
                            if (myTool != null && myTool.ToolObj != null)
                                MeshList.Remove(myTool.ToolObj);
                            myTool = new Tool(toolProgram.steps[step_index].tool);
                            MeshList.Add(myTool.ToolObj);
                            if (dx != 0 || dy != 0 || dz != 0)
                                myTool.ToolObj.Translate(dx, dy, dz);

                            //Update form
                            this.gbTool.Invoke(new Action(() =>
                            {                                
                                this.gbTool.Text = "Tool " + myTool.ToolName;
                            }));
                        }
                        else
                        {
                            if (OPTCAMSim.Properties.Settings.Default.UseAdvanced3D == 0 ||
                                (!(toolProgram.steps[step_index].step.Z != 0 
                                && (toolProgram.steps[step_index].step.X != 0 || toolProgram.steps[step_index].step.Y != 0))))
                            {
                                Net3dBool.Solid sub = null;
                                if (toolProgram.steps[step_index].is_first)
                                    DoCut(myTool.ToolObj);

                                toolProgram.GetStepFullSolidBodyAndMoveTool(step_index, ref myTool, ref sub);

                                DoCut(sub);
                                if (render_cuts)
                                    MeshList.Add(sub);

                                if (toolProgram.steps[step_index].is_last)
                                    DoCut(myTool.ToolObj);
                            }
                            else
                            {
                                Net3dBool.Solid sub = null;
                                toolProgram.GetStepFullSolidBodyAndMoveTool(step_index, ref myTool, ref sub);
                                DoCut(sub);
                                if (render_cuts)
                                    MeshList.Add(sub);
                            }
                        }
                        BGDW.ReportProgress((int)(100 * (1 + step_index) / ((float)toolProgram.steps.Count)));


                        //Pause in case of PlayM6 - until next tool change / or in case of step-by-step simulation
                        if ((PlayM6 && step_index < toolProgram.steps.Count - 1 && toolProgram.steps[step_index + 1].G == Gcode.TOOL_CHANGE) 
                            || (PlayStep && step_index < toolProgram.steps.Count -1 && cur_line < toolProgram.steps[step_index+1].line_number) )
                        {
                            bnPause.Invoke(new Action(() =>
                            {
                                this.bnPause_Click(null, null);
                            }));
                        }
                        

                        ///Wait to simulate tool speed...
                        if (OPTCAMSim.Properties.Settings.Default.StepLength < 33)
                        {
                            Thread.Sleep((int)(1000 / OPTCAMSim.Properties.Settings.Default.StepLength));
                        }

                    }
                    catch(Exception ex)
                    {
                        //MessageBox.Show(ex.ToString());
                        Thread.Sleep(1000);
                        continue;
                    }
                }
            }
            else
                MessageBox.Show("Toolpath program not loaded!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

        }
        private void DoCut(Net3dBool.Solid cutting)
        {

            if (OPTCAMSim.Properties.Settings.Default.MergeCuts)
            {
                var modeller = new Net3dBool.BooleanModeller(cutting, cutting, OPTCAMSim.Properties.Settings.Default.CutFaces, EQ_TOL, MAX_NUM_ITERS);
                cutting = modeller.GetUnion();
            }
            List<Net3dBool.Solid> cubics = GetRelevantCubics(cutting);

            //var watch = System.Diagnostics.Stopwatch.StartNew();

            cubics.Select(ser =>
            {
                Thread thread = new Thread(unused => DoSimpleCut(ref ser, ref cutting));
                thread.Start();
                return thread;
            }).ToList().ForEach(t => t.Join());

            //watch.Stop();
            //long elapsedMs = watch.ElapsedMilliseconds;
            //if (elapsedMs > 3000)
            //{//Decrease MAX_NUM_ITERS
            //    if (MAX_NUM_ITERS > 500)
            //        MAX_NUM_ITERS -= 200;
            //    else if (MAX_NUM_ITERS < 500)
            //        MAX_NUM_ITERS = 500;
            //}
            //System.Diagnostics.Debug.Print("Single cut timing: " + elapsedMs);
        }

        private void DoSimpleCut(ref Net3dBool.Solid cube, ref Net3dBool.Solid cutting)
        {
            int old_index = MeshList.IndexOf(cube);
            bool test = false;
            if (test)
            {
               // cube = Net3dBool.newCSG.GetDiff(cube, cutting);
            }
            else
            {  
                var modeller = new Net3dBool.BooleanModeller(cube, cutting, OPTCAMSim.Properties.Settings.Default.CutFaces, EQ_TOL, MAX_NUM_ITERS);
                cube = modeller.GetDifference();
            }
            //cleanup
            if (OPTCAMSim.Properties.Settings.Default.MergeHoles)
            {
                var modeller = new Net3dBool.BooleanModeller(cube, cube, OPTCAMSim.Properties.Settings.Default.CutFaces, EQ_TOL, MAX_NUM_ITERS);
                cube = modeller.GetUnion();
            }

            cube.Name = "PanelObj";
            cube.color = Color.Yellow;            

            MeshList[old_index] = cube;

            changed_indices.Add(old_index);
        }
        /// <summary>
        /// Rearrange program in case tool speed was changed
        /// </summary>
        /// <param name="base_step_index"></param>
        /// <param name="cur_pos"></param>
        /// <returns></returns>
        private int PrepareProgramForNewSpeed(int base_step_index, Vector3 cur_pos)
        {
            int new_step_index = -1;
            OPTCAMSim.Properties.Settings.Default.StepLength = (double)nudSpeed.Value / 3.0;
            OPTCAMSim.Properties.Settings.Default.Save();
            toolProgram.SplitStepsInSmallerSteps((double)nudSpeed.Value / 3.0);
            toolProgram.CombineStepsIntoLargerSteps(OPTCAMSim.Properties.Settings.Default.SmallSteps);
            new_step_index = toolProgram.GetNearestStepAndModifyIt(base_step_index, cur_pos);
            return new_step_index;
        }


        /// <summary>
        /// Draw panel on the base of current toolprogram
        /// </summary>
        private void DrawPanel(bool reset_camera=true)
        {
            MeshList.Clear();

            if (toolProgram!=null && toolProgram.steps.Count > 0)
            {
                cSGPanel = new OPTCAMSim.CSGPanel(toolProgram);
                glControl1.cSGPanel = cSGPanel;
                MeshList = cSGPanel.cubics;
                myTool = null;
            }
            RefillVBO();

            if(reset_camera)
                glControl1.oGLCamera.Reset(cSGPanel);

            glControl1.Invalidate();

        }
        
        private List<Net3dBool.Solid> GetRelevantCubics(Net3dBool.Solid cutting)
        {
            Net3dBool.Bound b = new Net3dBool.Object3D(cutting).Bound;
            List<Net3dBool.Solid> cubics = new List<Net3dBool.Solid> { };
            foreach (Net3dBool.Solid c in MeshList)
            {
                if (c.Name == "PanelObj" && c.IsEmpty == false)
                {
                    Net3dBool.Bound a = new Net3dBool.Object3D(c).Bound;
                    if (a.Overlap(b))
                        cubics.Add(c);
                }
            }
            return cubics;
        }
        private void bnStart_Click(object sender, System.EventArgs e)
        {
            //Warn user that we need to reload sim-file in case of size change
            if (size_changed)
            {
                DialogResult res;
                res = MessageBox.Show(
                    "Panel dimensions were changed." + Environment.NewLine +
                    "Changes will take place only after sim-file reload." + Environment.NewLine +
                    "Do you want to reload sim-file now (Yes) " + Environment.NewLine +
                    "or continue with previously set panel dimensions (No)?" + Environment.NewLine +
                    "You can cancel the simulation and restart later (Cancel)."
                    , "Panel dimensions change",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question
                    );
                if (res == DialogResult.Yes)
                {
                    LoadProgramToolStripMenuItem_Click(sender, e);                    
                }
                else if (res == DialogResult.Cancel)
                    return;
                
            }

            if (toolProgram == null || toolProgram.steps == null || toolProgram.steps.Count == 0)
            {
                MessageBox.Show("Toolpath program not loaded!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            

            if (redraw_panel)
            {
                FreeVao();
                InitVao();
                DrawPanel(false);
            }

            if (MeshList.Count == 0)
                return;

            MAX_NUM_ITERS = 2000;
            glControl1.oGLCamera.times_changed_sign = 0;

            glControl1.Invalidate();

            bnStart.Enabled = false;
            bnPause.Enabled = bnStop.Enabled = true;
            bnPlayM6.Enabled = false;
            bnStep.Enabled = false;
            cmbPanels.Enabled = false;

            start = DateTime.UtcNow;
            List<int> changed_indices = new List<int> { };
            if (BGDW.IsBusy)
                BGDW.CancelAsync();
            SetTimer();

            if (sender != bnPlayM6)
                PlayM6 = false;

            if (sender != bnStep)
                PlayStep = false;

            dvCode.Invoke(new Action(() => Restart()));            

            BGDW.RunWorkerAsync();
            redraw_panel = true;
        }


        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void LoadProgramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog() { Title = "Select sim file" })
            {
                DialogResult result = ofd.ShowDialog();

                if (result == DialogResult.OK)
                {
                    InitToolProgram(ref toolProgram, ofd.FileName);
                }
            }
        }

        private void InitToolProgram(ref ToolProgram toolProg, string simfile_path)
        {
            if (!File.Exists(simfile_path))
            {
                MessageBox.Show("Toolpath file not found!" + Environment.NewLine + simfile_path,
                   "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            BusyForm busy = new BusyForm("Please wait, initializing toolpath...");
            Thread thread = new Thread(unused => busy.ShowDialog());
            thread.Start();
           
            int err_ct = 0;
            string errors = "";
            bool result = false;
            try
            {
                toolProg = new ToolProgram();
                result = toolProg.ParseProgramFile(simfile_path, out err_ct, out errors);

                dvCode.DataSource = null;

                if (result)
                {
                    //Save last sim-file location
                    OPTCAMSim.Properties.Settings.Default.LastSimPath = simfile_path;
                    OPTCAMSim.Properties.Settings.Default.Save();

                    if (toolProgram != toolProg)
                        toolProgram = toolProg;

                    UpdateFormAfterToolProgramInit();
                }
                this.Activate();
                busy.Invoke(new Action(() => busy.Close()));               
            }
            catch (Exception ex)
            {
                this.Activate();
                busy.Invoke(new Action(() => busy.Close()));
                MessageBox.Show("Failed to read toolpath from file!" + Environment.NewLine + ex.Message,
                   "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Toolpath read from file, steps processed:" + toolProg.steps_real
                + (err_ct > 0 ? Environment.NewLine + "Error count:" + err_ct.ToString() + Environment.NewLine + errors : ""),
                  "Info", MessageBoxButtons.OK, err_ct > 0 ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }
        
        public void UpdateFormAfterToolProgramInit()
        {
            dvCode.DataSource = null;
            if (toolProgram.PanelSize != Vector3.Zero)
            {
                nudWidth.Value = (decimal)toolProgram.PanelSize.X;
                nudLength.Value = (decimal)toolProgram.PanelSize.Y;
                nudHeight.Value = (decimal)toolProgram.PanelSize.Z;
                size_changed = false;
                
                DrawPanel();
                redraw_panel = false;
            }

            //reload code after new sim loaded
            ReloadCodeView(toolProgram.all_sim_lines);
            pnl_index = toolProgram.pnl_line - 1;
            SetCurLine(toolProgram.steps[0].line_number);
        }

        private void InitShaders()
        {
            main_shader = new ShaderProgram("main_shader_vs.txt", "main_shader_fs.txt");            

            //Light
            main_shader.SetUniform3( "lightPos", LightPos);
            main_shader.SetUniform3("lightColor", new Vector3(1.0f, 1.0f, 1.0f));

            //Textures
            main_shader.SetUniform1("texture0", 0);// Panels' part names
            main_shader.SetUniform1("texture1", 1);// panel texture
            main_shader.SetUniform1("shadowMap", 2);// shadows texture

            //shadows shader
            if (showShadows)
            {
                shadows_shader = new ShaderProgram("shadows_shader_vs.txt", "shadows_shader_fs.txt");                
            }
        }

        void InitVao()
        {
            _vao = GL.GenVertexArray();
            _glbuf = GL.GenBuffer();
            GL.BindVertexArray(_vao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _glbuf);
            //Attribute pointers

            //Position
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 12 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            // normal attribute
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 12 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(1);
            // Color attribute
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 12 * sizeof(float), 6 * sizeof(float));
            GL.EnableVertexAttribArray(2);
            //Texture u v
            GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 12 * sizeof(float), 9 * sizeof(float));
            GL.EnableVertexAttribArray(3);
            //Texture index
            GL.VertexAttribPointer(4, 1, VertexAttribPointerType.Float, false, 12 * sizeof(float), 11 * sizeof(float));
            GL.EnableVertexAttribArray(4);


            GL.BindVertexArray(0);
        }

        void FreeShader()
        {
            if (main_shader != null)
                main_shader.Free();
           
            if(showShadows)
            {
                if (shadows_shader != null)
                    shadows_shader.Free();
            }
        }

        void FreeVao()
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_glbuf);
        }

 
             
        /// <summary>
        /// REfill vertex buffer with new data
        /// </summary>
        private void RefillVBO()
        {
            List<float> verts_ar = new List<float> { };
            for (int q = 0; q < MeshList.Count; q++)
            {
                Net3dBool.Solid Mesh = MeshList[q];
                Color col = Mesh.color;

                var verts = Mesh.GetVertices();
                int[] ind = Mesh.GetIndices();
                if (PNLTexID < 0 || Mesh.Name=="ToolObj")
                {
                    for (var i = 0; i < ind.Length; i = i + 3)
                    {
                        Vector3 n = -GetNormal(verts[ind[i]], verts[ind[i + 1]], verts[ind[i + 2]]);
                        verts_ar.AddRange(new List<float>{(float)verts[ind[i]].X, (float)verts[ind[i]].Y, (float)verts[ind[i]].Z, n.X, n.Y, n.Z, col.R/255f, col.G/255f,col.B /255f,0,0,0,
                                        (float)verts[ind[i + 1]].X, (float)verts[ind[i + 1]].Y, (float)verts[ind[i + 1]].Z, n.X, n.Y, n.Z, col.R/255f, col.G/255f,col.B /255f,0,0,0,
                                        (float)verts[ind[i + 2]].X, (float)verts[ind[i + 2]].Y, (float)verts[ind[i + 2]].Z, n.X, n.Y, n.Z, col.R/255f, col.G/255f,col.B /255f,0,0,0 }
                            );
                    }
                }
                else
                {
                    float w = toolProgram != null ? toolProgram.PanelSize.X / 10f: 210;
                    float h = toolProgram != null ? toolProgram.PanelSize.Y / 10f : 280;
                    for (var i = 0; i < ind.Length; i = i + 3)
                    {
                        float x1= (float)verts[ind[i]].X, x2= (float)verts[ind[i + 1]].X, x3 = (float)verts[ind[i + 2]].X,
                            y1= (float)verts[ind[i]].Y, y2= (float)verts[ind[i + 1]].Y, y3= (float)verts[ind[i + 2]].Y,
                            z1= (float)verts[ind[i]].Z, z2= (float)verts[ind[i + 1]].Z, z3= (float)verts[ind[i + 2]].Z,
                            u1 = x1/w, v1 = y1/h, u2 = x2 / w, v2 = y2 / h, u3 = x3 / w, v3 = y3 / h;
                        Vector3 n = -GetNormal(verts[ind[i]], verts[ind[i + 1]], verts[ind[i + 2]]);
                        verts_ar.AddRange(new List<float>{x1,y1 , z1, n.X, n.Y, n.Z, col.R/255f, col.G/255f,col.B /255f,u1,v1,2,
                                        x2, y2, z2, n.X, n.Y, n.Z, col.R/255f, col.G/255f,col.B /255f,u2,v2,2,
                                       x3, y3, z3, n.X, n.Y, n.Z, col.R/255f, col.G/255f,col.B /255f,u3,v3,2 }
                            );
                    }
                }
            }
            //fill with empty data 
            if(last_buffer_size>0 && verts_ar.Count< last_buffer_size)
            {
                verts_ar.AddRange(  Enumerable.Range(1, last_buffer_size - verts_ar.Count).Select(x => x * 0.0f) );
            }
            if (chkShowPartData.Checked) // show panel parts data
            {
                if (cmbPanels.SelectedIndex >= 0)  
                {
                    //Part names and IDs                   
                    NestedPanels p = panelList[cmbPanels.SelectedIndex];
                    float tw = p.textTexture.TextureWidth;
                    float th = p.textTexture.TextureHeight;
                    float zmin = 0.01f + (toolProgram.has_negative_z ? 0 : cSGPanel.Height);
                    foreach (Vector4 v in p.textTexture.PartsTex)
                    {
                        float x = v[0], y = v[1] , w = v[2], h = v[3];
                        float x1 = x, y1 = y - h,
                              x2 = x, y2 = y,
                              x3 = x + w, y3 = y,
                              x4 = x+w, y4 = y-h;
                        verts_ar.AddRange(new List<float>
                        {
                           x1/10f,y1/10f, zmin , 0,0,1, 1,1,1, x1 / tw,y1 / th,1,
                           x2/10f,y2/10f, zmin , 0,0,1, 1,1,1, x2 / tw,y2 / th,1,
                           x3/10f,y3/10f, zmin , 0,0,1, 1,1,1, x3 / tw,y3 / th,1,
                           x3/10f,y3/10f, zmin , 0,0,1, 1,1,1, x3 / tw,y3 / th,1,
                           x4/10f,y4/10f, zmin , 0,0,1, 1,1,1, x4 / tw,y4 / th,1,
                           x1/10f,y1/10f, zmin , 0,0,1, 1,1,1, x1 / tw,y1 / th,1
                        });
                    }
                }
            }

            _vertices = verts_ar.ToArray();
            float[] _vertices_null = null;
            GL.BufferData<float>(BufferTarget.ArrayBuffer, new IntPtr(sizeof(float) * _vertices.Length), _vertices_null, BufferUsageHint.DynamicDraw);
            GL.BufferSubData<float>(BufferTarget.ArrayBuffer, IntPtr.Zero, sizeof(float) * _vertices.Length, _vertices);
            
            GL.BindBuffer(BufferTarget.ArrayBuffer, _glbuf);
            last_buffer_size = _vertices.Length;
            //refill_VBO.Set();
        }
        public void RenderMesh(bool onlyShadows = false)
        {
            if (_vertices == null || _vertices.Length == 0)
                return;
            // Render depth of scene to texture (from light's perspective)
            Matrix4 lightProjection, lightView;
            Matrix4 lightSpaceMatrix;
            lightProjection = Matrix4.CreateOrthographicOffCenter(
                -toolProgram.PanelSize.X / 10, toolProgram.PanelSize.X / 10,
                -toolProgram.PanelSize.Y / 10, toolProgram.PanelSize.Y / 10,
                1.0f, 1000.5f);
            lightView = Matrix4.LookAt(LightPos, new Vector3(toolProgram.PanelSize.X / 20, toolProgram.PanelSize.Y / 20, 0)
                // Vector3.Zero
                , OGLCamera.OZ);
            lightSpaceMatrix = lightProjection * lightView;
            Matrix4 _model = Matrix4.Identity;
            if (onlyShadows)                
            {
                // render scene from light's point of view 
                shadows_shader.Use();            

                GL.Viewport(glControl1.ClientRectangle.X, glControl1.ClientRectangle.Y, SHADOW_WIDTH, SHADOW_HEIGHT);                              

                GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
                GL.Clear(ClearBufferMask.DepthBufferBit);

                shadows_shader.SetUniformMatrix4("model", _model);
                shadows_shader.SetUniformMatrix4("proj", lightProjection);
                shadows_shader.SetUniformMatrix4("view", lightView);

                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, depthMap);
            }
            else
            {
                GL.Viewport(glControl1.ClientRectangle.X, glControl1.ClientRectangle.Y, glControl1.ClientRectangle.Width, glControl1.ClientRectangle.Height);

                main_shader.Use();

                main_shader.SetUniformMatrix4("projection", glControl1.projection_);
                main_shader.SetUniformMatrix4("view", glControl1.oGLCamera.eyeMatrix);
                main_shader.SetUniformMatrix4("model", _model);
                main_shader.SetUniform3("viewPos", glControl1.oGLCamera.eyePos);
                main_shader.SetUniformMatrix4("light_proj", lightProjection);
                main_shader.SetUniformMatrix4("light_view", lightView);

                if (chkShowPartData.Checked) // show panel parts data
                {
                    if (cmbPanels.SelectedIndex >= 0)
                    {
                        GL.ActiveTexture(TextureUnit.Texture0);
                        GL.BindTexture(TextureTarget.Texture2D, panelList[cmbPanels.SelectedIndex].textTexture.FontTextureID);
                    }
                }
                if (PNLTexID > 0)
                {
                    GL.ActiveTexture(TextureUnit.Texture1);
                    GL.BindTexture(TextureTarget.Texture2D, PNLTexID);
                }
                if(showShadows)
                {
                    GL.ActiveTexture(TextureUnit.Texture2);
                    GL.BindTexture(TextureTarget.Texture2D, depthMap);
                }
            }
            
            
            GL.BindVertexArray(_vao);
            
            GL.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Length / 3);
            GL.BindVertexArray(0);

            DrawPartsBorders();

            if(onlyShadows)
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

            //float[] target = new float[SHADOW_WIDTH * SHADOW_HEIGHT ];
            //GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.DepthComponent, PixelType.Float, target);
            //System.Diagnostics.Debug.WriteLine(string.Format("Shadowed pixels: {0}", target.Where(x=>x>0 && x<1).Count()));
        }
        private void DrawPartsBorders()
        {
            if (chkShowPartData.Checked) // show panel parts data
            {
                if (cmbPanels.SelectedIndex >= 0) // To DO: Use Vertex array?
                {
                    GL.Begin(PrimitiveType.Lines);

                    Color col = Color.Black;
                    float zmin = 0.01f + (toolProgram.has_negative_z ? 0 : cSGPanel.Height);
                    for (var i = 0; i < panelList[cmbPanels.SelectedIndex].PartList.Count; i++)
                    {
                        NestedPartList part = panelList[cmbPanels.SelectedIndex].PartList[i];
                        GL.Color3(col);
                        GL.Vertex3(new Vector3d(part.X / 10, part.Y / 10, zmin) );
                        GL.Vertex3(new Vector3d((part.X + part.Width) / 10, part.Y / 10, zmin));

                        GL.Color3(col);
                        GL.Vertex3(new Vector3d((part.X + part.Width) / 10, part.Y / 10, zmin));
                        GL.Vertex3(new Vector3d((part.X + part.Width) / 10, (part.Y + part.Height) / 10, zmin) );

                        GL.Color3(col);
                        GL.Vertex3(new Vector3d((part.X + part.Width) / 10, (part.Y + part.Height) / 10, zmin));
                        GL.Vertex3(new Vector3d(part.X / 10, (part.Y + part.Height) / 10, zmin));

                        GL.Color3(col);
                        GL.Vertex3(new Vector3d(part.X / 10, (part.Y + part.Height) / 10, zmin));
                        GL.Vertex3(new Vector3d(part.X / 10, part.Y / 10, zmin));
                    }
                    GL.End();
                }
            }
        }
        public static Vector3 GetNormal(Net3dBool.Vector3 v1, Net3dBool.Vector3 v2, Net3dBool.Vector3 v3)
        {
            Net3dBool.Vector3 firstvec = v2 - v1;
            Net3dBool.Vector3 secondvec = v1 - v3;
            Net3dBool.Vector3 normal = Net3dBool.Vector3.Cross(firstvec, secondvec);
            normal.Normalize();
            return new Vector3((float)normal.X, (float)normal.Y, (float)normal.Z);
        }
        private void bnStop_Click(object sender, EventArgs e)
        {
            if (BGDW.IsBusy)
                BGDW.CancelAsync();

            while (BGDW.IsBusy)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
            }
            FreeVao();
            InitVao();
            DrawPanel(false);
            SetCurLine(toolProgram.steps[0].line_number);
        }

        private void bnPause_Click(object sender, EventArgs e)
        {
            if(!pause_thread)
            {
                bnPause.Text = "Continue";
                bnPlayM6.Enabled = true;
                bnStep.Enabled = true;
            }
            else
            {
                if(sender!=bnStep)
                    bnPause.Text = "Pause";
                bnPlayM6.Enabled = false;
                bnStep.Enabled = false;
                if (sender == bnPause)
                {
                    PlayM6 = false; // continue as usual
                    PlayStep = false;
                }
            }
            
            pause_thread = !pause_thread;                       

            bnPause.Invalidate();
            bnPause.Refresh();
            bnPlayM6.Refresh();
            bnStep.Refresh();
        }

        private void bnPlayM6_Click(object sender, EventArgs e)
        {
            PlayStep = false;
            PlayM6 = true;
            if (!BGDW.IsBusy)
            {
                bnStart_Click(sender, e);//start new simulation
            }
            else
            {
                if(pause_thread)
                    bnPause_Click(sender, e);
            }
        }

        private void toolsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormTools fm = new FormTools();
                fm.ShowDialog();
            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occured in the Tools form:"  + Environment.NewLine + ex.Message);
            }
            glControl1.MakeCurrent();
        }

        private void DemoForm_Load(object sender, EventArgs e)
        {
            nudSpeed.Value = (decimal)(OPTCAMSim.Properties.Settings.Default.StepLength * 3.0 > 100 ? 100 : OPTCAMSim.Properties.Settings.Default.StepLength * 3.0);
            this.dvCode.AutoGenerateColumns = false;

            //This code not needed - use OptCam file directly
            //Process UDT export files from OptCam
            //try
            //{
            //    foreach (string file in new DirectoryInfo(Application.StartupPath + "\\UDT\\").GetFiles("*.txt").Select(x => x.FullName).ToList())
            //    {
            //        if(File.ReadAllText(file).StartsWith("TL#")) // check if this is an OptCam export file
            //        {
            //            string tool_line = Tool.ParseOptCam(file);
            //            System.Diagnostics.Debug.WriteLine(tool_line);
            //        }
            //    }
            //}
            //catch(Exception ex)
            //{

            //}
        }

        private void DemoForm_Shown(object sender, EventArgs e)
        {
            RestoreSettings();
            this.Refresh();

            if (OPTCAMSim.Properties.Settings.Default.PanelMode)
                loadNestedPanels();
            else
            {
                if (File.Exists(Application.StartupPath + "\\Sim.txt"))
                {
                    InitToolProgram(ref toolProgram, Application.StartupPath + "\\Sim.txt");
                }
                else
                    LoadProgramToolStripMenuItem_Click(sender, e);
            }
            
            speed_changed = false;
            size_changed = false;
            SetTimer();

            
        }
        private void RestoreSettings()
        {
            if (OPTCAMSim.Properties.Settings.Default.MainFormMaximized)
            {
                WindowState = FormWindowState.Maximized;
                Location = OPTCAMSim.Properties.Settings.Default.MainFormLocation;
                Size = OPTCAMSim.Properties.Settings.Default.MainFormSize;
            }
            else if (OPTCAMSim.Properties.Settings.Default.MainFormMinimized)
            {
                WindowState = FormWindowState.Minimized;
                Location = OPTCAMSim.Properties.Settings.Default.MainFormLocation;
                Size = OPTCAMSim.Properties.Settings.Default.MainFormSize;
            }
            else
            {
                Location = OPTCAMSim.Properties.Settings.Default.MainFormLocation;
                Size = OPTCAMSim.Properties.Settings.Default.MainFormSize;
            }

            //splitter position
            splitCodeOGC.SplitterDistance = OPTCAMSim.Properties.Settings.Default.CodeSplitPos;
            splitCodeOGC.Panel1Collapsed = !OPTCAMSim.Properties.Settings.Default.ShowGCode;

            //Size of the panel
            if (OPTCAMSim.Properties.Settings.Default.UsePanelSizes)
            {
                nudWidth.Value = (decimal)OPTCAMSim.Properties.Settings.Default.PanelX;
                nudLength.Value = (decimal)OPTCAMSim.Properties.Settings.Default.PanelY;
                nudHeight.Value = (decimal)OPTCAMSim.Properties.Settings.Default.PanelZ;
            }
            else
            {
                nudWidth.Value = 2100;
                nudLength.Value = 2800;
                nudHeight.Value = 18;
                nudWidth.Enabled = nudLength.Enabled = nudHeight.Enabled = false;
            }
            pnlNestedPanels.Visible = OPTCAMSim.Properties.Settings.Default.PanelMode;
        }
        private void StoreSettings()
        {
            if (WindowState == FormWindowState.Maximized)
            {
                OPTCAMSim.Properties.Settings.Default.MainFormLocation = RestoreBounds.Location;
                OPTCAMSim.Properties.Settings.Default.MainFormSize = RestoreBounds.Size;
                OPTCAMSim.Properties.Settings.Default.MainFormMaximized = true;
                OPTCAMSim.Properties.Settings.Default.MainFormMinimized = false;
            }
            else if (WindowState == FormWindowState.Normal)
            {
                OPTCAMSim.Properties.Settings.Default.MainFormLocation = Location;
                OPTCAMSim.Properties.Settings.Default.MainFormSize = Size;
                OPTCAMSim.Properties.Settings.Default.MainFormMaximized = false;
                OPTCAMSim.Properties.Settings.Default.MainFormMinimized = false;
            }
            else
            {
                OPTCAMSim.Properties.Settings.Default.MainFormLocation = RestoreBounds.Location;
                OPTCAMSim.Properties.Settings.Default.MainFormSize = RestoreBounds.Size;
                OPTCAMSim.Properties.Settings.Default.MainFormMaximized = false;
                OPTCAMSim.Properties.Settings.Default.MainFormMinimized = true;
            }
            //splitter position
            OPTCAMSim.Properties.Settings.Default.CodeSplitPos = splitCodeOGC.SplitterDistance;
            OPTCAMSim.Properties.Settings.Default.Save();
        }

        private void bnStep_Click(object sender, EventArgs e)
        {
            PlayM6 = false;
            PlayStep = true;
            if (!BGDW.IsBusy)
            {
                bnStart_Click(sender, e);//start new simulation
            }
            else
            {
                if (pause_thread)
                    bnPause_Click(sender, e);
            }
        }

        private void tbToolSpeed_Scroll(object sender, EventArgs e)
        {
            nudSpeed.Value = tbToolSpeed.Value;
            if (clicked_speed)
                return;            
            speed_changed = true;
        }

        private void nudSpeed_ValueChanged(object sender, EventArgs e)
        {
            tbToolSpeed.Value = (int)nudSpeed.Value;
            if (clicked_speed)
                return;
            speed_changed = true;
        }

        #region code display methods
        public void ReloadCodeView(DataTable all_sim_lines)
        {
            try
            {
                prev_index = -1;               
                dvCode.DataSource = all_sim_lines;                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load the code to display!" + Environment.NewLine + ex.Message,
                   "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        public void SetCurLine(int line_num)
        {
            if (dvCode.Rows.Count >= line_num && line_num > 0)
            {
                dvCode.Rows[line_num - 1].Cells[0].Selected = true;
                //dvCode.FirstDisplayedScrollingRowIndex = line_num - 1;

                
                prev_index = line_num - 1;

            }
        }       

        private void nudWidth_ValueChanged(object sender, EventArgs e)
        {
            if (OPTCAMSim.Properties.Settings.Default.UsePanelSizes)
            {
                size_changed = true;
                OPTCAMSim.Properties.Settings.Default.PanelX = (double)nudWidth.Value;
                OPTCAMSim.Properties.Settings.Default.Save();
            }
        }

        private void nudLength_ValueChanged(object sender, EventArgs e)
        {
            if (OPTCAMSim.Properties.Settings.Default.UsePanelSizes)
            {
                size_changed = true;
                OPTCAMSim.Properties.Settings.Default.PanelY = (double)nudLength.Value;
                OPTCAMSim.Properties.Settings.Default.Save();
            }
        }

        private void nudHeight_ValueChanged(object sender, EventArgs e)
        {
            if (OPTCAMSim.Properties.Settings.Default.UsePanelSizes)
            {
                size_changed = true;
                OPTCAMSim.Properties.Settings.Default.PanelZ = (double)nudHeight.Value;
                OPTCAMSim.Properties.Settings.Default.Save();
            }
        }

        private void settingsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OPTCAMSim.FormSettings fm = new OPTCAMSim.FormSettings();
            fm.ShowDialog();
            //show/hide panel immidiately 
            splitCodeOGC.Panel1Collapsed = !OPTCAMSim.Properties.Settings.Default.ShowGCode;
            //update panel sizes
            if (OPTCAMSim.Properties.Settings.Default.UsePanelSizes)
            {
                nudWidth.Value = (decimal)OPTCAMSim.Properties.Settings.Default.PanelX;
                nudLength.Value = (decimal)OPTCAMSim.Properties.Settings.Default.PanelY;
                nudHeight.Value = (decimal)OPTCAMSim.Properties.Settings.Default.PanelZ;
                nudWidth.Enabled = nudLength.Enabled = nudHeight.Enabled = true;
            }
            else
            {
                nudWidth.Value = 2100;
                nudLength.Value = 2800;
                nudHeight.Value = 18;
                nudWidth.Enabled = nudLength.Enabled = nudHeight.Enabled = false;
            }
            //pnlNestedPanels.Visible = OPTCAMSim.Properties.Settings.Default.PanelMode;
            //if (OPTCAMSim.Properties.Settings.Default.PanelMode)
            //    loadNestedPanels();
        }

        private void loadNestedPanels(string folder_path="")
        {                         
            try
            {
                if (string.IsNullOrWhiteSpace(folder_path))
                    folder_path = OPTCAMSim.Properties.Settings.Default.PanelsDataPath + "\\ExportCoords\\";
                folder_path = Directory.GetDirectories(folder_path).First();
                panelList = GeneratePanelData.LoadJson(folder_path);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Failed to read panel data from folder!" + Environment.NewLine + ex.Message,
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if(panelList==null||panelList.Count==0)
            {
                MessageBox.Show("Panel list is empty!","Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            cmbPanels.Items.AddRange(panelList.Select(x => x.Name).ToArray());
            cmbPanels.SelectedIndex = 0;
        }

        private void cmbPanels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!BGDW.IsBusy)
            {
                if (cmbPanels.SelectedIndex >= 0)
                {                  
                    NestedPanels p = panelList[cmbPanels.SelectedIndex];
                    if(p.toolProgram==null)
                    {
                        try
                        {
                            string simpath = Directory.GetFiles(Directory.GetDirectories(
                                OPTCAMSim.Properties.Settings.Default.PanelsDataPath + "\\ExportSims\\").First(), p.Name + ".*").First();
                            InitToolProgram(ref p.toolProgram, simpath);
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show("Failed to read panel sim-file from folder!" + Environment.NewLine + ex.Message,
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    else
                    {
                        if (toolProgram != p.toolProgram)
                        {
                            toolProgram = p.toolProgram;
                            UpdateFormAfterToolProgramInit();
                            redraw_panel = true;
                        }
                    }
                }
                RefillVBO();
                glControl1.Invalidate();
            }
        }

        private void chkShowPartData_CheckedChanged(object sender, EventArgs e)
        {
            if (!BGDW.IsBusy)
            {
                RefillVBO();
                glControl1.Invalidate();
            }
        }

        private void bnTopView_Click(object sender, EventArgs e)
        {
            try
            {
                glControl1.SetTopView();
            }
            catch { }
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            GL.ClearColor(Color.SkyBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.Normalize);

            glControl1.projection_ = Matrix4.CreatePerspectiveFieldOfView((float)(80 * Math.PI / 180), (float)glControl1.Width / (float)glControl1.Height, 1, 1000);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref glControl1.projection_);

            glControl1.oGLCamera = new OGLCamera();

            float[] mat_diffuse = { 1.0f, 0.0f, 0.0f };
            GL.Material(MaterialFace.FrontAndBack, MaterialParameter.AmbientAndDiffuse, mat_diffuse);

            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.DepthTest);
            //GL.Enable(EnableCap.CullFace);
            GL.ShadeModel(ShadingModel.Smooth);

            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            try
            {
                InitShaders();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to initialize shader program!" + Environment.NewLine +
                    "Please check OpenGL drivers version, minimal version is 4.2." + Environment.NewLine +
                    "Additional information:" + Environment.NewLine +
                    ex.Message, "Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error
                    );
                this.BeginInvoke(new MethodInvoker(this.Close));
                return;
            }

            InitVao();
            if (usePNLTex)
            {
                Bitmap bitmapTex = new Bitmap("PNLTexture.bmp");
                GL.GenTextures(1, out PNLTexID);
                GL.ActiveTexture(TextureUnit.Texture1);
                GL.BindTexture(TextureTarget.Texture2D, PNLTexID);

                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
                float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, borderColor);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

                System.Drawing.Imaging.BitmapData data = bitmapTex.LockBits(new Rectangle(0, 0, bitmapTex.Width, bitmapTex.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmapTex.Width, bitmapTex.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                bitmapTex.UnlockBits(data);
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            //Generate Shadows frame buffer and texture
            if (showShadows)
            {
                GL.GenFramebuffers(1, out depthMapFBO);
                //Generate shadow map texture
                GL.GenTextures(1, out depthMap);
                GL.ActiveTexture(TextureUnit.Texture2);
                GL.BindTexture(TextureTarget.Texture2D, depthMap);
                GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.DepthComponent,
                             SHADOW_WIDTH, SHADOW_HEIGHT, 0, PixelFormat.DepthComponent, PixelType.Float, IntPtr.Zero);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
                GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
                //attach shadow map texture to frame buffer
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, depthMapFBO);
                GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, depthMap, 0);
                GL.DrawBuffer(DrawBufferMode.None);
                GL.ReadBuffer(ReadBufferMode.None);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }
            glControl1.loaded = true;
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(glControl1.ClientRectangle.X, glControl1.ClientRectangle.Y, glControl1.ClientRectangle.Width, glControl1.ClientRectangle.Height);

            glControl1.projection_ = Matrix4.CreatePerspectiveFieldOfView((float)(80 * Math.PI / 180), (float)glControl1.Width / (float)glControl1.Height, 1, 1000);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref glControl1.projection_);

            GL.MatrixMode(MatrixMode.Modelview);
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl1.loaded)
                return;

            if (showShadows)
            {
                //GL.CullFace(CullFaceMode.Front);
                RenderMesh(true);
                //GL.CullFace(CullFaceMode.Back);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                RenderMesh();
            }
            else
            {
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
                RenderMesh();
            }

            glControl1.SwapBuffers();
        }

        public void Restart()
        {
            prev_index = -1;
        }
        #endregion
    }
    public class BusyForm : Form
    {        
        public BusyForm(string caption)
        {
            Width = 350;
            Height = 75;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            Text = caption;
            StartPosition = FormStartPosition.CenterScreen;
            
            ProgressBar progress = new ProgressBar() { Left = 1, Top = 1, Dock = DockStyle.Fill, Style = ProgressBarStyle.Marquee, MarqueeAnimationSpeed = 200 };

            this.Controls.Add(progress);

        }       
    }
}