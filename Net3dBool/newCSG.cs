using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net3dBool
{
    public class newCSG
    {
        Mesh o1, o2;
        Net3dBool.Octree oc;
        public static float EqualityTolerance = 1e-6f;
        public newCSG(Mesh _o1, Mesh _o2)
        {
            this.o1 = _o1;
            this.o2 = _o2;
            this.oc = new Net3dBool.Octree(o1, o2);

            //Create intersection segments
            CreateIntersections();

            //Cleanup intersections: remove repeats
            CleanupIntersections(oc.Faces1);
            CleanupIntersections(oc.Faces2);

            //Form Intersection loops
            FormLoops(oc.Faces1);
            FormLoops(oc.Faces2);

            //Split all faces by intersection segments
            SplitByLoops(oc.Faces1);
            SplitByLoops(oc.Faces2);

            //Classify faces
            Classify(o1.Faces, o2.Faces, 1);
            Classify(o2.Faces, o1.Faces, 2);
        }

        private void CreateIntersections()
        {
            //ToDO: Parallelize for cells and different triangle pairs
            //Generate intersection lines
            foreach (Octree cell in oc.Cells)
            {
                foreach (Face face1 in cell.Faces1)
                {
                    foreach (Face face2 in cell.Faces2)
                    {
                        Line line;
                        Segment segment1;
                        Segment segment2;
                        double distFace1Vert1, distFace1Vert2, distFace1Vert3, distFace2Vert1, distFace2Vert2, distFace2Vert3;
                        int signFace1Vert1, signFace1Vert2, signFace1Vert3, signFace2Vert1, signFace2Vert2, signFace2Vert3;

                        //PART I - DO TWO POLIGONS INTERSECT?
                        //POSSIBLE RESULTS: INTERSECT, NOT_INTERSECT, COPLANAR

                        //distance from the face1 vertices to the face2 plane
                        distFace1Vert1 = Object3D.ComputeDistance(face1.V1, face2);
                        distFace1Vert2 = Object3D.ComputeDistance(face1.V2, face2);
                        distFace1Vert3 = Object3D.ComputeDistance(face1.V3, face2);

                        //distances signs from the face1 vertices to the face2 plane
                        signFace1Vert1 = (distFace1Vert1 > EqualityTolerance ? 1 : (distFace1Vert1 < -EqualityTolerance ? -1 : 0));
                        signFace1Vert2 = (distFace1Vert2 > EqualityTolerance ? 1 : (distFace1Vert2 < -EqualityTolerance ? -1 : 0));
                        signFace1Vert3 = (distFace1Vert3 > EqualityTolerance ? 1 : (distFace1Vert3 < -EqualityTolerance ? -1 : 0));

                        //if all the signs are zero, the planes are coplanar
                        //if all the signs are positive or negative, the planes do not intersect
                        //if the signs are not equal...
                        if (!(signFace1Vert1 == signFace1Vert2 && signFace1Vert2 == signFace1Vert3))
                        {
                            //distance from the face2 vertices to the face1 plane
                            distFace2Vert1 = Object3D.ComputeDistance(face2.V1, face1);
                            distFace2Vert2 = Object3D.ComputeDistance(face2.V2, face1);
                            distFace2Vert3 = Object3D.ComputeDistance(face2.V3, face1);

                            //distances signs from the face2 vertices to the face1 plane
                            signFace2Vert1 = (distFace2Vert1 > EqualityTolerance ? 1 : (distFace2Vert1 < -EqualityTolerance ? -1 : 0));
                            signFace2Vert2 = (distFace2Vert2 > EqualityTolerance ? 1 : (distFace2Vert2 < -EqualityTolerance ? -1 : 0));
                            signFace2Vert3 = (distFace2Vert3 > EqualityTolerance ? 1 : (distFace2Vert3 < -EqualityTolerance ? -1 : 0));

                            //if the signs are not equal...
                            if (!(signFace2Vert1 == signFace2Vert2 && signFace2Vert2 == signFace2Vert3))
                            {
                                line = new Line(face1, face2);

                                //intersection of the face1 and the plane of face2
                                segment1 = new Segment(line, face1, signFace1Vert1, signFace1Vert2, signFace1Vert3);

                                //intersection of the face2 and the plane of face1
                                segment2 = new Segment(line, face2, signFace2Vert1, signFace2Vert2, signFace2Vert3);

                                //if the two segments intersect...
                                if (segment1.Intersect(segment2))
                                {
                                    Segment overlap = segment1.GetTwoSegmentsOverlap(segment2);
                                    
                                    //Mark all vertices
                                    Status s1, s2, s3, s4, s5, s6;
                                    s1 = face1.V1.Status;
                                    s2 = face1.V2.Status;
                                    s3 = face1.V3.Status;
                                    s4 = signFace1Vert1 == 1 ? Status.OUTSIDE : signFace1Vert1 == 0 ? Status.OPPOSITE : Status.INSIDE;
                                    s5 = signFace1Vert2 == 1 ? Status.OUTSIDE : signFace1Vert2 == 0 ? Status.OPPOSITE : Status.INSIDE;
                                    s6 = signFace1Vert3 == 1 ? Status.OUTSIDE : signFace1Vert3 == 0 ? Status.OPPOSITE : Status.INSIDE;
                                        
                                    if (!(s1 == Status.OUTSIDE && s4 == Status.INSIDE) && !(s1 == Status.OPPOSITE))
                                        face1.V1.Mark(s4);
                                    if (!(s2 == Status.OUTSIDE && s5 == Status.INSIDE) && !(s2 == Status.OPPOSITE))
                                        face1.V2.Mark(s5);
                                    if (!(s3 == Status.OUTSIDE && s6 == Status.INSIDE) && !(s3 == Status.OPPOSITE))
                                        face1.V3.Mark(s6);
                                        

                                    Status s21, s22, s23, s24, s25, s26;
                                    s21 = face2.V1.Status;
                                    s22 = face2.V2.Status;
                                    s23 = face2.V3.Status;
                                    s24 = signFace2Vert1 == 1 ? Status.OUTSIDE : signFace2Vert1 == 0 ? Status.OPPOSITE : Status.INSIDE;
                                    s25 = signFace2Vert2 == 1 ? Status.OUTSIDE : signFace2Vert2 == 0 ? Status.OPPOSITE : Status.INSIDE;
                                    s26 = signFace2Vert3 == 1 ? Status.OUTSIDE : signFace2Vert3 == 0 ? Status.OPPOSITE : Status.INSIDE;

                                    if (!(s21 == Status.OUTSIDE && s24 == Status.INSIDE) && !(s21 == Status.OPPOSITE))
                                        face2.V1.Mark(s24);
                                    if (!(s22 == Status.OUTSIDE && s25 == Status.INSIDE) && !(s22 == Status.OPPOSITE))
                                        face2.V2.Mark(s25);
                                    if (!(s23 == Status.OUTSIDE && s26 == Status.INSIDE) && !(s23 == Status.OPPOSITE))
                                        face2.V3.Mark(s26);

                                    //Add intersection segments
                                    //Do not add points
                                    if ((overlap.StartPosition - overlap.EndPosition).Length > EqualityTolerance)
                                    {
                                        face1.IntersectionSegments.Add(overlap.Clone());
                                        face2.IntersectionSegments.Add(overlap.Clone());
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            }
        }

        private void CleanupIntersections(List<Face> Faces)
        {
            foreach (Face f in Faces)
            {
                //remove duplicates
                for (int k = f.IntersectionSegments.Count() - 1; k >= 0; k--)
                {
                    Segment seg = f.IntersectionSegments[k];
                    if (f.IntersectionSegments.Where(s => (((s.StartPosition - seg.StartPosition ).Length<EqualityTolerance
                                                 && (s.EndPosition - seg.EndPosition).Length < EqualityTolerance)
                                                            || (
                                                 (s.StartPosition - seg.EndPosition).Length < EqualityTolerance 
                                                 && (s.EndPosition - seg.StartPosition).Length < EqualityTolerance))
                                                        && s != seg).Any())
                    {
                        f.IntersectionSegments.Remove(seg);
                    }
                }
            }
        }

        private void FormLoops(List<Face> Faces)
        {
            foreach (Face f in Faces)
            {
                if (f.IntersectionSegments.Count > 0)
                {
                    if (f.IntersectionSegments.Count == 1 && f.IntersectionLoops.Sum(l => l.Count) < f.IntersectionSegments.Count)
                        f.IntersectionLoops.Add(f.IntersectionSegments);
                    else
                    {
                        // While there are unadded segments
                        while (f.IntersectionLoops.Sum(l => l.Count) < f.IntersectionSegments.Count)
                        {
                            List<Segment> cur_loop = new List<Segment> { };
                            int cur_len = 0, prev_len = 0;
                            do
                            {
                                prev_len = cur_len;
                                //All available segments - not in this loop and any other loop
                                List<Segment> segms = f.IntersectionSegments.Where(x => !cur_loop.Contains(x)
                                                                                && !f.IntersectionLoops.Where(l => l.Contains(x)).Any()).ToList();
                                if (segms.Count > 0)
                                {
                                    if (cur_loop.Count == 0)
                                    {
                                        cur_loop.Add(segms.First());
                                        cur_len++;
                                        continue;
                                    }
                                    else
                                    {
                                        if (segms.Where(cur_seg => cur_loop.Where(s =>(s.EndPosition - cur_seg.StartPosition).Length<EqualityTolerance).Any()).Any())
                                        {
                                            Segment cur_seg = segms.Where(cs => cur_loop.Where(s => (s.EndPosition - cs.StartPosition).Length<EqualityTolerance).Any()).First();
                                            int index = cur_loop.IndexOf(cur_loop.Where(s => (s.EndPosition - cur_seg.StartPosition).Length < EqualityTolerance).First());
                                            cur_loop.Insert(index + 1, cur_seg);
                                            cur_len++;
                                        }
                                        else if (segms.Where(cur_seg => cur_loop.Where(s => (s.StartPosition - cur_seg.EndPosition).Length < EqualityTolerance).Any()).Any())
                                        {
                                            Segment cur_seg = segms.Where(cs => cur_loop.Where(s => (s.StartPosition - cs.EndPosition).Length < EqualityTolerance).Any()).First();
                                            int index = cur_loop.IndexOf(cur_loop.Where(s => (s.StartPosition - cur_seg.EndPosition).Length < EqualityTolerance).First());
                                            cur_loop.Insert(index, cur_seg);
                                            cur_len++;
                                        }
                                    }
                                }

                            } while (cur_len > prev_len);
                            f.IntersectionLoops.Add(cur_loop);
                        }
                    }
                }
            }
            //Connect nearby segments
            foreach (Face f in Faces)
            {
                if (f.IntersectionLoops.Count > 0)
                {
                    foreach (List<Segment> l in f.IntersectionLoops.Where(l => l.Count > 1))
                    {
                        for (int k = l.Count - 1; k >= 1; k--)
                        {
                            Segment seg1 = l[k];
                            Segment seg2 = l[k - 1];
                            if(seg2._EndPos != seg1.StartPosition)
                                seg2._EndPos = seg1.StartPosition;
                        }
                    }
                }
            }

            //combine collinear vectors
            foreach (Face f in Faces)
            {
                if (f.IntersectionLoops.Count > 0)
                {
                    foreach (List<Segment> l in f.IntersectionLoops.Where(l => l.Count > 1))
                    {                        
                        for (int k = l.Count - 1; k >= 1; k--)
                        {
                            Segment seg1 = l[k];
                            Segment seg2 = l[k - 1];
                            Vector3 seg1v = seg1.EndPosition - seg1.StartPosition;
                            Vector3 seg2v = seg2.EndPosition - seg2.StartPosition;
                            if ((seg1v.GetNormal() - seg2v.GetNormal()).Length<EqualityTolerance)
                            {
                                l.Remove(seg1);
                                seg2._EndPos = seg1.EndPosition;
                                seg2._EndVertex = seg1.EndVertex;
                            }
                        }
                    }
                }
            }
        }

        private void SplitByLoops(List<Face> Faces)
        {
            foreach (Face f in Faces)
            {
                if (f.IntersectionLoops.Count > 0)
                {
                    List<Vertex> in_vertices = new List<Vertex> { f.V1, f.V2, f.V3 };
                    SplitTriangle t = new SplitTriangle(f);
                    t.PrepareLoops();
                    t.SplitByLoops();
                    f.subFaces.AddRange(t.Triangles.Select(x => new Face(new Vertex(x.p0.X, x.p0.Y, x.p0.Z),
                        new Vertex(x.p1.X, x.p1.Y, x.p1.Z), new Vertex(x.p2.X, x.p2.Y, x.p2.Z))));
                    f.subFaces.RemoveAll(sf => sf.GetArea() < 1E-10);
                    //Invert face to have corresponding normals, add status
                    foreach (Face sf in f.subFaces)
                    {
                        if (Vector3.Dot(sf.GetNormal(), f.GetNormal()) < 0)
                            sf.Invert();

                        List<Vertex> out_vertices = new List<Vertex> { sf.V1, sf.V2, sf.V3 };
                        foreach (Vertex iv in in_vertices)
                        {
                            if (out_vertices.Where(x => (x.Position - iv.Position).Length<EqualityTolerance).Any())
                            {
                                int iv_ind = out_vertices.IndexOf(out_vertices.Where(x => (x.Position - iv.Position).Length < EqualityTolerance).First());
                                out_vertices[iv_ind].SetStatus(iv.Status);
                            }
                        }
                        if (out_vertices.Where(v => v.Status == Status.UNKNOWN).Any())
                        {
                            if (out_vertices.Where(v => v.Status != Status.UNKNOWN).Any())
                                out_vertices.Where(v => v.Status == Status.UNKNOWN).ToList().ForEach(x => x.SetStatus(Status.BOUNDARY));
                            else
                                out_vertices.Where(v => v.Status == Status.UNKNOWN).ToList().ForEach(x => x.SetStatus(Status.INSIDE));
                        }
                    }
                }
            }
        }
        //Clasiify all faces
        private void Classify(List<Face> Faces1, List<Face> Faces2,int ind)
        {
            List<Face> new_faces1 = Faces1.SelectMany(x => x.subFaces.Count > 0 ? x.subFaces : new List<Face> { x }).ToList();
            
            //for each face
            new_faces1.AsParallel().ForAll(face =>
            {
                //if the face vertices aren't classified to make the simple classify
                //if (face.StrictClassify() == false)
                {
                    //makes the ray trace classification
                    face.RayTraceClassify(Faces2);
                }
            });

            if (ind == 1)
                oc.Faces1 = new_faces1;
            else
                oc.Faces2 = new_faces1;
        }

        public Solid GetDifference()
        {
            InvertInside(oc.Faces2);
            Solid result = ComposeSolid(Status.OUTSIDE, Status.OPPOSITE, Status.INSIDE, Status.SAME);
            InvertInside(oc.Faces2);

            return result;
        }

        public static Solid GetDiff(Solid s1, Solid s2)
        {
            Mesh m1 = new Mesh(s1.GetVertices().ToList(), s1.GetIndices().ToList());
            Mesh m2 = new Mesh(s2.GetVertices().ToList(), s2.GetIndices().ToList());

            newCSG nCSG = new Net3dBool.newCSG(m1, m2);
            Solid res = nCSG.GetDifference();
            return res;
        }

        private void InvertInside(List<Face> faces)
        {
            faces.AsParallel().ForAll(face =>
            {
                if (face.GetStatus() == Status.INSIDE || face.GetStatus() == Status.SAME)
                {
                    face.Invert();
                }
            });
        }
        public Solid GetIntersection()
        {
            return ComposeSolid(Status.INSIDE, Status.SAME, Status.INSIDE);
        }

        public Solid GetUnion()
        {
            return ComposeSolid(Status.OUTSIDE, Status.SAME, Status.OUTSIDE);
        }

        /**
        * Composes a solid based on the faces status of the two operators solids:
        * Status.INSIDE, Status.OUTSIDE, Status.SAME, Status.OPPOSITE
        *
        * @param faceStatus1 status expected for the first solid faces
        * @param faceStatus2 other status expected for the first solid faces
        * (expected a status for the faces coincident with second solid faces)
        * @param faceStatus3 status expected for the second solid faces
        */

        private Solid ComposeSolid(Status faceStatus1, Status faceStatus2, Status faceStatus3, Status faceStatus4 = Status.UNKNOWN)
        {
            var vertices = new List<Vertex>();
            var indices = new List<int>();

            //group the elements of the two solids whose faces fit with the desired status
            GroupObjectComponents(oc.Faces1, vertices, indices, faceStatus1, faceStatus2);
            GroupObjectComponents(oc.Faces2, vertices, indices, faceStatus3, faceStatus4 == Status.UNKNOWN? faceStatus3 : faceStatus4);

            //Cleanup lonely vertices
            bool cleanup = true;
            if (cleanup)
            {
                List<int> lonely = indices.Where(x => indices.Where(i => i == x).Count() == 1).OrderByDescending(x => x).ToList();
                for (int i = 0; i < lonely.Count; i++)
                {
                    int index = lonely[i];
                    int pos = indices.IndexOf(index);
                    if (pos >= 0)
                    {
                        for (int q = pos - pos % 3 + 2; q >= pos - pos % 3; q--)
                        {
                            indices.RemoveAt(q);
                        }
                    }
                    vertices.RemoveAt(index);
                    for (int k = 0; k < indices.Count; k++)
                    {
                        if (indices[k] > index)
                            indices[k]--;
                    }
                }
            }

            //returns the solid containing the grouped elements
            Solid res = new Solid(vertices.Select(x=>x.Position).ToArray(), indices.ToArray());
            
            return res;
        }


        private void GroupObjectComponents(List<Face> faces, List<Vertex> vertices, List<int> indices, Status faceStatus1, Status faceStatus2)
        {
            Face face;
            //for each face..
            for (int i = 0; i < faces.Count; i++)
            {
                face = faces[i];
                //if the face status fits with the desired status...
                if (face.GetStatus() == faceStatus1 || face.GetStatus() == faceStatus2)
                {
                    //adds the face elements into the arrays
                    Vertex[] faceVerts = { face.V1, face.V2, face.V3 };
                    for (int j = 0; j < faceVerts.Length; j++)
                    {
                        if (vertices.Contains(faceVerts[j]))
                        {
                            indices.Add(vertices.IndexOf(faceVerts[j]));
                        }
                        else
                        {
                            indices.Add(vertices.Count);
                            vertices.Add(faceVerts[j]);
                        }
                    }
                }
            }
        }
    }
}
