using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Poly2Tri;

namespace Net3dBool
{
    class Point
    {
        public double x;
        public double y;

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    };

    /// <summary>
    /// Class to store simplified segment representation - start and end
    /// </summary>
    public partial class SSegment {
        public Vector3 start;
        public Vector3 end;
        public double LengthSquared { get {
                if (!CachedLen)
                {
                    LengthSquaredCache = (end - start).LengthSquared;
                    CachedLen = true;
                }
                return LengthSquaredCache; } }
        private bool CachedLen = false;
        private double LengthSquaredCache;

        public Vector3 V
        {
            get
            {
                if (!CachedV)
                {
                    VCache = (end - start);
                    CachedV = true;
                }
                return VCache;
            }
        }
        private bool CachedV = false;
        private Vector3 VCache;
        public SSegment(Vector3 _start, Vector3 _end)
        {
            start = _start;
            end = _end;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SSegment))
                return false;

            return Equals((SSegment)obj);
        }

        public bool Equals(SSegment other)
        {
            if (start != other.start)
                return false;
            
            return end == other.end;
        }

        public static bool operator ==(SSegment s1, SSegment s2)
        {
            return s1.Equals(s2);
        }

        public static bool operator !=(SSegment s1, SSegment s2)
        {
            return !s1.Equals(s2);
        }

        public override int GetHashCode()
        {
            return this.start.GetHashCode()+ this.end.GetHashCode();
        }

    };
    public class SplitTriangle
    {
        static double INF = 1e6;

        public static double EqualityTolerance = 1e-6;
        ///initial triangle vertices
        public Vector3 p0, p1, p2;
        //orthonormal basis for 2D-representation
        public Vector3 e0, e1, e2;
        double[][] To2dMatrix;
        double[][] From2dMatrix;
        //2d vertex coordinates
        public Vector3 v0, v1, v2;
        public List<List<SSegment>> subFaces = new List<List<SSegment>> { };
        public List<SplitTriangle> Triangles = new List<SplitTriangle> { };
        public List<List<SSegment>> IntersectionLoops = new List<List<SSegment>> { };
        public Face ParentFace;
        
        public SplitTriangle(Face f)
        {
            p0 = f.V1.Position;
            p1 = f.V2.Position;
            p2 = f.V3.Position;
            ParentFace = f;
            
            To2D();
        }
        public SplitTriangle(Vector3 _p0, Vector3 _p1, Vector3 _p2)
        {
            p0 = _p0;
            p1 = _p1;
            p2 = _p2;
            //To2D();
        }

        /// <summary>
        /// Prepare matrices for 3D<->2D transformations in the triangle basis
        /// </summary>
        private void To2D()
        {
            e0 = p1 - p0;
            e0.Normalize();
            e2 = ParentFace!=null?ParentFace.GetNormal():Vector3.Cross( e0, (p2 - p0));
            e2.Normalize();
            e1 = Vector3.Cross(e0, e2);
            e1.Normalize();
            Vector3 O = p0;

            double x0 = 0;
            double y0 = 0;
            double x1 = (p1 - O).Length;
            double y1 = 0;
            double x2 = Vector3.Dot((p2 - O), e0);
            double y2 = Vector3.Dot((p2 - O), e1);
            v0 = new Vector3() { X = x0, Y = y0 };
            v1 = new Vector3() { X = x1, Y = y1 };
            v2 = new Vector3() { X = x2, Y = y2 };

            From2dMatrix = new double[][] { new double[] { e0.X, e0.Y, e0.Z }, 
                                            new double[] { e1.X, e1.Y, e1.Z }, 
                                            new double[] { e2.X, e2.Y, e2.Z} };
            To2dMatrix = Matrix.MatrixInverse(From2dMatrix);
        }
        /// <summary>
        /// Change vector coordinate system to triangle basis - 2D if point is in triangle plane
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector3 To2DByTriangleBasis(Vector3 p)
        {
            Vector3 rel = p - p0;
            return new Vector3(rel.X * To2dMatrix[0][0] + rel.Y * To2dMatrix[1][0] + rel.Z * To2dMatrix[2][0], 
                rel.X * To2dMatrix[0][1] + rel.Y * To2dMatrix[1][1] + rel.Z * To2dMatrix[2][1],
                rel.X * To2dMatrix[0][2] + rel.Y * To2dMatrix[1][2] + rel.Z * To2dMatrix[2][2]); 
        }
        /// <summary>
        /// Change vector coordinate system to regular basis from triangle basis
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public Vector3 To3DByTriangleBasis(Vector3 p)
        {
            return new Vector3(p.X * From2dMatrix[0][0] + p.Y * From2dMatrix[1][0] + p.Z * From2dMatrix[2][0],
                p.X * From2dMatrix[0][1] + p.Y * From2dMatrix[1][1] + p.Z * From2dMatrix[2][1],
                p.X * From2dMatrix[0][2] + p.Y * From2dMatrix[1][2] + p.Z * From2dMatrix[2][2])
                            + p0;
        }

        public void PrepareLoops()
        {
            if(ParentFace!= null && ParentFace.IntersectionLoops.Count>0)
            {
                IntersectionLoops = ParentFace.IntersectionLoops.Select(l => l.Select(s =>
                  new SSegment( To2DByTriangleBasis(s.StartPosition),
                                    To2DByTriangleBasis(s.EndPosition))).ToList()).ToList();
                IntersectionLoops.ForEach(l => l.RemoveAll(s => s.LengthSquared < EqualityTolerance * EqualityTolerance));
            }
        }
        public void SplitByLoops()
        {
            // ADD PARENT TRIANGLE
            subFaces.Clear();
            subFaces.Add(new List<SSegment> { new SSegment( v0, v1 ),
                                              new SSegment( v1, v2 ),
                                              new SSegment( v2, v0 ) });

            // Split by unclosed loops
            List<List<SSegment>> unclosedLoops = IntersectionLoops.Where(l => (l.First().start - l.Last().end).Length > EqualityTolerance).ToList();
            SplitByUnclosedLoops(subFaces, unclosedLoops);

            List<List<SSegment>> closedLoops = IntersectionLoops.Where(l => (l.First().start - l.Last().end).Length <= EqualityTolerance).ToList();

            //triangulate obtained subfaces
            if (closedLoops.Count == 0)
            {
                foreach (List<SSegment> f in subFaces)
                {
                    Polygon poly = new Polygon(f.Select(s => new PolygonPoint(Math.Abs(s.start.X) < EqualityTolerance ? 0 : s.start.X
                        , Math.Abs(s.start.Y) < EqualityTolerance ? 0 : s.start.Y)).ToList());
                    TriangulateAndAddTrinagles(poly);
                }
            }
            
            // Split by closed loops
            foreach (List<SSegment> l in closedLoops)
            {
                int rel_sub = -1;
                //determine, which subface contains current loop
                if (subFaces.Count > 1)
                {
                    List<Tuple<int, int>> subs_hits = new List<Tuple<int, int>> { };
                    int max_hits = 0;
                    for (int k = 0; k < subFaces.Count; k++)
                    {
                        int hits = 0;
                        List<Vector3> ll = l.SelectMany(x => new List<Vector3> { x.start, x.end }).Distinct().ToList();
                        foreach (Vector3 v in ll)
                            if (isInside(subFaces[k], v))
                                hits++;
                        subs_hits.Add(new Tuple<int, int>(k, hits));
                        if (hits > max_hits)
                            max_hits = hits;
                        if (hits == ll.Count) // all point inside
                            break;
                    }
                    rel_sub = subs_hits.Where(x => x.Item2 == max_hits).First().Item1;
                }
                else
                    rel_sub = 0;

                //check if any edge is touched by closed loop (Poly2Tri does not work with points on edge)
                //intersection segment, crossed edge
                List<Tuple<int, int>> edges_hits = new List<Tuple<int, int>> { };
                for (int k = 0; k < l.Count; k++)
                {
                    int ind = GetOnEdgeIndex(l[k].end, subFaces[rel_sub]);
                    if (ind != -1)
                        edges_hits.Add(new Tuple<int, int>(k, ind));
                }

                if (edges_hits.Count == 0) // Hole is inside polygon entirely
                {
                    //Triangulate polygon with hole                    
                    Polygon poly = new Polygon(subFaces[rel_sub].Select(s => new PolygonPoint(s.start.X, s.start.Y)).ToList());
                    Polygon hole = new Polygon(l.Select(s => new PolygonPoint(s.start.X, s.start.Y)).ToList());
                    poly.AddHole(hole);
                    TriangulateAndAddTrinagles(poly);

                    //Triangulate hole itself and add triangles
                    TriangulateAndAddTrinagles(hole);

                }
                else if (edges_hits.Count == 1) // One touching point - make small gap and consider as unclosed (lack of accuracy)
                {
                    int seg = edges_hits[0].Item1;
                    int edge = edges_hits[0].Item2;
                    List<SSegment> old_sub = subFaces[rel_sub];
                    List<SSegment> new_sub = new List<SSegment> { };
                    //add all edges but one crossed
                    new_sub.AddRange(old_sub.Where(x => old_sub.IndexOf(x) != edge));
                    //small gap
                    Vector3 dv = (old_sub[edge].start - l[seg].end).GetNormal() * EqualityTolerance;
                    new_sub.Add(new SSegment(old_sub[edge].start, l[seg].end + dv));
                    new_sub.AddRange(l.Where(x => l.IndexOf(x) > seg));
                    new_sub.AddRange(l.Where(x => l.IndexOf(x) < seg));
                    new_sub.Add(new SSegment(l[seg].end - dv,old_sub[edge].end));

                    new_sub.RemoveAll(x => x.LengthSquared < EqualityTolerance * EqualityTolerance);
                    new_sub = new_sub.Distinct().ToList();

                    //new sub-faces - at least triangles
                    if (new_sub.Count >= 3)
                    {
                        subFaces[rel_sub] = new_sub;
                    }

                    //triangulate new sub
                    Polygon poly = new Polygon(subFaces[rel_sub].Select(s => new PolygonPoint(s.start.X, s.start.Y)).ToList());
                    TriangulateAndAddTrinagles(poly);

                }
                else // Several edges touched - split loop on several unclosed loops and process them
                {
                    //sort by segment number
                    edges_hits = edges_hits.OrderBy(x => x.Item1).ToList();
                    List < List <SSegment>> new_inter_loops = new List<List<SSegment>>();
                    for(int k =0;k<edges_hits.Count;k++)
                    {
                        int next_seg = k < edges_hits.Count - 1 ? edges_hits[k+1].Item1 : edges_hits[0].Item1;
                        new_inter_loops.Add(l.Where(x => l.IndexOf(x) > edges_hits[k].Item1 && l.IndexOf(x) <= next_seg).ToList());
                    }
                    //Split current face by new list of unclosed loops
                    int count_before = subFaces.Count;
                    SplitByUnclosedLoops(new List<List<SSegment>> {subFaces[rel_sub] },new_inter_loops);
                    int count_after = subFaces.Count;
                    for(int k = rel_sub;k<= rel_sub + (count_after - count_before);k++ )
                    {
                        List<SSegment> f = subFaces[k];
                        Polygon poly = new Polygon(f.Select(s => new PolygonPoint(s.start.X, s.start.Y)).ToList());
                        TriangulateAndAddTrinagles(poly);
                    }
                }
            }
            //Convert back to 3D
            foreach(SplitTriangle t in Triangles)
            {
                t.p0 = To3DByTriangleBasis(t.p0);
                t.p1 = To3DByTriangleBasis(t.p1);
                t.p2 = To3DByTriangleBasis(t.p2);
            }
        }

        public void SplitByUnclosedLoops(List<List<SSegment>> subFaces, List<List<SSegment>> loops)
        {
            // Split by unclosed loops
            foreach (List<SSegment> l in loops)
            {
                List<SSegment> lt0 = new List<SSegment>(l);
                //Collect segments, while they do not hit an edge
                int used_segments = 0;
                while (used_segments < lt0.Count)
                {
                    List<SSegment> lt = new List<SSegment>();
                    for (int k = used_segments; k < lt0.Count; k++)
                    {
                        bool is_final = false;
                        if (k < lt0.Count - 1) // if there are more segments - check
                        {
                            for (int q = 0; q < subFaces.Count; q++)
                            {
                                if (GetOnEdgeIndex(lt0[k].end, subFaces[q]) != -1)
                                {
                                    is_final = true;
                                    break;
                                }
                            }
                        }
                        lt.Add(new SSegment(lt0[k].start, lt0[k].end));
                        used_segments++;
                        if (is_final)
                            break;
                    }

                    SSegment first = lt.First();
                    SSegment last = lt.Last();
                    for (int k = 0; k < subFaces.Count; k++)
                    {
                        List<SSegment> sub = subFaces[k];
                        int start_edge = GetOnEdgeIndex(first.start, sub);
                        int end_edge = GetOnEdgeIndex(last.end, sub);
                        //If two edges crossed
                        if (start_edge >= 0 && end_edge >= 0 && (start_edge != end_edge || start_edge != GetOnEdgeIndex(last.start, sub)))
                        {
                            if (start_edge > end_edge)
                            {
                                int t = start_edge;
                                start_edge = end_edge;
                                end_edge = t;
                                lt.Reverse();
                                lt = lt.Select(s => new SSegment(s.end, s.start)).ToList();
                                first = lt.First();
                                last = lt.Last();
                            }
                            List<SSegment> new_sub = new List<SSegment> { };
                            List<SSegment> new_sub2 = new List<SSegment> { };
                            if (start_edge != end_edge)
                            {
                                new_sub.Add(new SSegment(sub[start_edge].start, first.start));
                                new_sub.AddRange(lt);
                                new_sub.Add(new SSegment(last.end, sub[end_edge].end));
                                new_sub.AddRange(sub.Where(x => sub.IndexOf(x) > end_edge));
                                new_sub.AddRange(sub.Where(x => sub.IndexOf(x) < start_edge));

                                new_sub2.Add(new SSegment(first.start, sub[start_edge].end));
                                new_sub2.AddRange(sub.Where(x => sub.IndexOf(x) > start_edge && sub.IndexOf(x) < end_edge));
                                new_sub2.Add(new SSegment(sub[end_edge].start, last.end));

                                lt.Reverse();
                                lt = lt.Select(s => new SSegment(s.end, s.start)).ToList();

                                new_sub2.AddRange(lt);
                            }
                            else
                            {
                                new_sub.AddRange(lt);
                                new_sub.Add(new SSegment(last.end,first.start));

                                if ((last.end - sub[end_edge].start).Length < (first.start - sub[end_edge].start).Length)
                                {
                                    new_sub2.Add(new SSegment(first.start, sub[start_edge].end));
                                    new_sub2.AddRange(sub.Where(x => sub.IndexOf(x) > start_edge));
                                    new_sub2.AddRange(sub.Where(x => sub.IndexOf(x) < end_edge));
                                    new_sub2.Add(new SSegment(sub[end_edge].start, last.end));

                                    lt.Reverse();
                                    lt = lt.Select(s => new SSegment(s.end, s.start)).ToList();
                                    new_sub2.AddRange(lt);
                                }
                                else
                                {
                                    new_sub2.Add(new SSegment(last.end, sub[start_edge].end));
                                    new_sub2.AddRange(sub.Where(x => sub.IndexOf(x) > start_edge));
                                    new_sub2.AddRange(sub.Where(x => sub.IndexOf(x) < start_edge));
                                    new_sub2.Add(new SSegment(sub[end_edge].start, first.start));
                                    new_sub2.AddRange(lt);
                                }
                            }

                            new_sub.RemoveAll(x => x.LengthSquared < EqualityTolerance * EqualityTolerance);
                            new_sub2.RemoveAll(x => x.LengthSquared < EqualityTolerance * EqualityTolerance);

                            new_sub = new_sub.Distinct().ToList();
                            new_sub2 = new_sub2.Distinct().ToList();

                            //new sub-faces - at least triangles
                            if (new_sub.Count >= 3 && new_sub2.Count >= 3)
                            {
                                subFaces.RemoveAt(k);
                                subFaces.Insert(k, new_sub);
                                subFaces.Insert(k + 1, new_sub2);
                                k++;
                            }
                        }
                    }
                }
            }
        }

        private void TriangulateAndAddTrinagles(Polygon poly)
        {
            try
            {
                PolygonInfo info = new PolygonInfo("Poly", poly);
                info.Triangulate();
            
                List<DelaunayTriangle> tri = info.Polygon.Triangles.ToList();
                foreach (DelaunayTriangle t in tri)
                {
                    SplitTriangle st = new SplitTriangle(
                        new Vector3(t.Points[0].X, t.Points[0].Y, 0),
                        new Vector3(t.Points[1].X, t.Points[1].Y, 0),
                        new Vector3(t.Points[2].X, t.Points[2].Y, 0));
                    Triangles.Add(st);
                }
            }
            catch(Exception ex)
            {
                int q = 0;
            }
        }
        public int GetOnEdgeIndex(Vector3 point, List<SSegment> edges)
        {
            for (int k = 0; k < edges.Count; k++)
            {
                if (GetEdgeDistance(point, edges[k]) < EqualityTolerance)
                    return k;
            }

            return -1;
        }
        /// <summary>
        /// Return min distance from point to edge (2D!)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="edge_index"></param>
        /// <returns></returns>
        public double GetEdgeDistance(Vector3 point, SSegment edge)
        {            
            double l2 = edge.LengthSquared;
            Vector3 v = edge.start;         

            if (l2 == 0.0) return (v-point).Length;   // v == w case
                                                  // Consider the line extending the segment, parameterized as v + t (w - v).
                                                  // We find projection of point p onto the line. 
                                                  // It falls where t = [(p-v) . (w-v)] / |w-v|^2
                                                  // We clamp t from [0,1] to handle points outside the segment vw.
            double t = Math.Max(0, Math.Min(1, Vector3.Dot(point - v, edge.V) / l2));
            Vector3 projection = v + t * edge.V;  // Projection falls on the segment
            return (projection-point).Length;
        }

        #region Point-in-Polygon Methods
        // Refer https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/ 
        // Given three colinear points p, q, r,  
        // the function checks if point q lies 
        // on line segment 'pr' 
        static bool onSegment(Point p, Point q, Point r)
        {
            if (q.x <= Math.Max(p.x, r.x) &&
                q.x >= Math.Min(p.x, r.x) &&
                q.y <= Math.Max(p.y, r.y) &&
                q.y >= Math.Min(p.y, r.y))
            {
                return true;
            }
            return false;
        }

        // To find orientation of ordered triplet (p, q, r). 
        // The function returns following values 
        // 0 --> p, q and r are colinear 
        // 1 --> Clockwise 
        // 2 --> Counterclockwise 
        static int orientation(Point p, Point q, Point r)
        {
            int val =(int) ((q.y - p.y) * (r.x - q.x) -
                      (q.x - p.x) * (r.y - q.y));

            if (val == 0)
            {
                return 0; // colinear 
            }
            return (val > 0) ? 1 : 2; // clock or counterclock wise 
        }

        // The function that returns true if  
        // line segment 'p1q1' and 'p2q2' intersect. 
        static bool doIntersect(Point p1, Point q1,
                                Point p2, Point q2)
        {
            // Find the four orientations needed for  
            // general and special cases 
            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            // General case 
            if (o1 != o2 && o3 != o4)
            {
                return true;
            }

            // Special Cases 
            // p1, q1 and p2 are colinear and 
            // p2 lies on segment p1q1 
            if (o1 == 0 && onSegment(p1, p2, q1))
            {
                return true;
            }

            // p1, q1 and p2 are colinear and 
            // q2 lies on segment p1q1 
            if (o2 == 0 && onSegment(p1, q2, q1))
            {
                return true;
            }

            // p2, q2 and p1 are colinear and 
            // p1 lies on segment p2q2 
            if (o3 == 0 && onSegment(p2, p1, q2))
            {
                return true;
            }

            // p2, q2 and q1 are colinear and 
            // q1 lies on segment p2q2 
            if (o4 == 0 && onSegment(p2, q1, q2))
            {
                return true;
            }

            // Doesn't fall in any of the above cases 
            return false;
        }

        static bool isInside(List<SSegment> polygon, Vector3 point)
        {
            Point[] poly = polygon.SelectMany(x => new List<Vector3> { x.start, x.end }).Distinct().Select(x=>new Point(x.X,x.Y)).ToArray();
            return isInside(poly, poly.Length, new Point(point.X, point.Y));
        }
        // Returns true if the point p lies  
        // inside the polygon[] with n vertices 
        static bool isInside(Point[] polygon, int n, Point p)
        {
            // There must be at least 3 vertices in polygon[] 
            if (n < 3)
            {
                return false;
            }

            // Create a point for line segment from p to infinite 
            Point extreme = new Point(INF, p.y);

            // Count intersections of the above line  
            // with sides of polygon 
            int count = 0, i = 0;
            do
            {
                int next = (i + 1) % n;

                // Check if the line segment from 'p' to  
                // 'extreme' intersects with the line  
                // segment from 'polygon[i]' to 'polygon[next]' 
                if (doIntersect(polygon[i],
                                polygon[next], p, extreme))
                {
                    // If the point 'p' is colinear with line  
                    // segment 'i-next', then check if it lies  
                    // on segment. If it lies, return true, otherwise false 
                    if (orientation(polygon[i], p, polygon[next]) == 0)
                    {
                        return onSegment(polygon[i], p,
                                         polygon[next]);
                    }
                    count++;
                }
                i = next;
            } while (i != 0);

            // Return true if count is odd, false otherwise 
            return (count % 2 == 1); // Same as (count%2 == 1) 
        }
        #endregion

    }
}
