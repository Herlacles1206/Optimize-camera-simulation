/*
The MIT License (MIT)

Copyright (c) 2014 Sebastian Loncar

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

See:
D. H. Laidlaw, W. B. Trumbore, and J. F. Hughes.
"Constructive Solid Geometry for Polyhedral Objects"
SIGGRAPH Proceedings, 1986, p.161.

original author: Danilo Balby Silva Castanheira (danbalby@yahoo.com)

Ported from Java to C# by Sebastian Loncar, Web: http://www.loncar.de
Project: https://github.com/Arakis/Net3dBool

Optimized and refactored by: Lars Brubaker (larsbrubaker@matterhackers.com)
Project: https://github.com/MatterHackers/agg-sharp (an included library)
*/

using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Net3dBool
{
    /// <summary>
    /// Class representing a 3D solid.
    /// </summary>
    public class Solid
    {
        /** array of indices for the vertices from the 'vertices' attribute */
        protected int[] Indices;
        /** array of points defining the solid's vertices */
        protected Vector3[] Vertices;
        public string Name;
        public System.Drawing.Color color;
        public int arr_index;
        //--------------------------------CONSTRUCTORS----------------------------------//

        /** Constructs an empty solid. */
        public Solid()
        {
            SetInitialFeatures();
        }        
        /**
        * Construct a solid based on data arrays. An exception may occur in the case of 
        * abnormal arrays (indices making references to inexistent vertices, there are less
        * colors than vertices...)
        * 
        * @param vertices array of points defining the solid vertices
        * @param indices array of indices for a array of vertices
        * @param colors array of colors defining the vertices colors 
        */
        public Solid(Vector3[] vertices, int[] indices)
            : this()
        {
            SetData(vertices, indices);
        }

        /** Sets the initial features common to all constructors */
        protected void SetInitialFeatures()
        {
            Vertices = new Vector3[0];
            Indices = new int[0];

            //            setCapability(Shape3D.ALLOW_GEOMETRY_WRITE);
            //            setCapability(Shape3D.ALLOW_APPEARANCE_WRITE);
            //            setCapability(Shape3D.ALLOW_APPEARANCE_READ);
        }

        //---------------------------------------GETS-----------------------------------//

        /**
        * Gets the solid vertices
        * 
        * @return solid vertices
        */
        public Vector3[] GetVertices()
        {
            Vector3[] newVertices = new Vector3[Vertices.Length];
            for (int i = 0; i < newVertices.Length; i++)
            {
                newVertices[i] = Vertices[i];
            }
            return newVertices;
        }

        /** Gets the solid indices for its vertices
        * 
        * @return solid indices for its vertices
        */
        public int[] GetIndices()
        {
            int[] newIndices = new int[Indices.Length];
            Array.Copy(Indices, 0, newIndices, 0, Indices.Length);
            return newIndices;
        }

        /**
        * Gets if the solid is empty (without any vertex)
        * 
        * @return true if the solid is empty, false otherwise
        */
        public bool IsEmpty => Indices.Length == 0;

        //---------------------------------------SETS-----------------------------------//

        /**
        * Sets the solid data. Each vertex may have a different color. An exception may 
        * occur in the case of abnormal arrays (e.g., indices making references to  
        * inexistent vertices, there are less colors than vertices...)
        * 
        * @param vertices array of points defining the solid vertices
        * @param indices array of indices for a array of vertices
        * @param colors array of colors defining the vertices colors 
        */
        public void SetData(Vector3[] vertices, int[] indices)
        {
            Vertices = new Vector3[vertices.Length];
            Indices = new int[indices.Length];
            if (indices.Length != 0)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    Vertices[i] = vertices[i];
                }
                Array.Copy(indices, 0, Indices, 0, indices.Length);

                DefineGeometry();
            }
        }

        //-------------------------GEOMETRICAL_TRANSFORMATIONS-------------------------//

        /**
        * Applies a translation into a solid
        * 
        * @param dx translation on the x axis
        * @param dy translation on the y axis
        */
        public void Translate(double dx, double dy, double dz = 0)
        {
            if (dx != 0 || dy != 0 || dz != 0)
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].X += dx;
                    Vertices[i].Y += dy;
                    Vertices[i].Z += dz;
                }

                DefineGeometry();
            }
        }
        public void Translate(Vector3 v)
        {
            if (v!=Vector3.Zero)
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].X += v.X;
                    Vertices[i].Y += v.Y;
                    Vertices[i].Z += v.Z;
                }

                DefineGeometry();
            }
        }
        public void RoundVerticesCoords(int dec = 5)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].X = Math.Round(Vertices[i].X,dec);
                Vertices[i].Y = Math.Round(Vertices[i].Y, dec);
                Vertices[i].Z = Math.Round(Vertices[i].Z, dec);
            }

            DefineGeometry();
        }
        /**
        * Applies a rotation into a solid
        * 
        * @param dx rotation on the x axis
        * @param dy rotation on the y axis
        */
        public void Rotate(double dx, double dy, double dz = 0)
        {
            double cosX = Math.Cos(dx);
            double cosY = Math.Cos(dy);
            double cosZ = Math.Cos(dz);
            double sinX = Math.Sin(dx);
            double sinY = Math.Sin(dy);
            double sinZ = Math.Sin(dz);

            if (dx != 0 || dy != 0 || dz != 0)
            {
                //get mean
                Vector3 mean = GetMean();

                double newX, newY, newZ;
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].X -= mean.X;
                    Vertices[i].Y -= mean.Y;
                    Vertices[i].Z -= mean.Z;

                    //x rotation
                    if (dx != 0)
                    {
                        newY = Vertices[i].Y * cosX - Vertices[i].Z * sinX;
                        newZ = Vertices[i].Y * sinX + Vertices[i].Z * cosX;
                        Vertices[i].Y = newY;
                        Vertices[i].Z = newZ;
                    }

                    //y rotation
                    if (dy != 0)
                    {
                        newX = Vertices[i].X * cosY + Vertices[i].Z * sinY;
                        newZ = -Vertices[i].X * sinY + Vertices[i].Z * cosY;
                        Vertices[i].X = newX;
                        Vertices[i].Z = newZ;
                    }

                    //z rotation
                    if (dz != 0)
                    {
                        newX = Vertices[i].X * cosZ - Vertices[i].Y * sinZ;
                        newY = Vertices[i].X * sinZ + Vertices[i].Y * cosZ;

                        Vertices[i].X = newX;
                        Vertices[i].Y = newY;
                    }

                    Vertices[i].X += mean.X;
                    Vertices[i].Y += mean.Y;
                    Vertices[i].Z += mean.Z;
                }
            }

            DefineGeometry();
        }

        /**
        * Applies a zoom into a solid
        * 
        * @param dz translation on the z axis
        */
        public void Zoom(double dz)
        {
            if (dz != 0)
            {
                for (int i = 0; i < Vertices.Length; i++)
                {
                    Vertices[i].Z += dz;
                }

                DefineGeometry();
            }
        }

        /**
        * Applies a scale changing into the solid
        * 
        * @param dx scale changing for the x axis 
        * @param dy scale changing for the y axis
        * @param dz scale changing for the z axis
        */
        public void Scale(double dx, double dy, double dz)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].X *= dx;
                Vertices[i].Y *= dy;
                Vertices[i].Z *= dz;
            }

            DefineGeometry();
        }

        //-----------------------------------PRIVATES--------------------------------//

        /** Creates a geometry based on the indexes and vertices set for the solid */
        protected void DefineGeometry()
        {
            //            GeometryInfo gi = new GeometryInfo(GeometryInfo.TRIANGLE_ARRAY);
            //            gi.setCoordinateIndices(indices);
            //            gi.setCoordinates(vertices);
            //            NormalGenerator ng = new NormalGenerator();
            //            ng.generateNormals(gi);
            //
            //            gi.setColors(colors);
            //            gi.setColorIndices(indices);
            //            gi.recomputeIndices();
            //
            //            setGeometry(gi.getIndexedGeometryArray());
        }

        /**
        * Gets the solid mean
        * 
        * @return point representing the mean
        */
        public Vector3 GetMean()
        {
            Vector3 mean = new Vector3();
            for (int i = 0; i < Vertices.Length; i++)
            {
                mean.X += Vertices[i].X;
                mean.Y += Vertices[i].Y;
                mean.Z += Vertices[i].Z;
            }
            mean.X /= Vertices.Length;
            mean.Y /= Vertices.Length;
            mean.Z /= Vertices.Length;

            return mean;
        }
        /// <summary>
        /// Extrudes given contour along the given axis
        /// </summary>
        /// <param name="vertices">list of contour vertices</param>
        /// <param name="indices">list of contour indices</param>
        /// <param name="extrude_vector">extrude vector</param>
        /// <param name="last_pts"></param>
        public static void ExtrudeFlatContour(ref List<Vector3> vertices, ref List<int> indices, Vector3 extrude_vector)
        {
            int ct = vertices.Count();
            if (ct < 3)
                return;
            List<Vector3> new_vertices = new List<Vector3> { };
            Vector3 extrude_vectorXY = new Vector3(extrude_vector.X, extrude_vector.Y, 0);
            //Rotate contour to face extrusion direction
            double angle_rad = Vector3.CalculateAngle(extrude_vectorXY, new Vector3(0, 1, 0));
            RotateVertices(ref vertices, (extrude_vector.X!=0? Math.Sign(-extrude_vector.X) :1 )*angle_rad);

            foreach (Vector3 vert in vertices)
                new_vertices.Add(vert);
            foreach (Vector3 vert in vertices)
                new_vertices.Add(vert + extrude_vector);

            int num_lines = ct;
            int num_faces = num_lines + 2;
            int num_triangles = 2 * num_faces;
            List<List<int>> faces = new List<List<int>> { };
            //front
            List<int> front = new List<int> { };
            for (int p = ct - 1; p >= 0; p--)
                front.Add(p);
            faces.Add(front);
            //back
            List<int> back = new List<int> { };
            for (int p = 0; p < ct; p++)
                back.Add(p + ct);
            faces.Add(back);
            //sides
            for (int tr = 0; tr < num_lines - 1; tr++)
            {
                List<int> side = new List<int> { };
                side.Add(tr);
                side.Add(tr + 1);
                side.Add(tr + ct + 1);
                side.Add(tr + ct);
                faces.Add(side);
            }
            //top
            List<int> top = new List<int> { };
            top.Add(0);
            top.Add(ct);
            top.Add(2 * ct - 1);
            top.Add(ct - 1);                        
            faces.Add(top);

            indices = BuildFacesIndices(faces);
            vertices = new_vertices;
        }
        /// <summary>
        /// Create solid of revolution
        /// </summary>
        /// <param name="vertices">input/output contour vertices array</param>
        /// <param name="indices">input/output contour indices array</param>
        /// <param name="axis_pos">axis position</param>
        /// <param name="axis_dir">axis direction</param>
        /// <param name="merids">meridians count</param>
        /// <param name="angle_rad">rotation angle in radians</param>
        /// <param name="dz">each meridian altitude</param>
        public static void RotateFlatContour(ref List<Vector3> vertices, ref List<int> indices, Vector3 axis_pos, Vector3 axis_dir, int merids, double angle_rad, double dz=0)
        {
            int ct = vertices.Count();
            if (ct < 2)
                return;

            //double angle_r = Vector3.CalculateAngle(axis_pos, new Vector3(0, 1, 0));
            //RotateVertices(ref vertices, -angle_r);

            List<Vector3> new_vertices = new List<Vector3> { };
            foreach (Vector3 vert in vertices)
                new_vertices.Add(vert);

            axis_dir.Normalize();
            double angle_rad_m = angle_rad / merids;
            
            for (int k = 0; k < merids; k++)
            {
                double angle = angle_rad_m * (k + 1);
                foreach (Vector3 vert in vertices)
                {
                    Vector3 pt = new Vector3(vert.X, vert.Y, vert.Z);
                    pt = pt - axis_pos;
                    double R = pt.Length;
                    double oldx = pt.X;
                    double oldy = pt.Y;
                    pt.X = oldx * Math.Cos(angle) - oldy * Math.Sin(angle);
                    pt.Y = oldx * Math.Sin(angle) + oldy * Math.Cos(angle);
                    pt.Z += (k+1)*dz;
                    pt = pt + axis_pos;
                    new_vertices.Add(pt);
                }
            }
            List<List<int>> faces = new List<List<int>> { };
             
            for (int k = 0; k < merids; k++)
            {
                //Top
                if (axis_pos != Vector3.Zero)
                {
                    List<int> top = new List<int> { };
                    if (angle_rad > 0)
                    {
                        top.Add(k * ct);
                        top.Add((k + 1) * ct);
                        top.Add((k + 1) * ct + ct - 1);
                        top.Add((k + 1) * ct - 1);
                    }
                    else
                    {
                        top.Add(k * ct);
                        top.Add((k + 1) * ct - 1);
                        top.Add((k + 1) * ct + ct - 1);
                        top.Add((k + 1) * ct);                     
                        
                    }
                    faces.Add(top);
                }
                //fringes
                for (int f = 0; f < ct - 1; f++)
                {
                    List<int> side = new List<int> { };
                    if (angle_rad > 0)
                    {
                        side.Add(k * ct + f);
                        side.Add(k * ct + f + 1);
                        side.Add(k * ct + f + ct + 1);
                        side.Add(k * ct + f + ct);
                    }
                    else
                    {
                        side.Add(k * ct + f);
                        side.Add(k * ct + f + ct);
                        side.Add(k * ct + f + ct + 1);
                        side.Add(k * ct + f + 1);
                        
                        
                    }
                    faces.Add(side);
                }
            }
            //Add Sides
            if(angle_rad<Math.PI*2)
            {
                List<int> side1 = new List<int> { };
                List<int> side2 = new List<int> { };
                if (angle_rad > 0)
                {
                    for (int p = ct - 1; p >= 0; p--)
                        side1.Add(p);
                    
                    for (int p = new_vertices.Count - ct; p < new_vertices.Count; p++)
                        side2.Add(p);
                }
                else
                {
                    for (int p =0; p<ct;  p++)
                        side1.Add(p);

                    for (int p = new_vertices.Count-1;p>= new_vertices.Count - ct;  p--)
                        side2.Add(p);
                }

                faces.Add(side1);
                faces.Add(side2);
            }

            indices = BuildFacesIndices(faces);
            vertices = new_vertices;
        }

        /// <summary>
        /// Creates 3d Solid of extrusion
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="extrude_vector"></param>
        public static Solid ExtrudeSolid3D(List<Vector3> vertices, List<int> indices, Vector3 extrude_vector, bool add_check_inner=false, double mdh = 0)
        {
            int ct = vertices.Count();
            if (ct < 3)
                return null;
            List<Vector3> new_vertices = new List<Vector3> { };
            
            foreach (Vector3 vert in vertices)
                new_vertices.Add(vert);
            foreach (Vector3 vert in vertices)
                new_vertices.Add(vert + extrude_vector);

            List<List<int>> faces = new List<List<int>> { };
            //starting and ending objects
            for (int k = 0; k < indices.Count; k += 3)
            {                
                faces.Add(new List<int> { indices[k], indices[k + 1], indices[k + 2] });
                faces.Add(new List<int> { indices[k]+ct, indices[k + 1] + ct, indices[k + 2] + ct });
            }
            //extruded objects
            Vector3 mean = new Vector3(new_vertices.Average(x => x.X), new_vertices.Average(x => x.Y), mdh >0?
                extrude_vector.Z/2 + (new_vertices.Min(x => x.Z)+ mdh) : (new_vertices.Min(x => x.Z) + new_vertices.Max(x => x.Z)) /2);
            for (int k = 0; k < indices.Count; k += 3)
            {
                List<int> l1 = new List<int> { indices[k], indices[k + 1], indices[k + 1] + ct, indices[k] + ct };
                List<int> l2 = new List<int> { indices[k + 1], indices[k + 2], indices[k + 2] + ct, indices[k + 1] + ct };
                Vector3 mean1 = new Vector3(l1.Average(x => new_vertices[x].X), l1.Average(x => new_vertices[x].Y), l1.Average(x => new_vertices[x].Z));
                Vector3 mean2 = new Vector3(l2.Average(x => new_vertices[x].X), l2.Average(x => new_vertices[x].Y), l2.Average(x => new_vertices[x].Z));
                Vector3 dir1 = mean1 - mean;
                Vector3 dir2 = mean2 - mean;
                if (add_check_inner)
                    dir1.Z = dir2.Z = 0;
                Vector3 n1 = GetNormal(new_vertices[l1[0]], new_vertices[l1[1]], new_vertices[l1[2]]);
                Vector3 n2 = GetNormal(new_vertices[l2[0]], new_vertices[l2[1]], new_vertices[l2[2]]);
                int s1 = Math.Sign(Vector3.Dot(n1, dir1));
                int s2 = Math.Sign(Vector3.Dot(n2, dir2));
                if(s1==1)
                {
                    l1.Reverse();
                }
                if (s2 == 1)
                {
                    l2.Reverse();
                }
                faces.Add(l1);
                faces.Add(l2);
            }
                       

            indices = BuildFacesIndices(faces);
            vertices = new_vertices;
            var shape3D = new Net3dBool.Solid(new_vertices.ToArray(), indices.ToArray());
            //return shape3D;
            //Remove inner faces            
            BooleanModeller mod = new BooleanModeller(shape3D,10000);
            return RemoveInnerFaces(mod, add_check_inner);
        }
        /// <summary>
        /// Creates 3d Solid of revolution
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="axis_pos"></param>
        /// <param name="axis_dir"></param>
        /// <param name="merids"></param>
        /// <param name="angle_rad"></param>
        /// <param name="dz">each step altitude</param>
        /// <param name="add_check_inner"></param>
        /// <returns></returns>
        public static Solid RotateSolid3D(List<Vector3> vertices, List<int> indices, Vector3 axis_pos, Vector3 axis_dir, int merids, double angle_rad, double dz = 0, bool add_check_inner = false)
        {
            int ct = vertices.Count();
            if (ct < 3)
                return null;
            List<Vector3> new_vertices = new List<Vector3> { };

            foreach (Vector3 vert in vertices)
                new_vertices.Add(vert);
            axis_dir.Normalize();
            double angle_rad_m = angle_rad / merids;

            for (int k = 0; k < merids; k++)
            {
                double angle = angle_rad_m * (k + 1);
                foreach (Vector3 vert in vertices)
                {
                    Vector3 pt = new Vector3(vert.X, vert.Y, vert.Z);
                    pt = pt - axis_pos;
                    double R = pt.Length;
                    double oldx = pt.X;
                    double oldy = pt.Y;
                    pt.X = oldx * Math.Cos(angle) - oldy * Math.Sin(angle);
                    pt.Y = oldx * Math.Sin(angle) + oldy * Math.Cos(angle);
                    pt.Z += (k + 1) * dz;
                    pt = pt + axis_pos;
                    new_vertices.Add(pt);
                }
            }

            List<List<int>> faces = new List<List<int>> { };
            //starting and ending objects
            for (int m = 0; m <= merids; m++)
            {
                int add = m * ct;
                for (int k = 0; k < indices.Count; k += 3)
                {
                    faces.Add(new List<int> { indices[k] + add, indices[k + 1] + add, indices[k + 2] + add });
                }
            }
            //extruded objects
            
            for (int m = 0; m < merids; m++)
            {                
                int start = (m) * ct;
                int add = (m+1) * ct;
                List<Vector3> range = new_vertices.GetRange(start, ct);
                Vector3 mean = new Vector3(range.Average(x => x.X), range.Average(x => x.Y), range.Average(x => x.Z));
                for (int k = 0; k < indices.Count; k += 3)
                {
                    List<int> l1 = new List<int> { start + indices[k], start + indices[k + 1], indices[k + 1] + add, indices[k] + add };
                    List<int> l2 = new List<int> { start + indices[k + 1], start + indices[k + 2], indices[k + 2] + add, indices[k + 1] + add };
                    Vector3 mean1 = new Vector3(l1.Average(x => new_vertices[x].X), l1.Average(x => new_vertices[x].Y), l1.Average(x => new_vertices[x].Z));
                    Vector3 mean2 = new Vector3(l2.Average(x => new_vertices[x].X), l2.Average(x => new_vertices[x].Y), l2.Average(x => new_vertices[x].Z));
                    Vector3 dir1 = mean1 - mean;
                    Vector3 dir2 = mean2 - mean;
                    Vector3 n1 = GetNormal(new_vertices[l1[0]], new_vertices[l1[1]], new_vertices[l1[2]]);
                    Vector3 n2 = GetNormal(new_vertices[l2[0]], new_vertices[l2[1]], new_vertices[l2[2]]);
                    int s1 = Math.Sign(Vector3.Dot(n1, dir1));
                    int s2 = Math.Sign(Vector3.Dot(n2, dir2));
                    if (s1 == 1)
                    {
                        l1.Reverse();
                    }
                    if (s2 == 1)
                    {
                        l2.Reverse();
                    }
                    faces.Add(l1);
                    faces.Add(l2);
                }
            }
            indices = BuildFacesIndices(faces);
            vertices = new_vertices;
            var shape3D = new Net3dBool.Solid(new_vertices.ToArray(), indices.ToArray());

            //Remove inner faces            
            int max_f = 10000 * (merids > 1 ? (merids - 1)   : 1);
            BooleanModeller mod = new BooleanModeller(shape3D, max_f);
            return RemoveInnerFaces(mod, add_check_inner);
        }
        public static Solid RemoveInnerFaces(BooleanModeller mod,bool add_check_inner =false)
        {
            System.Collections.Concurrent.ConcurrentBag <Face> inner_faces = new System.Collections.Concurrent.ConcurrentBag<Face> { };
            mod.Object1.Faces.AsParallel().ForAll(x =>
               {
                   Vector3 c = x.GetCenter();
                   Vector3 n1 = x.GetNormal();
                   Plane pl0 = x.GetPlane();
                   bool is_inner = false;
                   double min_dir = 999;

                   for (int p = 0; p < mod.Object1.Faces.Count; p++)
                   {
                       if (mod.Object1.Faces[p] == x)
                           continue;
                       Face f2 = mod.Object1.Faces[p];
                       Plane pl = f2.GetPlane();

                       Vector3 I = new Vector3();

                       double d1 = Math.Abs(pl.GetDistanceFromPlane(c)); // not in the same plane
                        if (d1 > 1E-7 && CheckIfLineIntersectsPlane(f2.V1._Position, pl.PlaneNormal, c, n1, ref I)
                            && CheckIfPointInsideTriangle(f2, I))
                       {
                           //additional checks for self-intersection
                           if (add_check_inner)
                           {
                               if (d1 < min_dir)
                               {
                                   min_dir = d1;
                                   if (Vector3.Dot(n1, pl.PlaneNormal) <=0)
                                       is_inner = false;
                                   else
                                       is_inner = true;
                               }
                           }
                           else
                           {
                               inner_faces.Add(x);
                               break;
                           }
                       }
                   }
                   if(add_check_inner && is_inner)
                       inner_faces.Add(x);
               });
            mod.Object1.Faces = mod.Object1.Faces.Except(inner_faces).ToList();
            return mod.GetUnion();
        }
        public static bool CheckIfInnerFace(BooleanModeller mod, int start_index, Vector3 pl_center, Vector3 dir)
        {
            for (int p = 0; p < mod.Object1.Faces.Count; p++)
            {
                if (p == start_index)
                    continue;
                Face f2 = mod.Object1.Faces[p];
                Plane pl = f2.GetPlane();

                Vector3 I = new Vector3();

                double d1 = Math.Abs(pl.GetDistanceFromPlane(pl_center)); // not in the same plane
                if (d1 > 1E-7 && CheckIfLineIntersectsPlane(f2.V1._Position, pl.PlaneNormal, pl_center, dir, ref I)
                     && CheckIfPointInsideTriangle(f2, I))
                    return true;
            }
            return false;
        }
        public static bool CheckIfLineIntersectsPlane(Vector3 planePoint, Vector3 planeNormal, Vector3 linePoint, Vector3 lineDirection, ref Vector3 Inter)
        {
            //line parallel to the plane
            if (Math.Abs(Vector3.Dot( planeNormal,lineDirection))<1E-7)
            {
                return false;
            }

            double t = Vector3.Dot(planeNormal,planePoint-linePoint) / Vector3.Dot(planeNormal,lineDirection);            
            Inter = linePoint + (lineDirection * t);
            return t>0;
        }
        /// <summary>
        /// Berycentric coordinates  -check if point within coplanar triangle
        /// </summary>
        /// <param name="t"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static bool CheckIfPointInsideTriangle(Face t, Vector3 p)
        {
            Vector3 p0 = t.V1._Position;
            Vector3 p1 = t.V2._Position;
            Vector3 p2 = t.V3._Position;
            
            Vector3 v0 = p1 - p0, v1 = p2 - p0, v2 = p - p0;
            double d00 = Vector3.Dot(v0, v0);
            double d01 = Vector3.Dot(v0, v1);
            double d11 = Vector3.Dot(v1, v1);
            double d20 = Vector3.Dot(v2, v0);
            double d21 = Vector3.Dot(v2, v1);
            double denom = d00 * d11 - d01 * d01;
            double alpha = (d11 * d20 - d01 * d21) / denom;
            double beta = (d00 * d21 - d01 * d20) / denom;
            double gamma = 1  - alpha - beta;
            return (alpha >= 0 && alpha <= 1) && (beta >= 0 && beta <= 1) && (gamma >= 0 && gamma <= 1);
        }

        /// <summary>
        /// Build indices from the list of faces
        /// </summary>
        /// <param name="faces"></param>
        /// <returns></returns>
        private static List<int> BuildFacesIndices(List<List<int>> faces, bool old_method = false)
        {
            List<int> new_indices = new List<int> { };
            if (old_method)
            {
                foreach (List<int> face in faces)
                {
                    new_indices.AddRange(BuildSingleFaceIndices(face));
                }
            }
            else
            {
                foreach (List<int> face in faces)
                {
                    for (int trap = 0; trap < face.Count/2; trap++) // for each horizontal trapeze 
                    {
                        List<int> trapeze = new List<int> { };                        
                        trapeze.Add(face[trap ]);
                        trapeze.Add(face[trap + 1]);
                        trapeze.Add(face[face.Count - 2 - trap ]);
                        trapeze.Add(face[face.Count - 1 - trap ]);                        
                        new_indices.AddRange(BuildSingleFaceIndices(trapeze));
                    }
                }
            }
            return new_indices;
        }
        /// <summary>
        /// Build indices for the single face
        /// </summary>
        /// <param name="face"></param>
        /// <returns></returns>
        private static List<int> BuildSingleFaceIndices(List<int> face)
        {
            List<int> new_indices = new List<int> { };
            for (int pts = 0; pts < face.Count - 2; pts++)
            {
                new_indices.Add(face[(pts + 1) % 2 == 0 ? (pts + 1) : 0]);
                new_indices.Add(face[((int)(0.5 * (2 * pts + 3 + Math.Pow(-1, pts + 1))))]);
                new_indices.Add(face[pts % 2 == 0 ? pts + 2 : 0]);
            }
            return new_indices;
        }
        public static Vector3 GetNormal(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            Vector3 firstvec = v2 - v1;
            Vector3 secondvec = v1 - v3;
            Vector3 normal = Vector3.Cross(firstvec, secondvec);
            normal.Normalize();
            return new Vector3((float)normal.X, (float)normal.Y, (float)normal.Z);
        }
        /// <summary>
        /// Rotate flat contour for extrusion
        /// </summary>
        /// <param name="vertices">list of contour vertices</param>
        /// <param name="angle_rad">angle of rotation in rads</param>
        public static void RotateVertices(ref List<Vector3> vertices, double angle_rad)
        {
            List<Vector3> new_vertices = new List<Vector3> { };
            foreach (Vector3 vert in vertices)
            {
                Vector3 pt = new Vector3(vert.X, vert.Y, vert.Z);                
                double oldx = pt.X;
                double oldy = pt.Y;
                pt.X = oldx * Math.Cos(angle_rad) - oldy * Math.Sin(angle_rad);
                pt.Y = oldx * Math.Sin(angle_rad) + oldy * Math.Cos(angle_rad);                
                new_vertices.Add(pt);
            }
            vertices = new_vertices;
        }
        
        public double GetVolume()
        {
            double vol = 0;
            for(int k=0;k<Indices.Length / 3; k++)
            {
                vol += Math.Abs(Vector3.Dot(Vertices[Indices[k]], Vector3.Cross(Vertices[Indices[k + 1]],Vertices[Indices[k + 2]])) / 6.0f);
            }
            return vol;
        }

        public string GetOFFfileString()
        {
            string res = "OFF" + Environment.NewLine;
            res += Vertices.Count() + "\t" + (Indices.Length / 3) + "\t" + (Vertices.Count() + (Indices.Length / 3) - 2) + Environment.NewLine;
            for(int k = 0; k < Vertices.Count(); k++)
                res += (Vertices[k].X + "\t" + Vertices[k].Y + "\t" + Vertices[k].Z + Environment.NewLine).Replace(",",".");
            for (int k = 0; k < Indices.Length-2; k+=3)
            {
                res += "3\t" + Indices[k] + "\t" + Indices[k + 1] + "\t" + Indices[k + 2] + Environment.NewLine;
            }
            return res;
        }
        public Solid(string OFFfilePath)
            : this()
        {
            List<Vector3> vertices=new List<Vector3> { };
            List<int> indices = new List<int> { };
            using (var reader = new StreamReader(OFFfilePath))
            {
                int current_line = 0;
                int verts=0, cur_vert=0;
                System.Globalization.CultureInfo c = System.Globalization.CultureInfo.CurrentCulture;
                bool replace_dots = c.NumberFormat.NumberDecimalSeparator == ",";
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    current_line++;
                    if (line.StartsWith("OFF"))
                        continue;
                    List<string> values = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (current_line==2)
                    {
                        verts =Convert.ToInt32( values[0]);
                        continue;
                    }
                    if(current_line>2)
                    {
                        if(cur_vert<verts)
                        {
                            double x = 0, y = 0, z = 0;
                            try
                            {
                                if ( ParseDoubleCultureSpecific(values[0], replace_dots, ref x)
                                        && ParseDoubleCultureSpecific(values[1], replace_dots, ref y)
                                        && ParseDoubleCultureSpecific(values[2], replace_dots, ref z)
                                    )
                                    vertices.Add(new Vector3((float)x, (float)y, (float)z));
                            }
                            catch (Exception) 
                            {
                                continue;
                            }
                            cur_vert++;
                        }
                        else
                        {
                            int ct = Convert.ToInt32(values[0]);
                            values.RemoveAt(0);
                            indices.AddRange(values.Take(ct).Select(x=>Convert.ToInt32(x)));                            
                        }
                        
                    }
                }
            }
            SetData(vertices.ToArray(), indices.ToArray());
        }
        public static bool ParseDoubleCultureSpecific(string val, bool replace_dots, ref double d)
        {
            bool res = false;
            double try_d = 0;
            if (replace_dots)
                res = double.TryParse(val.Replace(".", ","), out try_d);
            else
                res = double.TryParse(val, out try_d);
            if (res)
            {
                d = try_d;
            }

            return res;
        }
    }
}

