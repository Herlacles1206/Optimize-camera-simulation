using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net3dBool
{
    public class Octree
    {
        public List<Octree> Cells = new List<Octree> { };
        public List<Face> Faces1 = new List<Face> { };
        public List<Face> Faces2 = new List<Face> { };
        public Vector3 Vmin, Vmax;
        public Bound _Bound;
        public int Depth = 1;
        public static int maxDepth = 8;
        public Octree(Mesh o1, Mesh o2, int depth = 1)
        {
            Depth = depth;
            if (o1._Bound.Overlap(o2._Bound) && o1.Faces.Count>0 && o2.Faces.Count>0)
            {
                //Create intersection volume
                Vmin = new Vector3(Math.Max(o1._Bound.XMin, o2._Bound.XMin),
                                    Math.Max(o1._Bound.YMin, o2._Bound.YMin),
                                    Math.Max(o1._Bound.ZMin, o2._Bound.ZMin));
                Vmax = new Vector3(Math.Min(o1._Bound.XMax, o2._Bound.XMax),
                                    Math.Min(o1._Bound.YMax, o2._Bound.YMax),
                                    Math.Min(o1._Bound.ZMax, o2._Bound.ZMax));
                _Bound = new Bound(Vmin, Vmax, Vmax);
                //Fill faces
                Faces1.AddRange(o1.Faces.Where(f => _Bound.Overlap(f.GetBound())).Select(x =>
                {
                    x.meshIndex = 1;
                    return x;
                }));

                Faces2.AddRange(o2.Faces.Where(f => _Bound.Overlap(f.GetBound())).Select(x =>
                {
                    x.meshIndex = 2;
                    return x;
                }));

                //Check and enlarge volume if needed
                /*
                double xmin, ymin, zmin, xmax, ymax, zmax;
                List<Vector3> all_verts = new List<Vector3>(Faces1.SelectMany(x => x.ToListVector3()));
                all_verts.AddRange(Faces2.SelectMany(x => x.ToListVector3()));
                all_verts = all_verts.Distinct().ToList();
                xmin = all_verts.Min(v => v.X);
                ymin = all_verts.Min(v => v.Y);
                zmin = all_verts.Min(v => v.Z);
                xmax = all_verts.Max(v => v.X);
                ymax = all_verts.Max(v => v.Y);
                zmax = all_verts.Max(v => v.Z);
                if (xmin < Vmin.X)
                    Vmin.X = xmin;
                if (ymin < Vmin.Y)
                    Vmin.Y = ymin;
                if (zmin < Vmin.Z)
                    Vmin.Z = zmin;

                if (xmax > Vmax.X)
                    Vmax.X = xmax;
                if (ymax > Vmax.Y)
                    Vmax.Y = ymax;
                if (zmax > Vmax.Z)
                    Vmax.Z = zmax;

                _Bound = new Bound(Vmin, Vmax, Vmax);
                */
                //Create cells
                if (Depth < maxDepth)
                {
                    if ((Faces1.Count > 50 || Faces2.Count > 50) && (Faces1.Count != 0 && Faces2.Count != 0))
                    {
                        double dx = (_Bound.XMax - _Bound.XMin) / 2;
                        double dy = (_Bound.YMax - _Bound.YMin) / 2;
                        double dz = (_Bound.ZMax - _Bound.ZMin) / 2;

                        for (int i = 0; i < 2; i++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                for (int k = 0; k < 2; k++)
                                {
                                    Vector3 cMin = new Vector3(Vmin.X + i * dx, Vmin.Y + j * dy, Vmin.Z + k * dz);
                                    Vector3 cMax = new Vector3(Vmin.X + (i + 1) * dx, Vmin.Y + (j + 1) * dy, Vmin.Z + (k + 1) * dz);
                                    Bound _cB = new Bound(cMin, cMax, cMax);
                                    Mesh _co1 = new Mesh(Faces1.Where(x => _cB.Overlap(x.GetBound())).ToList());
                                    Mesh _co2 = new Mesh(Faces2.Where(x => _cB.Overlap(x.GetBound())).ToList());
                                    Cells.Add(new Octree(_co1, _co2, Depth + 1));
                                }
                            }
                        }
                        //Propogate cells to upper levels
                        if(Cells.Where(c => c.Cells.Count > 0).Any())
                            Cells = Cells.Where(c => c.Cells.Count > 0).SelectMany(c => c.Cells).
                                Union(Cells.Where(c => c.Cells.Count == 0)).ToList();
                        if (Depth == 1)
                            Cells = Cells.Where(x => x.Faces1.Count > 0 && x.Faces2.Count > 0).ToList();

                        
                        //Depth++;
                    }
                }
                if (Cells.Count == 0) // simple case
                    Cells.Add(this);
            }
            else
            {
            }
        }
    }
}
