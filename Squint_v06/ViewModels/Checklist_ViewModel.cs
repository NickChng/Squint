using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using PropertyChanged;
using SquintScript.Interfaces;
using SquintScript.ViewModelClasses;

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
            var SliceSpacingValue = await Ctr.GetSliceSpacing(p.CourseId, p.PlanId);
            TestListDoubleValueItem SliceSpacing = new TestListDoubleValueItem("Slice spacing", SliceSpacingValue, SliceSpacingReference, "Slice spacing does not match protocol");
            SliceSpacing.CheckType = CheckTypes.SliceSpacing;
            TestListStringValueItem Series = new TestListStringValueItem("Series Id", await Ctr.GetSeriesId(p.CourseId, p.PlanId), null);
            TestListStringValueItem Study = new TestListStringValueItem("Study Id", await Ctr.GetStudyId(p.CourseId, p.PlanId), null);
            TestListStringValueItem SeriesComment = new TestListStringValueItem("Series comment / scan protocol", await Ctr.GetSeriesComments(p.CourseId, p.PlanId), null);
            TestListStringValueItem ImageComment = new TestListStringValueItem("Image comment", await Ctr.GetImageComments(p.CourseId, p.PlanId), null);
            TestListIntValueItem NumSlices = new TestListIntValueItem("Number of slices", await Ctr.GetNumSlices(p.CourseId, p.PlanId), null);
            Simulation_ViewModel.Tests = new ObservableCollection<ITestListItem>() { Study, Series, NumSlices, SliceSpacing, SeriesComment, ImageComment };

            // Populate Calculation ViewModel
            Calculation_ViewModel.Tests.Clear(); // = new ObservableCollection<Controls.TestListItem<string>>();
            var ProtocolAlgorithm = P.Checklist.Algorithm.Display();
            var ComponentAlgorithm = await Ctr.GetAlgorithmModel(p.CourseId, p.PlanId);
            List<string> AlgoOptions = new List<string>() { "AAA_11031", "AAA_13623", "AAA_15606" };
            TestListStringChoiceItem Algorithm = new TestListStringChoiceItem("Algorithm", ComponentAlgorithm, ProtocolAlgorithm, AlgoOptions, "Algorithm mismatch");
            Calculation_ViewModel.Tests.Add(Algorithm);

            var DGR_protocol = P.Checklist.AlgorithmResolution;
            var DGRwarningMessage = "Resolution deviation";
            var DGR_plan = await Ctr.GetDoseGridResolution(p.CourseId, p.PlanId);
            TestListDoubleValueItem DoseGridResolution = new TestListDoubleValueItem("Dose grid resolution", DGR_plan, DGR_protocol, DGRwarningMessage);
            Calculation_ViewModel.Tests.Add(DoseGridResolution);

            // Heterogeneity
            var HeteroOn = await Ctr.GetHeterogeneityOn(p.CourseId, p.PlanId);
            var ProtocolHeteroOn = P.Checklist.HeterogeneityOn;
            var HeteroWarningString = "Heterogeneity setting incorrect";
            TestListBoolValueItem HeterogeneityOn = new TestListBoolValueItem("Heterogeneity On", HeteroOn, ProtocolHeteroOn, HeteroWarningString, "Not set", "Not set");
            Calculation_ViewModel.Tests.Add(HeterogeneityOn);

            // Field Normalization
            var FieldNorm = await Ctr.GetFieldNormalizationMode(p.CourseId, p.PlanId);
            var ProtocolFieldNorm = P.Checklist.FieldNormalizationMode.Display();
            TestListStringValueItem FieldNormTest = new TestListStringValueItem("Field Norm Mode", FieldNorm, ProtocolFieldNorm, "Non-standard normalization");
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
            if (double.IsNaN(P.Checklist.CouchSurface))
                CouchSurfaceOption = ParameterOptions.Required;
            if (double.IsNaN(P.Checklist.CouchInterior))
                CouchInteriorOption = ParameterOptions.Required;
            TestListDoubleValueItem CouchSurfaceTest = new TestListDoubleValueItem("Couch Surface HU", CheckCouchSurface, RefCouchSurface, CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning, 0.1);
            CouchSurfaceTest.ParameterOption = CouchSurfaceOption;
            TestListDoubleValueItem CouchInteriorTest = new TestListDoubleValueItem("Couch Interior HU", CheckCouchInterior, RefCouchInterior, CouchWarningMessage, CouchNotFoundWarning, CouchHUNotSpecifiedWarning, 0.1);
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
                var RefHUString = string.Format("{0:0.#} \u00B1{1:0.#} HU", A.RefHU, A.ToleranceHU);
                if (A.E.AssignedStructureId != "")
                {
                    CheckHU = A.E.AssignedHU(p.StructureSetUID);
                }
                var ArtifactCheck = new TestListDoubleValueItem(string.Format(@"Artifact HU (""{0}"")", A.E.AssignedStructureId), CheckHU, A.RefHU, ArtifactWarningString, NoCheckHUString, NoRefHUString, A.ToleranceHU);
                ArtifactCheck.ParameterOption = ParameterOptions.Optional;
                Calculation_ViewModel.Tests.Add(ArtifactCheck);
            }

            // Course Intent
            var RefCourseIntent = Ctr.GetActiveProtocol().TreatmentIntent.Display();
            var CheckCourseIntent = await Ctr.GetCourseIntent(p.CourseId, p.PlanId);
            var CourseIntentWarningString = "";
            TestListStringValueItem CourseIntentTest = new TestListStringValueItem("Course Intent", CheckCourseIntent, RefCourseIntent, CourseIntentWarningString);

            // Plan normalization
            var ProtocolPNVMax = P.Checklist.PNVMax;
            var ProtocolPNVMin = P.Checklist.PNVMin;
            var PNVWarning = "Out of range";
            var PlanPNV = await Ctr.GetPNV(p.CourseId, p.PlanId);
            TestListDoubleRangeItem PNVCheck = new TestListDoubleRangeItem("Plan Normalization Value Range", PlanPNV, ProtocolPNVMin, ProtocolPNVMax, PNVWarning, "-", "Not specified");

            // Prescription percentage
            var PlanRxPercentage = await Ctr.GetPrescribedPercentage(p.CourseId, p.PlanId);
            TestListDoubleValueItem PlanRxPc = new TestListDoubleValueItem("Prescribed percentage", PlanRxPercentage, 100, "Not set to 100");

            // Check Rx and fractions
            var CheckRxDose = await Ctr.GetRxDose(p.CourseId, p.PlanId);
            var RxDoseWarningString = "Plan dose different from protocol";
            TestListDoubleValueItem RxCheck = new TestListDoubleValueItem("Prescription dose", CheckRxDose, Comp.ReferenceDose, RxDoseWarningString);

            var CheckFractions = await Ctr.GetNumFractions(p.CourseId, p.PlanId);
            var RefFractions = Comp.NumFractions;
            var CheckFractionWarningString = "Plan fractions different from protocol";
            TestListIntValueItem FxCheck = new TestListIntValueItem("Number of fractions", CheckFractions, Comp.NumFractions, CheckFractionWarningString);
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
            var IsoCentreWarning = Fields.Select(x => x.Isocentre).Distinct().Count() != Comp.NumIso;
            var NumIsoDetected = Fields.Select(x => x.Isocentre).Distinct().Count();
            TestListIntValueItem NumIsoCheck = new TestListIntValueItem("Number of isocentres", NumIsoDetected, Comp.NumIso, "Num isocentres differs");
            Beam_ViewModel.GroupTests.Tests.Add(NumIsoCheck);

            // Num Fields Check
            int? MaxBeams = Comp.MaxBeams;
            if (MaxBeams < 0) MaxBeams = null;
            int? MinBeams = Comp.MinBeams;
            if (MinBeams < 0) MinBeams = null;
            string BeamRangeWarning = "Number of beams outside range";
            TestListIntRangeItem FieldCountCheck = new TestListIntRangeItem("Number of fields", Fields.Count(), MinBeams, MaxBeams, BeamRangeWarning, "-", "Not specified");
            Beam_ViewModel.GroupTests.Tests.Add(FieldCountCheck);

            // Min Col Offset
            if (Comp.MaxBeams > 1 && !double.IsNaN(Comp.MinColOffset) && Fields.Count > 1)
            {
                TestListDoubleValueItem MinColOffsetCheck;
                if (Beam_ViewModel.Beams.Any(x => x.Field == null))
                {
                    MinColOffsetCheck = new TestListDoubleValueItem("Min collimator offset", null, Comp.MinColOffset, "Protocol fields not assigned");
                }
                else
                {
                    var ColOffset = Beam_ViewModel.Beams.Select(x => x.Field).Select(x => x.CollimatorAngle);
                    var MinColOffset = findMinDiff(ColOffset.ToArray());
                    double? ProtocolMinColOffset = Comp.MinColOffset;
                    MinColOffsetCheck = new TestListDoubleValueItem("Min collimator offset", MinColOffset, ProtocolMinColOffset, "Insufficient collimator offset");
                    MinColOffsetCheck.TestType = TestType.GreaterThan;
                }
                Beam_ViewModel.GroupTests.Tests.Add(MinColOffsetCheck);
            }


            // Target Structure Checks
            Targets_ViewModel.Tests.Clear();
            foreach (Ctr.ProtocolStructure E in Ctr.GetStructureList())
            {
                if (E.CheckList != null)
                {
                    var C = E.CheckList;
                    if (C.isPointContourChecked)
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
                        var TL = new TestListDoubleValueItem(string.Format(@"Min. Subvolume (""{0}"") [cc]", E.ProtocolStructureName, E.AssignedStructureId),
                            MinVol, C.PointContourVolumeThreshold, WarningString, "Not found", "Not specified");
                        TL.CheckNullWarningString = "Structure not found";
                        TL.TestType = TestType.GreaterThan;
                        Targets_ViewModel.Tests.Add(TL);
                    }

                }
            }
        }

        private void BLI_PropertyChanged(object sender, EventArgs e, Ctr.Component Comp)
        {
            //Refresh Field col separation check

            if (Beam_ViewModel.Beams.Any(x => x.Field == null) || Comp.MaxBeams < 2 || double.IsNaN(Comp.MinColOffset))
            {
                return;
            }
            var ColOffset = Beam_ViewModel.Beams.Select(x => x.Field).Select(x => x.CollimatorAngle).ToList();
            var MinColOffset = findMinDiff(ColOffset.ToArray());
            var OldTest = Beam_ViewModel.GroupTests.Tests.Where(x => x.TestName == "Min collimator offset").FirstOrDefault();
            Beam_ViewModel.GroupTests.Tests.Remove(OldTest);
            double ProtocolMinColOffset = Comp.MinColOffset;
            TestListDoubleValueItem MinColOffsetCheck = new TestListDoubleValueItem("Min collimator offset", MinColOffset, ProtocolMinColOffset, "Insufficient collimator offset");
            MinColOffsetCheck.TestType = TestType.GreaterThan;
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
            amEditing ^= true;
        }
    }
}
