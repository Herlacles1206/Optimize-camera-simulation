using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;

namespace OPTCAMSim
{
    /// <summary>
    /// Tool types enumeration
    /// </summary>
    public enum ToolType
    {
        Flat = 1,
        Drill = 2,
        Ball = 3,
        Bull = 4,
        VBit1 = 5,
        VBit2 = 6,
        VBit3 = 7,
        VBit4 = 8,
        VBit5 = 9,
        VBit6 = 10,
        VBit7 = 11,
        VBit8 = 12,
        VBit9 = 13,
        Custom = 14
    };

    public class Tool
    {
        public ToolType myType;
        public Dictionary<string, double> ToolParams;
        public Net3dBool.Solid ToolObj;
        public string ToolName = "";
        private double mdh = -1,md=-1;
        /// <summary>
        /// OptCam tool ID for simulation steps like M6 TID
        /// </summary>
        public int ocID;
        /// <summary>
        /// Tool contour for steps
        /// </summary>
        public List<Net3dBool.Vector3> tool_contour;
        public Tool(ToolType tt, Dictionary<string, double> Params)
        {
            myType = tt;
            ToolParams = Params;
            GetToolObj();
        }
        public Tool(Tool t)
        {
            myType = t.myType;
            ToolParams = t.ToolParams;
            ToolObj = new Net3dBool.Solid(t.ToolObj.GetVertices(), t.ToolObj.GetIndices());
            ToolObj.color = t.ToolObj.color;
            ToolObj.Name = t.ToolObj.Name;
            ToolName = t.ToolName;
            tool_contour = t.tool_contour;
        }
        public Net3dBool.Solid GetToolObj()
        {
            if (ToolObj != null)
                return ToolObj;
            if ((ToolParams is null || ToolParams.Count == 0) && (tool_contour == null || tool_contour.Count==0))
                return null;
            List<Net3dBool.Vector3> cont = ToolContour.GetToolContour(this, true);
            List<int> indices = new List<int> { };

            Net3dBool.Solid.RotateFlatContour(ref cont, ref indices, Net3dBool.Vector3.Zero, new Net3dBool.Vector3(0, 0, 1),
                OPTCAMSim.Properties.Settings.Default.ToolFringes, 2 * Math.PI);
            var par = new Net3dBool.Solid(cont.ToArray(), indices.ToArray());
            
            //remove not needed vertices
            var modeller = new Net3dBool.BooleanModeller(par, par);
            par = modeller.GetUnion();
            par.color = Color.Violet;
            par.Name = "ToolObj";
            par.RoundVerticesCoords();
            if (ToolObj == null)
                ToolObj = par;

            return par;
        }
        /// <summary>
        /// Return ToolType enum value by string of type name
        /// </summary>
        /// <param name="typeString"></param>
        /// <returns></returns>
        public static ToolType GetTypeByString(string typeString)
        {
            ToolType t = ToolType.Flat;
            if (!ToolType.TryParse(typeString.Replace("-", "").Replace("VShape", "Drill"), true, out t))
                return ToolType.Custom;
            return t;
        }
        public override string ToString()
        {
            if (myType != ToolType.Custom)
                return "TLN\t" +  ToolType.GetName(typeof(ToolType), myType) + "\tID:" + ocID.ToString() +  "\tName:" + ToolName.ToString() + "\t"
                    + string.Join("\t", ToolParams.Select(r => r.Key.ToString() + ":" +Math.Round((r.Key!="A"?10:1)* r.Value,5).ToString()));
            else
                return "TLN\t" + ToolType.GetName(typeof(ToolType), myType) + "\tID:" + ocID.ToString() + "\tName:" + ToolName.ToString() + Environment.NewLine +
                   string.Join(Environment.NewLine, tool_contour.Select(r => "X" + Math.Round(r.X, 5).ToString() + "\tY" + Math.Round(r.Z , 5).ToString()));

        }
        /// <summary>
        /// Get maximum tool diameter and it's minimal height
        /// </summary>
        /// <returns></returns>
        public void GetMaxDiamHeight(ref double MD, ref double MDH)
        {
            if (mdh != -1)
            {
                MD = md;
                MDH = mdh;
                return;
            }
            List<Net3dBool.Vector3> verts = (tool_contour != null ? tool_contour :
              (ToolObj!=null? ToolObj: GetToolObj()).GetVertices().ToList());
            if (tool_contour == null)
            {
                Net3dBool.Vector3 mean = ToolObj.GetMean();
                for (int k = 0; k < verts.Count; k++)
                    verts[k] -= mean;
                Net3dBool.Vector3 minZ = new Net3dBool.Vector3(0,0, verts.Select(v => v.Z).Min());
                for (int k = 0; k < verts.Count; k++)
                    verts[k]  -= minZ;
            }
            MD = md = Math.Round( verts.Select(p => Math.Abs(p.X)).Max(),5);
            MDH = mdh = Math.Round(verts.Where(p => Math.Round(Math.Abs(p.X), 5) == md).Select(p => p.Z).Min(), 5);             
        }
        /// <summary>
        /// Construct tool from OptCam export file
        /// </summary>
        /// <param name="OptCamExportFilePath">Full path to OptCam tool export file</param>
        public static Tool ParseOptCam(string OptCamExportFilePath, ref string errors)
        {
            Tool mytool = new Tool(ToolType.Flat, new Dictionary<string, double> { });
            mytool.ToolName = Path.GetFileNameWithoutExtension(OptCamExportFilePath);
            System.Globalization.CultureInfo c = System.Globalization.CultureInfo.CurrentCulture;
            bool replace_dots = c.NumberFormat.NumberDecimalSeparator == ",";
            try
            {
                using (var reader = new StreamReader(OptCamExportFilePath))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
                            continue;
                        List<string> values = line.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        //read tool params
                        switch (values[0])
                        {
                            case "TL#":
                                mytool.ocID = Int32.Parse(values[1]);
                                break;
                            case "CTN":
                                switch(values[1])
                                {
                                    case "Ball Head":
                                        mytool.myType = ToolType.Ball;
                                        break;
                                    case "Bull Head":
                                        mytool.myType = ToolType.Bull;
                                        break;
                                    case "Drill":
                                        mytool.myType = ToolType.Drill;
                                        break;
                                    case "Flat Head":
                                    case "Face Cutter":
                                    case "Threading Tap":
                                        mytool.myType = ToolType.Flat;
                                        break;
                                    case "VBit":
                                        mytool.myType = ToolType.VBit6;
                                        break;
                                    case "Slot Cutter":
                                        mytool.myType = ToolType.VBit7;
                                        break;
                                    case "Modelling":
                                        mytool.myType = ToolType.VBit4;
                                        break;
                                    default:
                                        throw new Exception("Unknown tool catalog: " + values[1]);
                                }
                                break;
                            default:
                                double value = 0;

                                if (ToolProgram.ParseDoubleCultureSpecific(values[1], replace_dots, ref value))
                                {
                                    if(value>0)
                                        mytool.ToolParams.Add(values[0],
                                            values[0] == "V" || values[0] == "A" ?(mytool.myType == ToolType.Drill? value : (90 - value / 2) ): 
                                            value / 10);
                                }
                                else
                                    throw new Exception("Unable to parse param value, param: " + values[1]);
                                break;
                        }                        
                    }                   
                }

                //CleanUp and fill params collections
                if (mytool.myType == ToolType.Flat || mytool.myType == ToolType.Ball)
                {
                    CleanupParamsCollection(mytool, 
                        new List<Tuple<string, string>> { new Tuple<string, string>("D1", "D") },
                        new List<string> { "D","L" });                     
                }
                else if (mytool.myType == ToolType.Bull)
                {
                    CleanupParamsCollection(mytool,
                        new List<Tuple<string, string>> { new Tuple<string, string>("D1", "D") },
                        new List<string> { "D", "L", "R" });
                }
                else if (mytool.myType == ToolType.Drill)
                {
                    CleanupParamsCollection(mytool,
                        new List<Tuple<string, string>> { new Tuple<string, string>("D1", "D") },
                        new List<string> { "D", "L", "A" });
                }
                else if (mytool.myType == ToolType.VBit4)
                {
                    if (mytool.ToolParams.ContainsKey("RR") && mytool.ToolParams["RR"] > 0)
                    {
                        CleanupParamsCollection(mytool,
                            new List<Tuple<string, string>> { },
                            new List<string> { "D1", "S", "S1", "R", "H", "L", "D", "RR" });
                        mytool.myType = ToolType.VBit1;
                    }
                    else
                    {
                        CleanupParamsCollection(mytool,
                            new List<Tuple<string, string>> { },
                            new List<string> { "D1", "S", "S1", "R", "H", "L", "D" });
                    }
                    if (!mytool.ToolParams.ContainsKey("D"))
                        mytool.ToolParams.Add("D", 2);
                }
                else if (mytool.myType == ToolType.VBit6)
                {
                    CleanupParamsCollection(mytool,
                        new List<Tuple<string, string>> { },
                        new List<string> { "D1","D","A","L","D2" });
                    if (!mytool.ToolParams.ContainsKey("D2"))
                        mytool.ToolParams.Add("D2", 1.4);
                    if (!mytool.ToolParams.ContainsKey("D1"))
                        mytool.ToolParams.Add("D1", 0);
                    else
                        mytool.ToolParams["D1"] = 0;
                }
                else if (mytool.myType == ToolType.VBit7)
                {
                    CleanupParamsCollection(mytool,
                        new List<Tuple<string, string>> { },
                        new List<string> { "D1","D","S1","H","A","L","D2" });
                    if (!mytool.ToolParams.ContainsKey("D2"))
                        mytool.ToolParams.Add("D2", 1.2);
                }

                //In case some parameters are missing - fill them from default
                FillDefaultParams(mytool);
                mytool.GetToolObj();
                /*
                 // This code not needed - don't use two files
                try
                {
                    using (StreamWriter sw = new StreamWriter(System.Windows.Forms.Application.StartupPath + "\\UDT\\T" + mytool.ocID + ".txt", false))
                    {
                        sw.WriteLine(mytool.ToString());
                    }
                
                    File.Move(OptCamExportFilePath, System.Windows.Forms.Application.StartupPath + "\\UDT\\Exported\\" + Path.GetFileName(OptCamExportFilePath));
                }
                catch { }
                */
                return mytool;
            }
            catch (Exception ex) // unknown code
            {
                ToolProgram.AddErrors(ref errors, "Unable to read tool params: " + Environment.NewLine + ex.Message);
                return null;
            }
        }
        private static void CleanupParamsCollection(Tool mytool, List<Tuple<string,string>> FromTo, List<string> listValid)
        {
            if(mytool.ToolParams.Count>0)
                foreach(Tuple<string, string>tuple in FromTo)
                    RenameKey(mytool.ToolParams, tuple.Item1, tuple.Item2);
            foreach (var s in mytool.ToolParams.Where(x => !listValid.Contains( x.Key )).ToList())
            {
                mytool.ToolParams.Remove(s.Key);
            }
        }
        public static void RenameKey(Dictionary<string, double> dic,
                                      string fromKey, string toKey)
        {
            if (dic.ContainsKey(fromKey))
            {
                double value = dic[fromKey];
                dic.Remove(fromKey);
                dic[toKey] = value;
            }
        }
        /// <summary>
        /// Fill default parameters depending on the tool type
        /// </summary>
        /// <param name="t">Tool object with defined type</param>
        public static void FillDefaultParams(Tool t)
        {
            switch (t.myType)
            {
                case ToolType.Flat:
                    FillMissingParams(t, new Dictionary<string, double>{
                        { "D", 0.6 } , { "L", 10 } });
                    break;
                case ToolType.Drill:
                    FillMissingParams(t, new Dictionary<string, double> { 
                        { "D", 0.6 }, { "L", 10 }, { "A", 60 } });
                    break;
                case ToolType.Ball:
                    FillMissingParams(t, new Dictionary<string, double> { 
                        { "D", 0.6 }, { "L", 10 } });
                    break;
                case ToolType.Bull:
                    FillMissingParams(t, new Dictionary<string, double> { 
                        { "D", 0.6 }, { "R", 0.1 }, { "L", 10 } });
                    break;
                case ToolType.VBit1:
                    FillMissingParams(t, new Dictionary<string, double> {
                        { "D1", 1.4 }, { "S", 2.55 }, { "S1", 1.9 }, { "R", 0.3 }, { "H", 1.18 }
                        , { "RR", 6.5 }, { "L", 10.5 }, { "D", 2.0 } });
                    break;
                case ToolType.VBit2:
                case ToolType.VBit3:
                    FillMissingParams(t, new Dictionary<string, double> { 
                        { "D1", 1.4 }, { "S1", 1.9 }, { "L", 8.5 }, { "D", 2.0 } });
                    break;
                case ToolType.VBit4:
                    FillMissingParams(t, new Dictionary<string, double> {
                        { "D1", 1.2 }, { "S", 1.1 }, { "S1", 1.9 }, { "R", 1.0 }
                    , { "H", 0.5 }, { "L", 8.5 }, { "D", 2.0 } });
                    break;
                case ToolType.VBit5:
                    FillMissingParams(t, new Dictionary<string, double> { 
                        { "D", 2.0 },{ "S1", 0.5 }, { "R", 1.25 }, { "L", 8.5 }, { "D2", 1.4 } });
                    break;
                case ToolType.VBit6:
                    FillMissingParams(t, new Dictionary<string, double> { 
                        { "D1", 1.2 },{ "D", 3.2 },{ "A", 45 }, { "L", 8.5 }, { "D2", 1.4 } });
                    break;
                case ToolType.VBit7:
                    FillMissingParams(t, new Dictionary<string, double> {
                        { "D1", 1.25 }, { "D", 3.0 }, { "S1", 0.9 }, { "H", 0.5 }, { "A", 45 },
                        { "L", 7.0 }, { "D2", 1.2 } });
                    break;
                case ToolType.VBit8:
                    FillMissingParams(t, new Dictionary<string, double> {
                        { "D1", 1.25 }, { "D", 3.0 }, { "S1", 0.9 }, { "H0", 1.8 }, { "L", 8.0 },
                        { "D2", 2.0 } });
                    break;
                case ToolType.VBit9:
                    FillMissingParams(t, new Dictionary<string, double> {
                        { "D1", 2.4 }, { "D", 1.2 }, { "S1", 1.4 }, { "A", 45 }, { "L", 8.5 },
                        { "D2", 2.0 } });
                    break;
                default: break;
            }
        }
        //Fill parameters, which are missing
        public static void FillMissingParams(Tool t, Dictionary<string, double> def_params)
        {
            if (t.ToolParams is null)
                t.ToolParams = new Dictionary<string, double> { };

            foreach (string key in def_params.Keys)
            {
                if(!t.ToolParams.ContainsKey(key))
                {
                    t.ToolParams.Add(key, def_params[key]);
                }
            }
        }

        public static Tool LoadToolDefFile(string filename, ref string errors)
        {
            Tool result = null;

            int err_count = 0;
            System.Globalization.CultureInfo c = System.Globalization.CultureInfo.CurrentCulture;
            bool replace_dots = c.NumberFormat.NumberDecimalSeparator == ",";
            try
            {
                using (var reader = new StreamReader(filename))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
                            continue;
                        List<string> values = line.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                        //read tool params
                        if (values[0] == "TLN")
                        {
                            try
                            {
                                string typeString = values[1];
                                ToolType tt = Tool.GetTypeByString(typeString);
                                Tool mytool = new Tool(tt, new Dictionary<string, double> { });
                                mytool.ToolName = Path.GetFileNameWithoutExtension(filename);
                                if (mytool.myType != ToolType.Custom)
                                {
                                    for (int k = 2; k < values.Count; k++)
                                    {
                                        double value = 0;
                                        string key = "";
                                        int colon = values[k].IndexOf(":");
                                        if (colon > 0)
                                        {
                                            key = values[k].Substring(0, colon);
                                            if (key == "ID")
                                            {
                                                ToolProgram.ParseDoubleCultureSpecific(values[k].Substring(colon + 1), replace_dots, ref value);
                                                mytool.ocID = (int)value;
                                            }
                                            else if (key == "Name")
                                            {
                                                mytool.ToolName = values[k].Substring(colon + 1);
                                            }
                                            else
                                            {
                                                if (ToolProgram.ParseDoubleCultureSpecific(values[k].Substring(colon + 1), replace_dots, ref value))
                                                    mytool.ToolParams.Add(key, value / (key == "A" ? 1 : 10));
                                                else
                                                    throw new Exception("Unable to parse param value, param: " + values[k]);
                                            }
                                        }
                                        else
                                            throw new Exception("Colon not found, param: " + values[k]);
                                    }
                                }
                                else
                                {
                                    for (int k = 2; k < values.Count; k++)
                                    {
                                        double value = 0;
                                        string key = "";
                                        int colon = values[k].IndexOf(":");
                                        if (colon > 0)
                                        {
                                            key = values[k].Substring(0, colon);
                                            if (key == "ID")
                                            {
                                                ToolProgram.ParseDoubleCultureSpecific(values[k].Substring(colon + 1), replace_dots, ref value);
                                                mytool.ocID = (int)value;
                                            }
                                            else if (key == "Name")
                                            {
                                                mytool.ToolName = values[k].Substring(colon + 1);
                                            }
                                        }
                                        else
                                            throw new Exception("Colon not found, param: " + values[k]);
                                    }
                                    int coord_multiplier = 10;
                                    while (!reader.EndOfStream)
                                    {
                                        Net3dBool.Vector3 crd = new Net3dBool.Vector3();
                                        string linec = reader.ReadLine();
                                        List<string> valuesc = linec.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                        for (int j = 0; j < valuesc.Count; j++)
                                        {
                                            string val = valuesc[j];
                                            switch (val.Substring(0, 1))
                                            {
                                                case "X":
                                                    double x = 0;
                                                    if (ToolProgram.ParseDoubleCultureSpecific(val.Substring(1), replace_dots, ref x))
                                                    {
                                                        crd.X = (float)(x / coord_multiplier);
                                                    }
                                                    else
                                                    {
                                                        err_count++;
                                                        ToolProgram.AddErrors(ref errors, "Unknown X-value: " + val.Substring(1));                                                        
                                                    }
                                                    break;
                                                case "Y":
                                                    double y = 0;
                                                    if (ToolProgram.ParseDoubleCultureSpecific(val.Substring(1), replace_dots, ref y))
                                                    {
                                                        crd.Z = (float)(y / coord_multiplier);
                                                    }
                                                    else
                                                    {
                                                        err_count++;
                                                        ToolProgram.AddErrors(ref errors, "Unknown Y-value: " + val.Substring(1));                                                        
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        if (mytool.tool_contour == null)
                                            mytool.tool_contour = new List<Net3dBool.Vector3> { };
                                        mytool.tool_contour.Add(crd);
                                    }
                                }
                                mytool.GetToolObj();

                                if (mytool.myType == ToolType.Custom)
                                {
                                    //Clone and rotate points
                                    List<Net3dBool.Vector3> cont2 = new List<Net3dBool.Vector3> { };
                                    foreach (Net3dBool.Vector3 v in mytool.tool_contour)
                                        cont2.Add(new Net3dBool.Vector3(v));
                                    Net3dBool.Solid.RotateVertices(ref cont2, Math.PI);

                                    for (int k = cont2.Count - 1; k >= 0; k--)
                                        if (cont2[k] != Net3dBool.Vector3.Zero)
                                            mytool.tool_contour.Add(cont2[k]);
                                }

                                result = mytool;
                                //LbTools.Items.Add(mytool.ToolName);
                            }
                            catch (Exception ex)
                            {
                                err_count++;
                                ToolProgram.AddErrors(ref errors, "Unable to read tool params: " + line + "; " + ex.Message);                                
                            }
                        }
                    }
                }
            }
            catch (Exception ex) // unknown code
            {
                err_count++;
                ToolProgram.AddErrors(ref errors, "Unable to read tool params: " + Environment.NewLine + ex.Message);
            }
            return result;
        }

        /// <summary>
        /// Function to read tool params both from OptCam export format and old OptCamSim format
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public static Tool GetToolFromFile(string filename, ref string errors)
        {
            if(!File.Exists(filename))
            {
                ToolProgram.AddErrors(ref errors, "Tool file not found: " + filename);
                return null;
            }

            if (File.ReadAllText(filename).StartsWith("TL#")) // check if this is an OptCam export file
            {
                return ParseOptCam(filename, ref errors);
            }
            else
                return LoadToolDefFile(filename, ref errors);

        }
    }
}
