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
        public ObservableCollection<TestListItem> Tests { get; set; } = new ObservableCollection<TestListItem>() { new TestListItem("Test1"), new TestListItem("Test2") };
    }
    [AddINotifyPropertyChangedInterface]
    public class Beam_ViewModel
    {
        public ObservableCollection<BeamListItem> Beams { get; set; } = new ObservableCollection<BeamListItem>();
    }
    [AddINotifyPropertyChangedInterface]
    public class BeamListItem
    {
        public Ctr.TxFieldItem Field { get; private set; }
        public string FieldDescription { get; set; }
        public string Id {get; private set;}
        public string FieldType { get; private set; }
        public double RefMinX { get; set; }
        public double RefMinY { get; set; }
        public double RefBolusHU { get; set; }
        public double RefBolusThickness { get; set; }
        public BeamListItem(Ctr.TxFieldItem TxField)
        {
            Field = TxField;
            FieldType = TxField.TypeString;
            Id = TxField.Id;
            FieldDescription = string.Format("Field: {0} ({1})", Id, FieldType);
            RefMinX = 3;
            RefMinY = 3;
            RefBolusHU = 0;
            RefBolusThickness = 0.5;
            // Define BeamChecks
            // Bolus
            BeamTests.Tests.Clear();
        }
        public void AddBolusHUCheck(double ProtocolRefBolusHU)
        {
            RefBolusHU = ProtocolRefBolusHU;
            foreach (Ctr.TxFieldItem.BolusInfo BI in Field.BolusInfos)
            {
                var Warning = false;
                if (Math.Abs(BI.HU - RefBolusHU) > 0.1)
                    Warning = true;
                var HU = new TestListItem(string.Format(@"Bolus HU (""{0}"")", BI.Id), string.Format("{0:0} HU", BI.HU), string.Format("{0:0} HU", RefBolusHU), Warning, "");
                BeamTests.Tests.Add(HU);
            }
        }
        public async Task<bool> AddBolusThickCheck(double ProtocolRefBolusThickness)
        {
            foreach (Ctr.TxFieldItem.BolusInfo BI in Field.BolusInfos)
            {
                RefBolusThickness = ProtocolRefBolusThickness;
                var Warning = false;
                var Thick = await Ctr.GetBolusThickness(Field.CourseId, Field.PlanId, BI.Id);
                if (Math.Abs(Thick - RefBolusThickness) > 0.1)
                {
                    Warning = true;
                }
                var ThickCheck = new TestListItem(string.Format(@"Bolus Thickness (""{0}"")", BI.Id), string.Format("{0:0.0#} cm", Thick),
                    string.Format("{0:0.#} cm", RefBolusThickness), Warning, "");
                BeamTests.Tests.Add(ThickCheck);
            }
            return true;
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
    [AddINotifyPropertyChangedInterface]
    public class TestListItem
    {
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
