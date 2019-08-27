using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using g3;

namespace SquintScript.Helpers
{
    public static class MeshHelper
    {
        //the Boolean operation
        private static DMesh3 BooleanSubtraction(DMesh3 mesh1, DMesh3 mesh2)
        {
            BoundedImplicitFunction3d meshA = meshToImplicitF(mesh1, 64, 0.2f);
            BoundedImplicitFunction3d meshB = meshToImplicitF(mesh2, 64, 0.2f);

            //take the difference of the bolus mesh minus the tools
            ImplicitDifference3d mesh = new ImplicitDifference3d() { A = meshA, B = meshB };

            //calculate the boolean mesh
            MarchingCubes c = new MarchingCubes();
            c.Implicit = mesh;
            c.RootMode = MarchingCubes.RootfindingModes.LerpSteps;
            c.RootModeSteps = 5;
            c.Bounds = mesh.Bounds();
            c.CubeSize = c.Bounds.MaxDim / 128;
            c.Bounds.Expand(3 * c.CubeSize);
            c.Generate();
            MeshNormals.QuickCompute(c.Mesh);

            int triangleCount = c.Mesh.TriangleCount / 2;
            Reducer r = new Reducer(c.Mesh);
            r.ReduceToTriangleCount(triangleCount);
            return c.Mesh;
        }

        private static DMesh3 BooleanUnion(DMesh3 mesh1, DMesh3 mesh2)
        {
            BoundedImplicitFunction3d meshA = meshToImplicitF(mesh1, 64, 0.2f);
            BoundedImplicitFunction3d meshB = meshToImplicitF(mesh2, 64, 0.2f);

            //take the difference of the bolus mesh minus the tools
            ImplicitUnion3d mesh = new ImplicitUnion3d() { A = meshA, B = meshB };

            //calculate the boolean mesh
            MarchingCubes c = new MarchingCubes();
            c.Implicit = mesh;
            c.RootMode = MarchingCubes.RootfindingModes.LerpSteps;
            c.RootModeSteps = 5;
            c.Bounds = mesh.Bounds();
            c.CubeSize = c.Bounds.MaxDim / 128;
            c.Bounds.Expand(3 * c.CubeSize);
            c.Generate();
            MeshNormals.QuickCompute(c.Mesh);

            int triangleCount = c.Mesh.TriangleCount / 2;
            Reducer r = new Reducer(c.Mesh);
            r.ReduceToTriangleCount(triangleCount);
            return c.Mesh;
        }

        private static DMesh3 BooleanIntersection(DMesh3 mesh1, DMesh3 mesh2)
        {
            BoundedImplicitFunction3d meshA = meshToImplicitF(mesh1, 64, 0.2f);
            BoundedImplicitFunction3d meshB = meshToImplicitF(mesh2, 64, 0.2f);

            //take the intersection of the meshes minus the tools
            ImplicitIntersection3d mesh = new ImplicitIntersection3d() { A = meshA, B = meshB };

            //calculate the boolean mesh
            MarchingCubes c = new MarchingCubes();
            c.Implicit = mesh;
            c.RootMode = MarchingCubes.RootfindingModes.LerpSteps;
            c.RootModeSteps = 5;
            c.Bounds = mesh.Bounds();
            c.CubeSize = c.Bounds.MaxDim / 128;
            c.Bounds.Expand(3 * c.CubeSize);
            c.Generate();
            MeshNormals.QuickCompute(c.Mesh);

            int triangleCount = c.Mesh.TriangleCount / 2;
            Reducer r = new Reducer(c.Mesh);
            r.ReduceToTriangleCount(triangleCount);
            return c.Mesh;
        }

        //used for Boolean calculations
        static Func<DMesh3, int, double, BoundedImplicitFunction3d> meshToImplicitF = (meshIn, numcells, max_offset) =>
        {
            double meshCellSize = meshIn.CachedBounds.MaxDim / numcells;
            MeshSignedDistanceGrid levelSet = new MeshSignedDistanceGrid(meshIn, meshCellSize);
            levelSet.ExactBandWidth = (int)(max_offset / meshCellSize) + 1;
            levelSet.Compute();
            return new DenseGridTrilinearImplicit(levelSet.Grid, levelSet.GridOrigin, levelSet.CellSize);
        };

        //convert a MeshGeometry3D object into a DMesh object
        private static DMesh3 MeshGeometryToDMesh(MeshGeometry3D mesh)
        {
            List<Vector3d> vertices = new List<Vector3d>();
            foreach (Point3D point in mesh.Positions)
                vertices.Add(new Vector3d(point.X, point.Y, point.Z));

            List<Vector3f> normals = new List<Vector3f>();
            foreach (Point3D normal in mesh.Normals)
                normals.Add(new Vector3f(normal.X, normal.Y, normal.Z));

            if (normals.Count() == 0)
                normals = null;

            List<Index3i> triangles = new List<Index3i>();
            for (int i = 0; i < mesh.TriangleIndices.Count; i += 3)
                triangles.Add(new Index3i(mesh.TriangleIndices[i], mesh.TriangleIndices[i + 1], mesh.TriangleIndices[i + 2]));

            //converting the meshes to use Implicit Surface Modeling
            return DMesh3Builder.Build(vertices, triangles, normals);
        }

        public static List<double> Volumes(MeshGeometry3D mesh)
        {
            DMesh3 m3 = MeshGeometryToDMesh(mesh);
            DMesh3[] mC = MeshConnectedComponents.Separate(m3);
            List<double> ListVol = new List<double>();
            foreach (DMesh3 m in mC)
            {
                var triangles = m.TriangleIndices();
                Func<int, Vector3d> getVertexF = (a) =>
                {
                    Vector3d V = m.GetVertex(a);
                    return V;
                };
                Vector2d Vol = MeshMeasurements.VolumeArea(m, triangles, getVertexF);
                ListVol.Add(Vol.x / 1000);
            }
            return ListVol;
        }
        /// <summary>
        /// Converts a DMesh3 object to a MeshGeometry3D object
        /// DMesh3 is used within the Bolusmesh class to perform calculations
        /// MeshGeometry3D is used by helix Toolkit to display the mesh
        /// This will not calculate normals
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static MeshGeometry3D DMeshToMeshGeometry(DMesh3 value)
        {
            if (value != null)
            {
                //compacting the DMesh to the indices are true
                var mesh_copy = new DMesh3(value, true);
                MeshGeometry3D mesh = new MeshGeometry3D();

                //calculate positions
                var vertices = value.Vertices();
                foreach (var vert in vertices)
                    mesh.Positions.Add(new Point3D(vert.x, vert.y, vert.z));

                //calculate faces
                var vID = value.VertexIndices().ToArray();
                var faces = value.Triangles();
                foreach (Index3i f in faces)
                {
                    mesh.TriangleIndices.Add(Array.IndexOf(vID, f.a));
                    mesh.TriangleIndices.Add(Array.IndexOf(vID, f.b));
                    mesh.TriangleIndices.Add(Array.IndexOf(vID, f.c));
                }

                return mesh;
            }
            else
                return null;
        }

        public static MeshGeometry3D Sub(MeshGeometry3D obj1, MeshGeometry3D obj2)
        {
            DMesh3 mesh1 = MeshGeometryToDMesh(obj1);
            DMesh3 mesh2 = MeshGeometryToDMesh(obj1);
            DMesh3 sub = BooleanSubtraction(mesh1, mesh2);
            return DMeshToMeshGeometry(sub);
        }
        public static MeshGeometry3D Union(MeshGeometry3D obj1, MeshGeometry3D obj2)
        {
            DMesh3 mesh1 = MeshGeometryToDMesh(obj1);
            DMesh3 mesh2 = MeshGeometryToDMesh(obj1);
            DMesh3 sub = BooleanUnion(mesh1, mesh2);
            return DMeshToMeshGeometry(sub);
        }
        public static MeshGeometry3D Intersection(MeshGeometry3D obj1, MeshGeometry3D obj2)
        {
            DMesh3 mesh1 = MeshGeometryToDMesh(obj1);
            DMesh3 mesh2 = MeshGeometryToDMesh(obj1);
            DMesh3 sub = BooleanIntersection(mesh1, mesh2);
            return DMeshToMeshGeometry(sub);
        }

        public static double GetVolume(MeshGeometry3D obj)
        {
            DMesh3 mesh = MeshGeometryToDMesh(obj);
            var triangles = mesh.TriangleIndices();
            Func<int, Vector3d> getVertexF = (a) =>
            {
                Vector3d V = mesh.GetVertex(a);
                return V;
            };
            Vector2d Vol = MeshMeasurements.VolumeArea(mesh, triangles, getVertexF);
            return Vol.x / 1000;
        }
        public static double GetOverlapVolume(MeshGeometry3D obj1, MeshGeometry3D obj2)
        {
            DMesh3 mesh1 = MeshGeometryToDMesh(obj1);
            DMesh3 mesh2 = MeshGeometryToDMesh(obj2);
            DMesh3 MI = BooleanIntersection(mesh1, mesh2);
            var triangles = MI.TriangleIndices();
            Func<int, Vector3d> getVertexF = (a) =>
            {
                Vector3d V = MI.GetVertex(a);
                return V;
            };
            Vector2d Vol = MeshMeasurements.VolumeArea(MI, triangles, getVertexF);
            return Vol.x / 1000;
        }

    }
}
