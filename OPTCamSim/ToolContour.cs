using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Net3dBool;

namespace OPTCAMSim
{
    /// <summary>
    /// Methods to build tool contours in 2D
    /// </summary>
    class ToolContour
    {
        public static List<Net3dBool.Vector3> GetToolContour(Tool t, bool half = false)
        {
            if ((!half && t.tool_contour != null && t.tool_contour.Count > 0) || ( t.myType == ToolType.Custom))
                return t.tool_contour;
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };
            switch (t.myType)
            {
                case ToolType.Flat:
                    cont = GetFlatContour(t.ToolParams["D"] / 2.0f, t.ToolParams["L"], half);
                    break;
                case ToolType.Drill:
                    cont = GetDrillContour(t.ToolParams["D"] / 2.0f, t.ToolParams["L"], t.ToolParams["A"], half);
                    break;
                case ToolType.Ball:
                    cont = GetBallContour(t.ToolParams["D"] / 2.0f, t.ToolParams["L"], OPTCAMSim.Properties.Settings.Default.ToolRounding, half);
                    break;
                case ToolType.Bull:
                    cont = GetBullContour(t.ToolParams["D"] / 2.0f, t.ToolParams["R"], t.ToolParams["L"], OPTCAMSim.Properties.Settings.Default.ToolRounding, half);
                    break;
                case ToolType.VBit1:
                    cont = GetVBit1Contour(t.ToolParams, OPTCAMSim.Properties.Settings.Default.ToolRounding, half);
                    break;
                case ToolType.VBit2:
                case ToolType.VBit3:
                    cont = GetVBit2Contour(t.ToolParams, OPTCAMSim.Properties.Settings.Default.ToolRounding, half);
                    break;
                case ToolType.VBit4:
                    cont = GetVBit4Contour(t.ToolParams, OPTCAMSim.Properties.Settings.Default.ToolRounding, half);
                    break;
                case ToolType.VBit5:
                    cont = GetVBit5Contour(t.ToolParams, OPTCAMSim.Properties.Settings.Default.ToolRounding, half);
                    break;
                case ToolType.VBit6:
                    cont = GetVBit6Contour(t.ToolParams, OPTCAMSim.Properties.Settings.Default.ToolRounding, half);
                    break;
                case ToolType.VBit7:
                    cont = GetVBit7Contour(t.ToolParams, OPTCAMSim.Properties.Settings.Default.ToolRounding, half);
                    break;
                case ToolType.VBit8:
                    cont = GetVBit8Contour(t.ToolParams, OPTCAMSim.Properties.Settings.Default.ToolRounding, half);
                    break;
                case ToolType.VBit9:
                    cont = GetVBit9Contour(t.ToolParams, OPTCAMSim.Properties.Settings.Default.ToolRounding, half);
                    break;
                default: break;
            }
            RoundVerticesCoords(ref cont);
            if (!half && (t.tool_contour == null || t.tool_contour.Count == 0))
                t.tool_contour = cont;
            return cont;
        }
        /// <summary>
        /// Get the contour for the ball tool
        /// </summary>
        /// <param name="rad">ball radius</param>
        /// <param name="height">tool height</param>
        /// <param name="ball_steps">ball datalization - number of steps</param>
        /// <returns></returns>
        public static List<Vector3> GetBallContour(double rad, double height, int ball_steps, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };
            if (half)
                cont.Add(new Net3dBool.Vector3(0, 0, height));
            cont.Add(new Net3dBool.Vector3(rad, 0, height));
            cont.Add(new Net3dBool.Vector3(rad, 0, rad));

            for (int q = 1; q <= ball_steps; q++)
            {
                double angle = q * Math.PI / (2 * ball_steps);
                cont.Add(new Net3dBool.Vector3(rad * Math.Cos(angle), 0, rad - rad * Math.Sin(angle)));
            }

            //cont.Add(new Net3dBool.Vector3(0, 0, 0));

            if (half)
                return cont;

            //Clone and rotate points
            List<Net3dBool.Vector3> cont2 = new List<Net3dBool.Vector3> { };
            foreach (Net3dBool.Vector3 v in cont)
                cont2.Add(new Vector3(v));
            Net3dBool.Solid.RotateVertices(ref cont2, Math.PI);

            for (int k = cont2.Count - 1; k >= 0; k--)
                if (cont2[k] != Vector3.Zero)
                    cont.Add(cont2[k]);
            return cont;
        }
        public static List<Vector3> GetBullContour(double rad, double rad_round, double height, int ball_steps, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };
            if (half)
                cont.Add(new Net3dBool.Vector3(0, 0, height));
            cont.Add(new Net3dBool.Vector3(rad, 0, height));
            cont.Add(new Net3dBool.Vector3(rad, 0, rad_round));
            for (int q = 1; q <= ball_steps; q++)
            {
                Net3dBool.Vector3 v0 = new Net3dBool.Vector3(rad_round * Math.Cos(q * Math.PI / (2 * ball_steps)), 0, rad_round - rad_round * Math.Sin(q * Math.PI / (2 * ball_steps)));
                Net3dBool.Vector3 v_origin = new Net3dBool.Vector3(rad - rad_round, 0, 0);
                Net3dBool.Vector3 v_res = v0 + v_origin;
                if (v_res.X >= 0)
                    cont.Add(v_res);
            }
            if (!half)
            {
                if (rad > rad_round)
                {
                    cont.Add(new Net3dBool.Vector3(rad - rad_round, 0, 0));
                    cont.Add(new Net3dBool.Vector3(-(rad - rad_round), 0, 0));
                }
                for (int q = 1; q <= ball_steps; q++)
                {
                    double angle = q * Math.PI / (2 * ball_steps);
                    Net3dBool.Vector3 v0 = new Net3dBool.Vector3(-rad_round * Math.Sin(angle), 0, rad_round - rad_round * Math.Cos(angle));
                    Net3dBool.Vector3 v_origin = new Net3dBool.Vector3(-(rad - rad_round), 0, 0);
                    Net3dBool.Vector3 v_res = v0 + v_origin;
                    if (v_res.X <= 0)
                        cont.Add(v_res);
                }
                cont.Add(new Net3dBool.Vector3(-rad, 0, rad_round));
                cont.Add(new Net3dBool.Vector3(-rad, 0, height));
            }
            else
            {
                if (rad > rad_round)
                {
                    cont.Add(new Net3dBool.Vector3(rad - rad_round, 0, 0));
                    cont.Add(new Net3dBool.Vector3(0, 0, 0));
                }
            }
            return cont;
        }
        /// <summary>
        /// Get the contour for the Drill tool
        /// </summary>
        /// <param name="rad">tool radius</param>
        /// <param name="height">tool height</param>
        /// <param name="angle_deg">tool opening angle in degrees</param>
        /// <returns></returns>
        public static List<Vector3> GetDrillContour(double rad, double height, double angle_deg, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };
            double angle_rad = angle_deg * Math.PI / 180;
            double z = Math.Abs(rad / Math.Tan(angle_rad / 2));
            if (half)
                cont.Add(new Net3dBool.Vector3(0, 0, height + z));
            cont.Add(new Net3dBool.Vector3(rad, 0, height + z));
            cont.Add(new Net3dBool.Vector3(rad, 0, z));
            cont.Add(new Net3dBool.Vector3(0, 0, 0));
            if (!half)
            {
                cont.Add(new Net3dBool.Vector3(-rad, 0, z));
                cont.Add(new Net3dBool.Vector3(-rad, 0, height + z));
            }
            return cont;
        }
        /// <summary>
        ///  Get the contour for the flat tool
        /// </summary>
        /// <param name="rad">tool radius</param>
        /// <param name="height">tool height</param>
        /// <returns></returns>
        public static List<Vector3> GetFlatContour(double rad, double height, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };
            if (!half)
            {
                cont.Add(new Net3dBool.Vector3(rad, 0, height));
                cont.Add(new Net3dBool.Vector3(rad, 0, 0));
                cont.Add(new Net3dBool.Vector3(-rad, 0, 0));
                cont.Add(new Net3dBool.Vector3(-rad, 0, height));
            }
            else
            {
                cont.Add(new Net3dBool.Vector3(0, 0, height));
                cont.Add(new Net3dBool.Vector3(rad, 0, height));
                cont.Add(new Net3dBool.Vector3(rad, 0, 0));
                cont.Add(new Net3dBool.Vector3(0, 0, 0));
            }
            return cont;
        }

        public static List<Vector3> GetVBit1Contour(Dictionary<string, double> VBitParams, int ball_steps, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };

            //Top of the tool
            if (half) // draw upper part only for the tool
            {
                cont.Add(new Net3dBool.Vector3(0, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, 0.5 + VBitParams["S1"]));
            }
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2 + VBitParams["S"], 0, 0.5 + VBitParams["S1"]));
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2 + VBitParams["S"], 0, VBitParams["S1"]));
            //Bigger rounding
            double minx = 0;
            //Circle-circle intersection
            double x0 = VBitParams["D1"] / 2 + VBitParams["R"]; double y0 = VBitParams["H"];
            double x1 = VBitParams["D1"] / 2 + VBitParams["S"]; double y1 = VBitParams["S1"];
            double d = Math.Sqrt(Math.Pow(y1 - y0, 2) + Math.Pow(x1 - x0, 2));
            double r0 = VBitParams["R"];
            double r1 = Math.Sqrt(Math.Pow(d, 2) - Math.Pow(r0, 2)); // as Tangent should point to the edge point,and radius is perpendicular to the tangent
            double a = (Math.Pow(r0, 2) - Math.Pow(r1, 2) + Math.Pow(d, 2)) / (2 * d);
            double h = Math.Sqrt(Math.Pow(r0, 2) - Math.Pow(a, 2));

            double x2 = x0 + a * (x1 - x0) / d; double y2 = y0 + a * (y1 - y0) / d;
            double x3 = x2 - h * (y1 - y0) / d; double y3 = y2 + h * (x1 - x0) / d;

            double ch = Math.Sqrt(Math.Pow(y1 - y3, 2) + Math.Pow(x1 - x3, 2));
            double alpha = Math.Acos(ch / (2 * VBitParams["RR"]));
            double beta = Math.Acos((x1 - x3) / ch);
            double gamma = alpha + beta + Math.PI;

            double x_O = x1 + VBitParams["RR"] * Math.Cos(gamma);
            double z_O = y1 + VBitParams["RR"] * Math.Sin(gamma);

            double b_angle = Math.PI - 2 * alpha;
            double b_angle_0 = Math.Asin((x_O - x1) / VBitParams["RR"]) + Math.PI / 2;

            Net3dBool.Vector3 v_origin = new Net3dBool.Vector3(x_O, 0, z_O);
            for (int q = 1; q <= ball_steps; q++)
            {
                double angle = q * b_angle / ball_steps + b_angle_0;
                Net3dBool.Vector3 v0 = new Net3dBool.Vector3(VBitParams["RR"] * Math.Cos(angle), 0, VBitParams["RR"] * Math.Sin(angle));
                Net3dBool.Vector3 v_res = v0 + v_origin;
                cont.Add(v_res);
                minx = v_res.X;
            }

            b_angle = Math.Atan2(y3 - y0, x0 - x3);
            //small rounding
            v_origin = new Net3dBool.Vector3(VBitParams["D1"] / 2 + VBitParams["R"], 0, VBitParams["H"]);
            for (int q = 1; q <= ball_steps; q++)
            {
                double angle = q * b_angle / ball_steps + Math.PI / 2;
                Net3dBool.Vector3 v0 = new Net3dBool.Vector3(VBitParams["R"] * Math.Cos(angle), 0, VBitParams["R"] * Math.Sin(angle));
                Net3dBool.Vector3 v_res = v0 + v_origin;
                if (minx > v_res.X)
                    cont.Add(v_res);
            }
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2, 0, VBitParams["H"]));
            //Lower part
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2, 0, 0));
            if (half)
                cont.Add(new Net3dBool.Vector3(0, 0, 0));
            if (half)
                return cont;

            //Clone and rotate points
            List<Net3dBool.Vector3> cont2 = new List<Net3dBool.Vector3> { };
            foreach (Net3dBool.Vector3 v in cont)
                cont2.Add(new Vector3(v));
            Net3dBool.Solid.RotateVertices(ref cont2, Math.PI);

            for (int k = cont2.Count - 1; k >= 0; k--)
                cont.Add(cont2[k]);
            return cont;
        }

        public static List<Vector3> GetVBit2Contour(Dictionary<string, double> VBitParams, int ball_steps, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };

            //Top of the tool
            if (half)
                cont.Add(new Net3dBool.Vector3(0, 0, VBitParams["L"]));

            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, VBitParams["L"]));
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, VBitParams["S1"]));
            double b_angle = Math.PI / 2;
            double R = (VBitParams["D"] - VBitParams["D1"]) / 2;
            for (int q = 1; q <= ball_steps; q++)
            {
                double angle = q * b_angle / ball_steps + Math.PI / 2;
                Net3dBool.Vector3 v0 = new Net3dBool.Vector3(R * Math.Cos(angle), 0, R * Math.Sin(angle));
                Net3dBool.Vector3 v_origin = new Net3dBool.Vector3(VBitParams["D1"] / 2 + R, 0, VBitParams["S1"] - R);
                Net3dBool.Vector3 v_res = v0 + v_origin;
                cont.Add(v_res);
            }
            //Lower part
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2, 0, 0));
            if (half)
            {
                cont.Add(new Net3dBool.Vector3(0, 0, 0));
                return cont;
            }

            //Clone and rotate points
            List<Net3dBool.Vector3> cont2 = new List<Net3dBool.Vector3> { };
            foreach (Net3dBool.Vector3 v in cont)
                cont2.Add(new Vector3(v));
            Net3dBool.Solid.RotateVertices(ref cont2, Math.PI);

            for (int k = cont2.Count - 1; k >= 0; k--)
                cont.Add(cont2[k]);
            return cont;
        }
        public static List<Vector3> GetVBit4Contour(Dictionary<string, double> VBitParams, int ball_steps, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };

            //Top of the tool
            if (half) // draw upper part only for the tool
            {
                cont.Add(new Net3dBool.Vector3(0, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, 0.5 + VBitParams["S1"]));
            }
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2 + VBitParams["S"], 0, 0.5 + VBitParams["S1"]));
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2 + VBitParams["S"], 0, VBitParams["S1"]));

            double b_angle = Math.PI / 2;
            double R = VBitParams["R"];
            //small rounding
            for (int q = 1; q <= ball_steps; q++)
            {
                double angle = q * b_angle / ball_steps + Math.PI / 2;
                Net3dBool.Vector3 v0 = new Net3dBool.Vector3(R * Math.Cos(angle), 0, R * Math.Sin(angle));
                Net3dBool.Vector3 v_origin = new Net3dBool.Vector3(VBitParams["D1"] / 2 + VBitParams["R"], 0, VBitParams["H"]);
                Net3dBool.Vector3 v_res = v0 + v_origin;
                cont.Add(v_res);
            }

            //Lower part
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2, 0, 0));
            if (half)
            {
                cont.Add(new Net3dBool.Vector3(0, 0, 0));
                return cont;
            }

            //Clone and rotate points
            List<Net3dBool.Vector3> cont2 = new List<Net3dBool.Vector3> { };
            foreach (Net3dBool.Vector3 v in cont)
                cont2.Add(new Vector3(v));
            Net3dBool.Solid.RotateVertices(ref cont2, Math.PI);

            for (int k = cont2.Count - 1; k >= 0; k--)
                cont.Add(cont2[k]);
            return cont;
        }
        public static List<Vector3> GetVBit5Contour(Dictionary<string, double> VBitParams, int ball_steps, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };

            //Top of the tool
            if (half) // draw upper part only for the tool
            {
                cont.Add(new Net3dBool.Vector3(0, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D2"] / 2, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D2"] / 2, 0, 2 + VBitParams["S1"]));
            }
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, 2 + VBitParams["S1"]));
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, VBitParams["S1"]));

            double b_angle = Math.Atan2(VBitParams["D"] / 2, VBitParams["R"] - VBitParams["S1"]);
            double R = VBitParams["R"];
            //small rounding
            for (int q = 1; q <= ball_steps; q++)
            {
                double angle = -(q * b_angle / ball_steps) - (Math.PI / 2 - b_angle);
                Net3dBool.Vector3 v0 = new Net3dBool.Vector3(R * Math.Cos(angle), 0, R * Math.Sin(angle));
                Net3dBool.Vector3 v_origin = new Net3dBool.Vector3(0, 0, R);
                Net3dBool.Vector3 v_res = v0 + v_origin;
                cont.Add(v_res);
            }
            if (half)
            {
                cont.Add(new Net3dBool.Vector3(0, 0, 0));
                return cont;
            }

            //Clone and rotate points
            List<Net3dBool.Vector3> cont2 = new List<Net3dBool.Vector3> { };
            foreach (Net3dBool.Vector3 v in cont)
                cont2.Add(new Vector3(v));
            Net3dBool.Solid.RotateVertices(ref cont2, Math.PI);

            for (int k = cont2.Count - 1; k >= 0; k--)
                cont.Add(cont2[k]);
            return cont;
        }
        public static List<Vector3> GetVBit6Contour(Dictionary<string, double> VBitParams, int ball_steps, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };
            double h = (VBitParams["D"] - VBitParams["D1"]) * Math.Tan(Math.PI * VBitParams["A"] / 180) / 2;
            //Top of the tool
            if (half) // draw upper part only for the tool
            {
                cont.Add(new Net3dBool.Vector3(0, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D2"] / 2, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D2"] / 2, 0, 2 + h));
            }
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, 2 + h));
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, h));
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2, 0, 0));

            if (half)
            {
                cont.Add(new Net3dBool.Vector3(0, 0, 0));
                return cont;
            }

            //Clone and rotate points
            List<Net3dBool.Vector3> cont2 = new List<Net3dBool.Vector3> { };
            foreach (Net3dBool.Vector3 v in cont)
                cont2.Add(new Vector3(v));
            Net3dBool.Solid.RotateVertices(ref cont2, Math.PI);

            for (int k = cont2.Count - 1; k >= 0; k--)
                cont.Add(cont2[k]);
            return cont;
        }
        public static List<Vector3> GetVBit7Contour(Dictionary<string, double> VBitParams, int ball_steps, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };
            double h = (1.0) * Math.Tan(Math.PI * VBitParams["A"] / 180) / 2;
            double w = (1.0) / Math.Tan(Math.PI * VBitParams["A"] / 180) / 2;
            //Top of the tool
            if (half) // draw upper part only for the tool
            {
                cont.Add(new Net3dBool.Vector3(0, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D2"] / 2, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D2"] / 2, 0, VBitParams["H"] + h + VBitParams["S1"]));
            }
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2 + w, 0, VBitParams["H"] + h + VBitParams["S1"]));
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2, 0, VBitParams["H"] + VBitParams["S1"]));
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2, 0, VBitParams["S1"]));
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, VBitParams["S1"]));
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, 0));

            if (half)
            {
                cont.Add(new Net3dBool.Vector3(0, 0, 0));
                return cont;
            }

            //Clone and rotate points
            List<Net3dBool.Vector3> cont2 = new List<Net3dBool.Vector3> { };
            foreach (Net3dBool.Vector3 v in cont)
                cont2.Add(new Vector3(v));
            Net3dBool.Solid.RotateVertices(ref cont2, Math.PI);

            for (int k = cont2.Count - 1; k >= 0; k--)
                cont.Add(cont2[k]);
            return cont;
        }
        public static List<Vector3> GetVBit8Contour(Dictionary<string, double> VBitParams, int ball_steps, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };
            //Top of the tool
            if (half) // draw upper part only for the tool
            {
                cont.Add(new Net3dBool.Vector3(0, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D2"] / 2, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D2"] / 2, 0, VBitParams["H0"] + 1));
            }
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, VBitParams["H0"] + 1));
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, VBitParams["H0"]));
            double b_angle = Math.PI / 2;
            double R = (VBitParams["D"] - VBitParams["D1"]) / 2;
            for (int q = 1; q <= ball_steps; q++)
            {
                double angle = q * b_angle / ball_steps + Math.PI / 2;
                Net3dBool.Vector3 v0 = new Net3dBool.Vector3(R * Math.Cos(angle), 0, R * Math.Sin(angle));
                Net3dBool.Vector3 v_origin = new Net3dBool.Vector3(VBitParams["D"] / 2, 0, VBitParams["H0"] - R);
                Net3dBool.Vector3 v_res = v0 + v_origin;
                cont.Add(v_res);
            }
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2, 0, VBitParams["S1"]));
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, VBitParams["S1"]));
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, 0));

            if (half)
            {
                cont.Add(new Net3dBool.Vector3(0, 0, 0));
                return cont;
            }

            //Clone and rotate points
            List<Net3dBool.Vector3> cont2 = new List<Net3dBool.Vector3> { };
            foreach (Net3dBool.Vector3 v in cont)
                cont2.Add(new Vector3(v));
            Net3dBool.Solid.RotateVertices(ref cont2, Math.PI);

            for (int k = cont2.Count - 1; k >= 0; k--)
                cont.Add(cont2[k]);
            return cont;
        }
        public static List<Vector3> GetVBit9Contour(Dictionary<string, double> VBitParams, int ball_steps, bool half = false)
        {
            List<Net3dBool.Vector3> cont = new List<Net3dBool.Vector3> { };
            double h = (float)(VBitParams["S1"] + (VBitParams["D1"] - VBitParams["D"]) * Math.Tan(Math.PI * VBitParams["A"] / 180) / 2);
            //Top of the tool
            if (half) // draw upper part only for the tool
            {
                cont.Add(new Net3dBool.Vector3(0, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D2"] / 2, 0, VBitParams["L"]));
                cont.Add(new Net3dBool.Vector3(VBitParams["D2"] / 2, 0, h + 1));
            }
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2, 0, h + 1));
            cont.Add(new Net3dBool.Vector3(VBitParams["D1"] / 2, 0, h));
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, VBitParams["S1"]));
            cont.Add(new Net3dBool.Vector3(VBitParams["D"] / 2, 0, 0));

            if (half)
            {
                cont.Add(new Net3dBool.Vector3(0, 0, 0));
                return cont;
            }

            //Clone and rotate points
            List<Net3dBool.Vector3> cont2 = new List<Net3dBool.Vector3> { };
            foreach (Net3dBool.Vector3 v in cont)
                cont2.Add(new Vector3(v));
            Net3dBool.Solid.RotateVertices(ref cont2, Math.PI);

            for (int k = cont2.Count - 1; k >= 0; k--)
                cont.Add(cont2[k]);
            return cont;
        }
        public static void RoundVerticesCoords(ref List<Vector3> contour, int dec = 5)
        {
            for (int i = 0; i < contour.Count; i++)
            {
                contour[i] = new Vector3(Math.Round(contour[i].X, dec), Math.Round(contour[i].Y, dec), Math.Round(contour[i].Z, dec));
            }
        }
    }
}
