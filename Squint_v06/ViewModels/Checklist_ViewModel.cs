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

    public class Checklist_ViewModel : ObservableObject
    {
        public Controls.Beam_ViewModel Beam_ViewModel { get; set; } = new Controls.Beam_ViewModel();
        public Controls.LoadingViewModel Loading_ViewModel { get; set; } = new Controls.LoadingViewModel();
        public Controls.Control_ViewModel Objectives_ViewModel { get; set; } = new Controls.Control_ViewModel();
        public Controls.Imaging_ViewModel Imaging_ViewModel { get; set; } = new Controls.Imaging_ViewModel();
        public Controls.TestList_ViewModel Simulation_ViewModel { get; set; } = new Controls.TestList_ViewModel();
        public Controls.TestList_ViewModel Targets_ViewModel { get; set; } = new Controls.TestList_ViewModel();
        public Controls.TestList_ViewModel Calculation_ViewModel { get; set; } = new Controls.TestList_ViewModel();
        public Controls.TestList_ViewModel Prescription_ViewModel { get; set; } = new Controls.TestList_ViewModel();
        public Controls.TestList_ViewModel DiagnosisIntent_ViewModel { get; set; } = new Controls.TestList_ViewModel();

        public ProtocolViewModel ParentView { get; private set; }
        public bool IsDirty { get; set; } = false;
        //public bool adminMode { get { return ParentView.ParentView.AdminOptionsToggle; } }
        public bool AddStructureCheckVisibility { get; set; }

        public bool EditBeamChecksVisibility { get; set; }

        public SquintScript.Controls.BeamListItem SelectedBeam { get; set; }

        private ComponentSelector _SelectedComponent;
        public ComponentSelector SelectedComponent
        {
            get { return _SelectedComponent; }
            set { _SelectedComponent = value; PopulateViewFromSelectedComponent(); }
        }
        private StructureSelector _SelectedStructure;
        public StructureSelector SelectedStructure
        {
            get { return _SelectedStructure; }
            set { _SelectedStructure = value; }
        }

        public Checklist_ViewModel(ProtocolViewModel parentView)
        {
            ParentView = parentView;
            Ctr.ProtocolUpdated += Ctr_ProtocolUpdated;
            Ctr.ProtocolOpened += Ctr_ProtocolUpdated;
        }

        public void Unsubscribe()
        {
            Ctr.ProtocolUpdated -= Ctr_ProtocolUpdated;
            Ctr.ProtocolOpened -= Ctr_ProtocolUpdated;
        }

        private void Ctr_ProtocolUpdated(object sender, EventArgs e)
        {
            PopulateViewFromProtocol();
            if (_SelectedComponent != null)
                SelectedComponent = ParentView.Components.FirstOrDefault(x => x.Id == _SelectedComponent.Id);
            if (SelectedComponent != null)
                PopulateViewFromSelectedComponent();
        }

        private void PopulateViewFromProtocol()
        {
            // No pre-population of the Objectives of imaging checks yet, as these aren't editable

            // Populate Simulation ViewModel
            var P = Ctr.CurrentProtocol;

            Simulation_ViewModel.Tests.Clear();
            CheckValueItem<double?> SliceSpacing = new CheckValueItem<double?>(CheckTypes.SliceSpacing, null, P.Checklist.SliceSpacing, new TrackedValue<double?>(1E-5), "Slice spacing does not match protocol");
            CheckValueItem<string> Series = new CheckValueItem<string>(CheckTypes.SeriesId, null, null) { ParameterOption = ParameterOptions.Optional };
            CheckValueItem<string> Study = new CheckValueItem<string>(CheckTypes.StudyId, null, null) { ParameterOption = ParameterOptions.Optional };
            CheckValueItem<string> SeriesComment = new CheckValueItem<string>(CheckTypes.SeriesComment, null, null) { ParameterOption = ParameterOptions.Optional };
            CheckValueItem<string> ImageComment = new CheckValueItem<string>(CheckTypes.ImageComment, null, null) { ParameterOption = ParameterOptions.Optional };
            CheckValueItem<int?> NumSlices = new CheckValueItem<int?>(CheckTypes.NumSlices, null, null) { ParameterOption = ParameterOptions.Optional };
            Simulation_ViewModel.Tests.Add(Study);
            Simulation_ViewModel.Tests.Add(Series);
            Simulation_ViewModel.Tests.Add(NumSlices);
            Simulation_ViewModel.Tests.Add(SliceSpacing);
            Simulation_ViewModel.Tests.Add(SeriesComment);
            Simulation_ViewModel.Tests.Add(ImageComment);



            // Populate Calculation ViewModel
            Calculation_ViewModel.Tests.Clear(); // = new ObservableCollection<Controls.TestListItem<string>>();
            CheckValueItem<AlgorithmVolumeDoseTypes> Algorithm = new CheckValueItem<AlgorithmVolumeDoseTypes>(CheckTypes.Algorithm, AlgorithmVolumeDoseTypes.Unset, P.Checklist.Algorithm, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(Algorithm);

            CheckValueItem<AlgorithmVMATOptimizationTypes> VMATAlgorithm = new CheckValueItem<AlgorithmVMATOptimizationTypes>(CheckTypes.AlgorithmVMATOptimization, AlgorithmVMATOptimizationTypes.Unset, P.Checklist.AlgorithmVMATOptimization, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(VMATAlgorithm);

            CheckValueItem<string> AirCavityCorrectionVMATOn = new CheckValueItem<string>(CheckTypes.AirCavityCorrectionVMAT, null, P.Checklist.AirCavityCorrectionVMAT, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(AirCavityCorrectionVMATOn);

            CheckValueItem<AlgorithmIMRTOptimizationTypes> IMRTAlgorithm = new CheckValueItem<AlgorithmIMRTOptimizationTypes>(CheckTypes.AlgorithmIMRTOptimization, AlgorithmIMRTOptimizationTypes.Unset, P.Checklist.AlgorithmIMRTOptimization, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(IMRTAlgorithm);

            CheckValueItem<string> AirCavityCorrectionIMRTOn = new CheckValueItem<string>(CheckTypes.AirCavityCorrectionIMRT, null, P.Checklist.AirCavityCorrectionIMRT, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(AirCavityCorrectionIMRTOn);

            var DGRwarningMessage = "Resolution deviation";
            CheckValueItem<double?> DoseGridResolution = new CheckValueItem<double?>(CheckTypes.DoseGridResolution, null, P.Checklist.AlgorithmResolution, new TrackedValue<double?>(1E-2), DGRwarningMessage);
            Calculation_ViewModel.Tests.Add(DoseGridResolution);

            // Heterogeneity
            var HeteroWarningString = "Heterogeneity setting incorrect";
            CheckValueItem<bool?> HeterogeneityOn = new CheckValueItem<bool?>(CheckTypes.HeterogeneityOn, null, P.Checklist.HeterogeneityOn, null, HeteroWarningString, "Not set", "Not set");
            Calculation_ViewModel.Tests.Add(HeterogeneityOn);

            // Field Normalization
            CheckValueItem<FieldNormalizationTypes> FieldNormTest = new CheckValueItem<FieldNormalizationTypes>(CheckTypes.FieldNormMode, FieldNormalizationTypes.Unset, P.Checklist.FieldNormalizationMode, null, "Non-standard normalization");
            Calculation_ViewModel.Tests.Add(FieldNormTest);

            // Support structures

            var RefCouchSurface = P.Checklist.CouchSurface;
            var RefCouchInterior = P.Checklist.CouchInterior;
            var CouchWarningMessage = "HU Deviation";
            string CouchNotFoundWarning = "Not Found";
            string CouchHUNotSpecifiedWarning = "Not Specified";
            ParameterOptions CouchInteriorOption = ParameterOptions.Optional;
            ParameterOptions CouchSurfaceOption = ParameterOptions.Optional;
            if (P.Checklist.CouchSurface.Value == null)
                CouchSurfaceOption = ParameterOptions.Optional;
            if (P.Checklist.CouchInterior.Value == null)
                CouchInteriorOption = ParameterOptions.Optional;
            CheckValueItem<double?> CouchSurfaceTest = new CheckValueItem<double?>(CheckTypes.CouchSurfaceHU, null, RefCouchSurface, new TrackedValue<double?>(0.1), CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning);
            CouchSurfaceTest.ParameterOption = CouchSurfaceOption;
            CheckValueItem<double?> CouchInteriorTest = new CheckValueItem<double?>(CheckTypes.CouchInteriorHU, null, RefCouchInterior, new TrackedValue<double?>(0.1), CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning);
            CouchInteriorTest.ParameterOption = CouchInteriorOption;
            Calculation_ViewModel.Tests.Add(CouchSurfaceTest);
            Calculation_ViewModel.Tests.Add(CouchInteriorTest);

            // Artifacts in calculaion
            foreach (var A in P.Checklist.Artifacts)
            {
                var ArtifactWarningString = "Assigned HU deviates from protocol";
                string NoCheckHUString = @"No artifact structure";
                string NoRefHUString = @"Not specified";
                var ArtifactCheck = new CheckValueItem<double?>(CheckTypes.ArtifactHU, null, A.RefHU, A.ToleranceHU, ArtifactWarningString, NoCheckHUString, NoRefHUString);
                if (A.E != null)
                    ArtifactCheck.OptionalNameSuffix = string.Format(@"(""{0}"")", A.E.AssignedStructureId);
                ArtifactCheck.ParameterOption = ParameterOptions.Optional;
                Calculation_ViewModel.Tests.Add(ArtifactCheck);
            }

            // Course Intent
            var RefCourseIntent = Ctr.CurrentProtocol._TreatmentIntent;
            var CourseIntentWarningString = "";
            CheckValueItem<TreatmentIntents> CourseIntentTest = new CheckValueItem<TreatmentIntents>(CheckTypes.CourseIntent, TreatmentIntents.Unset, RefCourseIntent, null, CourseIntentWarningString);
            DiagnosisIntent_ViewModel.Tests = new ObservableCollection<ITestListItem>() { CourseIntentTest };

            // Structures
            Targets_ViewModel.Tests.Clear();
            foreach (ProtocolStructure E in Ctr.CurrentProtocol.Structures)
            {
                if (E.CheckList != null)
                {
                    var C = E.CheckList;
                    if ((bool)C.isPointContourChecked.Value)
                    {
                        string WarningString = "Subvolume less than threshold";
                        var TL = new CheckValueItem<double?>(CheckTypes.MinSubvolume, null, C.PointContourVolumeThreshold, null, WarningString, "Not found", "Not specified");
                        TL.OptionalNameSuffix = string.Format(@" ({0})", E.ProtocolStructureName);
                        TL.Test = TestType.GreaterThan;
                        Targets_ViewModel.Tests.Add(TL);
                    }

                }
            }
        }
        private void PopulateViewFromSelectedComponent()
        {

            if (_SelectedComponent == null)
                return;
            var P = Ctr.CurrentProtocol;
            Component Comp = Ctr.GetComponent(_SelectedComponent.Id);

            var PNVWarning = "Out of range";
            CheckRangeItem<double?> PNVCheck = new CheckRangeItem<double?>(CheckTypes.PlanNormalization, null, Comp.PNVMin, Comp.PNVMax, PNVWarning);

            // Prescription percentage
            CheckValueItem<double?> PlanRxPc = new CheckValueItem<double?>(CheckTypes.PrescribedPercentage, null, Comp.PrescribedPercentage, new TrackedValue<double?>(1E-5), "Not set to 100");

            // Check Rx and fractions

            var RxDoseWarningString = "Plan dose different from protocol";
            var TrackedRefDose = new TrackedValue<double?>(Comp.ReferenceDose);
            var TrackedDoseTolerance = new TrackedValue<double?>(1E-5);
            CheckValueItem<double?> RxCheck = new CheckValueItem<double?>(CheckTypes.PrescriptionDose, null, TrackedRefDose, TrackedDoseTolerance, RxDoseWarningString);
            RxCheck.EditEnabled = false;

            var RefFractions = Comp.NumFractions;
            var TrackedRefFractions = new TrackedValue<int?>(Comp.NumFractions);
            var CheckFractionWarningString = "Plan fractions different from protocol";
            CheckValueItem<int?> FxCheck = new CheckValueItem<int?>(CheckTypes.NumFractions, null, TrackedRefFractions, null, CheckFractionWarningString);
            FxCheck.EditEnabled = false;
            Prescription_ViewModel.Tests = new ObservableCollection<ITestListItem>() { RxCheck, FxCheck, PNVCheck, PlanRxPc };

            // Beam checks
            Beam_ViewModel.Beams.Clear();
            Beam_ViewModel.GroupTests.Tests.Clear();
            foreach (var Beam in Comp.Beams)
            {
                var BLI = new Controls.BeamListItem(Beam, null);
                Beam_ViewModel.Beams.Add(BLI);
                BLI.InitializeTests();
                BLI.FieldChanged += new EventHandler((s, e) => BLI_PropertyChanged(s, e, Comp)); // this updates the MinColOffsetCheck if the field assignments on any reference beam are changed
            }

            // Iso Check
            //var IsoCentreWarning = Fields.Select(x => x.Isocentre).Distinct().Count() != Comp.NumIso;
            CheckValueItem<int?> NumIsoCheck = new CheckValueItem<int?>(CheckTypes.NumIsocentres, -1, Comp.NumIso, null, "Num isocentres differs");
            Beam_ViewModel.GroupTests.Tests.Add(NumIsoCheck);

            // Num Fields Check
            string BeamRangeWarning = "Number of beams outside range";
            CheckRangeItem<int?> FieldCountCheck = new CheckRangeItem<int?>(CheckTypes.NumFields, -1, Comp.MinBeams, Comp.MaxBeams, BeamRangeWarning);
            Beam_ViewModel.GroupTests.Tests.Add(FieldCountCheck);

            // Min Col Offset
            var MinColOffsetCheck = new CheckValueItem<int?>(CheckTypes.MinColOffset, null, Comp.MinColOffset, null, "Protocol fields not assigned");
            MinColOffsetCheck.Test = TestType.GreaterThan;
            Beam_ViewModel.GroupTests.Tests.Add(MinColOffsetCheck);

        }
        public async Task DisplayChecksForPlan(PlanSelector p)
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
            Component Comp = Ctr.CurrentProtocol.Components.FirstOrDefault(x => x.ComponentName == p.ACV.ComponentName);
            var ImageProtocolCheck = Ctr.CheckImagingProtocols(Comp, ImagingFields);
            Imaging_ViewModel.ImagingProtocols.Clear();
            foreach (ImagingProtocols IP in Comp.ImagingProtocols)
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
            Imaging_ViewModel.ImagingFields = new ObservableCollection<ImagingFieldItem>(ImagingFields);

            // Populate Simulation ViewModel
            var P = Ctr.CurrentProtocol;
            var SliceSpacingReference = P.Checklist.SliceSpacing;
            var SliceSpacingValue = await Ctr.GetSliceSpacing(p.CourseId, p.PlanId);
            CheckValueItem<double?> SliceSpacing = new CheckValueItem<double?>(CheckTypes.SliceSpacing, SliceSpacingValue, SliceSpacingReference, new TrackedValue<double?>(1E-5), "Slice spacing does not match protocol");
            SliceSpacing.CheckType = CheckTypes.SliceSpacing;
            CheckValueItem<string> Series = new CheckValueItem<string>(CheckTypes.SeriesId, await Ctr.GetSeriesId(p.CourseId, p.PlanId), null) { ParameterOption = ParameterOptions.Optional };
            CheckValueItem<string> Study = new CheckValueItem<string>(CheckTypes.StudyId, await Ctr.GetStudyId(p.CourseId, p.PlanId), null) { ParameterOption = ParameterOptions.Optional };
            CheckValueItem<string> SeriesComment = new CheckValueItem<string>(CheckTypes.SeriesComment, await Ctr.GetSeriesComments(p.CourseId, p.PlanId), null) { ParameterOption = ParameterOptions.Optional };
            CheckValueItem<string> ImageComment = new CheckValueItem<string>(CheckTypes.ImageComment, await Ctr.GetImageComments(p.CourseId, p.PlanId), null) { ParameterOption = ParameterOptions.Optional };
            var NumSlices = await Ctr.GetNumSlices(p.CourseId, p.PlanId);
            int Slices;
            if (NumSlices != null)
                Slices = (int)NumSlices;
            else
                Slices = int.MinValue;
            CheckValueItem<int> NumSlicesChk = new CheckValueItem<int>(CheckTypes.NumSlices, Slices, null) { ParameterOption = ParameterOptions.Optional };
            Simulation_ViewModel.Tests = new ObservableCollection<ITestListItem>() { Study, Series, NumSlicesChk, SliceSpacing, SeriesComment, ImageComment };

            // Populate Calculation ViewModel
            Calculation_ViewModel.Tests.Clear(); // = new ObservableCollection<Controls.TestListItem<string>>();
            var ProtocolAlgorithm = P.Checklist.Algorithm;
            AlgorithmVolumeDoseTypes ComponentAlgorithm;
            Enum.TryParse(await Ctr.GetAlgorithmModel(p.CourseId, p.PlanId), out ComponentAlgorithm);
            CheckValueItem<AlgorithmVolumeDoseTypes> Algorithm = new CheckValueItem<AlgorithmVolumeDoseTypes>(CheckTypes.Algorithm, ComponentAlgorithm, ProtocolAlgorithm, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(Algorithm);

            var ProtocolVMATAlgorithm = P.Checklist.AlgorithmVMATOptimization;
            if (ProtocolVMATAlgorithm.Value != AlgorithmVMATOptimizationTypes.Unset)
            {
                AlgorithmVMATOptimizationTypes ComponentVMATAlgorithm;
                Enum.TryParse(await Ctr.GetVMATAlgorithmModel(p.CourseId, p.PlanId), out ComponentVMATAlgorithm);
                CheckValueItem<AlgorithmVMATOptimizationTypes> VMATAlgorithm = new CheckValueItem<AlgorithmVMATOptimizationTypes>(CheckTypes.AlgorithmVMATOptimization, ComponentVMATAlgorithm, ProtocolVMATAlgorithm, null, "Algorithm mismatch");
                Calculation_ViewModel.Tests.Add(VMATAlgorithm);
            }

            var ProtocolVMATAirCavityCorrection = P.Checklist.AirCavityCorrectionVMAT;
            if (!string.IsNullOrEmpty(ProtocolVMATAirCavityCorrection.Value))
            {
                var PlanVMATAirCavityCorrection = await Ctr.GetAirCavityCorrectionVMAT(p.CourseId, p.PlanId);
                CheckValueItem<string> AirCavityCorrectionVMATOn = new CheckValueItem<string>(CheckTypes.AirCavityCorrectionVMAT, PlanVMATAirCavityCorrection, ProtocolVMATAirCavityCorrection, null, "Option mismatch");
                Calculation_ViewModel.Tests.Add(AirCavityCorrectionVMATOn);
            }

            var ProtocolIMRTAlgorithm = P.Checklist.AlgorithmIMRTOptimization;
            if (ProtocolIMRTAlgorithm.Value != AlgorithmIMRTOptimizationTypes.Unset)
            {
                AlgorithmIMRTOptimizationTypes ComponentIMRTAlgorithm;
                Enum.TryParse(await Ctr.GetIMRTAlgorithmModel(p.CourseId, p.PlanId), out ComponentIMRTAlgorithm);
                CheckValueItem<AlgorithmIMRTOptimizationTypes> IMRTAlgorithm = new CheckValueItem<AlgorithmIMRTOptimizationTypes>(CheckTypes.AlgorithmIMRTOptimization, ComponentIMRTAlgorithm, ProtocolIMRTAlgorithm, null, "Algorithm mismatch");
                Calculation_ViewModel.Tests.Add(IMRTAlgorithm);
            }

            var ProtocolIMRTAirCavityCorrection = P.Checklist.AirCavityCorrectionIMRT;
            if (!string.IsNullOrEmpty(ProtocolIMRTAirCavityCorrection.Value))
            {
                var PlanIMRTAirCavityCorrection = await Ctr.GetAirCavityCorrectionIMRT(p.CourseId, p.PlanId);
                CheckValueItem<string> AirCavityCorrectionIMRTOn = new CheckValueItem<string>(CheckTypes.AirCavityCorrectionIMRT, PlanIMRTAirCavityCorrection, ProtocolIMRTAirCavityCorrection, null, "Option mismatch");
                Calculation_ViewModel.Tests.Add(AirCavityCorrectionIMRTOn);
            }

            var DGR_protocol = P.Checklist.AlgorithmResolution;
            var DGRwarningMessage = "Resolution deviation";
            var DGR_plan = await Ctr.GetDoseGridResolution(p.CourseId, p.PlanId);
            CheckValueItem<double?> DoseGridResolution = new CheckValueItem<double?>(CheckTypes.DoseGridResolution, DGR_plan, DGR_protocol, new TrackedValue<double?>(1E-2), DGRwarningMessage);
            Calculation_ViewModel.Tests.Add(DoseGridResolution);

            // Heterogeneity
            var HeteroOn = await Ctr.GetHeterogeneityOn(p.CourseId, p.PlanId);
            var ProtocolHeteroOn = P.Checklist.HeterogeneityOn;
            var HeteroWarningString = "Heterogeneity setting incorrect";
            CheckValueItem<bool?> HeterogeneityOn = new CheckValueItem<bool?>(CheckTypes.HeterogeneityOn, HeteroOn, ProtocolHeteroOn, null, HeteroWarningString, "Not set", "Not set");
            Calculation_ViewModel.Tests.Add(HeterogeneityOn);

            // Field Normalization
            var ProtocolFieldNorm = P.Checklist.FieldNormalizationMode;
            CheckValueItem<FieldNormalizationTypes> FieldNormTest = new CheckValueItem<FieldNormalizationTypes>(CheckTypes.FieldNormMode, await Ctr.GetFieldNormalizationMode(p.CourseId, p.PlanId), ProtocolFieldNorm, null, "Non-standard normalization");
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
            CheckValueItem<double?> CouchSurfaceTest = new CheckValueItem<double?>(CheckTypes.CouchSurfaceHU, CheckCouchSurface, RefCouchSurface, new TrackedValue<double?>(0.1), CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning);
            CouchSurfaceTest.ParameterOption = CouchSurfaceOption;
            CheckValueItem<double?> CouchInteriorTest = new CheckValueItem<double?>(CheckTypes.CouchInteriorHU, CheckCouchInterior, RefCouchInterior, new TrackedValue<double?>(0.1), CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning);
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

                if (A.E != null)
                {
                    if (A.E.AssignedStructureId != "")
                    {
                        CheckHU = A.E.AssignedHU(p.StructureSetUID);
                    }
                }
                var ArtifactCheck = new CheckValueItem<double?>(CheckTypes.ArtifactHU, CheckHU, A.RefHU, A.ToleranceHU, ArtifactWarningString, NoCheckHUString, NoRefHUString);
                ArtifactCheck.ParameterOption = ParameterOptions.Optional;
                if (A.E != null)
                {
                    ArtifactCheck.OptionalNameSuffix = string.Format(@"(""{0}"")", A.E.AssignedStructureId);
                }
                Calculation_ViewModel.Tests.Add(ArtifactCheck);
            }

            // Course Intent
            var RefCourseIntent = Ctr.CurrentProtocol._TreatmentIntent;
            TreatmentIntents CourseTxIntent;
            Enum.TryParse<TreatmentIntents>(await Ctr.GetCourseIntent(p.CourseId, p.PlanId), out CourseTxIntent);
            var CourseIntentWarningString = "";
            CheckValueItem<TreatmentIntents> CourseIntentTest = new CheckValueItem<TreatmentIntents>(CheckTypes.CourseIntent, CourseTxIntent, RefCourseIntent, null, CourseIntentWarningString);
            DiagnosisIntent_ViewModel.Tests = new ObservableCollection<ITestListItem>() { CourseIntentTest };

            // Plan normalization
            var PNVWarning = "Out of range";
            var PlanPNV = await Ctr.GetPNV(p.CourseId, p.PlanId);
            CheckRangeItem<double?> PNVCheck = new CheckRangeItem<double?>(CheckTypes.PlanNormalization, PlanPNV, Comp.PNVMin, Comp.PNVMax, PNVWarning);

            // Prescription percentage
            var PlanRxPercentage = await Ctr.GetPrescribedPercentage(p.CourseId, p.PlanId);
            CheckValueItem<double> PlanRxPc = new CheckValueItem<double>(CheckTypes.PrescribedPercentage, PlanRxPercentage, new TrackedValue<double>(100), new TrackedValue<double>(1E-5), "Not set to 100");

            // Check Rx and fractions
            var CheckRxDose = await Ctr.GetRxDose(p.CourseId, p.PlanId);
            var RxDoseWarningString = "Plan dose different from protocol";
            var TrackedRefDose = new TrackedValue<double?>(Comp.ReferenceDose);
            var TrackedDoseTolerance = new TrackedValue<double?>(1E-5);
            CheckValueItem<double?> RxCheck = new CheckValueItem<double?>(CheckTypes.PrescriptionDose, CheckRxDose, TrackedRefDose, TrackedDoseTolerance, RxDoseWarningString);

            var CheckFractions = await Ctr.GetNumFractions(p.CourseId, p.PlanId);
            var RefFractions = Comp.NumFractions;
            var TrackedRefFractions = new TrackedValue<int?>(Comp.NumFractions);
            var CheckFractionWarningString = "Plan fractions different from protocol";
            CheckValueItem<int?> FxCheck = new CheckValueItem<int?>(CheckTypes.NumFractions, CheckFractions, TrackedRefFractions, null, CheckFractionWarningString);
            Prescription_ViewModel.Tests = new ObservableCollection<ITestListItem>() { RxCheck, FxCheck, PNVCheck, PlanRxPc };

            // Beam checks
            Beam_ViewModel.Beams.Clear();
            Beam_ViewModel.GroupTests.Tests.Clear();
            var Fields = await Ctr.GetTxFieldItems(p.CourseId, p.PlanId);
            foreach (var Beam in Comp.Beams)
            {
                var BLI = new Controls.BeamListItem(Beam, Fields);
                BLI.InitializeTests();
                Beam_ViewModel.Beams.Add(BLI);
                BLI.FieldChanged += new EventHandler((s, e) => BLI_PropertyChanged(s, e, Comp)); // this updates the MinColOffsetCheck if the field assignments on any reference beam are changed
            }

            // Iso Check
            //var IsoCentreWarning = Fields.Select(x => x.Isocentre).Distinct().Count() != Comp.NumIso;
            var NumIsoDetected = Fields.Select(x => x.Isocentre).Distinct().Count();
            CheckValueItem<int?> NumIsoCheck = new CheckValueItem<int?>(CheckTypes.NumIsocentres, NumIsoDetected, Comp.NumIso, null, "Num isocentres differs");
            Beam_ViewModel.GroupTests.Tests.Add(NumIsoCheck);

            // Num Fields Check
            string BeamRangeWarning = "Number of beams outside range";
            CheckRangeItem<int?> FieldCountCheck = new CheckRangeItem<int?>(CheckTypes.NumFields, Fields.Count(), Comp.MinBeams, Comp.MaxBeams, BeamRangeWarning);
            Beam_ViewModel.GroupTests.Tests.Add(FieldCountCheck);

            // Min Col Offset
            if (Comp.MaxBeams.Value > 1 && Comp.MinColOffset.Value != null && Fields.Count > 1)
            {
                if (Beam_ViewModel.Beams.Any(x => x.Field == null))
                {
                    var MinColOffsetCheck = new CheckValueItem<double?>(CheckTypes.MinColOffset, null, null, null, "Protocol fields not assigned");
                    MinColOffsetCheck.Test = TestType.GreaterThan;
                    Beam_ViewModel.GroupTests.Tests.Add(MinColOffsetCheck);
                }
                else
                {
                    var ColOffset = Beam_ViewModel.Beams.Select(x => x.Field).Select(x => x.CollimatorAngle);
                    int? MinColOffset = (int?)Math.Round(findMinDiff(ColOffset.ToArray()));
                    var MinColOffsetCheck = new CheckValueItem<int?>(CheckTypes.MinColOffset, MinColOffset, Comp.MinColOffset, null, "Insufficient collimator offset");
                    MinColOffsetCheck.Test = TestType.GreaterThan;
                    Beam_ViewModel.GroupTests.Tests.Add(MinColOffsetCheck);
                }

            }


            // Target Structure Checks
            Targets_ViewModel.Tests.Clear();
            foreach (ProtocolStructure E in Ctr.CurrentProtocol.Structures)
            {
                if (E.CheckList != null)
                {
                    var C = E.CheckList;
                    if ((bool)C.isPointContourChecked.Value)
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
                        var TL = new CheckValueItem<double?>(CheckTypes.MinSubvolume, MinVol, C.PointContourVolumeThreshold, null, WarningString, "Not found", "Not specified");
                        TL.OptionalNameSuffix = string.Format(@" of (""{0}"") [cc]", E.AssignedStructureId);
                        TL.Test = TestType.GreaterThan;
                        Targets_ViewModel.Tests.Add(TL);
                    }

                }
            }
        }

        private void BLI_PropertyChanged(object sender, EventArgs e, Component Comp)
        {
            //Refresh Field col separation check

            if (Beam_ViewModel.Beams.Any(x => x.Field == null) || Comp.MaxBeams.Value < 2 || (Comp.MinColOffset.Value != null))
            {
                return;
            }
            var ColOffset = Beam_ViewModel.Beams.Select(x => x.Field).Select(x => x.CollimatorAngle).ToList();
            var MinColOffset = (int?)Math.Round(findMinDiff(ColOffset.ToArray()));
            var OldTest = Beam_ViewModel.GroupTests.Tests.Where(x => x.CheckType == CheckTypes.MinColOffset).FirstOrDefault();
            Beam_ViewModel.GroupTests.Tests.Remove(OldTest);
            var MinColOffsetCheck = new CheckValueItem<int?>(CheckTypes.MinColOffset, MinColOffset, Comp.MinColOffset, null, "Insufficient collimator offset");
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

        public ICommand RejectEditsCommand
        {
            get { return new DelegateCommand(RejectEdits); }
        }

        private void RejectEdits(object param = null)
        {
            foreach (var Test in Simulation_ViewModel.Tests)
            {
                Test.RejectChanges();
            }
        }
        public ICommand AddNewContourCheckCommand
        {
            get { return new DelegateCommand(AddNewContourCheck); }
        }
        private void AddNewContourCheck(object param = null)
        {
            AddStructureCheckVisibility ^= true;
            //PopulateViewFromSelectedComponent();
        }
        public ICommand AddMinSubVolumeCheckCommand
        {
            get { return new DelegateCommand(AddMinSubVolumeCheck); }
        }
        private void AddMinSubVolumeCheck(object param = null)
        {
            StructureSelector SS = param as StructureSelector;
            if (SS != null)
            {
                Ctr.AddNewContourCheck(Ctr.GetProtocolStructure(SS.Id));
                PopulateViewFromProtocol();
            }
        }
        public ICommand RemoveContourCheckCommand
        {
            get { return new DelegateCommand(RemoveContourCheck); }
        }
        public void RemoveContourCheck(object param = null)
        {
            StructureSelector SS = param as StructureSelector;
            if (SS != null)
            {
                Ctr.RemoveNewContourCheck(Ctr.GetProtocolStructure(SS.Id));
                PopulateViewFromProtocol();
            }
        }

        public ICommand EditBeamChecksCommand
        {
            get { return new DelegateCommand(EditBeamChecks); }
        }

        public void EditBeamChecks(object param = null)
        {
            EditBeamChecksVisibility ^= true;
        }

        public ICommand AddNewBeamCommand
        {
            get { return new DelegateCommand(AddNewBeamCheck); }
        }

        public void AddNewBeamCheck(object param = null)
        {
            ComponentSelector CS = SelectedComponent as ComponentSelector;
            if (CS != null)
            {
                Ctr.AddNewBeamCheck(CS.Id);
                PopulateViewFromSelectedComponent();
                EditBeamChecksVisibility ^= true;
            }
        }

        public ICommand RemoveSelectedBeamCommand
        {
            get { return new DelegateCommand(RemoveSelectedBeam); }
        }

        public void RemoveSelectedBeam(object param = null)
        {
            SquintScript.Controls.BeamListItem B = param as SquintScript.Controls.BeamListItem;
            if (B != null)
            {
                B.RetireCheck();
                Beam_ViewModel.Beams.Remove(B);
            }
        }
    }
}
