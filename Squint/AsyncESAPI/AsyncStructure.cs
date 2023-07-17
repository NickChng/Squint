using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace Squint
{
    public class AsyncStructure
    {
        private AsyncESAPI A;
        private Structure S;
        public string Id { get; private set; }
        public string StructureSetID { get; private set; }
        public string StructureSetUID { get; private set; }
        public double Volume { get; private set; }
        public bool IsHighResolution { get; private set; }
        public string DicomType { get; private set; }
        public System.Windows.Media.Color Color { get; set; }
        public string Code { get; private set; }
        public double HU { get; private set; }
        public bool isEmpty { get; private set; }

        public string Label { get; private set; }

        public VVector Origin { get; set; }

        public int NumSlices { get; set; }
        public AsyncStructure(AsyncESAPI ACurrent, Structure Sin, string SSID, string SSUID, int numSlices, VVector origin)
        {
            A = ACurrent;
            S = Sin;
            Origin = origin;
            isEmpty = Sin.IsEmpty;
            IsHighResolution = S.IsHighResolution;
            Color = S.Color;
            StructureSetUID = SSUID;
            StructureSetID = SSID;
            NumSlices = numSlices;
            Code = S.StructureCodeInfos.FirstOrDefault().Code;
            Label = DbController.GetLabelByCode(Code);
            double HU_out = Double.NaN;
            S.GetAssignedHU(out HU_out);
            HU = Math.Round(HU_out);
            DicomType = S.DicomType;
            Volume = S.Volume;
            Id = S.Id;

        }
        // Volume measurements

        private double GetArea(VVector[] V)
        {
            double Area = 0;
            int c = 0;
            for (c = 0; c < V.Count() - 2; c++)
            {
                Area = Area + (V[c].x * V[c + 1].y - V[c].y * V[c + 1].x);
            }
            Area = Area + Math.Abs(V[c + 1].x * V[0].y - V[c + 1].y * V[0].x);
            return Math.Abs(Area)/2;

        }
        public async Task<Tuple<double, VVector>> GetMinArea()
        {
            return await A.ExecuteAsync(new Func<Structure, Tuple<double, VVector>>((S) =>
            {
                Tuple<double, VVector> MinArea = new Tuple<double, VVector>(0.0, new VVector());
                if (S != null)
                {
                    bool foundPointOutsideSegment = false;
                    var test = Ctr.CurrentStructureSet;
                    double Area = double.PositiveInfinity;
                    for (int z = 1; z < NumSlices; z++)
                    {
                        var contours = S.GetContoursOnImagePlane(z);
                        if (contours.Count() == 0)
                            continue;
                        VVector Centroid = new VVector();
                        foreach (VVector[] C in contours)
                        {
                            var contA = GetArea(C);
                            if (contA < Area)
                            {
                                var Testpoint = new VVector(C.Average(x => x.x)+0.5, C.Average(x => x.y)+0.5, C.Average(x => x.z));
                                if (!S.IsPointInsideSegment(Testpoint))
                                {
                                    foundPointOutsideSegment = true;
                                    Area = contA;
                                    Centroid.x = (C.Average(x => x.x) - Origin.x) / 10; // convert to cm
                                    Centroid.y = (C.Average(x => x.y) - Origin.y) / 10;
                                    Centroid.z = (C.Average(x => x.z) - Origin.z) / 10;
                                    MinArea = new Tuple<double, VVector>(Area, Centroid);
                                }
                            }
                        }
                    }
                    if (foundPointOutsideSegment)
                        return MinArea;
                    else
                        return null;
                }
                else
                {
                    return null;
                }
            }), S);
        }
        private int _NumSeparateParts = -1;
        private List<double> _PartVolumes;
        private List<VVector> _PartVolumes_Centroids;
        private async Task<bool> CalcPartVolumes()
        {
            return await A.ExecuteAsync(new Func<Structure, bool>((S) =>
            {
                if (S != null)
                {
                    if (!S.IsEmpty)
                    {
                        var result = Helpers.MeshHelper.Volumes(S.MeshGeometry); // first of tuple is volume, second of tuple is centroid
                        result.RemoveAll(x => x.Item1 < 1E-5);
                        _PartVolumes = result.Select(x => x.Item1).ToList();
                        _PartVolumes_Centroids = result.Select(x => x.Item2).ToList();
                        _NumSeparateParts = _PartVolumes.Count;
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    return false;
                }

            }), S);
        }
        public async Task<int> GetNumSeperateParts()
        {
            bool Success = await CalcPartVolumes();
            if (Success)
                return _NumSeparateParts;
            else
                return -1;

        }
        public async Task<int> GetVMS_NumParts()
        {
            return await A.ExecuteAsync(new Func<Structure, int>((S) =>
            {
                if (S != null)
                {
                    if (!S.IsEmpty)
                        return S.GetNumberOfSeparateParts();
                    else
                        return -1;
                }
                else
                {
                    return -1;
                }

            }), S);
        }
        public async Task<List<double>> GetPartVolumes()
        {
            if (_NumSeparateParts > 0)
            {
                return _PartVolumes;
            }
            else
            {
                bool Success = await CalcPartVolumes();
                if (Success)
                    return _PartVolumes;
                else
                    return null;
            }
        }
    }
}
