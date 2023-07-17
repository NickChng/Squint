using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using EApp = VMS.TPS.Common.Model.API.Application;


namespace Squint
{
    public class AsyncPlan
    {
        private AsyncESAPI A;
        private Patient pt = null;
        private PlanSetup p = null;
        private PlanSum ps = null;
        public AsyncCourse Course { get; private set; }
        public int HashId { get; private set; }
        public string Id { get; private set; }
        public string StructureSetId { get; private set; } = "";
        public string StructureSetUID { get; private set; } = "";
        public string UID { get; private set; }
        public bool Valid { get; private set; } = true;
        public bool IsDoseValid { get; private set; } = true;
        public int? NumFractions { get; private set; }
        public double? Dose { get; private set; }
        public DateTime HistoryDateTime { get; private set; }
        public ComponentTypes ComponentType { get; private set; }
        public List<string> StructureIds { get; private set; } = new List<string>();
        public List<AsyncPlan> ConstituentPlans { get; private set; } = new List<AsyncPlan>();
        private Dictionary<string, Structure> _Structures = new Dictionary<string, Structure>();
        public AsyncPlan(AsyncESAPI ACurrent, PlanSetup pIn, Patient ptIn, AsyncCourse cIn)
        {
            A = ACurrent;
            pt = ptIn;
            p = pIn;
            Course = cIn;
            UID = p.UID;
            HistoryDateTime = p.HistoryDateTime;
            Id = p.Id;
            NumFractions = p.NumberOfFractions;
            HashId = Convert.ToInt32(p.Id.GetHashCode() + p.Course.Id.GetHashCode() + UID.GetHashCode());
            Dose = p.TotalDose.Dose;
            IsDoseValid = p.IsDoseValid;
            if (p.StructureSet == null)
                Valid = false;
            else
            {
                StructureSetId = p.StructureSet.Id;
                StructureSetUID = p.StructureSet.UID;
                foreach (Structure s in p.StructureSet.Structures)
                {

                    StructureIds.Add(s.Id);
                    _Structures.Add(s.Id, s);
                    var AS = new AsyncStructure(ACurrent, s, p.StructureSet.Id, p.StructureSet.UID, p.StructureSet.Image.ZSize, p.StructureSet.Image.UserOrigin);
                    Structures.Add(s.Id, AS);
                }
            }
            ComponentType = ComponentTypes.Phase;
        }
        public AsyncPlan(AsyncESAPI ACurrent, PlanSum psIn, Patient ptIn, AsyncCourse cIn)
        {
            A = ACurrent;
            pt = ptIn;
            Course = cIn;
            ps = psIn;
            Id = ps.Id;
            var dates = ps.PlanSetups.Select(x => x.HistoryDateTime).OrderByDescending(x => x);
            HistoryDateTime = ps.PlanSetups.Select(x => x.HistoryDateTime).OrderByDescending(x => x).FirstOrDefault();
            ComponentType = ComponentTypes.Sum;
            NumFractions = 0;
            Dose = 0;
            if (ps.StructureSet == null)
                Valid = false;
            else
            {
                StructureSetId = ps.StructureSet.Id;
                StructureSetUID = ps.StructureSet.UID;
                foreach (Structure s in ps.StructureSet.Structures)
                {
                    StructureIds.Add(s.Id);
                    _Structures.Add(s.Id, s);
                    Structures.Add(s.Id, new AsyncStructure(ACurrent, s, ps.StructureSet.Id, ps.StructureSet.UID, ps.StructureSet.Image.ZSize, ps.StructureSet.Image.UserOrigin));
                }
            }
            foreach (PlanSetup p in ps.PlanSetups.OrderBy(x => x.UID))
            {
                ConstituentPlans.Add(new AsyncPlan(A, p, ptIn, cIn));
            }
            HashId = Convert.ToInt32(ps.Id.GetHashCode() + ps.Course.Id.GetHashCode() + string.Concat(ConstituentPlans.Select(x => x.UID)).GetHashCode());
            UID = PlanSumUIDGenerator.GetUID(ps);
        }
        public Dictionary<string, AsyncStructure> Structures { get; private set; } = new Dictionary<string, AsyncStructure>();
        public async Task<double> GetSliceSpacing()
        {
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return Math.Abs(p.StructureSet.Image.ZRes);
                    else return -1;
                }
                else return -1;
            }));
        }
        public async Task<List<AsyncStructure>> GetBolus()
        {
            return await A.ExecuteAsync(new Func<EApp, List<AsyncStructure>>((app) =>
            {
                if (p.StructureSet != null)
                {
                    var Boluses = new List<AsyncStructure>();
                    foreach (Structure s in p.StructureSet.Structures.Where(x => x.DicomType == "BOLUS"))
                    {
                        var Bolus = new AsyncStructure(A, s, p.StructureSet.Id, p.StructureSet.UID, p.StructureSet.Image.ZSize, p.StructureSet.Image.UserOrigin);
                        Boluses.Add(Bolus);
                    }
                    return Boluses;
                }
                else return new List<AsyncStructure>();
            }));
        }
        public async Task<string> GetCTDeviceId()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return p.StructureSet.Image.Series.ImagingDeviceId;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetStudyId()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return p.StructureSet.Image.Series.Study.Id;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetAlgorithmModel()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.StructureSet != null)
                {
                    var CalcModel = p.PhotonCalculationModel.ToString();
                    if (CalcModel != null)
                        return CalcModel;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetVMATAlgorithmModel()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.StructureSet != null)
                {
                    var CalcModel = p.GetCalculationModel(CalculationType.PhotonVMATOptimization);
                    if (CalcModel != null)
                        return CalcModel;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetVMATAirCavityOption()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.StructureSet != null)
                {
                    string OptionValue;
                    p.GetCalculationOption(p.GetCalculationModel(CalculationType.PhotonVMATOptimization),
                        "AirCavityCorrection", out OptionValue);
                    return OptionValue;
                }
                else return null;
            }));
        }
        public async Task<string> GetIMRTAlgorithmModel()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.StructureSet != null)
                {
                    var CalcModel = p.GetCalculationModel(CalculationType.PhotonIMRTOptimization);
                    if (CalcModel != null)
                        return CalcModel;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetIMRTAirCavityOption()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.StructureSet != null)
                {
                    string OptionValue;
                    p.GetCalculationOption(p.GetCalculationModel(CalculationType.PhotonIMRTOptimization),
                        "AirCavityCorrection", out OptionValue);
                    return OptionValue;
                }
                else return null;
            }));
        }
        public async Task<string> GetCourseIntent()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p != null)
                    return p.Course.Intent;
                else if (ps != null)
                    return ps.Course.Intent;
                else
                    return "";

            }));
        }
        public async Task<double> GetCouchSurface()
        {
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                if (p == null)
                    return double.NaN;
                if (p.StructureSet != null)
                {
                    var Couch = p.StructureSet.Structures.FirstOrDefault(x => x.Id == @"CouchSurface");
                    if (Couch != null)
                    {
                        double HU = double.NaN;
                        Couch.GetAssignedHU(out HU);
                        return HU;
                    }
                    else return double.NaN;
                }
                else return double.NaN;
            }));
        }
        public async Task<double> GetCouchInterior()
        {
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                if (p == null)
                    return double.NaN;
                if (p.StructureSet != null)
                {
                    var Couch = p.StructureSet.Structures.FirstOrDefault(x => x.Id == @"CouchInterior");
                    if (Couch != null)
                    {
                        double HU = double.NaN;
                        Couch.GetAssignedHU(out HU);
                        return HU;
                    }
                    else return double.NaN;
                }
                else return double.NaN;
            }));
        }
        public async Task<string> GetHeterogeneityOn()
        {
            if (p == null)
                return "";
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.Dose != null)
                {
                    return (p.PhotonCalculationOptions.Where(x => x.Key == "HeterogeneityCorrection").FirstOrDefault().Value);
                }
                else return "";
            }));
        }
        public async Task<string> GetFieldNormalizationMode()
        {
            if (p == null)
                return "";
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.PhotonCalculationOptions != null)
                {
                    return p.PhotonCalculationOptions.Where(x => x.Key == "FieldNormalizationType").FirstOrDefault().Value;
                }
                else return "";
            }));
        }
        public async Task<double> GetPNV()
        {
            if (p == null)
                return double.NaN;
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                if (p.Dose != null)
                {
                    return p.PlanNormalizationValue;
                }
                else return double.NaN;
            }));
        }
        public async Task<double> GetPrescribedPercentage()
        {
            if (p == null)
                return double.NaN;
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                if (p.Dose != null)
                {
                    return p.TreatmentPercentage * 100;
                }
                else return double.NaN;
            }));
        }
        public async Task<double> GetDoseGridResolution()
        {
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                double returnVal = double.NaN;
                if (p == null)
                    return double.NaN;
                if (p.StructureSet != null)
                {
                    if (p.Dose != null)
                    {
                        return p.Dose.XRes;
                        //Double.TryParse(p.PhotonCalculationOptions.Where(x => x.Key == "CalculationGridSizeInCM").FirstOrDefault().Value, out returnVal);
                        //return returnVal * 10;
                    }
                    else return returnVal;
                }
                else return double.NaN;
            }));
        }
        public async Task<string> GetSeriesId()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p == null)
                    return "";
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return p.StructureSet.Image.Series.Id;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetImageComments()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p == null)
                    return "";
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return p.StructureSet.Image.Comment;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetSeriesComments()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p == null)
                    return "";
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return p.StructureSet.Image.Series.Comment;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetLastModifiedDBy()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p == null)
                    return "";
                return p.HistoryUserName;
            }));
        }
        public async Task<int?> GetNumSlices()
        {
            return await A.ExecuteAsync(new Func<EApp, int?>((app) =>
            {
                if (p == null)
                    return null;
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return Math.Abs(p.StructureSet.Image.ZSize);
                    else return -1;
                }
                else return -1;
            }));
        }
        public async Task<int?> GetFractions()
        {
            return await A.ExecuteAsync(new Func<EApp, int?>((app) =>
            {
                if (p == null)
                    return null;
                return p.NumberOfFractions;
            }));
        }
        public Task<NTODefinition> GetNTOObjective()
        {
            return A.ExecuteAsync(new Func<PlanSetup, NTODefinition>((p) =>
            {
                if (p == null)
                    return null;
                foreach (OptimizationParameter OP in p.OptimizationSetup.Parameters)
                {
                    var NTO = OP as OptimizationNormalTissueParameter;
                    if (NTO != null)
                    {
                        var SquintNTO = new NTODefinition();
                        SquintNTO.DistanceFromTargetBorderMM = NTO.DistanceFromTargetBorderInMM;
                        SquintNTO.EndDosePercentage = NTO.EndDosePercentage;
                        SquintNTO.FallOff = NTO.FallOff;
                        SquintNTO.isAutomatic = NTO.IsAutomatic;
                        SquintNTO.StartDosePercentage = NTO.StartDosePercentage;
                        SquintNTO.Priority = NTO.Priority;
                        return SquintNTO;
                    }
                    else
                        return null;
                }
                return null;
            }), p);
        }
        public Task<List<ImagingFieldItem>> GetImagingFields()
        {
            return A.ExecuteAsync(new Func<PlanSetup, List<ImagingFieldItem>>((p) =>
            {
                var L = new List<ImagingFieldItem>();
                foreach (var F in p.Beams.Where(x => x.IsSetupField))
                {
                    L.Add(new ImagingFieldItem(F, p.TreatmentOrientation));
                }
                return L;
            }), p);
        }
        public Task<List<TxFieldItem>> GetTxFields()
        {
            return A.ExecuteAsync(new Func<PlanSetup, List<TxFieldItem>>((p) =>
            {
                var L = new List<TxFieldItem>();
                foreach (var F in p.Beams.Where(x => !x.IsSetupField))
                {
                    L.Add(new TxFieldItem(p.Course.Id, p.Id, F, p.TreatmentOrientation));
                }
                return L;
            }), p);
        }
        public Task<VVector> GetPlanIsocentre()
        {
            return A.ExecuteAsync(new Func<PlanSetup, VVector>((p) =>
            {
                //
                return p.Beams.First().IsocenterPosition;
            }), p);
        }
        public Task<VVector> GetPatientCentre()
        {
            return A.ExecuteAsync(new Func<PlanSetup, VVector>((p) =>
            {
                var Iso = p.Beams.First().IsocenterPosition;
                var SeriesUID = p.StructureSet.Image.Series.UID;
                var CT = p.StructureSet.Image.Series.Images.Where(x => x.Id == p.StructureSet.Image.Id).FirstOrDefault();
                var zIso = Math.Abs(Math.Round((Iso.z - CT.Origin.z) / CT.ZRes));
                var BODYContour = p.StructureSet.Structures.First(x => x.DicomType == "EXTERNAL").GetContoursOnImagePlane((int)zIso);
                double x0 = BODYContour.SelectMany(x => x.Select(y => y.x)).Average();
                double y0 = BODYContour.SelectMany(x => x.Select(y => y.y)).Average();
                double z0 = BODYContour.SelectMany(x => x.Select(y => y.z)).Average();
                return new VVector(x0, y0, z0);
            }), p);
        }
        public Task<List<ObjectiveDefinition>> GetObjectiveItems()
        {
            return A.ExecuteAsync(new Func<PlanSetup, List<ObjectiveDefinition>>((p) =>
            {
                var L = new List<ObjectiveDefinition>();
                for (int c = 0; c < p.OptimizationSetup.Objectives.Count(); c++)
                {
                    var OD = new ObjectiveDefinition();
                    var OP = p.OptimizationSetup.Objectives.ToList()[c];
                    switch (OP)
                    {
                        case OptimizationPointObjective OPO:
                            OD.StructureId = OPO.StructureId;
                            OD.Dose = OPO.Dose.Dose;
                            OD.Priority = OPO.Priority;
                            OD.Volume = OPO.Volume;
                            OD.DvhType = Dvh_Types.V;
                            if (p.IsDoseValid)
                            {
                                OD.ResultDose = p.GetDoseAtVolume(OPO.Structure, OD.Volume, VolumePresentation.Relative, DoseValuePresentation.Absolute).Dose;
                                OD.ResultVolume = p.GetVolumeAtDose(OPO.Structure, OPO.Dose, VolumePresentation.Relative);
                            }
                            switch (OPO.Operator)
                            {
                                case OptimizationObjectiveOperator.Lower:
                                    OD.Type = ReferenceTypes.Lower;
                                    OD.DoseDifference = -(OD.ResultDose - OD.Dose);
                                    OD.VolDifference = -(OD.ResultVolume - OD.Volume);
                                    break;
                                case OptimizationObjectiveOperator.Upper:
                                    OD.Type = ReferenceTypes.Upper;
                                    OD.DoseDifference = (OD.ResultDose - OD.Dose);
                                    OD.VolDifference = (OD.ResultVolume - OD.Volume);
                                    break;
                            }
                            L.Add(OD);
                            break;
                        case OptimizationMeanDoseObjective OMO:
                            OD.StructureId = OMO.StructureId;
                            OD.Type = ReferenceTypes.Upper;
                            OD.DvhType = Dvh_Types.M;
                            OD.Dose = OMO.Dose.Dose;
                            OD.Priority = OMO.Priority;
                            OD.Volume = double.NaN;
                            if (p.IsDoseValid)
                            {
                                OD.ResultDose = p.GetDVHCumulativeData(OMO.Structure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.1).MeanDose.Dose;
                                OD.DoseDifference = OD.ResultDose - OD.Dose;
                            }
                            L.Add(OD);
                            break;
                        default:
                            L.Add(OD);
                            break;
                    }
                }
                return L;
            }), p);
        }
        public Task<double> GetDoseAtVolume(string StructureId, double ConstraintValue, VolumePresentation VP, DoseValuePresentation DVP)
        {
            switch (ComponentType)
            {
                case ComponentTypes.Phase:
                    return A.ExecuteAsync(new Func<PlanSetup, double>((p) =>
                    {
                        return (p.GetDoseAtVolume(_Structures[StructureId], ConstraintValue, VP, DVP)).Dose;

                    }), p);
                case ComponentTypes.Sum:
                    return A.ExecuteAsync(new Func<PlanSum, double>((ps) =>
                    {
                        return (ps.GetDoseAtVolume(_Structures[StructureId], ConstraintValue, VP, DVP)).Dose;
                    }), ps);
                default:
                    return null;
            }
        }
        public Task<double> GetVolumeAtDose(string StructureId, DoseValue Dose, VolumePresentation VP)
        {
            switch (ComponentType)
            {
                case ComponentTypes.Phase:
                    return A.ExecuteAsync(new Func<PlanSetup, double>((p) =>
                    {
                        return (p.GetVolumeAtDose(_Structures[StructureId], Dose, VP));
                    }), p);
                case ComponentTypes.Sum:
                    return A.ExecuteAsync(new Func<PlanSum, double>((ps) =>
                    {
                        return (ps.GetVolumeAtDose(_Structures[StructureId], Dose, VP));
                    }), ps);
                default:
                    return null;
            }
        }
        public Task<double> GetMeanDose(string StructureId, VolumePresentation VP, DoseValuePresentation DVP, double binwidth)
        {
            switch (ComponentType)
            {
                case ComponentTypes.Phase:
                    return A.ExecuteAsync(new Func<PlanSetup, double>((p) =>
                    {
                        DVHData dvh = p.GetDVHCumulativeData(_Structures[StructureId], DVP, VP, binwidth);
                        if (!ReferenceEquals(null, dvh))
                        {
                            return dvh.MeanDose.Dose;
                        }
                        else return -1;
                    }), p);
                case ComponentTypes.Sum:
                    return A.ExecuteAsync(new Func<PlanSum, double>((ps) =>
                    {
                        DVHData dvh = ps.GetDVHCumulativeData(_Structures[StructureId], DVP, VP, binwidth);
                        if (!ReferenceEquals(null, dvh))
                        {

                            return dvh.MeanDose.Dose;
                        }
                        else return -1;
                    }), ps);
                default:
                    return null;
            }
        }
        public Task<string> GetDoseSOPUID()
        {
            return A.ExecuteAsync(new Func<PlanSetup, string>((p) =>
            {
                return p.Dose.UID;
            }), p);
        }
        public Task<string> GetStructureSetSOPUID()
        {
            return A.ExecuteAsync(new Func<PlanSetup, string>((p) =>
            {
                return p.StructureSet.UID;
            }), p);
        }
        public Task<List<string>> GetCTSOPUIDS()
        {
            return A.ExecuteAsync(new Func<PlanSetup, List<string>>((p) =>
            {
                List<string> returnList = new List<string>();
                return returnList;
            }), p);
        }
        public Task<string> GetImageSeriesUID()
        {
            return A.ExecuteAsync(new Func<PlanSetup, string>((p) =>
            {
                return p.StructureSet.Image.Series.Study.UID;
            }), p);
        }
        public async Task<double> GetBolusThickness(string BolusId)
        {
            return await A.ExecuteAsync(new Func<PlanSetup, double>((pl) =>
            {
                Structure S = pl.StructureSet.Structures.FirstOrDefault(x => x.Id == BolusId && x.DicomType.ToUpper() == @"BOLUS");
                Structure B = pl.StructureSet.Structures.FirstOrDefault(x => x.DicomType.ToUpper() == @"EXTERNAL");
                if (S != null & B != null)
                {
                    int Rate = 1;
                    if (B.Volume > 50000)
                    {
                        Rate = 50;
                    }
                    else if (B.Volume > 10000)
                        Rate = 10;
                    var thickness = BolusTools.GetThickness(S.MeshGeometry, B.MeshGeometry, Rate);
                    return thickness;
                }
                else
                    return double.NaN;
            }), p);
        }

        public async Task<double> CheckMargin(string StructureId, double MarginExpansion)
        {
            return await A.ExecuteAsync(new Func<PlanSetup, double>((pl) =>
            {
                Structure S = pl.StructureSet.Structures.FirstOrDefault(x => x.Id == StructureId);
                if (S != null)
                {
                    if (!S.IsEmpty)
                    {

                        var testExpansion = S.Margin(MarginExpansion);
                        var S2 = pl.StructureSet.AddStructure("Control", "Temp");
                        S2.SegmentVolume = testExpansion;
                        return S2.Volume;
                    }
                    else
                        return double.NaN;
                }
                else
                {
                    return double.NaN;
                }

            }), p);
        }
    }
}
