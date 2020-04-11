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
        public ObservableCollection<Ctr.ImagingFieldItem> ImagingFields { get; set; } = new ObservableCollection<Ctr.ImagingFieldItem>() { new Ctr.ImagingFieldItem(), new Ctr.ImagingFieldItem() };
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
    public class TestList_ViewModel
    {
        public event EventHandler TargetIdChange;

        public List<string> TestListTargetIds { get; set; } = new List<string>();
        private string _TestListTargetId = "unset";
        public string TestListTargetId
        {
            get
            {
                return _TestListTargetId;
            }
            set
            {
                _TestListTargetId = value;
                TargetIdChange?.Invoke(this, EventArgs.Empty);
            }
        }
        public string TestListTitle { get; set; } = "Default_Title";
        public ObservableCollection<ITestListItem> Tests { get; set; } = new ObservableCollection<ITestListItem>();
    }
    [AddINotifyPropertyChangedInterface]
    public class Beam_ViewModel
    {
        public string DebugString { get; set; } = "Default_Value";
        public ObservableCollection<BeamListItem> Beams { get; set; } = new ObservableCollection<BeamListItem>();
        public TestList_ViewModel GroupTests { get; set; } = new TestList_ViewModel() { TestListTitle = "Default Beam Title" };

    }
    [AddINotifyPropertyChangedInterface]
    public class BeamListItem
    {
        public event EventHandler FieldChanged;

        private Ctr.TxFieldItem _Field;
        public Ctr.TxFieldItem Field
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
        public List<Ctr.TxFieldItem> Fields { get; set; }
        private Ctr.Beam RefBeam;
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
        public FieldType Technique_ref
        {
            get { return RefBeam.Technique; }
            set { RefBeam.Technique = value; }
        }

        public string ToleranceTable_ref
        {
            get { return RefBeam.ToleranceTable; }
            set { RefBeam.ToleranceTable = value; }
        }
        public double Xmin { get { return Field.Xmin; } }
        public double Ymin { get { return Field.Ymin; } }
        public double Xmax { get { return Field.Xmax; } }
        public double Ymax { get { return Field.Ymax; } }
        public double MinX_ref { get { return RefBeam.MinX; } set { RefBeam.MinX = value; } }
        public double MinY_ref { get { return RefBeam.MinY; } set { RefBeam.MinY = value; } }
        public double MaxX_ref { get { return RefBeam.MaxX; } set { RefBeam.MaxX = value; } }
        public double MaxY_ref { get { return RefBeam.MaxY; } set { RefBeam.MaxY = value; } }
        public double MU
        {
            get { return Field.MU; }
        }
        public bool MinMUWarning
        {
            get
            {

                if (MU < RefBeam.MinMUWarning)
                    return true;
                else
                    return false;
            }
        }
        public bool MaxMUWarning
        {
            get
            {
                if (double.IsNaN(MU))
                    return true;
                if (MU > RefBeam.MaxMUWarning)
                    return true;
                else
                    return false;
            }
        }
        public double ColRotation { get { return Field.CollimatorAngle; } }
        public double MinColRotation { get; set; }
        public double MaxColRotation { get; set; }
        public double CouchRotation { get { return Field.CouchRotation; } }
        public double StartAngle { get { return Field.GantryStart; } }
        public double EndAngle { get { return Field.GantryEnd; } }
        public double CouchRotation_ref { get { return RefBeam.CouchRotation; } set { RefBeam.CouchRotation = value; } }

        public bool NoFieldAssigned { get; set; } = true;
        public BeamListItem(Ctr.Beam RefBeam_in, List<Ctr.TxFieldItem> TxFields)
        {
            RefBeam = RefBeam_in;
            Fields = TxFields;
            Field = null;
            BeamTests.Tests.Clear();
            foreach (string alias in RefBeam.EclipseAliases)
                foreach (Ctr.TxFieldItem F in Fields)
                    if (F.Id == alias)
                    {
                        Field = F;
                        NoFieldAssigned = false;
                    }
            FieldDescription = string.Format(@"Protocol field ""{0}"" assigned to plan field:", RefBeam.ProtocolBeamName);

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
            BeamTests.Tests.Clear(); // remove default model test beams
            if (Field == null)
                return;
            // Add default checks:

            BeamTests.Tests.Add(new TestListDoubleRangeItem(string.Format(@"MU range"), MU, RefBeam.MinMUWarning, RefBeam.MaxMUWarning, "MU outside normal range"));

            // Energies           
            var ValidEnergiesString = new List<string>();
            foreach (var E in RefBeam.ValidEnergies)
            {
                ValidEnergiesString.Add(E.Display());
            }
            BeamTests.Tests.Add(new TestListStringValueItem(@"Valid energies", Field.Energy.Display(), null, ValidEnergiesString, "Not a valid energy"));

            // Beam Geometries
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
            var CheckBeam = new Ctr.BeamGeometry() { StartAngle = Field.GantryStart, EndAngle = Field.GantryEnd, Trajectory=BeamTrajectory };
            BeamTests.Tests.Add(new TestListBeamStartStopItem(string.Format(@"Beam geometry"), CheckBeam, RefBeam.ValidGeometries, "No valid geometry found"));
            BeamTests.Tests.Add(new TestListDoubleValueItem(string.Format(@"Couch rotation"), CouchRotation, CouchRotation_ref, TestType.Equality, null, "Non-standard couch rotation"));
            if (RefBeam.VMAT_JawTracking == ParameterOptions.Required)
            {
                BeamTests.Tests.Add(new TestListBoolValueItem("Jaw tracking", Field.isJawTracking, true, TestType.Equality, "No tracking detected"));
            }

            // Col rotation
            bool MinColRotationWarning;
            double Offset = ColRotation;
            if (ColRotation < 90)
                MinColRotationWarning = ColRotation < RefBeam.MinColRotation;
            else if (ColRotation < 180)
                Offset = (180 - ColRotation);
            else if (ColRotation < 270)
                Offset = (ColRotation - 180);
            else
                Offset = 360 - ColRotation;
            BeamTests.Tests.Add(new TestListDoubleValueItem(string.Format(@"Minimum MLC offset from axial plane"), Offset, RefBeam.MinColRotation, TestType.GreaterThan, null, "Collimator less than minimum offset") { ParameterOption = ParameterOptions.Optional });


            BeamTests.Tests.Add(new TestListDoubleValueItem(string.Format(@"Minimum X field size"), Xmin, RefBeam.MinX, TestType.GreaterThan, null, "X field too small"));
            BeamTests.Tests.Add(new TestListDoubleValueItem(string.Format(@"Maximum X field size"), Xmax, RefBeam.MaxX, TestType.LessThan, null, "X field too large"));
            BeamTests.Tests.Add(new TestListDoubleValueItem(string.Format(@"Minimum Y field size"), Ymin, RefBeam.MinY, TestType.GreaterThan, null, "Y field too small"));
            BeamTests.Tests.Add(new TestListDoubleValueItem(string.Format(@"Maximum Y field size"), Ymax, RefBeam.MaxY, TestType.LessThan, null, "Y field too large"));
            BeamTests.Tests.Add(new TestListStringValueItem("Tolerance Table", ToleranceTable, ToleranceTable_ref, null, "Incorrect Tol Table"));

            var CompletedBolusCheck = await AddBolusCheck();

        }
        public async Task<bool> AddBolusCheck()
        {

            TestListDoubleValueItem BolusTest = null;
            var ReqBoluses = Ctr.GetActiveProtocol().Checklist.Boluses;
            foreach (Ctr.BolusDefinition B in RefBeam.Boluses)
            {
                int numBoluses = 0;
                foreach (Ctr.TxFieldItem.BolusInfo BI in Field.BolusInfos)
                {
                    numBoluses++;
                    BolusTest = new TestListDoubleValueItem(string.Format(@"Bolus HU (""{0}"")", BI.Id), BI.HU, B.HU, TestType.Equality, null, @"HU deviation", null, null, B.ToleranceHU);
                    BolusTest.ParameterOption = B.Indication;
                    BolusTest.RefNullWarningString = "No bolus in protocol";
                    BolusTest.CheckNullWarningString = "Bolus required";
                    switch (B.Indication)
                    {
                        case ParameterOptions.None:
                            BolusTest.RefNullWarningString = "Bolus not indicated in protocol";
                            break;
                        default:
                            break;
                    };
                    BeamTests.Tests.Add(BolusTest);
                    //ThickCheck
                    var Thick = await Ctr.GetBolusThickness(Field.CourseId, Field.PlanId, BI.Id);
                    var ThickCheck = new TestListDoubleValueItem(string.Format(@"Bolus Thickness (""{0}"") [cm]", BI.Id), Thick, B.Thickness, TestType.Equality, null, null, null, null, B.ToleranceThickness);
                    ThickCheck.ParameterOption = B.Indication;
                    switch (B.Indication)
                    {
                        case ParameterOptions.None:
                            ThickCheck.RefNullWarningString = "Bolus not indicated in protocol";
                            break;
                        default:
                            break;
                    };
                    BeamTests.Tests.Add(ThickCheck);
                }
                if (numBoluses == 0 && B.Indication == ParameterOptions.Required)
                {
                    var ThickCheck = new TestListDoubleRangeItem(@"Bolus Check [cm]", null);
                    ThickCheck.CheckNullWarningString = "Bolus required";
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
        public TestList_ViewModel BeamTests { get; set; } = new TestList_ViewModel() { TestListTitle = "Default Beam Title" };
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
