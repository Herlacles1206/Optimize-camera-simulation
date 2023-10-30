using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DemoForm;
using Net3dBool;
using System.Drawing;
using System.Collections.Concurrent;

namespace OPTCAMSim
{
    /// <summary>
    /// Class for heat map creation - to reduce number of cuts per single cubic
    /// </summary>
    class HeatMap
    {
        public Dictionary<int, int> heatPixels = new Dictionary<int, int> { };
        public Dictionary<int, float> norm_heatPixels = new Dictionary<int, float> { };
        public HeatMap(CSGPanel cSGPanel, ToolProgram toolProgram)
        {
            List<Net3dBool.Solid> solids = cSGPanel.GetCutPart();
            List<Tuple<int, int>> heatSolids = new List<Tuple<int, int>> { };
            Dictionary<int, Net3dBool.Bound> bounds = new Dictionary<int, Bound> { };
            foreach (Net3dBool.Solid s in solids)
                bounds.Add(s.arr_index, new Net3dBool.Object3D(s).Bound);

            heatSolids = toolProgram.steps.AsParallel().Select(s =>
              {
                
                if (s.G == Gcode.QUICK_MOVE || s.G == Gcode.TOOL_CHANGE)
                    return new List <Tuple<int, int>>{ };
                else
                {
                    //step bound
                    OpenTK.Vector3 Ts_n = s.step.Normalized();
                    OpenTK.Vector3 Ts_f = 2 * (s.far_point - s.curve_center).Normalized();
                    Vector3 Tooladd = new Vector3(2 * Ts_n.X, 2 * Ts_n.Y, 2 * Ts_n.Z);
                    Net3dBool.Bound b = new Bound((new Vector3(s.start.X, s.start.Y, s.start.Z) - Tooladd) * 0.999,
                         (new Vector3(s.pos.X, s.pos.Y, s.start.Z < 0 ? s.pos.Z + 10 : s.pos.Z) + Tooladd) * 1.001,
                         (s.G == Gcode.LINEAR_MOVE ? new Vector3(s.pos.X, s.pos.Y, s.pos.Z) :
                         new Vector3(s.far_point.X, s.far_point.Y, s.far_point.Z)) + new Vector3(Ts_f.X, Ts_f.Y, Ts_f.Z)
                         );
                    //number of hits
                    int hits = toolProgram.GetStepVerticesCount(s);
                    List<Tuple<int, int>> valid_cubes = new List<Tuple<int, int>> { };
                    foreach (KeyValuePair<int, Net3dBool.Bound> cube_bound in bounds)
                    {
                        if (cube_bound.Value.Overlap(b))
                        {
                              valid_cubes.Add( new Tuple<int, int>(cube_bound.Key, hits));
                        }
                    }
                      return valid_cubes;
                }
            }).SelectMany(x => x).ToList();
            
            //Count total number of hits per each pixel
            heatPixels = heatSolids.GroupBy(item => item.Item1)
                        .Select(group => new KeyValuePair<int, int>(group.Key, group.Select(x => x.Item2).Sum()))
             .ToDictionary(x => x.Key, x => x.Value);

            if (heatPixels.Count > 0)
            {
                int max = heatPixels.Values.Max();
                foreach (var p in heatPixels)
                    norm_heatPixels.Add(p.Key, p.Value >= 0 ? (float)p.Value / (float)max : 1);
            }
        }        
    }
}
