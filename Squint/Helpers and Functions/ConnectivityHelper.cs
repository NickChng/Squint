using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Squint.Helpers
{
    
    static class ConnectivityHelper
    {
        public static List<int> GetConnectedVertices(int vertexId, out Point3D centerOfMass, int[] triangles, Point3D[] vertices)
        {
            var result = new List<int>(vertices.Length);
            var selectedVertices = new bool[vertices.Length];
            var ignoreTriangles = new bool[triangles.Length / 3];
            var v = new int[3];

            selectedVertices[vertexId] = true;
            result.Add(vertexId);

            centerOfMass = new Point3D();

            bool isSearching = true;
            while (isSearching)
            {

                isSearching = false;

                for (int i = 0; i < triangles.Length; i += 3)
                {

                    if (ignoreTriangles[i / 3])
                    {
                        continue;
                    }

                    v[0] = triangles[i];
                    v[1] = triangles[i + 1];
                    v[2] = triangles[i + 2];

                    if (selectedVertices[v[0]] || selectedVertices[v[1]] || selectedVertices[v[2]])
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            if (!selectedVertices[v[j]])
                            {
                                result.Add(v[j]);
                                centerOfMass.X += vertices[v[j]].X;
                                centerOfMass.Y += vertices[v[j]].Y;
                                centerOfMass.Z += vertices[v[j]].Z;

                                selectedVertices[v[j]] = true;
                                isSearching = true;
                            }
                        }
                        ignoreTriangles[i / 3] = true;
                    }
                }

                if (!isSearching)
                {
                    break;
                }

                for (int i = 0; i < vertices.Length; i++)
                {
                    for (int j = i + 1; j < vertices.Length; j++)
                    {
                        if (selectedVertices[j] != selectedVertices[i])
                        {
                            if ((vertices[i] - vertices[j]).LengthSquared < 0.00001f)
                            {
                                selectedVertices[j] = selectedVertices[i] = true;
                            }
                        }
                    }
                }
            }

            centerOfMass.X /= result.Count;
            centerOfMass.Y /= result.Count;
            centerOfMass.Z /= result.Count;

            return result;
        }
    }
}
