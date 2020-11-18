using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;


namespace SquintScript
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
        public AsyncStructure(AsyncESAPI ACurrent, Structure Sin, string SSID, string SSUID)
        {
            A = ACurrent;
            S = Sin;
            isEmpty = Sin.IsEmpty;
            IsHighResolution = S.IsHighResolution;
            Color = S.Color;
            StructureSetUID = SSUID;
            StructureSetID = SSID;
            Code = S.StructureCodeInfos.FirstOrDefault().Code;
            Label = DbController.GetLabelByCode(Code);
            double HU_out = Double.NaN;
            S.GetAssignedHU(out HU_out);
            HU = HU_out;
            DicomType = S.DicomType;
            Volume = S.Volume;
            Id = S.Id;

        }
        // Volume measurements
        //private async Task<VVector[][]> GetContours(int planeId)
        //{
        //    return await A.ExecuteAsync(new Func<Structure, VVector[][]>((S) =>
        //    {
        //        if (S != null)
        //        {
        //            return S.GetContoursOnImagePlane(planeId);
        //        }
        //        else
        //        {
        //            return null;
        //        }

        //    }), S);
        //}
        private int _NumSeparateParts = -1;
        private List<double> _PartVolumes;
        private async Task<bool> CalcPartVolumes()
        {
            return await A.ExecuteAsync(new Func<Structure, bool>((S) =>
            {
                if (S != null)
                {
                    if (!S.IsEmpty)
                    {
                        _PartVolumes = Helpers.MeshHelper.Volumes(S.MeshGeometry);
                        _PartVolumes.RemoveAll(x => x < 0);
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
            if (_NumSeparateParts > 0)
            {
                return _NumSeparateParts;
            }
            else
            {
                bool Success = await CalcPartVolumes();
                if (Success)
                    return _NumSeparateParts;
                else
                    return -1;
            }
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
