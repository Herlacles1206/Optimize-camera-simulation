using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net3dBool
{
    public class Mesh
    {
        public List<Vector3> Vertices = new List<Vector3> { };
        public List<Face> Faces = new List<Face> { };
        public Bound _Bound;
        public float EqualityTolerance = 1e-6f;
        public Mesh(List<Vector3> vertices, List<int> indices)
        {
            Vertices = new List<Vector3>(vertices);

            //create faces
            Faces = new List<Face>();
            for (int i = 0; i < indices.Count; i = i + 3)
            {
                AddFace(new Vertex(Vertices[indices[i]], Status.UNKNOWN),
                   new Vertex(vertices[indices[i + 1]], Status.UNKNOWN),
                   new Vertex(vertices[indices[i + 2]], Status.UNKNOWN));
            }

            //create bound
            _Bound = new Bound(Vertices.ToArray());
        }

        public Mesh(List<Face> Faces)
        {
            Vertices = new List<Vector3>(Faces.SelectMany(f => f.ToListVector3()).Distinct());

            //create faces
            this.Faces = new List<Face>(Faces);

            //create bound
            if (this.Faces.Count > 0)
                _Bound = new Bound(Vertices.ToArray());
            else
                _Bound = new Bound(new Vector3(0, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        }
        private Face AddFace(Vertex v1, Vertex v2, Vertex v3)
        {
            if (!(v1.Equals(v2) || v1.Equals(v3) || v2.Equals(v3)))
            {
                Face face = new Face(v1, v2, v3);
                if (face.GetArea() > EqualityTolerance)
                {
                    Faces.Add(face);
                    return face;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
    }
}
