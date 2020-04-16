﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PropertyChanged;
using SquintScript.Interfaces;
using SquintScript.ViewModelClasses;
using SquintScript.Extensions;

namespace SquintScript.ViewModels
{
    [AddINotifyPropertyChangedInterface]

    public class Checklist_ViewModel
    {
        public Controls.Beam_ViewModel Beam_ViewModel { get; set; } = new Controls.Beam_ViewModel();
        public Controls.LoadingViewModel Loading_ViewModel { get; set; } = new Controls.LoadingViewModel();
        public Controls.Control_ViewModel Objectives_ViewModel { get; set; } = new Controls.Control_ViewModel();
        public Controls.Imaging_ViewModel Imaging_ViewModel { get; set; } = new Controls.Imaging_ViewModel();
        public Controls.TestList_ViewModel Simulation_ViewModel { get; set; } = new Controls.TestList_ViewModel();
        public Controls.TestList_ViewModel Targets_ViewModel { get; set; } = new Controls.TestList_ViewModel();
        public Controls.TestList_ViewModel Calculation_ViewModel { get; set; } = new Controls.TestList_ViewModel();
        public Controls.TestList_ViewModel Prescription_ViewModel { get; set; } = new Controls.TestList_ViewModel();

        public bool amEditing { get; set; } = false;


        public Checklist_ViewModel()
        {

        }
        public async Task PopulateViews(PlanSelector p)
        {
            Objectives_ViewModel = new Controls.Control_ViewModel();
            var Objectives = await Ctr.GetOptimizationObjectiveList(p.CourseId, p.PlanId);
            List<string> StructureIds = new List<string>();
            ObservableCollection<Controls.ObjectiveItem> objectiveItems = new ObservableCollection<Controls.ObjectiveItem>();
            foreach (Controls.ObjectiveDefinition OD in Objectives)
            {
                if (!StructureIds.Contains(OD.StructureId))
                {
                    objectiveItems.Add(new Controls.ObjectiveItem(OD.StructureId, OD));
                    StructureIds.Add(OD.StructureId);
                }
                else
                    objectiveItems.Where(x => x.StructureId == OD.StructureId).FirstOrDefault().ObjectiveDefinitions.Add(OD);

            }
            Objectives_ViewModel.Objectives = objectiveItems;
            Objectives_ViewModel.NTO = await Ctr.GetNTOObjective(p.CourseId, p.PlanId);

            var ImagingFields = await Ctr.GetImagingFieldList(p.CourseId, p.PlanId);
            Ctr.Component Comp = Ctr.GetComponentList().FirstOrDefault(x => x.ComponentName == p.ACV.ComponentName);
            var ImageProtocolCheck = Ctr.CheckImagingProtocols(Comp, ImagingFields);
            Imaging_ViewModel.ImagingProtocols.Clear();
            foreach (ImagingProtocols IP in Comp.ImagingProtocolsAttached)
            {
                Controls.ProtocolImagingView PIV = new Controls.ProtocolImagingView() { ImagingProtocolName = IP.Display() };
                if (ImageProtocolCheck.ContainsKey(IP))
                {
                    if (ImageProtocolCheck[IP].Count > 0)
                    {
                        PIV.WarningMessages = ImageProtocolCheck[IP];
                        PIV.isWarning = true;
                    }
                }
                Imaging_ViewModel.ImagingProtocols.Add(PIV);
            }
            Imaging_ViewModel.ImagingFields = new ObservableCollection<Ctr.ImagingFieldItem>(ImagingFields);

            // Populate Simulation ViewModel
            var P = Ctr.GetActiveProtocol();
            var SliceSpacingReference = P.Checklist.SliceSpacing;
            var SliceSpacingValue = (double?)await Ctr.GetSliceSpacing(p.CourseId, p.PlanId);
            CheckValueItem<double?> SliceSpacing = new CheckValueItem<double?>("Slice spacing", SliceSpacingValue, SliceSpacingReference, new TrackedValue<double?>(1E-5), "Slice spacing does not match protocol");
            SliceSpacing.CheckType = CheckTypes.SliceSpacing;
            CheckValueItem<string> Series = new CheckValueItem<string>("Series Id", await Ctr.GetSeriesId(p.CourseId, p.PlanId), null);
            CheckValueItem<string> Study = new CheckValueItem<string>("Study Id", await Ctr.GetStudyId(p.CourseId, p.PlanId), null);
            CheckValueItem<string> SeriesComment = new CheckValueItem<string>("Series comment / scan protocol", await Ctr.GetSeriesComments(p.CourseId, p.PlanId), null);
            CheckValueItem<string> ImageComment = new CheckValueItem<string>("Image comment", await Ctr.GetImageComments(p.CourseId, p.PlanId), null);
            CheckValueItem<int?> NumSlices = new CheckValueItem<int?>("Number of slices", await Ctr.GetNumSlices(p.CourseId, p.PlanId), null);
            Simulation_ViewModel.Tests = new ObservableCollection<ITestListItem>() { Study, Series, NumSlices, SliceSpacing, SeriesComment, ImageComment };

            // Populate Calculation ViewModel
            Calculation_ViewModel.Tests.Clear(); // = new ObservableCollection<Controls.TestListItem<string>>();
            var ProtocolAlgorithm = P.Checklist.Algorithm;
            AlgorithmTypes ComponentAlgorithm;
            Enum.TryParse(await Ctr.GetAlgorithmModel(p.CourseId, p.PlanId), out ComponentAlgorithm);
            CheckValueItem<AlgorithmTypes> Algorithm = new CheckValueItem<AlgorithmTypes>("Algorithm", ComponentAlgorithm, ProtocolAlgorithm, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(Algorithm);

            var DGR_protocol = P.Checklist.AlgorithmResolution;
            var DGRwarningMessage = "Resolution deviation";
            var DGR_plan = await Ctr.GetDoseGridResolution(p.CourseId, p.PlanId);
            CheckValueItem<double?> DoseGridResolution = new CheckValueItem<double?>("Dose grid resolution", DGR_plan, DGR_protocol, new TrackedValue<double?>(1E-2), DGRwarningMessage);
            Calculation_ViewModel.Tests.Add(DoseGridResolution);

            // Heterogeneity
            var HeteroOn = await Ctr.GetHeterogeneityOn(p.CourseId, p.PlanId);
            var ProtocolHeteroOn = P.Checklist.HeterogeneityOn;
            var HeteroWarningString = "Heterogeneity setting incorrect";
            CheckValueItem<bool?> HeterogeneityOn = new CheckValueItem<bool?>("Heterogeneity On", HeteroOn, ProtocolHeteroOn, null, HeteroWarningString, "Not set", "Not set");
            Calculation_ViewModel.Tests.Add(HeterogeneityOn);

            // Field Normalization
            FieldNormalizationTypes FieldNorm;
            Enum.TryParse(await Ctr.GetFieldNormalizationMode(p.CourseId, p.PlanId), out FieldNorm);
            var ProtocolFieldNorm = P.Checklist.FieldNormalizationMode;
            CheckValueItem<FieldNormalizationTypes> FieldNormTest = new CheckValueItem<FieldNormalizationTypes>("Field Norm Mode", FieldNorm, ProtocolFieldNorm, null, "Non-standard normalization");
            Calculation_ViewModel.Tests.Add(FieldNormTest);

            // Support structures
            var CheckCouchSurface = await Ctr.GetCouchSurface(p.CourseId, p.PlanId);
            var CheckCouchInterior = await Ctr.GetCouchInterior(p.CourseId, p.PlanId);
            var RefCouchSurface = P.Checklist.CouchSurface;
            var RefCouchInterior = P.Checklist.CouchInterior;
            var CouchWarningMessage = "HU Deviation";
            string CouchNotFoundWarning = "Not Found";
            string CouchHUNotSpecifiedWarning = "Not Specified";
            ParameterOptions CouchInteriorOption = ParameterOptions.Optional;
            ParameterOptions CouchSurfaceOption = ParameterOptions.Optional;
            if (P.Checklist.CouchSurface.Value != null)
                CouchSurfaceOption = ParameterOptions.Required;
            if (P.Checklist.CouchInterior.Value != null)
                CouchInteriorOption = ParameterOptions.Required;
            CheckValueItem<double?> CouchSurfaceTest = new CheckValueItem<double?>("Couch Surface HU", CheckCouchSurface, RefCouchSurface, new TrackedValue<double?>(0.1), CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning);
            CouchSurfaceTest.ParameterOption = CouchSurfaceOption;
            CheckValueItem<double?> CouchInteriorTest = new CheckValueItem<double?>("Couch Interior HU", CheckCouchInterior, RefCouchInterior, new TrackedValue<double?>(0.1), CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning);
            CouchInteriorTest.ParameterOption = CouchInteriorOption;
            Calculation_ViewModel.Tests.Add(CouchSurfaceTest);
            Calculation_ViewModel.Tests.Add(CouchInteriorTest);

            // Artifacts in calculaion
            foreach (var A in P.Checklist.Artifacts)
            {
                var ArtifactWarningString = "Assigned HU deviates from protocol";
                string NoCheckHUString = @"No artifact structure";
                string NoRefHUString = @"Not specified";
                double? CheckHU = null;
                //var RefHUString = string.Format("{0:0.#} \u00B1{1:0.#} HU", A.RefHU, A.ToleranceHU);
                if (A.E.AssignedStructureId != "")
                {
                    CheckHU = A.E.AssignedHU(p.StructureSetUID);
                }
                var ArtifactCheck = new CheckValueItem<double?>(string.Format(@"Artifact HU (""{0}"")", A.E.AssignedStructureId), CheckHU, A.RefHU, A.ToleranceHU, ArtifactWarningString, NoCheckHUString, NoRefHUString);
                ArtifactCheck.ParameterOption = ParameterOptions.Optional;
                Calculation_ViewModel.Tests.Add(ArtifactCheck);
            }

            // Course Intent
            var RefCourseIntent = Ctr.GetActiveProtocol()._TreatmentIntent;
            TreatmentIntents CourseTxIntent;
            Enum.TryParse<TreatmentIntents>(await Ctr.GetCourseIntent(p.CourseId, p.PlanId), out CourseTxIntent);
            var CourseIntentWarningString = "";
            CheckValueItem<TreatmentIntents> CourseIntentTest = new CheckValueItem<TreatmentIntents>("Course Intent", CourseTxIntent, RefCourseIntent, null, CourseIntentWarningString);

            // Plan normalization
            var PNVWarning = "Out of range";
            var PlanPNV = await Ctr.GetPNV(p.CourseId, p.PlanId);
            CheckRangeItem<double?> PNVCheck = new CheckRangeItem<double?>("Plan Normalization Value Range", PlanPNV, P.Checklist.PNVMin, P.Checklist.PNVMax, PNVWarning);

            // Prescription percentage
            var PlanRxPercentage = await Ctr.GetPrescribedPercentage(p.CourseId, p.PlanId);
            CheckValueItem<double> PlanRxPc = new CheckValueItem<double>("Prescribed percentage", PlanRxPercentage, new TrackedValue<double>(100), new TrackedValue<double>(1E-5), "Not set to 100");

            // Check Rx and fractions
            var CheckRxDose = await Ctr.GetRxDose(p.CourseId, p.PlanId);
            var RxDoseWarningString = "Plan dose different from protocol";
            var TrackedRefDose = new TrackedValue<double?>(Comp.ReferenceDose);
            var TrackedDoseTolerance = new TrackedValue<double?>(1E-5);
            CheckValueItem<double?> RxCheck = new CheckValueItem<double?>("Prescription dose", CheckRxDose, TrackedRefDose, TrackedDoseTolerance, RxDoseWarningString);

            var CheckFractions = await Ctr.GetNumFractions(p.CourseId, p.PlanId);
            var RefFractions = Comp.NumFractions;
            var TrackedRefFractions = new TrackedValue<int?>(Comp.NumFractions);
            var CheckFractionWarningString = "Plan fractions different from protocol";
            CheckValueItem<int?> FxCheck = new CheckValueItem<int?>("Number of fractions", CheckFractions, TrackedRefFractions, null, CheckFractionWarningString);
            Prescription_ViewModel.Tests = new ObservableCollection<ITestListItem>() { RxCheck, FxCheck, CourseIntentTest, PNVCheck, PlanRxPc };

            // Beam checks
            Beam_ViewModel.Beams.Clear();
            Beam_ViewModel.GroupTests.Tests.Clear();
            var Fields = await Ctr.GetTxFieldItems(p.CourseId, p.PlanId);
            foreach (var Beam in Comp.GetBeams())
            {
                var BLI = new Controls.BeamListItem(Beam, Fields);
                Beam_ViewModel.Beams.Add(BLI);
                BLI.FieldChanged += new EventHandler((s, e) => BLI_PropertyChanged(s, e, Comp)); // this updates the MinColOffsetCheck if the field assignments on any reference beam are changed
            }

            // Iso Check
            //var IsoCentreWarning = Fields.Select(x => x.Isocentre).Distinct().Count() != Comp.NumIso;
            var NumIsoDetected = Fields.Select(x => x.Isocentre).Distinct().Count();
            CheckValueItem<int> NumIsoCheck = new CheckValueItem<int>("Number of isocentres", NumIsoDetected, Comp.NumIso, null, "Num isocentres differs");
            Beam_ViewModel.GroupTests.Tests.Add(NumIsoCheck);

            // Num Fields Check
            string BeamRangeWarning = "Number of beams outside range";
            CheckRangeItem<int> FieldCountCheck = new CheckRangeItem<int>("Number of fields", Fields.Count(), Comp.MinBeams, Comp.MaxBeams, BeamRangeWarning);
            Beam_ViewModel.GroupTests.Tests.Add(FieldCountCheck);

            // Min Col Offset
            if (Comp.MaxBeams.Value > 1 && !double.IsNaN(Comp.MinColOffset.Value) && Fields.Count > 1)
            {
                if (Beam_ViewModel.Beams.Any(x => x.Field == null))
                {
                    var MinColOffsetCheck = new CheckValueItem<double?>("Min collimator offset", null, null, null, "Protocol fields not assigned");
                    MinColOffsetCheck.Test = TestType.GreaterThan;
                    Beam_ViewModel.GroupTests.Tests.Add(MinColOffsetCheck);
                }
                else
                {
                    var ColOffset = Beam_ViewModel.Beams.Select(x => x.Field).Select(x => x.CollimatorAngle);
                    var MinColOffset = findMinDiff(ColOffset.ToArray());
                    var MinColOffsetCheck = new CheckValueItem<double>("Min collimator offset", MinColOffset, Comp.MinColOffset, new TrackedValue<double>(1E-2), "Insufficient collimator offset");
                    MinColOffsetCheck.Test = TestType.GreaterThan;
                    Beam_ViewModel.GroupTests.Tests.Add(MinColOffsetCheck);
                }
                
            }


            // Target Structure Checks
            Targets_ViewModel.Tests.Clear();
            foreach (Ctr.ProtocolStructure E in Ctr.GetStructureList())
            {
                if (E.CheckList != null)
                {
                    var C = E.CheckList;
                    if (C.isPointContourChecked.Value)
                    {
                        var VolParts = await E.PartVolumes(p.StructureSetUID);
                        double MinVol = double.NaN;
                        if (VolParts != null)
                            MinVol = VolParts.Min();
                        string WarningString = "Subvolume less than threshold";
                        if (!double.IsNaN(MinVol))
                        {
                            var NumDetectedParts = await E.NumParts(p.StructureSetUID);
                            var VMS_NumParts = await E.VMS_NumParts(p.StructureSetUID);
                            if (VMS_NumParts > NumDetectedParts)
                            {
                                MinVol = 0.01;
                            }
                        }
                        var TL = new CheckValueItem<double>(string.Format(@"Min. Subvolume (""{0}"") [cc]", E.ProtocolStructureName, E.AssignedStructureId),
                            MinVol, C.PointContourVolumeThreshold, null, WarningString, "Not found", "Not specified");
                        TL.Test = TestType.GreaterThan;
                        Targets_ViewModel.Tests.Add(TL);
                    }

                }
            }
        }

        private void BLI_PropertyChanged(object sender, EventArgs e, Ctr.Component Comp)
        {
            //Refresh Field col separation check

            if (Beam_ViewModel.Beams.Any(x => x.Field == null) || Comp.MaxBeams.Value < 2 || double.IsNaN(Comp.MinColOffset.Value))
            {
                return;
            }
            var ColOffset = Beam_ViewModel.Beams.Select(x => x.Field).Select(x => x.CollimatorAngle).ToList();
            var MinColOffset = findMinDiff(ColOffset.ToArray());
            var OldTest = Beam_ViewModel.GroupTests.Tests.Where(x => x.TestName == "Min collimator offset").FirstOrDefault();
            Beam_ViewModel.GroupTests.Tests.Remove(OldTest);
            var MinColOffsetCheck = new CheckValueItem<double>("Min collimator offset", MinColOffset, Comp.MinColOffset, new TrackedValue<double>(1E-2), "Insufficient collimator offset");
            MinColOffsetCheck.Test = TestType.GreaterThan;
            Beam_ViewModel.GroupTests.Tests.Add(MinColOffsetCheck);
        }

        private double findMinDiff(double[] arr)
        {
            // Sort array in  
            // non-decreasing order 
            Array.Sort(arr);
            var n = arr.Length;
            // Initialize difference 
            // as infinite 
            double diff = double.MaxValue;

            // Find the min diff by  
            // comparing adjacent pairs 
            // in sorted array 
            for (int i = 0; i < n - 1; i++)
                if (arr[i + 1] - arr[i] > 180)
                {
                    var val = ((360 - arr[i + 1]) + arr[i]);
                    if (val > 90)
                        val = 180 - val;
                    if (val < diff)
                    {
                        diff = val;
                    }
                }
                else
                    if (arr[i + 1] - arr[i] < diff)
                    diff = arr[i + 1] - arr[i];

            // Return min diff 
            return diff;
        }


        public ICommand StartEditingCommand
        {
            get { return new DelegateCommand(StartEditing); }
        }

        public ICommand AcceptEditsCommand
        {
            get { return new DelegateCommand(AcceptEdits); }
        }

        public ICommand RejectEditsCommand
        {
            get { return new DelegateCommand(RejectEdits); }
        }

        private void StartEditing(object param = null)
        {
            amEditing ^= true;
        }

        private async void AcceptEdits(object param = null)
        {
            amEditing ^= true;
            foreach (var Test in Simulation_ViewModel.Tests)
            {
                Test.CommitChanges();
            }
            Ctr.Save_UpdateProtocolChecklist();
        }

        private void RejectEdits(object param = null)
        {
            foreach (var Test in Simulation_ViewModel.Tests)
            {
                Test.RejectChanges();
            }
            amEditing ^= true;
        }
    }
}
