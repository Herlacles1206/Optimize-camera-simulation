using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using netDxf;
using netDxf.Blocks;
using netDxf.Collections;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using netDxf.Units;
using Attribute = netDxf.Entities.Attribute;
using FontStyle = netDxf.Tables.FontStyle;
using Image = netDxf.Entities.Image;
using Point = netDxf.Entities.Point;
using Trace = netDxf.Entities.Trace;

namespace OPTCAMSim
{
    /// <summary>
    /// Class-loader from DXF file
    /// </summary>
    class DXFLoader
    {
        /// <summary>
        /// Gets list of vertices from dxf file with tool definition
        /// </summary>
        /// <param name="file">full path to .dxf file</param>
        /// <param name="output">output log file name</param>
        /// <param name="bulgePrecision">number of points describing bulge</param>
        /// <param name="weldThreshold">weld accuracy</param>
        /// <param name="bulgeThreshold">bulge accuracy</param>
        /// <returns></returns>
        public static List<Vector2> GetDXFPolyVertices(string file, string output, int bulgePrecision, double weldThreshold =0, double bulgeThreshold = 0)
        {
            List<Vector2> res = new List<Vector2> { };

            if (weldThreshold == 0)
                weldThreshold = MathHelper.Epsilon;
            if(bulgeThreshold==0)
                bulgeThreshold = MathHelper.Epsilon;

            // optionally you can save the information to a text file
            bool outputLog = !string.IsNullOrEmpty(output);
            TextWriter writer = null;
            if (outputLog)
            {
                writer = new StreamWriter(File.Create(output));
                Console.SetOut(writer);
            }

            // check if the dxf actually exists
            FileInfo fileInfo = new FileInfo(file);

            if (!fileInfo.Exists)
            {
                Console.WriteLine("THE FILE {0} DOES NOT EXIST", file);
                Console.WriteLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }
            bool isBinary;
            DxfVersion dxfVersion = DxfDocument.CheckDxfFileVersion(file, out isBinary);

            // check if the file is a dxf
            if (dxfVersion == DxfVersion.Unknown)
            {
                Console.WriteLine("THE FILE {0} IS NOT A VALID DXF OR THE DXF DOES NOT INCLUDE VERSION INFORMATION IN THE HEADER SECTION", file);
                Console.WriteLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }

            // check if the dxf file version is supported
            if (dxfVersion < DxfVersion.AutoCad2000)
            {
                Console.WriteLine("THE FILE {0} IS NOT A SUPPORTED DXF", file);
                Console.WriteLine();

                Console.WriteLine("FILE VERSION: {0}", dxfVersion);
                Console.WriteLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }

            Stopwatch watch = new Stopwatch();
            watch.Start();
            DxfDocument dxf = DxfDocument.Load(file, new List<string> { @".\Support" });
            watch.Stop();

            // check if there has been any problems loading the file,
            // this might be the case of a corrupt file or a problem in the library
            if (dxf == null)
            {
                Console.WriteLine("ERROR LOADING {0}", file);
                Console.WriteLine();

                Console.WriteLine("Press a key to continue...");
                Console.ReadLine();

                if (outputLog)
                {
                    writer.Flush();
                    writer.Close();
                }
                else
                {
                    Console.WriteLine("Press a key to continue...");
                    Console.ReadLine();
                }
                return null;
            }

            // the dxf has been properly loaded

            //Read vertices 
            List<List<Vector2>> AllLines = new List<List<Vector2>> { };
            for ( int k = 0; k< dxf.LwPolylines.Count(); k++ )
            {
                LwPolyline l = dxf.LwPolylines.ToList()[k];
                List<Vector2> Verts = l.PolygonalVertexes(bulgePrecision, weldThreshold, bulgeThreshold);
                AllLines.Add(Verts);
            }
            AllLines = AllLines.OrderBy(r => -r.Average(v => v.Y)).ToList(); // order by mean Y desc
            //add right half parts
            for(int q = 0;q<AllLines.Count/2;q++)
            {
                List<Vector2> Verts = new List<Vector2> { };
                if (AllLines[2*q].Average(r => r.X) > AllLines[2*q + 1].Average(r => r.X))
                    Verts = AllLines[2*q];
                else
                    Verts = AllLines[2*q+1];

                bool wrong_order = !CheckYDesc(ref Verts);

                if (!wrong_order)
                    res.AddRange(Verts);
                else
                {
                    for (int k = Verts.Count - 1; k >= 0; k--)
                        res.Add(Verts[k]);
                }
            }
            return res;
        }

        /// <summary>
        /// Normalizes the list of of coordinates (right half of the tool)
        /// </summary>
        /// <param name="Vertices">list of tool vertices</param>
        /// <param name="HScale">horizontal scale (width),mm</param>
        /// <param name="VScale">vertical scale (height),mm</param>
        public static void Normalize(ref List<Vector2> Vertices, double HScale=0, double VScale = 0)
        {
            if (Vertices is null || Vertices.Count == 0)
                return;
            
            double height = Vertices.Max(v => v.Y) - Vertices.Min(v => v.Y);
            double width = 2 * (Vertices.Max(v => v.X) - Vertices.Min(v => v.X));
            double minX = Vertices.Min(v => v.X);
            double minY = Vertices.Min(v => v.Y);
            for(int q=0;q<Vertices.Count;q++)
            {
                double newX = Vertices[q].X - minX;
                double newY = Vertices[q].Y - minY;
                if (HScale != 0)
                    newX *= HScale / width;
                if (VScale != 0)
                    newY *= VScale / height;

                Vertices[q] = new Vector2(newX, newY);
            }

            FilterPosXY(ref Vertices);

            AddMissingAndReorder(ref Vertices);
        }
        /// <summary>
        /// Filters XY by non-negative values
        /// </summary>
        /// <param name="Vertices"></param>
        public static void FilterPosXY(ref List<Vector2> Vertices)
        {
            if (Vertices is null || Vertices.Count == 0)
                return;
            List<Vector2> NEW_Vertices = new List<Vector2> { };
            foreach(Vector2 v in Vertices)
            {
                if (v.X >= 0 && v.Y >= 0)
                    NEW_Vertices.Add(v);
            }
            Vertices = NEW_Vertices;
        }
        /// <summary>
        /// Adds missing beginning and end points
        /// </summary>
        /// <param name="Vertices"></param>
        public static void AddMissingAndReorder(ref List<Vector2> Vertices)
        {           
            if (Vertices is null || Vertices.Count == 0)
                return;

            bool wrong_order =!CheckYDesc(ref Vertices);
            if (wrong_order)
            {
                List<Vector2> NEW_Vertices = new List<Vector2> { };

                for (int q = 0; q < Vertices.Count; q++)
                    NEW_Vertices.Add(Vertices[Vertices.Count - 1 - q]);

                Vertices = NEW_Vertices;
            }

            //Check if there is Zero point
            if (Vertices.Where(v => v.X == 0 && v.Y == 0).Count() == 0)
                Vertices.Add(new Vector2());
            if (Vertices[0].X != 0)
                Vertices.Insert(0, new Vector2(0, Vertices[0].Y));

            Vertices = Vertices.Distinct().ToList();
        }
        /// <summary>
        /// Check if Y is descending
        /// </summary>
        /// <param name="Vertices"></param>
        /// <returns></returns>
        public static bool CheckYDesc(ref List<Vector2> Vertices)
        {
            bool res = false;
            if (Vertices is null || Vertices.Count == 0)
                return res;
            double deltaY = Vertices[Vertices.Count - 1].Y - Vertices[0].Y;

            return deltaY<=0;
        }
    }
}
