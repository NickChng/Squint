using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Reflection;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using g3;

namespace Squint
{
    public static class BolusTools
    {
        public static double GetThickness(MeshGeometry3D Bolus, MeshGeometry3D Body, int Rate = 1)
        {
            ModelVisual3D BolusModel = new ModelVisual3D() { Content = new GeometryModel3D(Bolus, new DiffuseMaterial(new SolidColorBrush(Colors.AliceBlue))) };
            ModelVisual3D BodyModel = new ModelVisual3D() { Content = new GeometryModel3D(Body, new DiffuseMaterial(new SolidColorBrush(Colors.AliceBlue))) };
            List<double> SampledThickness = new List<double>();
            var HitsFar = new List<RayMeshGeometry3DHitTestResult>();
            var HitsClose = new List<RayMeshGeometry3DHitTestResult>();
            HitTestResultCallback resultCallback = delegate (HitTestResult result)
            {
                if (result is RayMeshGeometry3DHitTestResult)       //  It could also be a RayHitTestResult, which isn't as exact as RayMeshGeometry3DHitTestResult
                {
                    RayMeshGeometry3DHitTestResult resultCast = (RayMeshGeometry3DHitTestResult)result;
                    HitsFar.Add(resultCast);
                }
                return HitTestResultBehavior.Stop; // get all hittest only
            };
            HitTestResultCallback resultCallback2 = delegate (HitTestResult result)
            {
                if (result is RayMeshGeometry3DHitTestResult)       //  It could also be a RayHitTestResult, which isn't as exact as RayMeshGeometry3DHitTestResult
                {
                    RayMeshGeometry3DHitTestResult resultCast = (RayMeshGeometry3DHitTestResult)result;
                    HitsClose.Add(resultCast);
                }
                return HitTestResultBehavior.Stop; // get all hittest only
            };
            for (int triangle = 0; triangle < Body.TriangleIndices.Count; triangle += 3*Rate)
            {
                // Get the triangle's vertices.
                Point3D point1 =
                    Body.Positions[Body.TriangleIndices[triangle]];
                Point3D point2 =
                    Body.Positions[Body.TriangleIndices[triangle + 1]];
                Point3D point3 =
                    Body.Positions[Body.TriangleIndices[triangle + 2]];

                var n = FindTriangleNormal(point1, point2, point3);
                n.Normalize();
                n = n * 100;
                var test = n.Length;
                Point3D Centre = new Point3D((point1.X + point2.X + point3.X) / 3 + n.X, (point1.Y + point2.Y + point3.Y) / 3 + n.Y, (point1.Z + point2.Z + point3.Z) / 3 + n.Z);
                //Point3D Centre = new Point3D(point1.X + n.X, point1.Y + n.Y, point1.Z + n.Z);
                Vector3D Dir = new Vector3D() { X = n.X, Y = n.Y, Z = n.Z };
                HitsFar.Clear();
                HitsClose.Clear();
                VisualTreeHelper.HitTest(BolusModel, null, resultCallback2, new RayHitTestParameters(Centre, -Dir));
                if (HitsClose.Count > 0)
                {
                   // var Thick = HitsFar.Max(x => x.DistanceToRayOrigin)+1; //+ Math.Sqrt(2);
                    var Close = HitsClose.Min(x => x.DistanceToRayOrigin);
                    if (Math.Abs(Close) < test )
                    {
                        SampledThickness.Add((100-Close) / 10);
                    }
                }
            }
            double numHits = SampledThickness.Count;
            if (numHits > 20)
            {
                var SortedThickness = SampledThickness.OrderBy(n => n).ToList();
                var SamplesToAverage = SortedThickness.GetRange((int)Math.Round(numHits / 3), (int)Math.Round(numHits * 0.2));
                return SamplesToAverage.Average();
            }
            else
                return -1;

            //return SampledThickness.Average();
            // Get ant SSD at tattoos



        }

        // Calculate a triangle's normal vector.
        private static Vector3D FindTriangleNormal(
            Point3D point1, Point3D point2, Point3D point3)
        {
            // Get two edge vectors.
            Vector3D v1 = point2 - point1;
            Vector3D v2 = point3 - point2;

            // Get the cross product.
            Vector3D n = Vector3D.CrossProduct(v1, v2);

            // Normalize.
            n.Normalize();

            return n;
        }

    }
}
