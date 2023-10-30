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

using System.Collections.Generic;
using System.Linq;

namespace Net3dBool
{
    /// <summary>
    /// Class used to apply boolean operations on solids.
    /// Two 'Solid' objects are submitted to this class constructor. There is a methods for
    /// each boolean operation. Each of these return a 'Solid' resulting from the application
    /// of its operation into the submitted solids.
    /// </summary>
    public class BooleanModeller
    {
        /** solid where boolean operations will be applied */
        public Object3D Object1;
        public Object3D Object2;
        /// <summary>
        /// Vector used to make coordinates relative to the central point of 2 objects
        /// </summary>
        private Vector3 Center;
        //--------------------------------CONSTRUCTORS----------------------------------//

        /**
        * Constructs a BooleanModeller object to apply boolean operation in two solids.
        * Makes preliminary calculations
        *
        * @param solid1 first solid where boolean operations will be applied
        * @param solid2 second solid where boolean operations will be applied
        */

        public BooleanModeller(Solid solid1, Solid solid2, int MAX_FACES = 0, double eqTol = 0, int MAX_NUM_ITERS=2000)
        {
            Center = (solid1.GetMean() + solid2.GetMean()) / 2;

            //Make copy of solids and translate them 
            Solid s1 = new Solid(solid1.GetVertices(), solid1.GetIndices());
            Solid s2 = new Solid(solid2.GetVertices(), solid2.GetIndices());
            s1.Translate(-Center);
            s2.Translate(-Center);
            //representation to apply boolean operations
            Object1 = new Object3D(s1, eqTol);
            Object2 = new Object3D(s2, eqTol);

            //split the faces so that none of them intercepts each other
            List<System.Threading.Thread> objs = new List<System.Threading.Thread> { };
            objs.Add(new System.Threading.Thread(() => Object1.SplitFaces(new Object3D(s2, eqTol), MAX_FACES, MAX_NUM_ITERS)));
            objs.Add(new System.Threading.Thread(() => Object2.SplitFaces(new Object3D(s1, eqTol), MAX_FACES, MAX_NUM_ITERS)));
            objs.ForEach(t => t.Start());
            objs.ForEach(t => t.Join());

            objs.Clear();

            //classify faces as being inside or outside the other solid
            objs.Add(new System.Threading.Thread(() => Object1.ClassifyFaces(Object2)));
            objs.Add(new System.Threading.Thread(() => Object2.ClassifyFaces(Object1)));
            objs.ForEach(t => t.Start());
            objs.ForEach(t => t.Join());
        }
        public BooleanModeller(Solid solid1, int MAX_FACES = 0, double eqTol = 0, int MAX_NUM_ITERS = 2000)
        {
            Center = solid1.GetMean();

            //Make copy of solids and translate them 
            Solid s1 = new Solid(solid1.GetVertices(), solid1.GetIndices());
             
            s1.Translate(-Center);
             
            //representation to apply boolean operations
            Object1 = new Object3D(s1, eqTol);            

            //split the faces so that none of them intercepts each other
            Object1.SplitFaces(new Object3D(s1, eqTol), MAX_FACES, MAX_NUM_ITERS);
            
            Object1.Faces.AsParallel().ForAll(f =>
                {
                    f.V1.SetStatus(Status.BOUNDARY);
                    f.V2.SetStatus(Status.BOUNDARY);
                    f.V3.SetStatus(Status.BOUNDARY);
                    f.Status = Status.SAME;
                }
                );

            Object2 = new Object3D(s1, eqTol);
            Object2.Faces = Object1.Faces;
        }
        private BooleanModeller()
        {
        }

        //----------------------------------OVERRIDES-----------------------------------//

        /**
        * Clones the BooleanModeller object
        *
        * @return cloned BooleanModeller object
        */

        public BooleanModeller Clone()
        {
            BooleanModeller clone = new BooleanModeller();
            clone.Object1 = Object1.Clone();
            clone.Object2 = Object2.Clone();
            return clone;
        }

        //-------------------------------BOOLEAN_OPERATIONS-----------------------------//

        /**
        * Gets the solid generated by the union of the two solids submitted to the constructor
        *
        * @return solid generated by the union of the two solids submitted to the constructor
        */

        public Solid GetDifference()
        {
            Object2.InvertInsideFaces();
            Solid result = ComposeSolid(Status.OUTSIDE, Status.OPPOSITE, Status.INSIDE);
            Object2.InvertInsideFaces();

            return result;
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
        * Gets the solid generated by the intersection of the two solids submitted to the constructor
        *
        * @return solid generated by the intersection of the two solids submitted to the constructor.
        * The generated solid may be empty depending on the solids. In this case, it can't be used on a scene
        * graph. To check this, use the Solid.isEmpty() method.
        */
        /** Gets the solid generated by the difference of the two solids submitted to the constructor.
        * The fist solid is substracted by the second.
        *
        * @return solid generated by the difference of the two solids submitted to the constructor
        */

        //--------------------------PRIVATES--------------------------------------------//

        /**
        * Composes a solid based on the faces status of the two operators solids:
        * Status.INSIDE, Status.OUTSIDE, Status.SAME, Status.OPPOSITE
        *
        * @param faceStatus1 status expected for the first solid faces
        * @param faceStatus2 other status expected for the first solid faces
        * (expected a status for the faces coincident with second solid faces)
        * @param faceStatus3 status expected for the second solid faces
        */

        private Solid ComposeSolid(Status faceStatus1, Status faceStatus2, Status faceStatus3)
        {
            var vertices = new List<Vertex>();
            var indices = new List<int>();

            //group the elements of the two solids whose faces fit with the desired status
            GroupObjectComponents(Object1, vertices, indices, faceStatus1, faceStatus2);
            GroupObjectComponents(Object2, vertices, indices, faceStatus3, faceStatus3);

            //turn the arrayLists to arrays
            Vector3[] verticesArray = new Vector3[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                verticesArray[i] = vertices[i].Position;
            }
            int[] indicesArray = new int[indices.Count];
            for (int i = 0; i < indices.Count; i++)
            {
                indicesArray[i] = indices[i];
            }

            //returns the solid containing the grouped elements
            Solid res = new Solid(verticesArray, indicesArray);
            res.Translate(Center);
            return res;
        }

        /**
        * Fills solid arrays with data about faces of an object generated whose status
        * is as required
        *
        * @param object3d solid object used to fill the arrays
        * @param vertices vertices array to be filled
        * @param indices indices array to be filled
        * @param faceStatus1 a status expected for the faces used to to fill the data arrays
        * @param faceStatus2 a status expected for the faces used to to fill the data arrays
        */

        private void GroupObjectComponents(Object3D obj, List<Vertex> vertices, List<int> indices, Status faceStatus1, Status faceStatus2)
        {
            Face face;
            //for each face..
            for (int i = 0; i < obj.NumFaces; i++)
            {
                face = obj.GetFace(i);
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