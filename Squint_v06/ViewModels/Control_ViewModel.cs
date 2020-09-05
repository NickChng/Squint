using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using VMS.TPS.Common.Model;
using VMS.TPS.Common.Model.API;
using System.Windows.Controls.Primitives;
using ESAPI = VMS.TPS.Common.Model.API.Application;
using VMSAPI = VMS.TPS.Common.Model.API;
using PropertyChanged;
using SquintScript.Interfaces;
using SquintScript.ViewModelClasses;
using SquintScript.ViewModels;
using SquintScript.Extensions;

namespace SquintScript.Controls
{
    public class NTODefinition : ObservableObject
    {
        public double Priority { get; set; } = 0;
        public double FallOff { get; set; }
        public bool isAutomatic { get; set; }
        public double DistanceFromTargetBorderMM { get; set; }
        public double StartDosePercentage { get; set; }
        public double EndDosePercentage { get; set; }
    }
    public class ObjectiveDefinition : ObservableObject
    {
        public string StructureId { get; set; } = "DefaultStructure";
        public string Definition
        {
            get
            {
                switch (DvhType)
                {
                    case Dvh_Types.M:
                        return string.Format("Mean Dose < {0:0.0} cGy", Dose);
                    case Dvh_Types.V:
                        return string.Format("V{0:0.0} cGy {1} {2:0.0}%", Dose, Type.Display(), Volume);
                    default:
                        return "";
                }
            }
        }
        public double ResultDose { get; set; } = double.NaN;
        public double ResultVolume { get; set; } = double.NaN;
        public string ResultString
        {
            get
            {
                switch (DvhType)
                {
                    case Dvh_Types.M:
                        return string.Format("{0:0.#} cGy", ResultDose);
                    case Dvh_Types.V:
                        return string.Format("{0:0.#} cGy, {1:0.#} %", ResultDose, ResultVolume);
                    default:
                        return "";
                }
            }
        }
        public Dvh_Types DvhType { get; set; } = Dvh_Types.Unset;
        public ReferenceTypes Type { get; set; } = ReferenceTypes.Unset;
        public double DoseDifference { get; set; } = 0;
        public double VolDifference { get; set; } = 0;
        public string DoseDifferenceString
        {
            get
            {
                return string.Format("{0:0.0} cGy", DoseDifference);
            }
        }
        public string VolDifferenceString
        {
            get
            {
                switch (DvhType)
                {
                    case Dvh_Types.V:
                        return string.Format(", {0:0.0} %", VolDifference);
                    default:
                        return "";
                }

            }
        }
        public string WarningText
        {
            get
            {
                if (isInactive)
                    return "No influence";
                else
                    return "";
            }
        }
        public bool isInactive
        {
            get
            {
                if (VolDifference < -0.1 || DoseDifference < -0.1)
                    return true;
                else return false;
            }
        }
        public double Priority { get; set; } = 80;
        public double PriorityWidth { get { return Priority / 2; } }
        public double Volume { get; set; } = 50;
        public double Dose { get; set; } = 1000;
    }
    public class ObjectiveItem : ObservableObject
    {
        public ObjectiveItem(string StructureId_init, ObjectiveDefinition OPO = null)
        {
            StructureId = StructureId_init;
            if (OPO != null)
                ObjectiveDefinitions.Add(OPO);
        }
        public string StructureId { get; set; } = "Default";
        public ObservableCollection<ObjectiveDefinition> ObjectiveDefinitions { get; set; } = new ObservableCollection<ObjectiveDefinition>();
    }
    [AddINotifyPropertyChangedInterface]
    public class Control_ViewModel
    {
        public NTODefinition NTO { get; set; }
        public bool NoNTO
        {
            get
            {
                if (NTO == null)
                    return true;
                else
                    return false;
            }
        }
        public bool NoObjectives
        {
            get
            {
                if (Objectives.Count == 0)
                    return true;
                else return false;
            }
        }
        public ObservableCollection<ObjectiveItem> Objectives { get; set; } = new ObservableCollection<ObjectiveItem>() { new ObjectiveItem("Default"), new ObjectiveItem("Default1") };
    }

    [AddINotifyPropertyChangedInterface]
    public class ProtocolImagingView
    {
        public int Id { get; set; } // the Db key
        public string ImagingProtocolName { get; set; } = "";
        public List<string> WarningMessages { get; set; } = new List<string>();
        public bool isWarning { get; set; } = false;
    }

    [AddINotifyPropertyChangedInterface]
    public class Imaging_ViewModel
    {
        public ObservableCollection<ImagingFieldItem> ImagingFields { get; set; } = new ObservableCollection<ImagingFieldItem>() { new ImagingFieldItem(), new ImagingFieldItem() };
        public ObservableCollection<string> GeneralErrors { get; set; } = new ObservableCollection<string>();
        public bool isGeneralErrors { get { if (GeneralErrors.Count() > 0) return true; else return false; } }
        public bool isProtocolAttached { get { if (ImagingProtocols.Count() > 0) return true; else return false; } }
        public ObservableCollection<ProtocolImagingView> ImagingProtocols { get; set; } = new ObservableCollection<ProtocolImagingView>();
    }
    [AddINotifyPropertyChangedInterface]
    public class Simulation_ViewModel
    {
        public double SliceSpacingProtocol { get; set; } = -1;
        public double SliceSpacingDetected { get; set; } = -1;
        public string SeriesComment { get; set; } = "(No comments found)";
        public string ImageComment { get; set; } = "(No comments found)";
        public string StudyId { get; set; } = "(No Id found)";
        public string SeriesId { get; set; } = "(No Id found)";
        public int NumSlices { get; set; } = 0;
        public bool Warning { get; set; }
        public string WarningMessage { get; set; }
    }
    [AddINotifyPropertyChangedInterface]
    public class TestList_ViewModel :ObservableObject
    {
        public void SendUpdate()
        {
            RaisePropertyChangedEvent(nameof(Tests));
        }
        public ObservableCollection<ITestListItem> Tests { get; set; } = new ObservableCollection<ITestListItem>();
    }
    [AddINotifyPropertyChangedInterface]
    public class Beam_ViewModel
    {
        public string DebugString { get; set; } = "Default_Value";
        public ObservableCollection<BeamListItem> Beams { get; set; } = new ObservableCollection<BeamListItem>();
        public TestList_ViewModel GroupTests { get; set; } = new TestList_ViewModel();

        

    }
    [AddINotifyPropertyChangedInterface]
    public class BeamListItem :ObservableObject
    {
        public event EventHandler FieldChanged;

        private TxFieldItem _Field;
        public TxFieldItem Field
        {
            get { return _Field; }
            set
            {
                _Field = value;
                if (_Field != null & RefBeam != null)
                {
                    RefreshTests();
                    NoFieldAssigned = false;
                    FieldChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public List<TxFieldItem> Fields { get; set; }
        private Beam RefBeam;

        public void RetireCheck()
        {
            RefBeam.ToRetire = true;
        }
        public ObservableCollection<string> Aliases { get; set; } = new ObservableCollection<string> { @"Field1" };
        public string FieldDescription { get; set; }
        public string BeamId { get { return Field.Id; } }
        public string ProtocolBeamName
        {
            get { return RefBeam.ProtocolBeamName; }
            set { RefBeam.ProtocolBeamName = value; }
        }
        public string Technique
        {
            get { return Field.TypeString; }
        }
        public string Energy
        {
            get { return Field.Energy.Display(); }
        }
        public string ToleranceTable
        {
            get { return Field.ToleranceTable; }
        }
        public double Xmin { get { return Field.Xmin; } }
        public double Ymin { get { return Field.Ymin; } }
        public double Xmax { get { return Field.Xmax; } }
        public double Ymax { get { return Field.Ymax; } }
        public double MU
        {
            get { return Field.MU; }
        }
        public double ColRotation { get { return Field.CollimatorAngle; } }
        public double MinColRotation { get; set; }
        public double MaxColRotation { get; set; }
        public double CouchRotation { get { return Field.CouchRotation; } }
        public double StartAngle { get { return Field.GantryStart; } }
        public double EndAngle { get { return Field.GantryEnd; } }
        public bool NoFieldAssigned { get; set; } = true;
        public string SelectedAlias { get; set; }
        public string NewAlias { get; set; }
     
        public BeamListItem(Beam RefBeam_in, List<TxFieldItem> TxFields)
        {
            RefBeam = RefBeam_in;
            Aliases = RefBeam_in.EclipseAliases;
            Fields = TxFields;
            Field = null;
        }
        public void InitializeTests()
        {
            BeamTests.Tests.Clear();
            if (Fields != null)
                foreach (string alias in RefBeam.EclipseAliases)
                    foreach (TxFieldItem F in Fields)
                        if (F.Id == alias)
                        {
                            Field = F;
                            NoFieldAssigned = false;
                        }
            FieldDescription = string.Format(@"Protocol field ""{0}"" assigned to plan field:", RefBeam.ProtocolBeamName);

            // Populate Tests
            BeamTests.Tests.Add(new CheckRangeItem<double?>(CheckTypes.MURange, double.NaN, RefBeam.MinMUWarning, RefBeam.MaxMUWarning, "MU outside normal range"));
            BeamTests.Tests.Add(new CheckContainsItem<Energies>(CheckTypes.ValidEnergies, Energies.Unset, RefBeam.ValidEnergies, "Not a valid energy") { ParameterOption = ParameterOptions.Required});
            BeamTests.Tests.Add(new CheckContainsItem<BeamGeometry>(CheckTypes.BeamGeometry, new BeamGeometry(), RefBeam.ValidGeometries, "No valid geometry found") { ParameterOption = ParameterOptions.Required });
            BeamTests.Tests.Add(new CheckValueItem<double?>(CheckTypes.CouchRotation, -1, RefBeam.CouchRotation, new TrackedValue<double?>(1E-2), "Non-standard couch rotation"));
            switch (RefBeam.JawTracking_Indication.Value)
            {
                case ParameterOptions.Required:
                    BeamTests.Tests.Add(new CheckValueItem<bool?>(CheckTypes.JawTracking, null, new TrackedValue<bool?>(true), null, "No tracking detected"));
                    break;
                case ParameterOptions.Optional:
                    BeamTests.Tests.Add(new CheckValueItem<bool?>(CheckTypes.JawTracking, null, null, null, "No tracking detected"));
                    break;
                case ParameterOptions.None:
                    BeamTests.Tests.Add(new CheckValueItem<bool?>(CheckTypes.JawTracking, null, new TrackedValue<bool?>(false), null, "No tracking detected"));
                    break;
            }
            BeamTests.Tests.Add(new CheckValueItem<double?>(CheckTypes.MinMLCOffsetFromAxial, -1, RefBeam.MinColRotation, new TrackedValue<double?>(1E-2), "Collimator less than minimum offset") { ParameterOption = ParameterOptions.Optional, Test = TestType.GreaterThan });
            BeamTests.Tests.Add(new CheckValueItem<double?>(CheckTypes.MinimumXfieldSize, -1, RefBeam.MinX, null, "X field too small") { Test = TestType.GreaterThan });
            BeamTests.Tests.Add(new CheckValueItem<double?>(CheckTypes.MaximumXfieldSize, -1, RefBeam.MaxX, null, "X field too large") { Test = TestType.LessThan });
            BeamTests.Tests.Add(new CheckValueItem<double?>(CheckTypes.MinimumYfieldSize, -1, RefBeam.MinY, null, "Y field too small") { Test = TestType.GreaterThan });
            BeamTests.Tests.Add(new CheckValueItem<double?>(CheckTypes.MaximumYfieldSize, -1, RefBeam.MaxY, null, "Y field too large") { Test = TestType.LessThan });
            BeamTests.Tests.Add(new CheckValueItem<string>(CheckTypes.ToleranceTable, "", RefBeam.ToleranceTable, null, "Incorrect Tol Table"));
            if (Field != null)
                RefreshTests();
            //RaisePropertyChangedEvent(nameof(BeamTests));
        }
        public void BeamChangeAction(string newFieldId = null)
        {
            if (newFieldId != string.Empty)
            {
                Field = Fields.FirstOrDefault(x => x.Id == newFieldId);
                RefreshTests();
            }
        }
        private async void RefreshTests()
        {
            if (Field == null)
                return;
            // Add default checks:
            foreach (var Test in BeamTests.Tests)
            {
                switch (Test.CheckType)
                {
                    case CheckTypes.MURange:
                        Test.SetCheckValue(MU);
                        break;
                    case CheckTypes.ValidEnergies:
                        Test.SetCheckValue(Field.Energy);
                        break;
                    case CheckTypes.BeamGeometry:
                        Trajectories BeamTrajectory = Trajectories.Static;
                        switch (Field.GantryDirection)
                        {
                            case VMS.TPS.Common.Model.Types.GantryDirection.Clockwise:
                                BeamTrajectory = Trajectories.CW;
                                break;
                            case VMS.TPS.Common.Model.Types.GantryDirection.CounterClockwise:
                                BeamTrajectory = Trajectories.CCW;
                                break;
                            default:
                                break;
                        }
                        var CheckBeam = new BeamGeometry() { StartAngle = Field.GantryStart, EndAngle = Field.GantryEnd, GeometryName="Off protocol", Trajectory = BeamTrajectory };
                        Test.SetCheckValue(CheckBeam);
                        break;
                    case CheckTypes.CouchRotation:
                        Test.SetCheckValue(CouchRotation);
                        break;
                    case CheckTypes.JawTracking:
                        Test.SetCheckValue(Field.isJawTracking);
                        break;
                    case CheckTypes.MinMLCOffsetFromAxial:
                        double Offset = double.NaN;
                        if (ColRotation < 90)
                            Offset = ColRotation;
                        else if (ColRotation < 180)
                            Offset = (180 - ColRotation);
                        else if (ColRotation < 270)
                            Offset = (ColRotation - 180);
                        else
                            Offset = 360 - ColRotation;
                        Test.SetCheckValue(Offset);
                        break;
                    case CheckTypes.MinimumXfieldSize:
                        Test.SetCheckValue(Xmin);
                        break;
                    case CheckTypes.MinimumYfieldSize:
                        Test.SetCheckValue(Ymin);
                        break;
                    case CheckTypes.MaximumXfieldSize:
                        Test.SetCheckValue(Xmax);
                        break;
                    case CheckTypes.MaximumYfieldSize:
                        Test.SetCheckValue(Ymax);
                        break;
                    case CheckTypes.ToleranceTable:
                        Test.SetCheckValue(ToleranceTable);
                        break;
                }
            }

            await AddBolusCheck();

        }
        public async Task<bool> AddBolusCheck()
        {

            CheckValueItem<double> BolusTest = null;
            foreach (var bolustest in BeamTests.Tests.Where(x => x.CheckType == CheckTypes.BolusHU).ToList())
                BeamTests.Tests.Remove(bolustest);
            foreach (var bolustest in BeamTests.Tests.Where(x => x.CheckType == CheckTypes.BolusThickness).ToList())
                BeamTests.Tests.Remove(bolustest);
            foreach (BolusDefinition B in RefBeam.Boluses)
            {
                int numBoluses = 0;
                foreach (TxFieldItem.BolusInfo BI in Field.BolusInfos)
                {
                    numBoluses++;
                    BolusTest = new CheckValueItem<double>(CheckTypes.BolusHU, BI.HU, B.HU, B.ToleranceHU, @"HU deviation", "Bolus required", "No bolus in protocol");
                    BolusTest.OptionalNameSuffix = string.Format(@"(""{0}"")", BI.Id);
                    BolusTest.ParameterOption = B.Indication.Value;
                    BeamTests.Tests.Add(BolusTest);
                    //ThickCheck
                    var Thick = await Ctr.GetBolusThickness(Field.CourseId, Field.PlanId, BI.Id);
                    var ThickCheck = new CheckValueItem<double>(CheckTypes.BolusThickness, Thick, B.Thickness, B.ToleranceThickness, null, null, null);
                    ThickCheck.OptionalNameSuffix = string.Format(@"(""{0}"") [cm]", BI.Id);
                    ThickCheck.ParameterOption = B.Indication.Value;
                    BeamTests.Tests.Add(ThickCheck);
                }
                if (numBoluses == 0 && B.Indication.Value == ParameterOptions.Required)
                {
                    var ThickCheck = new CheckRangeItem<double?>(CheckTypes.BolusThickness, null, null, null, "Bolus Required");
                    BeamTests.Tests.Add(ThickCheck);
                }
            }
            return true;
        }
        public void AddToleranceTableCheck(string ProtocolTolTable)
        {
            //var Warning = false;
            //if (Field.ToleranceTable.ToUpper() != ProtocolTolTable.ToUpper())
            //    Warning = true;
            //var TolTable = new TestListItem(string.Format(@"Tolerance Table (""{0}"")", Field.Id), string.Format("{0:0} HU", Field.ToleranceTable), string.Format("{0:0} HU", ProtocolTolTable), Warning, "");
            //BeamTests.Tests.Add(TolTable);
        }
        public ICommand RemoveAliasCommand
        {
            get { return new DelegateCommand(RemoveAlias); }
        }
        public void RemoveAlias(object param = null)
        {
            string remAlias = param as string;
            if (remAlias != null)
                if (Aliases.Contains(remAlias))
                    Aliases.Remove(remAlias);
        }
        public ICommand AddAliasCommand
        {
            get { return new DelegateCommand(AddAlias); }
        }
        public void AddAlias(object param = null)
        {
            if (NewAlias != null)
                if (!Aliases.Contains(NewAlias))
                    Aliases.Add(NewAlias);
        }
        public TestList_ViewModel BeamTests { get; set; } = new TestList_ViewModel();
    }
    [AddINotifyPropertyChangedInterface]
    public class Prescription_ViewModel
    {
        public double RxDose { get; set; } = -1;
        public int Fractions { get; set; } = -1;
        public double PercentRx { get; set; } = -1;
    }
    [AddINotifyPropertyChangedInterface]
    public class LoadingViewModel
    {
        public string LoadingMessage { get; set; } = "Loading";
    }




}
