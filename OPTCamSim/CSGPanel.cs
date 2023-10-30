using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DemoForm;
using Net3dBool;
using System.Drawing;

namespace OPTCAMSim
{
    public class CSGPanel
    {
        /// <summary>
        /// X-size of the panel
        /// </summary>
        public float Length = 0;
        /// <summary>
        /// Y-size of the panel
        /// </summary>
        public float Width = 0;
        /// <summary>
        /// Z-size of the panel
        /// </summary>
        public float Height = 0;
        /// <summary>
        /// full list of solids
        /// </summary>
        public List<Net3dBool.Solid> cubics = new List<Net3dBool.Solid> { };
        /// <summary>
        /// Not cutted part
        /// </summary>
        Net3dBool.Solid PanelObj;
        /// <summary>
        /// Heat map cutoff value: split cubic into smaller if humber of hits exceeds this value (relative to maximum)
        /// </summary>
        public double HM_CUTOFF = 0.8;
        private int cubx ;
        private int cuby;
        public CSGPanel(ToolProgram toolProgram)
        {
            Length = toolProgram.PanelSize.X / 10.0f;
            Width = toolProgram.PanelSize.Y / 10.0f;
            Height = toolProgram.PanelSize.Z / 10.0f;
            PreparePanel(toolProgram);
        }

        private void PreparePanel(ToolProgram toolProgram )
        {
            if (toolProgram.steps.Count > 0)
            {
                //prepare big Panel
                PanelObj = PreparePanel(Length, Width, Height, Vector3.Zero);
                PanelObj.RoundVerticesCoords();

                //prepare small Panel
                float extra_space = 10.0f;
                
                double sizex = toolProgram.maxXY.X - toolProgram.minXY.X + extra_space; // add a bit extra to count the tool width
                double sizey = toolProgram.maxXY.Y - toolProgram.minXY.Y + extra_space;

                if (sizex > Length)
                    sizex = Length;
                if (sizey > Width)
                    sizey = Width;

                if (toolProgram.minXY.X + sizex > Length) // in case extra was too large
                    sizex = Length - toolProgram.minXY.X;
                if (toolProgram.minXY.Y + sizey > Width) // in case extra was too large
                    sizey = Width - toolProgram.minXY.Y;

                //Adjust splitting for sim
                // Use maximum of setting and calc value
                int new_cubics = GetNewSplitting((float)sizex, (float)sizey, toolProgram);
                new_cubics = OPTCAMSim.Properties.Settings.Default.PanelSplitting > new_cubics ? OPTCAMSim.Properties.Settings.Default.PanelSplitting : new_cubics;

                int cubics_count = new_cubics;
                double wid = Width / cubics_count;
                double len = Length / cubics_count;


                float disp_x = (sizex < Length ? (toolProgram.minXY.X > extra_space / 2 ? extra_space / 2 : toolProgram.minXY.X / 2) : 0);
                float disp_y = (sizey < Width ? (toolProgram.minXY.Y > extra_space / 2 ? extra_space / 2 : toolProgram.minXY.Y / 2) : 0);
                Vector3 start = new Vector3(toolProgram.minXY.X + (float)sizex / 2f - disp_x,
                                            toolProgram.minXY.Y + (float)sizey / 2f - disp_y,
                                            (float)Height / 2.0f);

                Net3dBool.Solid smallPanel = PreparePanel(sizex, sizey, Height, start);
                smallPanel.RoundVerticesCoords();
                //make hole the size of cutted area
                var modeller = new Net3dBool.BooleanModeller(PanelObj, smallPanel);
                PanelObj = modeller.GetDifference();
                if (!PanelObj.IsEmpty)
                {
                    PanelObj.RoundVerticesCoords();
                    modeller = new Net3dBool.BooleanModeller(PanelObj, PanelObj);
                    PanelObj = modeller.GetUnion();
                    PanelObj.RoundVerticesCoords();
                    PanelObj.color = Color.Yellow;
                    PanelObj.Name = "PanelObj";

                    cubics.Add(PanelObj);
                }
                //Prepare cubics
                cubx = (int)Math.Ceiling(sizex / len);
                cuby = (int)Math.Ceiling(sizey / wid);
                double lastx = sizex - (cubx - 1) * len;
                double lasty = sizey - (cuby - 1) * wid;


                Vector3 start2 = new Vector3(toolProgram.minXY.X + (float)len / 2f - disp_x, toolProgram.minXY.Y + (float)wid / 2f - disp_y, (float)Height / 2.0f);
                Net3dBool.Solid cubeLarge = PreparePanel(len, wid, Height, start2);
                start2 = new Vector3(toolProgram.minXY.X + (float)lastx / 2f - disp_x, toolProgram.minXY.Y + (float)wid / 2f - disp_y, (float)Height / 2.0f);
                Net3dBool.Solid LastX = PreparePanel(lastx, wid, Height, start2);
                start2 = new Vector3(toolProgram.minXY.X + (float)len / 2f - disp_x, toolProgram.minXY.Y + (float)lasty / 2f - disp_y, (float)Height / 2.0f);
                Net3dBool.Solid LastY = PreparePanel(len, lasty, Height, start2);
                start2 = new Vector3(toolProgram.minXY.X + (float)lastx / 2f - disp_x, toolProgram.minXY.Y + (float)lasty / 2f - disp_y, (float)Height / 2.0f);
                Net3dBool.Solid LastXY = PreparePanel(lastx, lasty, Height, start2);

                Vector3[] clv = cubeLarge.GetVertices();
                int[] cli = cubeLarge.GetIndices();

                Vector3[] lyv = LastY.GetVertices();
                int[] lyi = LastY.GetIndices();

                Vector3[] lxv = LastX.GetVertices();
                int[] lxi = LastX.GetIndices();

                Vector3[] lxyv = LastXY.GetVertices();
                int[] lxyi = LastXY.GetIndices();

                for (int j = 0; j < cuby; j++)
                {
                    for (int k = 0; k < cubx; k++)
                    {
                        Net3dBool.Solid p = null;
                        if (j != cuby - 1 && k != cubx - 1)
                            p = new Net3dBool.Solid(clv,cli);
                        else
                        {
                            if (j == cuby - 1 && k != cubx - 1)
                                p = new Net3dBool.Solid(lyv, lyi);
                            else if (j != cuby - 1 && k == cubx - 1)
                                p = new Net3dBool.Solid(lxv, lxi);
                            else
                                p = new Net3dBool.Solid(lxyv, lxyi);
                        }
                        p.Translate(k * len, j * wid, 0);
                        p.RoundVerticesCoords();
                        p.color = Color.Yellow;
                        p.Name = "PanelObj";                        
                        cubics.Add(p);
                        cubics.Last().arr_index = cubics.Count - 1;
                    }
                }

                //Move Down
                if (toolProgram.has_negative_z)
                {
                    foreach (Net3dBool.Solid Mesh in cubics)
                        Mesh.Translate(0, 0, -Height);
                }

                try
                {
                    //Prepare Heat Map
                    HeatMap hm = new HeatMap(this, toolProgram);
                    //RenderHeatMap(ref hm);

                    ///spplit overloaded pixels
                    foreach (KeyValuePair<int, float> pix in hm.norm_heatPixels.Where(r => r.Value > HM_CUTOFF))
                    {
                        int times = 2;
                        if (hm.heatPixels[pix.Key] > 1000)
                            times = 3;
                        SplitPixel(pix.Key, times);
                    }
                }
                catch(Exception)
                { }
                //combine not required pixels
                //Dictionary<int, float> hml =  hm.norm_heatPixels.Where(r => r.Value == 0).ToDictionary(x=>x.Key, x=>x.Value);
                //foreach (KeyValuePair<int, float> pix in hml)
                //{
                //    CombinePixel(pix.Key, ref hml);
                //}

                // where do I need it???
                //cubics.AsParallel().ForAll(new Action<Solid>(s => s.arr_index = cubics.IndexOf(s)));
                
                //hm = new HeatMap(this, toolProgram);
                //RenderHeatMap(ref hm);
                //Random col = new Random();
                //foreach (Net3dBool.Solid Mesh in cubics)
                //    Mesh.color = Color.FromArgb(col.Next(0, 255), col.Next(0, 255), col.Next(0, 255));
            }
        }

        private void RenderHeatMap(ref HeatMap hm)
        {
            foreach (Net3dBool.Solid Mesh in cubics)
            {
                if (hm.norm_heatPixels.ContainsKey(Mesh.arr_index))
                {
                    float f = hm.norm_heatPixels[Mesh.arr_index];
                    if (f < 0)
                        continue;
                    Mesh.color = Color.FromArgb((int)(255 * f), (int)(255 * (1 - f)), 0);
                }
                else
                    Mesh.color = Color.Blue;
            }
        }
        public List<Net3dBool.Solid> GetCutPart()
        {
            return cubics.Where(r => r != PanelObj).ToList();
        }

        /// <summary>
        /// Create panel of given size and translate if needed
        /// </summary>
        /// <param name="length"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="translate"></param>
        /// <returns></returns>
        private Net3dBool.Solid PreparePanel(double length, double width, double height, Vector3 translate)
        {
            Net3dBool.Solid Panel = new Net3dBool.Solid(Net3dBool.DefaultCoordinates.DEFAULT_BOX_VERTICES, Net3dBool.DefaultCoordinates.DEFAULT_BOX_COORDINATES);
            Panel.Scale(length, width, height);
            if (translate != Vector3.Zero)
                Panel.Translate(translate.X, translate.Y, translate.Z);
            else
                Panel.Translate(length / 2.0, width / 2.0, height / 2.0);
            Panel.Name = "PanelObj";
            Panel.color = Color.Yellow;
            return Panel;
        }
        /// <summary>
        /// Splits panel into smaller parts
        /// </summary>
        /// <param name="array_index"></param>
        /// <param name="times"></param>
        /// <returns></returns>
        private void SplitPixel(int array_index, int times = 2)
        {
            Net3dBool.Solid pixel = cubics.Where(r => r.arr_index == array_index).First();
            Net3dBool.Bound b = new Net3dBool.Bound(pixel.GetVertices());
            cubics.Remove(pixel);
            double xsize = (b.XMax - b.XMin) / times;
            double ysize = (b.YMax - b.YMin) / times;
            double zsize = b.ZMax - b.ZMin;
            double Zmin = b.ZMin;
            Vector3 start = new Vector3(b.XMin + (float)xsize / 2f, b.YMin + (float)ysize / 2f, b.ZMin + (float)zsize / 2.0f);
            Net3dBool.Solid cube = PreparePanel(xsize, ysize, zsize, start);
            for (int j = 0; j < times; j++)
            {
                for (int k = 0; k < times; k++)
                {
                    Net3dBool.Solid p = null;
                    p = new Net3dBool.Solid(cube.GetVertices(), cube.GetIndices());                    
                    p.Translate(k * xsize, j * ysize, Zmin - (p.GetMean().Z - zsize/2));
                    //p.RoundVerticesCoords();
                    p.color = Color.Yellow;
                    p.Name = "PanelObj";
                    cubics.Add(p);                    
                }
            }
        }

        private void CombinePixel(int array_index, ref Dictionary<int, float>hml)
        {
            if(cubics.Where(r => r.arr_index == array_index).Count()>0)
            {
                Net3dBool.Solid pixel = cubics.Where(r => r.arr_index == array_index).First();
                Net3dBool.Bound b = new Net3dBool.Bound(pixel.GetVertices());
                int ind_y = array_index / cuby;
                int ind_x = array_index % cuby;
                //find all neighboring zero hits pixels
                bool UR4, DR4, UL4, DL4, U6, D6, R6, L6, ALL9, UR, R, L, U, D, DR, UL, DL, H3,V3;
                int Ri= ind_y + ind_x + 1, Li= ind_y + ind_x - 1
                    , Ui= array_index + cubx, Di= array_index - cubx,
                    URi= array_index + cubx + 1, DRi = array_index - cubx + 1, 
                    ULi= array_index + cubx - 1, DLi= array_index - cubx - 1;
                //Right
                R = hml.ContainsKey(Ri);
                //Left
                L = hml.ContainsKey(Li);
                //Up
                U = hml.ContainsKey(Ui);
                //Down
                D = hml.ContainsKey(Di);
                //Up-Right
                UR = hml.ContainsKey(URi);
                //Down-right
                DR = hml.ContainsKey(DRi);
                 //Up-left
                UL = hml.ContainsKey(ULi);
                //Down-left
                DL = hml.ContainsKey(DLi);
                UR4 = U && R && UR;
                DR4 = D && R && DR;
                UL4 = U && L && UL;
                DL4 = D && L && DL;
                //Go right

            }
        }
            
        /// <summary>
        /// Adjust splitting for sim file
        /// </summary>
        /// <param name="sizex"></param>
        /// <param name="sizey"></param>
        /// <returns></returns>
        private int GetNewSplitting(float sizex, float sizey, ToolProgram toolProgram)
        {
            int result = 50; // start with small number
            //OPTCAMSim.Properties.Settings.Default.PanelSplitting; 
            if (toolProgram != null && toolProgram.steps.Count > 0)
            {
                int ct = toolProgram.steps.Where(s => s.G != Gcode.QUICK_MOVE && s.G != Gcode.TOOL_CHANGE).Count();
                int ctlin = toolProgram.steps.Where(s => s.G == Gcode.LINEAR_MOVE).Count();
                int ct_circ = toolProgram.steps.Where(s => s.G == Gcode.COUNTER_CLOCKWISE_CURVE || s.G == Gcode.CLOCKWISE_CURVE).Count();
                if (ct == 0)
                    return result;
                int wct = ctlin + 3 * ct_circ; // circular steps has more weight as there are more vertices
                float area = sizex * sizey; // affected Panel area
                float wct_pa = wct / area; // cuts per area unit

                float wid = Width / result;
                float len = Length / result;
                float mean_ar = wid * len;
                float mean_num_cuts = wct_pa * mean_ar; // cuts per single cubic
                if (mean_num_cuts > 0.3)
                    while (mean_num_cuts > 0.25)
                    {
                        result = (int)Math.Ceiling(result * 1.5);
                        wid = Width / result;
                        len = Length / result;
                        mean_ar = wid * len;
                        mean_num_cuts = wct_pa * mean_ar; // cuts per single cubic
                    }

            }
            if (result != 50)
                result = (int)Math.Ceiling(1.1f * result);
            if (result > 350 && OPTCAMSim.Properties.Settings.Default.PanelSplitting != result)
                result = 350;
            return result;
        }
    }
}
