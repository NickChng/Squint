using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PropertyChanged;
using Squint.Interfaces;
using Squint.TestFramework;
using Squint.Extensions;
using Squint.Views;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class PointCheck_ViewModel : ObservableObject
    {
        public class PointCheckObject
        {
            public string ProtocolStructureId { get; set; }
            public string MinSubVolume { get; set; }
            public string Centroid_x { get; set; }
            public string Centroid_y { get; set; }
            public string Centroid_z { get; set; }

            public string CentroidString 
            { 
                get
                {
                    if (Warning != null)
                    {
                        if ((bool)Warning)
                            return string.Format("{0},{1},{2}", Centroid_x, Centroid_y, Centroid_z);
                        else
                            return "";
                    }
                    else
                        return "";
                }
            }
            public bool? Warning { get; set; }
            public string WarningString { get; set; }
            public bool Assigned { get; set; }
            public bool isEmpty { get; set; } = false;

            public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;

        }
        public ObservableCollection<PointCheckObject> Checks { get; set; } = new ObservableCollection<PointCheckObject>();
    }

    [AddINotifyPropertyChangedInterface]

    public class Checklist_ViewModel : ObservableObject
    {
        public Beam_ViewModel Beam_ViewModel { get; set; } = new Beam_ViewModel();
        public LoadingViewModel Loading_ViewModel { get; set; } = new LoadingViewModel();
        public OptimizationCheckViewModel Objectives_ViewModel { get; set; } = new OptimizationCheckViewModel();
        public Imaging_ViewModel Imaging_ViewModel { get; set; } = new Imaging_ViewModel();
        public TestList_ViewModel Simulation_ViewModel { get; set; } = new TestList_ViewModel();
        public PointCheck_ViewModel PointCheck_VM { get; set; } = new PointCheck_ViewModel();
        public TestList_ViewModel Calculation_ViewModel { get; set; } = new TestList_ViewModel();
        public TestList_ViewModel Prescription_ViewModel { get; set; } = new TestList_ViewModel();
        public TestList_ViewModel DiagnosisIntent_ViewModel { get; set; } = new TestList_ViewModel();
        public ProtocolViewModel ParentView { get; private set; }
        public bool IsDirty { get; set; } = false;
        //public bool adminMode { get { return ParentView.ParentView.AdminOptionsToggle; } }
        public bool AddStructureCheckVisibility { get; set; }

        public bool EditBeamChecksVisibility { get; set; }

        public BeamListItem SelectedBeam { get; set; }

        private ComponentSelector _SelectedComponent;

        public List<AsyncStructure> StructuresWithDensityOverride { get; set; }
        public List<AsyncStructure> StructuresWithHighResolutionContours { get; set; }

        public ObservableCollection<ComponentSelector> Components { get; set; } = new ObservableCollection<ComponentSelector>();
        public ComponentSelector SelectedComponent
        {
            get { return _SelectedComponent; }
            set { _SelectedComponent = value; PopulateViewFromSelectedComponent(); }
        }
        public StructureSelector SelectedStructure { get; set; }
        public Checklist_ViewModel(ProtocolViewModel parentView)
        {
            ParentView = parentView;
            Ctr.ProtocolUpdated += Ctr_ProtocolUpdated;
            Ctr.ProtocolOpened += Ctr_ProtocolUpdated;
        }

        private void Ctr_ProtocolUpdated(object sender, EventArgs e)
        {
            int? OldComponentId = null;
            if (_SelectedComponent != null)
                OldComponentId = _SelectedComponent.Id;
            PopulateViewFromProtocol();
            if (OldComponentId != null)
                SelectedComponent = Components.FirstOrDefault(x => x.Id == OldComponentId);
            if (SelectedComponent != null)
                PopulateViewFromSelectedComponent();
        }

        private void PopulateViewFromProtocol()
        {
            // No pre-population of the Objectives of imaging checks yet, as these aren't editable

            // Populate Simulation ViewModel
            var P = Ctr.CurrentProtocol;
            Components.Clear();
            foreach (var Comp in Ctr.CurrentProtocol.Components.OrderBy(x => x.DisplayOrder))
            {
                Components.Add(new ComponentSelector(Comp));
            }

            Simulation_ViewModel.Tests.Clear();
            TestValueItem<double?> SliceSpacing = new TestValueItem<double?>(CheckTypes.SliceSpacing, null, P.Checklist.SliceSpacing, new TrackedValue<double?>(1E-5), "Slice spacing does not match protocol");
            TestValueItem<string> Series = new TestValueItem<string>(CheckTypes.SeriesId, null, null) { ParameterOption = ParameterOptions.Optional, IsInfoOnly = true };
            TestContainsItem<string> CTDevice = new TestContainsItem<string>(CheckTypes.CTDeviceId, null, P.Checklist.CTDeviceIds) { IsInfoOnly = false }; 
            TestValueItem<string> Study = new TestValueItem<string>(CheckTypes.StudyId, null, null) { ParameterOption = ParameterOptions.Optional, IsInfoOnly = true };
            TestValueItem<string> SeriesComment = new TestValueItem<string>(CheckTypes.SeriesComment, null, null) { ParameterOption = ParameterOptions.Optional, IsInfoOnly = true };
            TestValueItem<string> ImageComment = new TestValueItem<string>(CheckTypes.ImageComment, null, null) { ParameterOption = ParameterOptions.Optional, IsInfoOnly = true };
            TestValueItem<int?> NumSlices = new TestValueItem<int?>(CheckTypes.NumSlices, null, null) { ParameterOption = ParameterOptions.Optional, IsInfoOnly = true };
            Simulation_ViewModel.Tests.Add(Study);
            Simulation_ViewModel.Tests.Add(Series);
            Simulation_ViewModel.Tests.Add(CTDevice);
            Simulation_ViewModel.Tests.Add(NumSlices);
            Simulation_ViewModel.Tests.Add(SliceSpacing);
            Simulation_ViewModel.Tests.Add(SeriesComment);
            Simulation_ViewModel.Tests.Add(ImageComment);

            // Populate Calculation ViewModel
            Calculation_ViewModel.Tests.Clear(); // = new ObservableCollection<Controls.TestListItem<string>>();
            TestValueItem<AlgorithmVolumeDoseTypes> Algorithm = new TestValueItem<AlgorithmVolumeDoseTypes>(CheckTypes.Algorithm, AlgorithmVolumeDoseTypes.Unset, P.Checklist.Algorithm, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(Algorithm);

            TestValueItem<AlgorithmVMATOptimizationTypes> VMATAlgorithm = new TestValueItem<AlgorithmVMATOptimizationTypes>(CheckTypes.AlgorithmVMATOptimization, AlgorithmVMATOptimizationTypes.Unset, P.Checklist.AlgorithmVMATOptimization, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(VMATAlgorithm);

            TestValueItem<string> AirCavityCorrectionVMATOn = new TestValueItem<string>(CheckTypes.AirCavityCorrectionVMAT, null, P.Checklist.AirCavityCorrectionVMAT, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(AirCavityCorrectionVMATOn);

            TestValueItem<AlgorithmIMRTOptimizationTypes> IMRTAlgorithm = new TestValueItem<AlgorithmIMRTOptimizationTypes>(CheckTypes.AlgorithmIMRTOptimization, AlgorithmIMRTOptimizationTypes.Unset, P.Checklist.AlgorithmIMRTOptimization, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(IMRTAlgorithm);

            TestValueItem<string> AirCavityCorrectionIMRTOn = new TestValueItem<string>(CheckTypes.AirCavityCorrectionIMRT, null, P.Checklist.AirCavityCorrectionIMRT, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(AirCavityCorrectionIMRTOn);

            var DGRwarningMessage = "Resolution deviation";
            TestValueItem<double?> DoseGridResolution = new TestValueItem<double?>(CheckTypes.DoseGridResolution, null, P.Checklist.AlgorithmResolution, new TrackedValue<double?>(1E-2), DGRwarningMessage);
            Calculation_ViewModel.Tests.Add(DoseGridResolution);

            // Heterogeneity
            var HeteroWarningString = "Heterogeneity setting incorrect";
            TestValueItem<bool?> HeterogeneityOn = new TestValueItem<bool?>(CheckTypes.HeterogeneityOn, null, P.Checklist.HeterogeneityOn, null, HeteroWarningString, "Not set", "Not set");
            Calculation_ViewModel.Tests.Add(HeterogeneityOn);

            // Field Normalization
            TestValueItem<FieldNormalizationTypes> FieldNormTest = new TestValueItem<FieldNormalizationTypes>(CheckTypes.FieldNormMode, FieldNormalizationTypes.Unset, P.Checklist.FieldNormalizationMode, null, "Non-standard normalization");
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
            TestValueItem<double?> CouchSurfaceTest = new TestValueItem<double?>(CheckTypes.CouchSurfaceHU, null, RefCouchSurface, new TrackedValue<double?>(0.1), CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning);
            CouchSurfaceTest.ParameterOption = CouchSurfaceOption;
            TestValueItem<double?> CouchInteriorTest = new TestValueItem<double?>(CheckTypes.CouchInteriorHU, null, RefCouchInterior, new TrackedValue<double?>(0.1), CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning);
            CouchInteriorTest.ParameterOption = CouchInteriorOption;
            Calculation_ViewModel.Tests.Add(CouchSurfaceTest);
            Calculation_ViewModel.Tests.Add(CouchInteriorTest);

            // Artifacts in calculaion
            foreach (var A in P.Checklist.Artifacts)
            {
                var ArtifactWarningString = "Assigned HU deviates from protocol";
                string NoCheckHUString = @"No artifact structure";
                string NoRefHUString = @"Not specified";
                var ArtifactCheck = new TestValueItem<double?>(CheckTypes.ArtifactHU, null, A.RefHU, A.ToleranceHU, ArtifactWarningString, NoCheckHUString, NoRefHUString);
                var PS = Ctr.GetProtocolStructure(A.ProtocolStructureId.Value);
                if (PS != null)
                    ArtifactCheck.OptionalNameSuffix = string.Format(@"(""{0}"")", PS.AssignedStructureId);
                ArtifactCheck.ParameterOption = ParameterOptions.Optional;
                Calculation_ViewModel.Tests.Add(ArtifactCheck);
            }

            // Course Intent
            var ValidTreatmentIntents = Ctr.CurrentProtocol.TreatmentIntents;
            var CourseIntentWarningString = "Invalid course intent";
            TestContainsItem<TreatmentIntents> CourseIntentTest = new TestContainsItem<TreatmentIntents>(CheckTypes.CourseIntent, TreatmentIntents.Unset, ValidTreatmentIntents, null, CourseIntentWarningString);
            DiagnosisIntent_ViewModel.Tests = new ObservableCollection<ITestListItem>() { CourseIntentTest };

            // Structure
            // old point checks.
        }
        private void PopulateViewFromSelectedComponent()
        {

            if (_SelectedComponent == null)
                return;
            var P = Ctr.CurrentProtocol;
            Component Comp = Ctr.GetComponent(_SelectedComponent.Id);

            var PNVWarning = "Out of range";
            TestRangeItem<double?> PNVCheck = new TestRangeItem<double?>(CheckTypes.PlanNormalization, null, Comp.PNVMin, Comp.PNVMax, PNVWarning);

            // Prescription percentage
            TestValueItem<double?> PlanRxPc = new TestValueItem<double?>(CheckTypes.PrescribedPercentage, null, Comp.PrescribedPercentage, new TrackedValue<double?>(1E-5), "Not set to 100");

            // Check Rx and fractions

            var RxDoseWarningString = "Plan dose different from protocol";
            var TrackedRefDose = new TrackedValue<double?>(Comp.TotalDose);
            var TrackedDoseTolerance = new TrackedValue<double?>(1E-5);
            TestValueItem<double?> RxCheck = new TestValueItem<double?>(CheckTypes.PrescriptionDose, null, TrackedRefDose, TrackedDoseTolerance, RxDoseWarningString);
            RxCheck.EditEnabled = false;

            var RefFractions = Comp.NumFractions;
            var TrackedRefFractions = new TrackedValue<int?>(Comp.NumFractions);
            var CheckFractionWarningString = "Plan fractions different from protocol";
            TestValueItem<int?> FxCheck = new TestValueItem<int?>(CheckTypes.NumFractions, null, TrackedRefFractions, null, CheckFractionWarningString);
            FxCheck.EditEnabled = false;
            Prescription_ViewModel.Tests = new ObservableCollection<ITestListItem>() { RxCheck, FxCheck, PNVCheck, PlanRxPc };

            // Beam checks
            Beam_ViewModel.Beams.Clear();
            Beam_ViewModel.GroupTests.Tests.Clear();
            foreach (var Beam in Comp.Beams)
            {
                var BLI = new BeamListItem(Beam, null);
                Beam_ViewModel.Beams.Add(BLI);
                BLI.InitializeTests();
                BLI.FieldChanged += new EventHandler((s, e) => BLI_PropertyChanged(s, e, Comp)); // this updates the MinColOffsetCheck if the field assignments on any reference beam are changed
            }

            // Iso Check
            //var IsoCentreWarning = Fields.Select(x => x.Isocentre).Distinct().Count() != Comp.NumIso;
            TestValueItem<int?> NumIsoCheck = new TestValueItem<int?>(CheckTypes.NumIsocentres, -1, Comp.NumIso, null, "Num isocentres differs");
            Beam_ViewModel.GroupTests.Tests.Add(NumIsoCheck);

            // Num Fields Check
            string BeamRangeWarning = "Number of beams outside range";
            TestRangeItem<int?> FieldCountCheck = new TestRangeItem<int?>(CheckTypes.NumFields, -1, Comp.MinBeams, Comp.MaxBeams, BeamRangeWarning);
            Beam_ViewModel.GroupTests.Tests.Add(FieldCountCheck);

            // Min Col Offset
            var MinColOffsetCheck = new TestValueItem<double?>(CheckTypes.MinColOffset, null, Comp.MinColOffset, null, "Protocol fields not assigned");
            MinColOffsetCheck.Test = TestType.GreaterThan;
            Beam_ViewModel.GroupTests.Tests.Add(MinColOffsetCheck);

        }
        public async Task DisplayChecksForPlan(PlanSelector p)
        {

            StructuresWithDensityOverride = Ctr.CurrentStructureSet.GetAllStructures().Where(x => !double.IsNaN(x.HU) && x.DicomType != @"SUPPORT").ToList();
            StructuresWithHighResolutionContours = Ctr.CurrentStructureSet.GetAllStructures().Where(x => x.IsHighResolution).ToList();
            Objectives_ViewModel = new OptimizationCheckViewModel();
            var Objectives = await Ctr.GetOptimizationObjectiveList(p.CourseId, p.PlanId);
            List<string> StructureIds = new List<string>();
            ObservableCollection<ObjectiveItem> objectiveItems = new ObservableCollection<ObjectiveItem>();
            foreach (ObjectiveDefinition OD in Objectives)
            {
                if (!StructureIds.Contains(OD.StructureId))
                {
                    objectiveItems.Add(new ObjectiveItem(OD.StructureId, OD));
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
            foreach (ImagingProtocolTypes IP in Comp.ImagingProtocols)
            {
                ProtocolImagingViewModel PIV = new ProtocolImagingViewModel() { ImagingProtocolName = IP.Display() };
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
            TestValueItem<double?> SliceSpacing = new TestValueItem<double?>(CheckTypes.SliceSpacing, SliceSpacingValue, SliceSpacingReference, new TrackedValue<double?>(1E-5), "Slice spacing does not match protocol");
            SliceSpacing.CheckType = CheckTypes.SliceSpacing;
            TestValueItem<string> Series = new TestValueItem<string>(CheckTypes.SeriesId, await Ctr.GetSeriesId(p.CourseId, p.PlanId), null) { ParameterOption = ParameterOptions.Optional };
            TestValueItem<string> Study = new TestValueItem<string>(CheckTypes.StudyId, await Ctr.GetStudyId(p.CourseId, p.PlanId), null) { ParameterOption = ParameterOptions.Optional };
            TestContainsItem<string> CTDevice = new TestContainsItem<string>(CheckTypes.CTDeviceId, await Ctr.GetCTDeviceId(p.CourseId, p.PlanId), P.Checklist.CTDeviceIds) { IsInfoOnly = false };
            TestValueItem<string> SeriesComment = new TestValueItem<string>(CheckTypes.SeriesComment, await Ctr.GetSeriesComments(p.CourseId, p.PlanId), null) { ParameterOption = ParameterOptions.Optional };
            TestValueItem<string> ImageComment = new TestValueItem<string>(CheckTypes.ImageComment, await Ctr.GetImageComments(p.CourseId, p.PlanId), null) { ParameterOption = ParameterOptions.Optional };
            var NumSlices = await Ctr.GetNumSlices(p.CourseId, p.PlanId);
            int Slices;
            if (NumSlices != null)
                Slices = (int)NumSlices;
            else
                Slices = int.MinValue;
            TestValueItem<int> NumSlicesChk = new TestValueItem<int>(CheckTypes.NumSlices, Slices, null) { ParameterOption = ParameterOptions.Optional };
            Simulation_ViewModel.Tests = new ObservableCollection<ITestListItem>() { Study, Series, CTDevice, NumSlicesChk, SliceSpacing, SeriesComment, ImageComment };

            // Populate Calculation ViewModel
            Calculation_ViewModel.Tests.Clear(); // = new ObservableCollection<Controls.TestListItem<string>>();
            var ProtocolAlgorithm = P.Checklist.Algorithm;
            AlgorithmVolumeDoseTypes ComponentAlgorithm;
            Enum.TryParse(await Ctr.GetAlgorithmModel(p.CourseId, p.PlanId), out ComponentAlgorithm);
            TestValueItem<AlgorithmVolumeDoseTypes> Algorithm = new TestValueItem<AlgorithmVolumeDoseTypes>(CheckTypes.Algorithm, ComponentAlgorithm, ProtocolAlgorithm, null, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(Algorithm);

            var ProtocolVMATAlgorithm = P.Checklist.AlgorithmVMATOptimization;
            if (ProtocolVMATAlgorithm.Value != AlgorithmVMATOptimizationTypes.Unset)
            {
                AlgorithmVMATOptimizationTypes ComponentVMATAlgorithm;
                Enum.TryParse(await Ctr.GetVMATAlgorithmModel(p.CourseId, p.PlanId), out ComponentVMATAlgorithm);
                TestValueItem<AlgorithmVMATOptimizationTypes> VMATAlgorithm = new TestValueItem<AlgorithmVMATOptimizationTypes>(CheckTypes.AlgorithmVMATOptimization, ComponentVMATAlgorithm, ProtocolVMATAlgorithm, null, "Algorithm mismatch");
                Calculation_ViewModel.Tests.Add(VMATAlgorithm);
            }

            var ProtocolVMATAirCavityCorrection = P.Checklist.AirCavityCorrectionVMAT;
            if (!string.IsNullOrEmpty(ProtocolVMATAirCavityCorrection.Value))
            {
                var PlanVMATAirCavityCorrection = await Ctr.GetAirCavityCorrectionVMAT(p.CourseId, p.PlanId);
                TestValueItem<string> AirCavityCorrectionVMATOn = new TestValueItem<string>(CheckTypes.AirCavityCorrectionVMAT, PlanVMATAirCavityCorrection, ProtocolVMATAirCavityCorrection, null, "Option mismatch");
                Calculation_ViewModel.Tests.Add(AirCavityCorrectionVMATOn);
            }

            var ProtocolIMRTAlgorithm = P.Checklist.AlgorithmIMRTOptimization;
            if (ProtocolIMRTAlgorithm.Value != AlgorithmIMRTOptimizationTypes.Unset)
            {
                AlgorithmIMRTOptimizationTypes ComponentIMRTAlgorithm;
                Enum.TryParse(await Ctr.GetIMRTAlgorithmModel(p.CourseId, p.PlanId), out ComponentIMRTAlgorithm);
                TestValueItem<AlgorithmIMRTOptimizationTypes> IMRTAlgorithm = new TestValueItem<AlgorithmIMRTOptimizationTypes>(CheckTypes.AlgorithmIMRTOptimization, ComponentIMRTAlgorithm, ProtocolIMRTAlgorithm, null, "Algorithm mismatch");
                Calculation_ViewModel.Tests.Add(IMRTAlgorithm);
            }

            var ProtocolIMRTAirCavityCorrection = P.Checklist.AirCavityCorrectionIMRT;
            if (!string.IsNullOrEmpty(ProtocolIMRTAirCavityCorrection.Value))
            {
                var PlanIMRTAirCavityCorrection = await Ctr.GetAirCavityCorrectionIMRT(p.CourseId, p.PlanId);
                TestValueItem<string> AirCavityCorrectionIMRTOn = new TestValueItem<string>(CheckTypes.AirCavityCorrectionIMRT, PlanIMRTAirCavityCorrection, ProtocolIMRTAirCavityCorrection, null, "Option mismatch");
                Calculation_ViewModel.Tests.Add(AirCavityCorrectionIMRTOn);
            }

            var DGR_protocol = P.Checklist.AlgorithmResolution;
            var DGRwarningMessage = "Resolution deviation";
            var DGR_plan = await Ctr.GetDoseGridResolution(p.CourseId, p.PlanId);
            TestValueItem<double?> DoseGridResolution = new TestValueItem<double?>(CheckTypes.DoseGridResolution, DGR_plan, DGR_protocol, new TrackedValue<double?>(1E-2), DGRwarningMessage);
            Calculation_ViewModel.Tests.Add(DoseGridResolution);

            // Heterogeneity
            var HeteroOn = await Ctr.GetHeterogeneityOn(p.CourseId, p.PlanId);
            var ProtocolHeteroOn = P.Checklist.HeterogeneityOn;
            var HeteroWarningString = "Heterogeneity setting incorrect";
            TestValueItem<bool?> HeterogeneityOn = new TestValueItem<bool?>(CheckTypes.HeterogeneityOn, HeteroOn, ProtocolHeteroOn, null, HeteroWarningString, "Not set", "Not set");
            Calculation_ViewModel.Tests.Add(HeterogeneityOn);

            // Field Normalization
            var ProtocolFieldNorm = P.Checklist.FieldNormalizationMode;
            TestValueItem<FieldNormalizationTypes> FieldNormTest = new TestValueItem<FieldNormalizationTypes>(CheckTypes.FieldNormMode, await Ctr.GetFieldNormalizationMode(p.CourseId, p.PlanId), ProtocolFieldNorm, null, "Non-standard normalization");
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
            TestValueItem<double?> CouchSurfaceTest = new TestValueItem<double?>(CheckTypes.CouchSurfaceHU, CheckCouchSurface, RefCouchSurface, new TrackedValue<double?>(0.1), CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning);
            CouchSurfaceTest.ParameterOption = CouchSurfaceOption;
            TestValueItem<double?> CouchInteriorTest = new TestValueItem<double?>(CheckTypes.CouchInteriorHU, CheckCouchInterior, RefCouchInterior, new TrackedValue<double?>(0.1), CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning);
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
                ProtocolStructure PS = Ctr.GetProtocolStructure(A.ProtocolStructureId.Value);
                if (PS != null)
                {
                    if (PS.AssignedStructureId != "")
                    {
                        CheckHU = PS.GetAssignedHU(p.StructureSetUID);
                    }
                }
                var ArtifactCheck = new TestValueItem<double?>(CheckTypes.ArtifactHU, CheckHU, A.RefHU, A.ToleranceHU, ArtifactWarningString, NoCheckHUString, NoRefHUString);
                ArtifactCheck.ParameterOption = ParameterOptions.Optional;
                if (PS != null)
                {
                    ArtifactCheck.OptionalNameSuffix = string.Format(@"(""{0}"")", PS.AssignedStructureId);
                }
                Calculation_ViewModel.Tests.Add(ArtifactCheck);
            }

            // Course Intent
            var RefCourseIntent = Ctr.CurrentProtocol.TreatmentIntents;
            TreatmentIntents CourseTxIntent;
            Enum.TryParse<TreatmentIntents>(await Ctr.GetCourseIntent(p.CourseId, p.PlanId), out CourseTxIntent);
            var CourseIntentWarningString = "";
            TestContainsItem<TreatmentIntents> CourseIntentTest = new TestContainsItem<TreatmentIntents>(CheckTypes.CourseIntent, CourseTxIntent, RefCourseIntent, null, CourseIntentWarningString);
            DiagnosisIntent_ViewModel.Tests = new ObservableCollection<ITestListItem>() { CourseIntentTest };

            // Plan normalization
            var PNVWarning = "Out of range";
            var PlanPNV = await Ctr.GetPNV(p.CourseId, p.PlanId);
            TestRangeItem<double?> PNVCheck = new TestRangeItem<double?>(CheckTypes.PlanNormalization, PlanPNV, Comp.PNVMin, Comp.PNVMax, PNVWarning);

            // Prescription percentage
            var PlanRxPercentage = await Ctr.GetPrescribedPercentage(p.CourseId, p.PlanId);
            TestValueItem<double> PlanRxPc = new TestValueItem<double>(CheckTypes.PrescribedPercentage, PlanRxPercentage, new TrackedValue<double>(100), new TrackedValue<double>(1E-5), "Not set to 100");

            // Check Rx and fractions
            var CheckRxDose = await Ctr.GetRxDose(p.CourseId, p.PlanId);
            var RxDoseWarningString = "Plan dose different from protocol";
            var TrackedRefDose = new TrackedValue<double?>(Comp.TotalDose);
            var TrackedDoseTolerance = new TrackedValue<double?>(1E-5);
            TestValueItem<double?> RxCheck = new TestValueItem<double?>(CheckTypes.PrescriptionDose, CheckRxDose, TrackedRefDose, TrackedDoseTolerance, RxDoseWarningString);

            var CheckFractions = await Ctr.GetNumFractions(p.CourseId, p.PlanId);
            var RefFractions = Comp.NumFractions;
            var TrackedRefFractions = new TrackedValue<int?>(Comp.NumFractions);
            var CheckFractionWarningString = "Plan fractions different from protocol";
            TestValueItem<int?> FxCheck = new TestValueItem<int?>(CheckTypes.NumFractions, CheckFractions, TrackedRefFractions, null, CheckFractionWarningString);
            Prescription_ViewModel.Tests = new ObservableCollection<ITestListItem>() { RxCheck, FxCheck, PNVCheck, PlanRxPc };

            // Beam checks
            Beam_ViewModel.Beams.Clear();
            Beam_ViewModel.GroupTests.Tests.Clear();
            var Fields = await Ctr.GetTxFieldItems(p.CourseId, p.PlanId);
            foreach (var Beam in Comp.Beams)
            {
                var BLI = new BeamListItem(Beam, Fields);
                BLI.InitializeTests();
                Beam_ViewModel.Beams.Add(BLI);
                BLI.FieldChanged += new EventHandler((s, e) => BLI_PropertyChanged(s, e, Comp)); // this updates the MinColOffsetCheck if the field assignments on any reference beam are changed
            }

            // Iso Check
            //var IsoCentreWarning = Fields.Select(x => x.Isocentre).Distinct().Count() != Comp.NumIso;
            var NumIsoDetected = Fields.Select(x => x.Isocentre).Distinct().Count();
            TestValueItem<int?> NumIsoCheck = new TestValueItem<int?>(CheckTypes.NumIsocentres, NumIsoDetected, Comp.NumIso, null, "Num isocentres differs");
            Beam_ViewModel.GroupTests.Tests.Add(NumIsoCheck);

            // Num Fields Check
            string BeamRangeWarning = "Number of beams outside range";
            TestRangeItem<int?> FieldCountCheck = new TestRangeItem<int?>(CheckTypes.NumFields, Fields.Count(), Comp.MinBeams, Comp.MaxBeams, BeamRangeWarning);
            Beam_ViewModel.GroupTests.Tests.Add(FieldCountCheck);

            // Min Col Offset
            if (Comp.MaxBeams.Value > 1 && Comp.MinColOffset.Value != null && Fields.Count > 1)
            {
                var MinColOffsetCheck = new TestValueItem<double?>(CheckTypes.MinColOffset, null, Comp.MinColOffset, null, "Insufficient collimator offset", "Fields not assigned");
                MinColOffsetCheck.ParameterOption = ParameterOptions.Optional;
                MinColOffsetCheck.Test = TestType.GreaterThan;
                Beam_ViewModel.GroupTests.Tests.Add(MinColOffsetCheck);
                if (!Beam_ViewModel.Beams.Any(x => x.Field == null))
                {
                    var ColOffset = Beam_ViewModel.Beams.Select(x => x.Field).Select(x => x.CollimatorAngle);
                    double? MinColOffset = (double?)Math.Round(findMinDiff(ColOffset.ToArray()));
                    MinColOffsetCheck.SetCheckValue(MinColOffset);
                }

            }

            // Target Structure Checks
            PointCheck_VM.Checks.Clear();
            foreach (ProtocolStructure E in Ctr.CurrentProtocol.Structures)
            {
                if (E.CheckList != null)
                {
                    var C = E.CheckList;
                    if ((bool)C.isPointContourChecked.Value)
                    {
                        PointCheck_ViewModel.PointCheckObject PO = new PointCheck_ViewModel.PointCheckObject();
                        PO.ProtocolStructureId = string.Format("{0} ({1})", E.ProtocolStructureName, E.AssignedStructureId);
                        if (E.AssignedStructureId == "")
                        {
                            PO.Assigned = false;
                            PO.Warning = null;
                            PO.ParameterOption = ParameterOptions.Optional;
                        }
                        else
                        {
                            PO.Assigned = true;
                            var VolParts = await E.PartVolumes(p.StructureSetUID);
                            double MinVol = double.NaN;
                            if (VolParts != null)
                            {
                                if (VolParts.Count() == 0)
                                {
                                    // Structure found but mesh problems.. likely failing isClosed()in g3helpers 
                                    PO.isEmpty = false;
                                    PO.Warning = null;
                                    PO.ParameterOption = ParameterOptions.Optional;
                                    PO.WarningString = "Unable to validate mesh, cannot check";
                                }
                                else
                                {
                                    MinVol = VolParts.Min();
                                    if (!double.IsNaN(MinVol))
                                    {
                                        var NumDetectedParts = await E.NumParts(p.StructureSetUID);
                                        var VMS_NumParts = await E.VMS_NumParts(p.StructureSetUID);
                                        if (VMS_NumParts > NumDetectedParts)
                                        {
                                            PO.MinSubVolume = @"< 0.1";
                                            var Result = await E.GetMinArea(p.StructureSetUID);
                                            PO.Warning = true;
                                            PO.WarningString = "Stray Voxel";
                                            PO.Centroid_x = string.Format("{0:0.#}", Result.Item2.x);
                                            PO.Centroid_y = string.Format("{0:0.#}", Result.Item2.y);
                                            PO.Centroid_z = string.Format("{0:0.#}", Result.Item2.z);
                                        }
                                        else if (MinVol < 0.2)
                                        {

                                            var Result = await E.GetMinArea(p.StructureSetUID);
                                            if (Result != null)
                                            {
                                                if (Result.Item1 < 50)
                                                {
                                                    PO.MinSubVolume = @"< 0.1";
                                                    PO.Warning = true;
                                                    PO.WarningString = "Small 2D contour";
                                                    PO.Centroid_x = string.Format("{0:0.#}", Result.Item2.x);
                                                    PO.Centroid_y = string.Format("{0:0.#}", Result.Item2.y);
                                                    PO.Centroid_z = string.Format("{0:0.#}", Result.Item2.z);
                                                }
                                                else
                                                {
                                                    PO.Warning = false;
                                                    //PO.WarningString = "Uncertainty in volume measurements";
                                                }
                                            }
                                            else
                                                PO.Warning = false;
                                        }
                                        else
                                        {
                                            PO.MinSubVolume = string.Format("{0:0.##}", MinVol);
                                            PO.Warning = false;
                                            PO.WarningString = "";
                                        }
                                    }

                                }
                            }
                            else
                            {
                                PO.isEmpty = true;
                                PO.Warning = null;
                                PO.ParameterOption = ParameterOptions.Optional;
                                PO.WarningString = "Empty structure";
                            }
                        
                        }
                        PointCheck_VM.Checks.Add(PO);
                    }
                }
            }
        }

        private void BLI_PropertyChanged(object sender, EventArgs e, Component Comp)
        {
            //Refresh Field col separation check

            if (Beam_ViewModel.Beams.Any(x => x.Field == null) || Comp.MaxBeams.Value < 2 || (Comp.MinColOffset.Value == null))
            {
                return;
            }
            var ColOffset = Beam_ViewModel.Beams.Select(x => x.Field).Select(x => x.CollimatorAngle).ToList();
            var MinColOffset = (double?)Math.Round(findMinDiff(ColOffset.ToArray()));
            var OldTest = Beam_ViewModel.GroupTests.Tests.Where(x => x.CheckType == CheckTypes.MinColOffset).FirstOrDefault();
            OldTest.SetCheckValue(MinColOffset);
            //Beam_ViewModel.GroupTests.Tests.Remove(OldTest);
            //var MinColOffsetCheck = new CheckValueItem<int?>(CheckTypes.MinColOffset, MinColOffset, Comp.MinColOffset, null, "Insufficient collimator offset");
            //MinColOffsetCheck.Test = TestType.GreaterThan;
            //Beam_ViewModel.GroupTests.Tests.Add(MinColOffsetCheck);
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
            BeamListItem B = param as BeamListItem;
            if (B != null)
            {
                B.RetireCheck();
                Beam_ViewModel.Beams.Remove(B);
            }
        }
    }
}
