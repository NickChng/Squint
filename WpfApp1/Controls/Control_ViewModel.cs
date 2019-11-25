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
        public ObservableCollection<TestListItem> Tests { get; set; } = new ObservableCollection<TestListItem>() { new TestListItem("Test1"), new TestListItem("Test2") };
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
        public double RefBolusHU { get; set; } = 0;
        public double RefBolusThickness { get; set; } = 0.5;
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
            if (!double.IsNaN(MU))
            {
                string MURange = ""; // no string if no reference
                if (RefBeam.MinMUWarning > 0 && (RefBeam.MaxMUWarning > 0 && (RefBeam.MaxMUWarning > RefBeam.MinMUWarning)))
                    MURange = string.Format("{0:0} - {1:0}", RefBeam.MinMUWarning, RefBeam.MaxMUWarning);
                BeamTests.Tests.Add(new TestListItem(string.Format(@"MU range"), string.Format("{0:0.#} MU", MU), MURange));
            }
            else
                BeamTests.Tests.Add(new TestListItem(string.Format(@"MU range"), @"N/A", string.Format("{0} - {1} MU", RefBeam.MinMUWarning, RefBeam.MaxMUWarning), true, "Not calculated"));
            var EnergyWarning = !RefBeam.ValidEnergies.Contains(Field.Energy) && !RefBeam.ValidEnergies.Contains(Energies.Unset);
            var ValidEnergiesString = new List<string>();
            foreach (var E in RefBeam.ValidEnergies)
            {
                ValidEnergiesString.Add(E.Display());
            }
            BeamTests.Tests.Add(new TestListItem(@"Valid energies", Field.Energy.Display(), string.Join(", ", ValidEnergiesString), EnergyWarning, ""));
            bool GeometryWarning = true;
            string GeometryName = "";
            string GeometryWarningString = "No valid beam geometry found";
            double eps = 0.1;
            if (RefBeam.ValidGeometries.Count > 0)
            {
                foreach (Ctr.BeamGeometry G in RefBeam.ValidGeometries)
                {
                    double InvariantMaxStart;
                    double InvariantMinStart;
                    double InvariantMaxEnd; 
                    double InvariantMinEnd; 

                    if (G.GetInvariantAngle(G.MaxStartAngle) > G.GetInvariantAngle(G.MinStartAngle))
                    {
                        InvariantMaxStart = G.GetInvariantAngle(G.MaxStartAngle);
                        InvariantMinStart = G.GetInvariantAngle(G.MinStartAngle);
                    }
                    else
                    {
                        InvariantMaxStart = G.GetInvariantAngle(G.MinStartAngle);
                        InvariantMinStart = G.GetInvariantAngle(G.MaxStartAngle);
                    }
                    if (G.GetInvariantAngle(G.MaxEndAngle) > G.GetInvariantAngle(G.MinEndAngle))
                    {
                        InvariantMaxEnd = G.GetInvariantAngle(G.MaxEndAngle);
                        InvariantMinEnd = G.GetInvariantAngle(G.MinEndAngle);
                    }
                    else
                    {
                        InvariantMaxEnd = G.GetInvariantAngle(G.MinEndAngle);
                        InvariantMinEnd = G.GetInvariantAngle(G.MaxEndAngle);
                    }


                    if (G.GetInvariantAngle(Field.GantryStart) <= InvariantMaxStart + eps &&
                        G.GetInvariantAngle(Field.GantryStart) >= InvariantMinStart - eps &&
                        G.GetInvariantAngle(Field.GantryEnd) <= InvariantMaxEnd + eps &&
                        G.GetInvariantAngle(Field.GantryEnd) >= InvariantMinEnd - eps)
                        {
                            GeometryWarning = false;
                            GeometryName = G.GeometryName;
                            GeometryWarningString = "";
                        }
                }
            }
            var GeometryTest = new TestListItem(string.Format(@"Beam geometry"), string.Format("{0:0.#} - {1:0.#} degrees", StartAngle, EndAngle), GeometryName,
                          GeometryWarning, GeometryWarningString);
            BeamTests.Tests.Add(GeometryTest);
            if (!double.IsNaN(CouchRotation_ref))
            {
                bool CouchRotationWarning = (Math.Abs(CouchRotation_ref - CouchRotation) > 1E-5);
                BeamTests.Tests.Add(new TestListItem(string.Format(@"Couch rotation"), string.Format("{0:0.#} degrees", CouchRotation), string.Format("{0:0.#} degrees", RefBeam.CouchRotation),
                    CouchRotationWarning, ""));
            }
            if (RefBeam.VMAT_JawTracking == ParameterOptions.Required)
            {
                var JawTrackingWarning = false;
                string Message;
                if (!Field.isJawTracking)
                {
                    JawTrackingWarning = true;
                    Message = @"No tracking detected";
                }
                else
                    Message = @"Tracking detected";
                BeamTests.Tests.Add(new TestListItem(@"Jaw tracking", Message, RefBeam.VMAT_JawTracking.Display(), JawTrackingWarning, ""));
            }
            if (!double.IsNaN(MinColRotation))
            {
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
                MinColRotationWarning = Offset < RefBeam.MinColRotation;
                BeamTests.Tests.Add(new TestListItem(string.Format(@"Minimum MLC offset from axial plane"), string.Format("{0:0.#} degrees", Offset), string.Format("{0:0.#} degrees", RefBeam.MinColRotation),
                    MinColRotationWarning, ""));
            }
            if (!double.IsNaN(MinX_ref))
            {
                bool MinXWarning = MinX_ref > Xmin;
                BeamTests.Tests.Add(new TestListItem(string.Format(@"Minimum X field size"), string.Format("{0:0.#} cm", Xmin), string.Format("{0:0.#} cm", RefBeam.MinX),
                    MinXWarning, ""));
            }
            if (!double.IsNaN(MinY_ref))
            {
                bool MinYWarning = MinY_ref > Ymin;
                BeamTests.Tests.Add(new TestListItem(string.Format(@"Minimum Y field size"), string.Format("{0:0.#} cm", Ymin), string.Format("{0:0.#} cm", RefBeam.MinY),
                    MinYWarning, ""));
            }
            if (!double.IsNaN(MaxX_ref))
            {
                bool MaxXWarning = MaxX_ref < Xmax;
                BeamTests.Tests.Add(new TestListItem(string.Format(@"Maximum X field size"), string.Format("{0:0.#} cm", Xmax), string.Format("{0:0.#} cm", RefBeam.MaxX),
                    MaxXWarning, ""));
            }
            if (!double.IsNaN(MaxY_ref))
            {
                bool MaxYWarning = MaxY_ref < Ymax;
                BeamTests.Tests.Add(new TestListItem(string.Format(@"Maximum Y field size"), string.Format("{0:0.#} cm", Ymax), string.Format("{0:0.#} cm", RefBeam.MaxY),
                    MaxYWarning, ""));
            }
            if (ToleranceTable_ref != "")
            {
                bool TolTableWarning = ToleranceTable_ref.ToUpper() != ToleranceTable.ToUpper();
                BeamTests.Tests.Add(new TestListItem(string.Format(@"Tolerance table"), ToleranceTable, ToleranceTable_ref, TolTableWarning, ""));
            }
            var CompletedBolusCheck = await AddBolusCheck();

        }
        public async Task<bool> AddBolusCheck()
        {
            int numBoluses = 0;
            foreach (Ctr.TxFieldItem.BolusInfo BI in Field.BolusInfos)
            {
                var Warning = false;
                string WarningString = "";
                numBoluses++;
                if (Math.Abs(BI.HU - RefBeam.RefBolusHU) > 0.1)
                {
                    Warning = true;
                    WarningString = @"HU deviation";
                }
                if (RefBeam.BolusParameter == ParameterOptions.None)
                {
                    Warning = true;
                    WarningString = @"Off-protocol bolus";
                }
                var HU = new TestListItem(string.Format(@"Bolus HU (""{0}"")", BI.Id), string.Format("{0:0} HU", BI.HU), string.Format("{0:0} HU", RefBolusHU), Warning, WarningString);
                BeamTests.Tests.Add(HU);

                //ThickCheck
                if (RefBeam.BolusParameter != ParameterOptions.None)
                {
                    var Thick = await Ctr.GetBolusThickness(Field.CourseId, Field.PlanId, BI.Id);
                    if (Thick < (RefBeam.BolusClinicalMinThickness - 0.1) || Thick > (RefBeam.BolusClinicalMaxThickness + 0.1))
                    {
                        Warning = true;
                    }
                    var ThickCheck = new TestListItem(string.Format(@"Bolus Thickness (""{0}"")", BI.Id), string.Format("{0:0.0#} cm", Thick),
                        string.Format("{0:0.#} - {1:0.#} cm", RefBeam.BolusClinicalMinThickness, RefBeam.BolusClinicalMaxThickness), Warning, "");
                    BeamTests.Tests.Add(ThickCheck);
                }
            }
            if (numBoluses == 0 && RefBeam.BolusParameter == ParameterOptions.Required)
            {
                var HU = new TestListItem(string.Format(@"Bolus Check"), string.Format(" - "), string.Format("{0:0} HU", RefBolusHU), true, "Bolus indicated but not found");
                BeamTests.Tests.Add(HU);
            }
            return true;
        }
        public void AddToleranceTableCheck(string ProtocolTolTable)
        {
            var Warning = false;
            if (Field.ToleranceTable.ToUpper() != ProtocolTolTable.ToUpper())
                Warning = true;
            var TolTable = new TestListItem(string.Format(@"Tolerance Table (""{0}"")", Field.Id), string.Format("{0:0} HU", Field.ToleranceTable), string.Format("{0:0} HU", ProtocolTolTable), Warning, "");
            BeamTests.Tests.Add(TolTable);
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
    [AddINotifyPropertyChangedInterface]
    public class TestListItem
    {
        public List<string> TestItemIds { get; set; }
        private string _TestItemId = "";
        public string TestItemId
        {
            get
            {
                return _TestItemId;
            }
            set
            {
                _TestItemId = value;
                new DelegateCommand(OnTestItemIdChange);
            }
        }
        public Action<object> OnTestItemIdChange;
        public string TestName { get; private set; }
        public TestType TestType { get; private set; } // to implement
        public string ReferenceValue { get; private set; } = "";
        public Visibility CheckVisibility
        {
            get
            {
                if (ReferenceValue == "")
                    return Visibility.Hidden;
                else return Visibility.Visible;
            }
        }
        public string Value { get; private set; } = "Value";
        public bool Warning { get; private set; } = true;
        public string WarningString { get; private set; } = "DefaultWarning";
        public TestListItem(string TN, string V = "", string RV = "", bool W = false, string WS = "")
        {
            TestName = TN;
            ReferenceValue = RV;
            Value = V;
            Warning = W;
            WarningString = WS;
        }
    }

}
