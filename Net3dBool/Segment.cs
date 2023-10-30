﻿/*
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
using System.Collections;
using System.Collections.Generic;

namespace Net3dBool
{
    /// <summary>
    /// Represents a line segment resulting from a intersection of a face and a plane.
    /// </summary>
    public class Segment
    {
        /** line resulting from the two planes intersection */
        public Line Line;
        /** shows how many ends were already defined */
        private int _Index;

        /** distance from the segment starting point to the point defining the plane */
        public double StartDist { get; private set; }
        /** distance from the segment ending point to the point defining the plane */
        private double _EndDist;

        /** starting point status relative to the face */
        private int _StartType;
        /** intermediate status relative to the face */
        private int _MiddleType;
        /** ending point status relative to the face */
        private int _EndType;

        /** nearest vertex from the starting point */
        public Vertex _StartVertex;
        /** nearest vertex from the ending point */
        public Vertex _EndVertex;

        /** start of the intersection point */
        public Vector3 _StartPos;
        /** end of the intersection point */
        public Vector3 _EndPos;

        /** define as vertex one of the segment ends */
        public static int VERTEX = 1;
        /** define as face one of the segment ends */
        public static int FACE = 2;
        /** define as edge one of the segment ends */
        public static int EDGE = 3;

        /** tolerance value to test equalities */
        private static double TOL = 1e-10f;

        //---------------------------------CONSTRUCTORS---------------------------------//

        /**
        * Constructs a Segment based on elements obtained from the two planes relations 
        * 
        * @param line resulting from the two planes intersection
        * @param face face that intersects with the plane
        * @param sign1 position of the face vertex1 relative to the plane (-1 behind, 1 front, 0 on)
        * @param sign2 position of the face vertex1 relative to the plane (-1 behind, 1 front, 0 on)
        * @param sign3 position of the face vertex1 relative to the plane (-1 behind, 1 front, 0 on)  
        */
        public Segment(Line line, Face face, int sign1, int sign2, int sign3)
        {
            Line = line;
            _Index = 0;

            //VERTEX is an end
            if (sign1 == 0)
            {
                SetVertex(face.V1);
                //other vertices on the same side - VERTEX-VERTEX VERTEX
                if (sign2 == sign3)
                {
                    SetVertex(face.V1);
                }
            }

            //VERTEX is an end
            if (sign2 == 0)
            {
                SetVertex(face.V2);
                //other vertices on the same side - VERTEX-VERTEX VERTEX
                if (sign1 == sign3)
                {
                    SetVertex(face.V2);
                }
            }

            //VERTEX is an end
            if (sign3 == 0)
            {
                SetVertex(face.V3);
                //other vertices on the same side - VERTEX-VERTEX VERTEX
                if (sign1 == sign2)
                {
                    SetVertex(face.V3);
                }
            }

            //There are undefined ends - one or more edges cut the planes intersection line
            if (NumEndsSet != 2)
            {
                //EDGE is an end
                if ((sign1 == 1 && sign2 == -1) || (sign1 == -1 && sign2 == 1))
                {
                    SetEdge(face.V1, face.V2);
                }
                //EDGE is an end
                if ((sign2 == 1 && sign3 == -1) || (sign2 == -1 && sign3 == 1))
                {
                    SetEdge(face.V2, face.V3);
                }
                //EDGE is an end
                if ((sign3 == 1 && sign1 == -1) || (sign3 == -1 && sign1 == 1))
                {
                    SetEdge(face.V3, face.V1);
                }
            }
        }
        public Segment(Line line, Vector3 start, Vector3 end)
        {
            Line = line;
            _Index = 0;
            Vertex vertex1 = new Vertex(start, Status.UNKNOWN);
            Vertex vertex2 = new Vertex(end, Status.UNKNOWN);
            Vector3 point1 = vertex1.Position;
            Vector3 point2 = vertex2.Position;
            Vector3 edgeDirection = new Vector3(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            Line edgeLine = new Line(edgeDirection, point1);

            _StartVertex = vertex1;
            _EndVertex = vertex2;
            _StartPos = start;
            _EndPos = end;
            _EndDist =  Line.ComputePointToPointDistance(_EndPos);
            StartDist = Line.ComputePointToPointDistance(_StartPos);
            
            _Index++;

        }
        private Segment()
        {
        }

        //-----------------------------------OVERRIDES----------------------------------//

        /**
        * Clones the Segment object
        * 
        * @return cloned Segment object
        */
        public Segment Clone()
        {
            Segment clone = new Segment();
            clone.Line = Line;
            clone._Index = _Index;
            clone.StartDist = StartDist;
            clone._EndDist = _EndDist;
            clone.StartDist = _StartType;
            clone._MiddleType = _MiddleType;
            clone._EndType = _EndType;
            clone._StartVertex = _StartVertex;
            clone._EndVertex = _EndVertex;
            clone._StartPos = _StartPos;
            clone._EndPos = _EndPos;

            return clone;
        }

        //-------------------------------------GETS-------------------------------------//

        /**
        * Gets the start vertex
        * 
        * @return start vertex
        */
        public Vertex StartVertex => _StartVertex;

        /**
        * Gets the end vertex
        * 
        * @return end vertex
        */
        public Vertex EndVertex => _EndVertex;

        /**
        * Gets the distance from the origin until ending point
        * 
        * @return distance from the origin until the ending point
        */
        public double EndDistance => _EndDist;

        /**
        * Gets the type of the starting point
        * 
        * @return type of the starting point
        */
        public int StartType => _StartType;

        /**
        * Gets the type of the segment between the starting and ending points
        * 
        * @return type of the segment between the starting and ending points
        */
        public int IntermediateType => _MiddleType;

        /**
        * Gets the type of the ending point
        * 
        * @return type of the ending point
        */
        public int EndType => _EndType;

        /**
        * Gets the number of ends already set
        *
        * @return number of ends already set
        */
        public int NumEndsSet => _Index;

        /**
        * Gets the starting position
        * 
        * @return start position
        */
        public Vector3 StartPosition => _StartPos;

        /**
        * Gets the ending position
        * 
        * @return ending position
        */
        public Vector3 EndPosition => _EndPos;

        //------------------------------------OTHERS------------------------------------//

        /**
        * Checks if two segments intersect
        * 
        * @param segment the other segment to check the intesection
        * @return true if the segments intersect, false otherwise
        */
        public bool Intersect(Segment segment)
        {
            if (_EndDist < segment.StartDist + TOL || segment._EndDist < StartDist + TOL)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public Segment GetTwoSegmentsOverlap(Segment segment)
        {            
            Vector3 startPos, endPos;
            int startType, endType, middleType;
            double startDist, endDist;

            Vertex startVertex = this.StartVertex;
            Vertex endVertex = this.EndVertex;

            //starting point: deeper starting point
            if (segment.StartDist > this.StartDist + TOL)
            {
                startDist = segment.StartDist;
                startType = this.IntermediateType;
                startPos = segment.StartPosition;
            }
            else
            {
                startDist = this.StartDist;
                startType = this.StartType;
                startPos = this.StartPosition;
            }

            //ending point: deepest ending point
            if (segment.EndDistance < this.EndDistance - TOL)
            {
                endDist = segment.EndDistance;
                endType = this.IntermediateType;
                endPos = segment.EndPosition;
            }
            else
            {
                endDist = this.EndDistance;
                endType = this.EndType;
                endPos = this.EndPosition;
            }
            middleType = this.IntermediateType;            
            return new Segment(Line, startPos, endPos);
        }
        //---------------------------------PRIVATES-------------------------------------//

        /**
        * Sets an end as vertex (starting point if none end were defined, ending point otherwise)
        * 
        * @param vertex the vertex that is an segment end 
        * @return false if all the ends were already defined, true otherwise
        */
        private bool SetVertex(Vertex vertex)
        {
            //none end were defined - define starting point as VERTEX
            if (_Index == 0)
            {
                _StartVertex = vertex;
                _StartType = VERTEX;
                StartDist = Line.ComputePointToPointDistance(vertex.Position);
                _StartPos = _StartVertex.Position;
                _Index++;
                return true;
            }
            //starting point were defined - define ending point as VERTEX
            if (_Index == 1)
            {
                _EndVertex = vertex;
                _EndType = VERTEX;
                _EndDist = Line.ComputePointToPointDistance(vertex.Position);
                _EndPos = _EndVertex.Position;
                _Index++;

                //defining middle based on the starting point
                //VERTEX-VERTEX-VERTEX
                if (_StartVertex.Equals(_EndVertex))
                {
                    _MiddleType = VERTEX;
                }
                //VERTEX-EDGE-VERTEX
                else if (_StartType == VERTEX)
                {
                    _MiddleType = EDGE;
                }

                //the ending point distance should be smaller than  starting point distance 
                if (StartDist > _EndDist)
                {
                    SwapEnds();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /**
        * Sets an end as edge (starting point if none end were defined, ending point otherwise)
        * 
        * @param vertex1 one of the vertices of the intercepted edge 
        * @param vertex2 one of the vertices of the intercepted edge
        * @return false if all ends were already defined, true otherwise
        */
        public bool SetEdge(Vertex vertex1, Vertex vertex2)
        {
            Vector3 point1 = vertex1.Position;
            Vector3 point2 = vertex2.Position;
            Vector3 edgeDirection = new Vector3(point2.X - point1.X, point2.Y - point1.Y, point2.Z - point1.Z);
            Line edgeLine = new Line(edgeDirection, point1);

            if (_Index == 0)
            {
                _StartVertex = vertex1;
                _StartType = EDGE;
                _StartPos = Line.ComputeLineIntersection(edgeLine);
                StartDist = Line.ComputePointToPointDistance(_StartPos);
                _MiddleType = FACE;
                _Index++;
                return true;
            }
            else if (_Index == 1)
            {
                _EndVertex = vertex1;
                _EndType = EDGE;
                _EndPos = Line.ComputeLineIntersection(edgeLine);
                _EndDist = Line.ComputePointToPointDistance(_EndPos);
                _MiddleType = FACE;
                _Index++;

                //the ending point distance should be smaller than  starting point distance 
                if (StartDist > _EndDist)
                {
                    SwapEnds();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /** Swaps the starting point and the ending point */
        private void SwapEnds()
        {
            double distTemp = StartDist;
            StartDist = _EndDist;
            _EndDist = distTemp;

            int typeTemp = _StartType;
            _StartType = _EndType;
            _EndType = typeTemp;

            Vertex vertexTemp = _StartVertex;
            _StartVertex = _EndVertex;
            _EndVertex = vertexTemp;

            Vector3 posTemp = _StartPos;
            _StartPos = _EndPos;
            _EndPos = posTemp;
        }
    }
}

