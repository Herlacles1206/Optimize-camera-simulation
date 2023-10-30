using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using OpenTK;
using System.Globalization;
using System.Windows.Forms;
using System.Data;

namespace OPTCAMSim
{
    /// <summary>
    /// Enumeration for different kinds of tool movements - G codes
    /// </summary>
    public enum Gcode
    {
        QUICK_MOVE= 0,
        LINEAR_MOVE = 1,
        CLOCKWISE_CURVE = 2,
        COUNTER_CLOCKWISE_CURVE = 3,
        TOOL_CHANGE = 4
    }
    /// <summary>
    /// Special actions during simulation
    /// </summary>
    public enum SpecialActionType
    {
        Stop = 1,
        Pause = 2
    };
    public class SpecialAction
    {
        /// <summary>
        /// Action type
        /// </summary>
        public SpecialActionType actType;
        /// <summary>
        /// Parameters list
        /// </summary>
        public Dictionary<string, double> pars = new Dictionary<string, double> { };
    }

    /// <summary>
    /// Single step of tool program
    /// </summary>
    public class ToolProgramStep
    {
        public Gcode G;
        /// <summary>
        /// Final point coords
        /// </summary>
        public Vector3 pos;
        /// <summary>
        /// Speed of movement in mm / min
        /// </summary>
        public double F;
        /// <summary>
        /// the distance from the start point to the center of the circle by the X axis - G02 / G03  only
        /// </summary>
        public float I;
        /// <summary>
        /// the distance from the start point to the center of the circle by the Y axis - G02 / G03  only
        /// </summary>
        public float J;
        /// <summary>
        /// the distance from the start point to the center of the circle by the Z axis - G02 / G03  only
        /// </summary>
        public float K;
        /// <summary>
        /// Start point coords
        /// </summary>
        public Vector3 start;
        // <summary>
        /// Curve center coords
        /// </summary>
        public Vector3 curve_center;

        /// <summary>
        /// Step
        /// </summary>
        public Vector3 step;
        // <summary>
        /// Curve radius
        /// </summary>
        public double R;
        /// <summary>
        /// Rotational angle in radians
        /// </summary>
        public double angle;
        /// <summary>
        /// Far point of rotation step
        /// </summary>
        public Vector3 far_point;
        public bool is_first = false;
        public bool is_last = false;
        /// <summary>
        /// Stores base program step index
        /// </summary>
        public int base_step_index;
        /// <summary>
        /// line number in sim-file
        /// </summary>
        public int line_number = -1;

        /// <summary>
        /// Tool for tool change
        /// </summary>
        public Tool tool;

        /// <summary>
        /// Special action during simulation
        /// </summary>
        public SpecialAction action;
    }
    /// <summary>
    /// Class for the tool movement program storing
    /// </summary>
    public class ToolProgram
    {
        /// <summary>
        /// All steps of the program
        /// </summary>
        public List<ToolProgramStep> steps = new List<ToolProgramStep> { };
        public List<ToolProgramStep> base_steps = new List<ToolProgramStep> { };
        /// <summary>
        /// Counter of steps
        /// </summary>
        public int steps_real;
        //Coords in file in mm, in Scene - in cm
        public double coord_multiplier = 10.0;
        public bool has_negative_z = false;
        /// <summary>
        /// Use to split steps in smaller steps
        /// </summary>
        public double max_step_length = OPTCAMSim.Properties.Settings.Default.StepLength;
        /// <summary>
        /// Tool path bound - minimum XY
        /// </summary>
        public Vector3 minXY = new Vector3(999, 999, 0);
        /// <summary>
        /// Tool path bound - maximum XY
        /// </summary>
        public Vector3 maxXY = new Vector3(-999, -999, 0);

        /// <summary>
        /// Plate size in mm
        /// </summary>
        public Vector3 PanelSize = new Vector3(2100, 2800, 18);
        public int pnl_line = -1;
        /// <summary>
        /// All code lines
        /// </summary>
        public DataTable all_sim_lines = new DataTable();
        public ToolProgram()
        {
            all_sim_lines.Columns.Add("Line");
            all_sim_lines.Columns.Add("Code");
        }
        /// <summary>
        /// Method to read simulation program from the .txt file
        /// </summary>
        /// <param name="filename">Path to the source file</param>
        /// <param name="err_count">number of errors during parsing</param>
        /// <returns>true if parsed OK</returns>
        public bool ParseProgramFile(string filename, out int err_count, out string errors)
        {
            bool result = true;
            errors = "";
            err_count = 0;
            CultureInfo c = CultureInfo.CurrentCulture;
            bool replace_dots = c.NumberFormat.NumberDecimalSeparator == ",";
            
            //List of known G-codes
            List<int> KnownGCodes = new List<int> { 0, 1, 2, 3 };
            //Possible G-codes string
            List<string> Gcodes = new List<string> { };
            List<string> GplaceHolders = new List<string> { "G" };
            if (Properties.Settings.Default.GcodeSyn != null && Properties.Settings.Default.GcodeSyn.Count > 0)
                GplaceHolders.AddRange(Properties.Settings.Default.GcodeSyn);
            //Add all combinations of G-placeholders and codes
            Gcodes.AddRange(GplaceHolders.SelectMany(x => KnownGCodes.Select(y => x + y)));
            Gcodes = Gcodes.Distinct().ToList();

            //1000 predefined tools
            List<string> preTools = new List<string> { };
            List<string> TplaceHolders = new List<string> { "T" };
            if (Properties.Settings.Default.TcodeSyn != null && Properties.Settings.Default.TcodeSyn.Count > 0)
                TplaceHolders.AddRange(Properties.Settings.Default.TcodeSyn);
            TplaceHolders = TplaceHolders.Distinct().ToList();
            preTools.AddRange(TplaceHolders.SelectMany(t=>Enumerable.Range(1, 1000).Select(x => t + x.ToString()).ToList()).ToList());
            //User - defined tools
            preTools.AddRange(new DirectoryInfo(Application.StartupPath + "//UDT//").GetFiles("*.txt").Select(x => x.Name.Replace(".txt", "")).ToList());
            preTools = preTools.Distinct().ToList();

            //sim line values separators
            char[] SimSep = new char[] { ' ', '\t' };

            //Special actions codes
            Dictionary<string, int> SpecialCodes = new Dictionary<string, int> { };
            if (!string.IsNullOrWhiteSpace(Properties.Settings.Default.SpecialCodes))
            {
                SpecialCodes = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(Properties.Settings.Default.SpecialCodes);                
            }

            try
            {               
                if (filename.EndsWith(".ngc", StringComparison.InvariantCultureIgnoreCase)) // for .ngc coordinates are in cm
                    coord_multiplier = 1.0;
                if (OPTCAMSim.Properties.Settings.Default.UsePanelSizes)
                    PanelSize = new Vector3((float)OPTCAMSim.Properties.Settings.Default.PanelX,
                        (float)OPTCAMSim.Properties.Settings.Default.PanelY,
                        (float)OPTCAMSim.Properties.Settings.Default.PanelZ);

                //Check general errors at first
                string all_data = File.ReadAllText(filename);
                if(string.IsNullOrWhiteSpace(all_data))
                {
                    throw new Exception("Sim file is empty!");
                }                
                if (Gcodes.Max(x=> c.CompareInfo.IndexOf(all_data, x, CompareOptions.IgnoreCase)) < 0)
                {
                    throw new Exception("Tool steps info not found in sim file!");
                }
                
                using (var reader = new StreamReader(filename))
                {
                    int current_line = 0;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        current_line++;
                        all_sim_lines.Rows.Add(current_line.ToString(), line.Replace("\t", " "));
                        
                        if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
                            continue;
                        List<string> values = line.Split(SimSep, StringSplitOptions.RemoveEmptyEntries).ToList();

                        //read panel size
                        if(values[0]=="PNL")
                        {
                            double x=0,y=0,z=0;
                            try
                            {
                                if (    ParseDoubleCultureSpecific(values[1], replace_dots, ref x)
                                        && ParseDoubleCultureSpecific(values[2], replace_dots, ref y)
                                        && ParseDoubleCultureSpecific(values[3], replace_dots, ref z)
                                    )
                                    PanelSize = new Vector3((float)x, (float)y, (float)z);
                            }
                            catch (Exception) // unknown code
                            {
                                continue;
                            }
                            pnl_line = current_line;
                            continue;
                        }

                        ToolProgramStep step = new ToolProgramStep { pos = new Vector3(0, 0, 0) };
                        step.line_number = current_line;
                        // by default each coord is the same as for the last step
                        if (this.steps != null && this.steps.Count > 0)
                        {
                            step.pos.X = this.steps.Last().pos.X;
                            step.pos.Y = this.steps.Last().pos.Y;
                            step.pos.Z = this.steps.Last().pos.Z;
                            step.tool = steps.Last().tool;
                        }
                        
                        //read tool params
                        if (/*values[0] == "TLN" ||*/values.Intersect(preTools).Any() 
                            || preTools.Where(x => line.IndexOf(x) >= 0).Any())
                        {
                            try
                            {
                                List<string> rel_t = preTools.Where(x => line.IndexOf(x) >= 0).ToList();
                                if (rel_t.SelectMany(x => SimSep.Select(s => s + x)).Where(x => line.IndexOf(x) >= 0).Count() > 0)
                                {
                                    //Select only these tools, which are found in line stating with separator, exclude entries like 'START01'
                                    rel_t = rel_t.SelectMany(x => SimSep.Select(s => s + x)).Where(x => line.IndexOf(x) >= 0).Select(x=>x.Substring(1)).ToList();
                                    step.G = Gcode.TOOL_CHANGE;

                                    string typeString = values.Intersect(preTools).Any() ?
                                        values.Intersect(preTools).First() :
                                        (rel_t.Any() ? rel_t.First() : values[1]);
                                    /*if (values.Count > 2)
                                        typeString = line.Substring(line.IndexOf(typeString));
                                        */
                                    if (rel_t.Count() > 1) // there is a space in tool name
                                    {

                                        if (rel_t.Count > 1)
                                        {
                                            int maxl = rel_t.Select(x => x.Length).Max();
                                            typeString = rel_t.Where(x => x.Length == maxl).First();
                                        }
                                    }

                                    //if there is relevant T-code synonim - replace it with single "T"
                                    if (TplaceHolders.Where(x => typeString.StartsWith(x) && x != "T").Any())
                                    {
                                        List<string> rel_ts = TplaceHolders.Where(x => typeString.StartsWith(x) && x != "T").ToList();
                                        string G = rel_ts[0];
                                        if (rel_ts.Count > 1)
                                        {
                                            int maxl = rel_ts.Select(x => x.Length).Max();
                                            G = rel_ts.Where(x => x.Length == maxl).First();
                                        }
                                        typeString = "T" + typeString.Substring(G.Length);
                                    }

                                    string err = "";
                                    step.tool = Tool.GetToolFromFile(Application.StartupPath + "\\UDT\\" + typeString + ".txt", ref err);
                                    if(err.Length>0 || step.tool==null)
                                        throw new Exception(err);
                                    step.step = step.pos; // new tool createdin (0,0,0)
                                    step.base_step_index = base_steps.Count;
                                    //Add step to the toolpath
                                    this.steps.Add(step);
                                    this.base_steps.Add(step);
                                    continue;
                                }
                            }
                            catch (Exception ex)  
                            {
                                err_count++;
                                AddErrors(ref errors, "Error in the line: " + line + Environment.NewLine  + ex.Message);                                
                                return false;
                            }                            
                        }

                        //Check for special codes
                        bool has_special_code = false;
                        foreach(KeyValuePair<string,int> sc in SpecialCodes)
                        {
                            if((SpecialActionType)sc.Value == SpecialActionType.Stop) 
                            {
                                if (values.Contains(sc.Key))
                                {
                                    step.G = Gcode.QUICK_MOVE;
                                    step.action = new SpecialAction { actType = SpecialActionType.Stop };
                                    step.base_step_index = base_steps.Count;
                                    //Add step to the toolpath
                                    this.steps.Add(step);
                                    this.base_steps.Add(step);
                                    has_special_code = true;
                                    break;
                                }
                            }
                            else if((SpecialActionType)sc.Value == SpecialActionType.Pause)
                            {
                                double q = 0;
                                if(values.Where(x=>x.StartsWith(sc.Key)
                                    && ParseDoubleCultureSpecific(x.Substring(sc.Key.Length),replace_dots,ref q )).Any())
                                {
                                    List<string> pauses = values.Where(x => x.StartsWith(sc.Key)
                                    && ParseDoubleCultureSpecific(x.Substring(sc.Key.Length), replace_dots, ref q)).ToList();
                                    foreach(string p in pauses)
                                    {
                                        double t = 0;
                                        ParseDoubleCultureSpecific(p.Substring(sc.Key.Length), replace_dots, ref t);
                                        step.G = Gcode.QUICK_MOVE;
                                        step.action = new SpecialAction { actType = SpecialActionType.Pause,
                                            pars = new Dictionary<string, double> { ["t"] = t } };
                                        step.base_step_index = base_steps.Count;
                                        //Add step to the toolpath
                                        this.steps.Add(step);
                                        this.base_steps.Add(step);                                        
                                    }
                                    has_special_code = true;
                                    break;
                                }
                            }
                        }
                        if (has_special_code)
                            continue;

                        //Parse coords
                        if (!values.Intersect(Gcodes).Any()) // if known G codes not found - skip useless line
                            continue;
                        bool skip_line = false;
                        for (int j = 0; j < values.Count; j++)
                        {
                            string val = values[j];

                            //if there is relevant G-code synonim - replace it with single "G"
                            if(Gcodes.Where(x=>val.StartsWith(x) && val!=x && x!="G").Any() )
                            {
                                List<string> rel_g = Gcodes.Where(x => val.StartsWith(x) && val != x && x != "G").ToList();
                                string G = rel_g[0];
                                if (rel_g.Count > 1)
                                {
                                    int maxl = rel_g.Select(x => x.Length).Max();
                                    G = rel_g.Where(x => x.Length == maxl).First();
                                }
                                val = "G" + val.Substring(G.Length);
                            }
                            string key = new string(val.TakeWhile(q => char.IsLetter(q)).ToArray()); //key value representing variable
                            string number = key.Length > 0 ? new string(val.Substring(key.Length).  
                                TakeWhile(q => char.IsDigit(q) || q=='.' || q==',' || q == '-').ToArray()) : ""; // numeric value of the variable
                            switch (key)
                            {
                                case "N": break; // line  number;
                                case "G":                                    
                                    int g = 0;
                                    if (Int32.TryParse(number, out g))
                                    {
                                        try
                                        {
                                            if (g > 3) // in case of unknown G - skip
                                            {
                                                skip_line = true;
                                                break;
                                            }
                                            step.G = (Gcode)g;                                            
                                        }
                                        catch(Exception) // unknown code
                                        {
                                            skip_line = true;
                                        }
                                    }
                                    else
                                    {
                                        skip_line = true;                                        
                                    }
                                    break;
                                case "X":
                                    double x = 0;                                    
                                    if (ParseDoubleCultureSpecific(number, replace_dots, ref x))
                                    {
                                        step.pos.X = (float)(x / coord_multiplier);
                                        if (step.pos.X < 0)
                                            step.pos.X = 0;
                                    }
                                    else                                    
                                    {
                                        skip_line = true;
                                    }
                                    break;
                                case "Y":
                                    double y = 0;
                                                                        
                                    if (ParseDoubleCultureSpecific(number, replace_dots, ref y))
                                    {
                                        step.pos.Y = (float)(y / coord_multiplier);
                                        if (step.pos.Y < 0)
                                            step.pos.Y = 0;
                                    }
                                    else
                                    {
                                        skip_line = true;
                                    }
                                    break;
                                case "Z":
                                    double z = 0;
                                    
                                    if (ParseDoubleCultureSpecific(number, replace_dots, ref z))
                                    {
                                        step.pos.Z = (float)(z / coord_multiplier);
                                        if (step.pos.Z < 0)
                                            has_negative_z = true;
                                    }
                                    else
                                    {
                                        skip_line = true;
                                    }
                                    break;
                                case "I":
                                    if(step.G!=Gcode.CLOCKWISE_CURVE && step.G != Gcode.COUNTER_CLOCKWISE_CURVE  )
                                    {
                                        skip_line = true;
                                        break;
                                    }
                                    
                                    double i = 0;
                                    if (ParseDoubleCultureSpecific(number, replace_dots, ref i))
                                    {
                                        step.I = (float)(i / coord_multiplier);
                                    }
                                    else
                                    {
                                        skip_line = true;
                                    }
                                    break;
                                case "J":
                                    if (step.G != Gcode.CLOCKWISE_CURVE && step.G != Gcode.COUNTER_CLOCKWISE_CURVE)
                                    {
                                        skip_line = true;
                                        break;
                                    }
                                   
                                    double jj = 0;
                                    if (ParseDoubleCultureSpecific(number, replace_dots, ref jj))
                                    {
                                        step.J = (float)(jj / coord_multiplier);
                                    }
                                    else
                                    {
                                        skip_line = true;
                                    }
                                    break;
                                case "K":
                                    if (step.G != Gcode.CLOCKWISE_CURVE && step.G != Gcode.COUNTER_CLOCKWISE_CURVE)
                                    {
                                        skip_line = true;
                                        break;
                                    }
                                    
                                    double k = 0;
                                    if (ParseDoubleCultureSpecific(number, replace_dots, ref k))
                                    {
                                        step.K = (float)(k / coord_multiplier);
                                    }
                                    else
                                    {
                                        skip_line = true;
                                    }
                                    break;
                                case "R":
                                    if (step.G != Gcode.CLOCKWISE_CURVE && step.G != Gcode.COUNTER_CLOCKWISE_CURVE)
                                    {
                                        skip_line = true;
                                        break;
                                    }

                                    double rr = 0;
                                    if (ParseDoubleCultureSpecific(number, replace_dots, ref rr))
                                    {
                                        step.R = (float)(rr / coord_multiplier);
                                    }
                                    else
                                    {
                                        skip_line = true;
                                    }
                                    break;
                                case "F": // Not used                                    
                                    break;
                                case "FD":
                                    break;
                                default:
                                    skip_line = true;                                   
                                    break;
                            }
                            if (skip_line)
                                break;
                        }

                        if (skip_line)
                            continue;
                        step.base_step_index = base_steps.Count;

                        //Check step Coords
                        if (step.G == Gcode.LINEAR_MOVE || step.G == Gcode.CLOCKWISE_CURVE || step.G == Gcode.COUNTER_CLOCKWISE_CURVE)
                        {
                            //if cut is outside the panel - throw error
                            if (step.pos.X < 0 || step.pos.Y < 0 || step.pos.X > PanelSize.X/10f || step.pos.Y > PanelSize.Y / 10f)
                            {
                                throw new Exception(@" 
=====================
    W R O N G  C O O R D I N A T E S
=====================
line: "+ current_line.ToString() + @"
text: "+ line + @"

Cutting outside the panel is PROHIBITED.
Cannot continue parsing the sim file.
"
                                    );
                            }
                        }
                        //Add step to the toolpath
                        this.steps.Add(step);
                        this.base_steps.Add(step);
                    }
                }
                PrepareVectorsAndCurves();
                steps_real = base_steps.Count();
                SplitStepsInSmallerSteps(max_step_length);
                CombineStepsIntoLargerSteps(OPTCAMSim.Properties.Settings.Default.SmallSteps);
            }
            catch (Exception ex)
            {
                AddErrors(ref errors, "Error: " + ex.Message);
                err_count++;
                result = false;
            }
            return result;
        }
        public static void AddErrors(ref string err, string new_err)
        {
            if (err.Length < 1000)
                err += Environment.NewLine + new_err;
        }
        public static bool ParseDoubleCultureSpecific(string val, bool replace_dots, ref double d)
        {
            bool res = false;
            double try_d = 0;
            if(replace_dots)
                res = double.TryParse(val.Replace(".", ","), out try_d);
            else
                res = double.TryParse(val, out try_d);
            if (res)
            {
                d = try_d;
            }

            return res;
        }
       
        /// <summary>
        /// Returns vector for the tool step start point
        /// </summary>
        /// <param name="step_index"></param>
        /// <returns></returns>
        public Vector3 GetToolStepStartVector(int step_index)
        {
            float x = 0, y = 0, z = 0;
            if (step_index > 0)
            {
                if (this.steps != null && this.steps.Count > step_index)
                {
                    return steps[step_index - 1].pos;
                }
            }
            return new Vector3(x, y, z);
        }
        /// <summary>
        /// Returns vector of the curve center distance from the start point for G02, G03 
        /// </summary>
        /// <param name="step_index"></param>
        /// <returns></returns>
        public Vector3 GetCurveCenterDistVector(int step_index)
        {
            float x = 0, y = 0, z = 0;
            if (this.steps != null && this.steps.Count > step_index)
            {
                x = steps[step_index].I;
                y = steps[step_index].J;
                z = 0;// steps[step_index].K; K - not used yet
            }
            return new Vector3(x, y, z);
        }
        /// <summary>
        /// Returns vector of the curve center for G02, G03 
        /// </summary>
        /// <param name="step_index"></param>
        /// <returns></returns>
        public Vector3 GetCurveCenterVector(int step_index)
        {
            float x = 0, y = 0, z = 0;
            if (this.steps != null && this.steps.Count > step_index)
            {
                Vector3 start = steps[step_index].start;
                Vector3 dist = GetCurveCenterDistVector(step_index);
                return start+ dist;
            }
            return new Vector3(x, y, z);
        }
        /// <summary>
        /// Returns curve radius for G02, G03 
        /// </summary>
        /// <param name="step_index"></param>
        /// <returns></returns>
        public double GetCurveRadius(int step_index)
        {
            double r = 0;
            if (this.steps != null && this.steps.Count > step_index)
            {
                Vector3 end = steps[step_index].pos.Z != steps[step_index].start.Z ? 
                    new Vector3(steps[step_index].pos.X, steps[step_index].pos.Y, steps[step_index].start.Z) :
                    steps[step_index].pos
                    ;
                Vector3 center = GetCurveCenterVector(step_index);
                return (end- center).Length;
            }
            return r;
        }
        /// <summary>
        /// Get curve I and J by R 
        /// https://math.stackexchange.com/questions/27535/how-to-find-center-of-an-arc-given-start-point-end-point-radius-and-arc-direc
        /// </summary>
        /// <param name="step_index"></param>
        /// <returns></returns>
        public void GetCurveIJbyR(int step_index)
        {            
            if (this.steps != null && this.steps.Count > step_index)
            {
                Vector3 end = steps[step_index].pos.Z != steps[step_index].start.Z ?
                    new Vector3(steps[step_index].pos.X, steps[step_index].pos.Y, steps[step_index].start.Z) :
                    steps[step_index].pos
                    ;
                Vector3 start = steps[step_index].start;
                double d = (end-start).Length;
                double h = Math.Sqrt(steps[step_index].R * steps[step_index].R - d * d / 4);
                double e = steps[step_index].G == Gcode.COUNTER_CLOCKWISE_CURVE ? 1 : -1;
                if (d != 0)
                {
                    steps[step_index].I = (float)((end.X - start.X) / 2 - e * h * (end.Y - start.Y) / d);
                    steps[step_index].J = (float)((end.Y - start.Y) / 2 + e * h * (end.X - start.X) / d);
                }
            } 
        }
        /// <summary>
        /// Prepares start positions and curves params
        /// </summary>
        private void PrepareVectorsAndCurves()
        {
            if (this.steps != null && this.steps.Count > 0)
            {
                for (int step_index = 0; step_index < steps.Count; step_index++)
                {                    
                    steps[step_index].start = GetToolStepStartVector(step_index);
                    steps[step_index].step = steps[step_index].pos- steps[step_index].start;

                    if (steps[step_index].G == Gcode.CLOCKWISE_CURVE || steps[step_index].G == Gcode.COUNTER_CLOCKWISE_CURVE)
                    {
                        //if step defined by R - code - find IJ
                        if(steps[step_index].R!=0 && 
                            steps[step_index].I== steps[step_index].J && steps[step_index].J==steps[step_index].K && steps[step_index].K== 0)
                        {
                            GetCurveIJbyR(step_index);
                        }
                        int iter_count = 0;
                        do
                        {
                            steps[step_index].curve_center = GetCurveCenterVector(step_index);
                            if(steps[step_index].R==0 || iter_count>0)
                                steps[step_index].R = GetCurveRadius(step_index);
                            // rotational angle in radians
                            double angle = 2 * Math.Asin(new Vector3(steps[step_index].step.X, steps[step_index].step.Y, 0).Length / (2 * steps[step_index].R));
                            steps[step_index].angle = steps[step_index].G == Gcode.CLOCKWISE_CURVE ? -angle : angle;
                            steps[step_index].far_point = GetFarPoint(step_index);
                            iter_count++;
                        }
                        while (RecalcIJ(step_index) && iter_count<10);
                        
                    }
                    if (steps[step_index].G != Gcode.QUICK_MOVE && steps[step_index].G != Gcode.TOOL_CHANGE)
                    {
                        CheckVector(steps[step_index].start);
                        CheckVector(steps[step_index].pos);
                        if (steps[step_index].G == Gcode.CLOCKWISE_CURVE || steps[step_index].G == Gcode.COUNTER_CLOCKWISE_CURVE)
                        {
                            CheckVector(steps[step_index].far_point);
                        }
                    }                    
                }
                if (minXY.X < 0)
                    minXY.X = 0;
                if (minXY.Y < 0)
                    minXY.Y = 0;
               
            }
        }
        /// <summary>
        /// Recalculation of given I and J values - they may be incorrect due to rounding
        /// </summary>
        /// <param name="step_index"></param>
        /// <returns></returns>
        public bool RecalcIJ(int step_index)
        {
            bool result = false;
            ToolProgramStep step = steps[step_index];
                
            double x1, y1, x2, y2, R;
            x1 = step.start.X; y1 = step.start.Y; x2 = step.pos.X; y2 = step.pos.Y; 
            //Preserve initial radius
            R = step.R;
            double d, h, x01, x02, y01, y02, x0, y0;

            //Solve two equations of circle with 2 given dots and radius to find central point
            d = Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
            h = Math.Sqrt(R * R - (d / 2) * (d / 2));

            x01 = x1 + (x2 - x1) / 2 + h * (y2 - y1) / d;
            y01 = y1 + (y2 - y1) / 2 - h * (x2 - x1) / d;

            x02 = x1 + (x2 - x1) / 2 - h * (y2 - y1) / d;
            y02 = y1 + (y2 - y1) / 2 + h * (x2 - x1) / d;

            //Take point, which is nearest to initial (2 possible solutions)
            if (Math.Pow(x1 + step.I - x01, 2) + Math.Pow(y1 + step.J - y01, 2) < Math.Pow(x1 + step.I - x02, 2) + Math.Pow(y1 + step.J - y02, 2))
            {
                x0 = x01; y0 = y01;
            }
            else
            {
                x0 = x02; y0 = y02;
            }

            //If there is signigicant difference - change I and J (coordinates of central point of the curve)
            if (Math.Abs(x0 - x1 - step.I) > 1E-5)
            {
                step.I = (float)(x0 - x1);
                result = true;
            }
            if (Math.Abs(y0 - y1 - step.J) > 1E-5)
            {
                step.J = (float)(y0 - y1);
                result = true;
            }
            return result;
           
        }
        /// <summary>
        /// Get far point for circular steps
        /// </summary>
        /// <param name="step_index"></param>
        /// <returns></returns>
        private Vector3 GetFarPoint(int step_index)
        {
            Vector3 far = new Vector3();

            double dist = (new Vector3(steps[step_index].step.X, steps[step_index].step.Y,0)).Length;    //step length
            Vector3 cent = steps[step_index].start + (new Vector3(steps[step_index].step.X, steps[step_index].step.Y, 0)) / 2; // center point of step
            Vector3 max_vect = (cent - steps[step_index].curve_center); //vector from the curve center to the far point
            float factor = (float)(steps[step_index].R / Math.Sqrt(steps[step_index].R * steps[step_index].R - dist * dist / 4));
            far = steps[step_index].curve_center + factor * max_vect;
            return far;
        }

        private void CheckVector(Vector3 v)
        {
            //Preparing bound...
            if (v.X > maxXY.X)
                maxXY.X = v.X;
            if (v.Y > maxXY.Y)
                maxXY.Y = v.Y;
            if (v.X < minXY.X)
                minXY.X = v.X;
            if (v.Y < minXY.Y)
                minXY.Y = v.Y;
        }
        /// <summary>
        /// Splitting the steps into smaller steps
        /// </summary>
        /// <param name="max_step_length"></param>
        public void SplitStepsInSmallerSteps(double max_step_length)
        {
            steps = base_steps;
            List<ToolProgramStep> new_steps = new List<ToolProgramStep> { };
            for (int k =0;k<steps.Count;k++)
            {                
                if(steps[k].G == Gcode.TOOL_CHANGE)
                {
                    new_steps.Add(new ToolProgramStep
                    {
                        start = steps[k].start,
                        pos = steps[k].pos,
                        step = steps[k].step,
                        G = steps[k].G,
                        base_step_index = k,
                        tool = steps[k].tool,
                        line_number = steps[k].line_number
                    });
                    continue;
                }
                int ct = 0;
                Vector3 cur_pos = steps[k].start;
                do
                {
                    ToolProgramStep small_step = new ToolProgramStep
                    {
                        start = cur_pos,
                        G = steps[k].G,
                        F = steps[k].F,
                        base_step_index = k,
                        tool = steps[k].tool,
                        angle = steps[k].angle,
                        far_point = steps[k].far_point,
                        line_number = steps[k].line_number,
                        action = steps[k].action
                    };
                    if (ct == 0)
                        small_step.is_first = true;
                    small_step.step = GetNextDisplacementVector(k, cur_pos, max_step_length);
                    small_step.pos = cur_pos + small_step.step;
                    if (steps[k].G != Gcode.QUICK_MOVE && steps[k].G != Gcode.TOOL_CHANGE)
                    {
                        Vector3 gap = GetNextDisplacementVector(k, small_step.pos, max_step_length / 1000f);
                        if (((cur_pos - steps[k].pos).Length > gap.Length))//add small step to cover gap
                            small_step.step += gap;
                    }

                    if (small_step.G == Gcode.CLOCKWISE_CURVE || small_step.G == Gcode.COUNTER_CLOCKWISE_CURVE)
                    {
                        small_step.curve_center = steps[k].curve_center;
                        small_step.R = steps[k].R;
                        if (small_step.pos != steps[k].pos)
                        {
                            Vector3 dist = small_step.pos - steps[k].start;
                            small_step.I = steps[k].I - dist.X;
                            small_step.J = steps[k].J - dist.Y;
                            small_step.K = 0;// steps[k].K - dist.Z;
                        }
                        else
                        {
                            small_step.I = steps[k].I;
                            small_step.J = steps[k].J;
                            small_step.K = 0;// steps[k].K;
                        }
                        double angle = 2 * Math.Asin(new Vector3(small_step.step.X, small_step.step.Y, 0).Length / (2 * small_step.R));
                        small_step.angle = small_step.G == Gcode.CLOCKWISE_CURVE ? -angle : angle;
                    }
                    cur_pos = small_step.pos;
                    new_steps.Add(small_step);
                    ct++;
                } while ((cur_pos - steps[k].pos).Length > 1E-3);
                if (new_steps.Count>0)
                    new_steps.Last().is_last = true;
            }
            steps = new_steps;
        }
        /// <summary>
        /// Combine steps into larger to avoid microscopic steps
        /// </summary>
        /// <param name="min_step_len"></param>
        public void CombineStepsIntoLargerSteps(double min_step_len)
        {
            if (OPTCAMSim.Properties.Settings.Default.StepLength < min_step_len)
                min_step_len = OPTCAMSim.Properties.Settings.Default.StepLength / 2.0f;
            List<ToolProgramStep> new_steps = new List<ToolProgramStep> { };
            for (int k = 0; k < steps.Count; k++)
            {
                if(steps[k].G!= Gcode.LINEAR_MOVE || steps[k].step.Length> min_step_len)
                {
                    new_steps.Add(steps[k]);
                    continue;
                }
                else
                {
                    ToolProgramStep new_step = new ToolProgramStep()
                    {
                        start = steps[k].start,
                        G = steps[k].G,
                        F = steps[k].F,
                        pos = steps[k].pos,
                        step = steps[k].step,
                        is_first = steps[k].is_first,
                        is_last = steps[k].is_last,
                        base_step_index = k,
                        tool = steps[k].tool,
                        line_number = steps[k].line_number,
                        action = steps[k].action
                    };
                    while (k<steps.Count-1 &&
                        new_step.step.Length<min_step_len &&
                        steps[k + 1].G == Gcode.LINEAR_MOVE &&
                        steps[k + 1].step.Length< min_step_len
                        )
                    {                        
                        new_step.pos = steps[k + 1].pos;
                        new_step.is_last = steps[k + 1].is_last;
                        new_step.step += steps[k + 1].step;
                        new_step.line_number = steps[k + 1].line_number;
                        k++;
                    }
                    new_steps.Add(new_step);
                }
            }
            steps = new_steps;
        }
        /// <summary>
        /// In case of speed shange we need to find new step and modify it
        /// </summary>
        /// <param name="base_step_index">current base step index</param>
        /// <param name="cur_pos">current position</param>
        /// <returns></returns>
        public int GetNearestStepAndModifyIt(int base_step_index, Vector3 cur_pos)
        {
            int new_step_index = -1;
            List<ToolProgramStep> s = steps.Where(r => r.base_step_index == base_step_index && 
                                                    (cur_pos - r.start).Length < r.step.Length  ).ToList();
            if (s.Count == 0)
                s = steps.Where(r => r.start == cur_pos && r.base_step_index == base_step_index + 1).ToList();
            if (s.Count == 0)
                s = steps.Where(r => r.start == cur_pos && r.base_step_index == base_step_index).ToList();
            if (s.Count == 0)
                s = steps.Where(r => r.base_step_index == base_step_index).ToList();
            if (s.Count == 0)
            {
                int minstep = steps.Where(r => r.base_step_index < base_step_index).Max(r => r.base_step_index);
                s = steps.Where(r => r.base_step_index == minstep).ToList();
            }
            if (s.Count>1)
                s.Sort((s1, s2) => ((s1.pos - cur_pos).Length < (s2.pos - cur_pos).Length) ? 1 : -1);
            
            ToolProgramStep nearest = s.First();
            new_step_index = steps.IndexOf(nearest);
            nearest.step =  nearest.pos - cur_pos;
            nearest.start = cur_pos;                       
            return new_step_index;
        }
        /// Retruns next displacement vector for the current step
        /// </summary>
        /// <param name="step_index"></param>
        /// <param name="cur_pos">current tool position</param>
        /// <param name="step">tool step</param>
        /// <returns></returns>
        public Vector3 GetNextDisplacementVector(int step_index, Vector3 cur_pos, double step)
        {
            Vector3 res = new Vector3();
            if (this.steps != null && this.steps.Count > step_index)
            {
                switch (steps[step_index].G)
                {
                    case Gcode.TOOL_CHANGE:
                    case Gcode.QUICK_MOVE:
                        res = this.steps[step_index].step; // quick move to next location
                        break;
                    case Gcode.LINEAR_MOVE:
                        res = steps[step_index].pos- cur_pos;
                        double dist = res.Length;
                        if (dist > step)
                        {
                             res = Vector3.Multiply(res, (float)(step / dist));
                        }
                        break;
                    case Gcode.CLOCKWISE_CURVE:
                    case Gcode.COUNTER_CLOCKWISE_CURVE:
                        res = steps[step_index].pos- cur_pos;
                        double dist_c = res.Length;
                        if (dist_c > step)
                        {
                            res = GetNextPointCurve(step_index, cur_pos, step);
                            if (steps[step_index].step.Z != 0 && step>0.01)
                            {
                                double angle = 2 * Math.Asin(res.Length / (2 * steps[step_index].R));
                                angle = steps[step_index].G == Gcode.CLOCKWISE_CURVE ? -angle : angle;
                                res.Z += (float)((angle / steps[step_index].angle) * steps[step_index].step.Z);
                            }
                        }
                        break;
                    default: break;
                }
            }
            return res;
        }
        /// <summary>
        /// Finding net point on the curve, knowing previous point, radius and chord length
        /// https://math.stackexchange.com/questions/164541/finding-a-point-having-the-radius-chord-length-and-another-point
        /// </summary>
        /// <param name="step_index"></param>
        /// <param name="cur_pos"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public Vector3 GetNextPointCurve(int step_index, Vector3 cur_pos, double step)
        {
            Vector3 res = new Vector3();
            if (this.steps != null && this.steps.Count > step_index)
            {
                Vector3 cur_pos_c = cur_pos- steps[step_index].curve_center;
                int quart_xy = cur_pos_c.X >= 0 ? (cur_pos_c.Y > 0 ? 1 : 4) : (cur_pos_c.Y > 0 ? 2 : 3);
                double alpha, beta, theta;
                alpha = quart_xy == 1 || quart_xy == 4 ?
                    Math.Asin(Math.Round(cur_pos_c.Y / steps[step_index].R, 5))
                    :
                    Math.PI - Math.Asin(Math.Round(cur_pos_c.Y / steps[step_index].R, 5))
                    ;
                if (steps[step_index].G == Gcode.CLOCKWISE_CURVE)
                {
                    theta = 2 * Math.Asin(step / (2 * steps[step_index].R));
                }
                else
                {
                    theta = 2 * Math.Asin(-step / (2 * steps[step_index].R));
                }
                beta = alpha - theta;
                //Calculation in coord system of curve center
                res.X = (float)(steps[step_index].R * Math.Cos(beta));
                res.Y = (float)(steps[step_index].R * Math.Sin(beta));
                int quart_xy_new = res.X > 0 ? (res.Y > 0 ? 1 : 4) : (res.Y > 0 ? 2 : 3);
                // Calculation in normal coord system 
                res.X += steps[step_index].curve_center.X;
                res.Y += steps[step_index].curve_center.Y;
                res.Z = cur_pos.Z;
                res = res- cur_pos;
            }
            return res;
        }
        public void GetLinearStepFullSolidBodyAndMoveTool(int step_index, ref Tool t, ref Net3dBool.Solid sub)
        {
            if (this.steps != null && this.steps.Count > step_index)
            {
                Vector3 step = steps[step_index].step;
                Net3dBool.Solid myblock = null;
                if (OPTCAMSim.Properties.Settings.Default.UseAdvanced3D == 0 //do not use adv. 3D
                    || (!(step.Z!=0 && (step.X!=0 || step.Y!=0))))//or simple 2d step
                {
                    myblock = GetLinearStepBody(t, step);
                    myblock.Translate(steps[step_index].start.X, steps[step_index].start.Y, steps[step_index].start.Z);
                }
                else
                {
                    List<Net3dBool.Vector3> verts = t.ToolObj.GetVertices().ToList();
                    List<int> indices = t.ToolObj.GetIndices().ToList();
                    double mdh = 0, md = 0;
                    t.GetMaxDiamHeight(ref md, ref mdh);
                    mdh = ((int)t.myType) > 4 && ((int)t.myType) != 11 && ((int)t.myType) != 12 && ((int)t.myType) != 14 ? mdh : 0;
                    bool add_checks = t.myType == ToolType.VBit7 || t.myType == ToolType.VBit8 || t.myType == ToolType.Custom;
                    myblock = Net3dBool.Solid.ExtrudeSolid3D(verts, indices, new Net3dBool.Vector3(step.X, step.Y, step.Z), add_checks, mdh);
                    myblock.color = System.Drawing.Color.Violet;
                }
                GetStepToolBodyUnionAndMoveTool(ref myblock, ref t, step, ref sub);                 
            }
        }
        /// <summary>
        /// Get full solid body for rotation step and move the tool to the step end
        /// </summary>
        /// <param name="step_index"></param>
        /// <param name="Tool"></param>
        /// <param name="tp"></param>
        /// <returns></returns>
        public void GetRotationStepFullSolidBodyAndMoveTool(int step_index, ref Tool t, ref Net3dBool.Solid sub)
        {
            if (this.steps != null && this.steps.Count > step_index)
            {
                Vector3 step = steps[step_index].step;
                Net3dBool.Solid myblock = GetRotationStepBody(t, step_index);
                GetStepToolBodyUnionAndMoveTool(ref myblock, ref t, step, ref sub);
            }
        }

       
        /// <summary>
        /// Get full solid body for vertical step and move the tool to the step end
        /// </summary>
        /// <param name="step_index"></param>
        /// <param name="Tool"></param>
        /// <param name="tp"></param>
        /// <returns></returns>
        public void GetVerticalStepFullSolidBodyAndMoveTool(int step_index, ref Tool t, ref Net3dBool.Solid sub)
        {
            if (this.steps != null && this.steps.Count > step_index)
            {
                Vector3 step = steps[step_index].step;
                if ((int)t.myType < 5)
                {

                    t.ToolObj.Translate(step.X, step.Y, step.Z);
                    sub = t.ToolObj;
                    return;
                }
                else
                {
                    double MD = 0;
                    double mdh = 0;
                    t.GetMaxDiamHeight(ref MD, ref mdh);
                    Net3dBool.Solid myblock = GetVerticalStepBody(2*MD, step.Length);
                    double dz = step.Z > 0 ? mdh : mdh + step.Z;
                    myblock.Translate(steps[step_index].start.X, steps[step_index].start.Y, steps[step_index].start.Z + dz);
                    GetStepToolBodyUnionAndMoveTool(ref myblock, ref t, step, ref sub);
                }
            }
        }
        public void GetStepToolBodyUnionAndMoveTool(ref Net3dBool.Solid myblock, ref Tool t, Vector3 step, ref Net3dBool.Solid sub)
        {
            t.ToolObj.Translate(step.X, step.Y, step.Z);            
            sub = myblock;
        }
        /// <summary>
        /// Returns vertical cylinder
        /// </summary>
        /// <param name="d"></param>
        /// <param name="l"></param>
        /// <returns></returns>
        public static Net3dBool.Solid GetVerticalStepBody(double d, double l)
        {
            Tool tt = new Tool(ToolType.Flat, new Dictionary<string, double> { { "D", d }, { "L", l } });
            return tt.ToolObj;
        }
        public Net3dBool.Solid GetLinearStepBody(Tool t, Vector3 step)
        {
            List<Net3dBool.Vector3> cont = ToolContour.GetToolContour(t, false);
            List<int> indices = new List<int> { };

            Net3dBool.Solid.ExtrudeFlatContour(ref cont, ref indices, new Net3dBool.Vector3(step.X,step.Y,step.Z));
            var par = new Net3dBool.Solid(cont.ToArray(), indices.ToArray());
            par.color = System.Drawing.Color.Violet;
            return par;
        }
        public Net3dBool.Solid GetRotationStepBody(Tool t,int step_index)
        {
            Vector3 step = steps[step_index].step;
            Vector3 start = steps[step_index].start;
           
            List<Net3dBool.Vector3> cont = ToolContour.GetToolContour(t, false);
            Net3dBool.Vector3 st = new Net3dBool.Vector3(start.X, start.Y, start.Z);
            
            // rotational angle in radians
            double angle = steps[step_index].angle;

            Net3dBool.Vector3 p1 = new Net3dBool.Vector3(steps[step_index].curve_center.X,
                steps[step_index].curve_center.Y, steps[step_index].curve_center.Z);

            //turning angle of the countour
            int quart_xy = step.X >= 0 ? (step.Y > 0 ? 1 : 4) : (step.Y > 0 ? 2 : 3);
            Net3dBool.Vector3 p2 = st - p1;
            double alpha_ = Math.Atan2(p2.Y, p2.X);
            //alpha_ = (step.Y != 0 ? Math.Sign(-step.Y) : -1) * alpha_;

            if (OPTCAMSim.Properties.Settings.Default.UseAdvanced3D == 0
                 || (!(step.Z != 0 && (step.X != 0 || step.Y != 0))))//or simple 2d step)
            {
                Net3dBool.Solid.RotateVertices(ref cont, alpha_);

                //Move to start point
                for (int q = 0; q < cont.Count; q++)
                {
                    Net3dBool.Vector3 n = cont[q] + st;
                    cont[q] = n;
                }
            }
            int numsteps = (int)Math.Ceiling(Math.Abs(angle * 180 / Math.PI) / OPTCAMSim.Properties.Settings.Default.StepAngle);
            if (numsteps < 2)
                numsteps = 2;
            double dz = steps[step_index].step.Z / numsteps;
            List<int> indices = new List<int> { };
            Net3dBool.Solid par = null;
            if (OPTCAMSim.Properties.Settings.Default.UseAdvanced3D == 0 || (!(step.Z != 0 && (step.X != 0 || step.Y != 0))))//or simple 2d step)
            {
                Net3dBool.Solid.RotateFlatContour(ref cont, ref indices, p1, new Net3dBool.Vector3(0, 0, 1), numsteps, angle, dz);
                par = new Net3dBool.Solid(cont.ToArray(), indices.ToArray());
                par.color = System.Drawing.Color.Violet;
            }
            else
            {                
                bool add_checks = t.myType == ToolType.VBit7 || t.myType == ToolType.VBit8 || t.myType == ToolType.Custom;
                par = Net3dBool.Solid.RotateSolid3D(t.ToolObj.GetVertices().ToList(),t.ToolObj.GetIndices().ToList()
                    , p1, new Net3dBool.Vector3(0, 0, 1), numsteps, angle, dz, add_checks);
                par.color = System.Drawing.Color.Violet;
            }
            
            return par;
        }
        /// <summary>
        /// Get full solid body for any step and move the tool to the step end
        /// </summary>
        /// <param name = "step_index" ></ param >
        /// < param name="Tool"></param>
        /// <param name = "tp" ></ param >
        /// <returns></returns>
        public void GetStepFullSolidBodyAndMoveTool(int step_index, ref Tool t, ref Net3dBool.Solid sub)
        {
            if (this.steps != null && this.steps.Count > step_index)
            {

                if (steps[step_index].G == Gcode.LINEAR_MOVE)
                {
                    if (steps[step_index].step.Z != 0 && steps[step_index].step.X == steps[step_index].step.Y && steps[step_index].step.X == 0)
                    {
                        GetVerticalStepFullSolidBodyAndMoveTool(step_index, ref t, ref sub);
                    }
                    else
                        GetLinearStepFullSolidBodyAndMoveTool(step_index, ref t, ref sub);
                }
                else
                    GetRotationStepFullSolidBodyAndMoveTool(step_index, ref t, ref sub);

                //if (sub != t.ToolObj)
                //    sub.RoundVerticesCoords(7);
            }
            else
                return;
        }

        /// <summary>
        /// Estimate step body vertices count - to build heat map
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public int GetStepVerticesCount(ToolProgramStep s)
        {
            int res = 0;
            Tool t = s.tool != null ? s.tool : steps.Where(r => steps.IndexOf(r) < steps.IndexOf(s) && r.tool != null).Last().tool;
            int ct_vert = t.ToolObj.GetVertices().Count()  / OPTCAMSim.Properties.Settings.Default.ToolFringes+2;
            switch (s.G)
            {
                case Gcode.LINEAR_MOVE:
                    if (s.step.Z != 0 && s.step.X == s.step.Y && s.step.X == 0)
                        res = OPTCAMSim.Properties.Settings.Default.ToolFringes * (4 + (ct_vert + 4) / 2);
                    else
                        res = OPTCAMSim.Properties.Settings.Default.ToolFringes * (ct_vert + 4) + 2 * ct_vert;
                    break;
                case Gcode.CLOCKWISE_CURVE:
                case Gcode.COUNTER_CLOCKWISE_CURVE:
                    res = (int)(OPTCAMSim.Properties.Settings.Default.ToolFringes * (ct_vert + 4) +
                       ct_vert * Math.Ceiling(Math.Abs(s.angle * 180 / Math.PI) / OPTCAMSim.Properties.Settings.Default.StepAngle));

                    break;
                default: break;
            }


            return res;
        }
    }
}
