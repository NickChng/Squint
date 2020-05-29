using System;
using System.Linq;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Threading;
using PropertyChanged;
using System.Windows.Controls;
using System.Windows;
using wpfcolors = System.Windows.Media.Colors;
using wpfcolor = System.Windows.Media.Color;
using wpfbrush = System.Windows.Media.SolidColorBrush;
using Controls = SquintScript.Controls;
using SquintScript.ViewModelClasses;
using SquintScript.Extensions;
using SquintScript.Controls;

namespace SquintScript.ViewModels
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChangedEvent([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DelegateCommand : ICommand
    {
        private readonly Action<object> _action;

        public DelegateCommand(Action<object> action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

#pragma warning disable 67
        public event EventHandler CanExecuteChanged;
#pragma warning restore 67
    }
    public enum ViewEnums
    {
        [Description("Centre:")] Centre,
        [Description("Approval:")] Approval,
        [Description("Protocol type:")] ProtocolType,
        [Description("Approver:")] Approver,
        [Description("Site:")] Site,

    }

    //[AddINotifyPropertyChangedInterface]
    public class StructureProperties
    {
        public bool isBeingEdited { get; set; } = false;
        public string Label { get; set; } = "Label";
        public string abRatio { get; set; } = "-1";
    }
    [AddINotifyPropertyChangedInterface]
    public class ProtocolSelector : ObservableObject
    {
        private Ctr.ProtocolPreview _pp;
        public ProtocolSelector(Ctr.ProtocolPreview pp)
        {
            _pp = pp;
            //Ctr.ProtocolListUpdated += OnProtocolListUpdated;
        }

        public int Id { get { return _pp.ID; } }
        public string ProtocolName { get { return _pp.ProtocolName; } }

        public TreatmentCentres TreatmentCentre { get { return _pp.TreatmentCentre; } }
        public TreatmentSites TreatmentSite { get { return _pp.TreatmentSite; } }

        public string LastModifiedBy { get { return _pp.LastModifiedBy; } }
        public ProtocolTypes ProtocolType { get { return _pp.ProtocolType; } }
        public ApprovalLevels ApprovalLevel { get { return _pp.Approval; } }

    }
    [AddINotifyPropertyChangedInterface]
    public class StructureSelector : ObservableObject
    {
        private Ctr.ProtocolStructure E;
        public int Id
        {
            get { return E.ID; }
        }
        public bool isUserAdded
        {
            get
            {
                if (E.ID < 0)
                    return true;
                else
                    return false;
            }
        }
        public string ProtocolStructureName
        {
            get { return E.ProtocolStructureName; }
            set
            {
                if (value.Length > 1)
                {
                    E.ProtocolStructureName = value;
                    RaisePropertyChangedEvent("StructureId");
                }
                else MessageBox.Show("Structure name must be greater than 1 character");
            }
        }
        public bool Pinned { get; set; } = false;
        public string AssignedStructureId
        {
            get { return E.AssignedStructureId; }
            set
            {
                if (value != null)
                {
                    E.AssignedStructureId = value;
                    RaisePropertyChangedEvent(nameof(this.StructureColor));
                    Ctr.UpdateConstraintsLinkedToStructure(Id);
                }
            }
        }
        public ObservableCollection<string> Aliases { get; set; }
        public string LabelName
        {
            get
            {
                if (Ctr.CurrentStructureSet != null)
                    return E.EclipseStructureLabel(Ctr.CurrentStructureSet.UID);
                else
                    return "";
            }
        }
        public string AlphaBetaRatio
        {
            get
            {
                if (E.StructureLabel.AlphaBetaRatio > 0.1)
                    return string.Format(@"({0}\{1} = {2:0})", '\u03B1', '\u03B2', E.StructureLabel.AlphaBetaRatio);
                else
                    return string.Format(@"(No BED adjustment)");
            }
        }
        public bool LabelIsConsistent
        {
            get
            {
                if (Ctr.CurrentStructureSet == null)
                    return false;
                else
                    if (E.EclipseStructureLabel(Ctr.CurrentStructureSet.UID).ToUpper() == E.StructureLabel.LabelName.ToUpper() || E.AssignedStructureId == "")
                {
                    return false;
                }
                else
                    return true;
            }
        }
        public string LabelMismatchTooltip
        {
            get
            {
                if (Ctr.CurrentStructureSet == null)
                    return "";
                else
                    return string.Format("Label mismatch: Assigned structure label is {0}", E.EclipseStructureLabel(Ctr.CurrentStructureSet.UID));
            }
        }
        public StructureSelector(Ctr.ProtocolStructure Ein)
        {
            E = Ein;
            Aliases = E.DefaultEclipseAliases;
            E.PropertyChanged += OnProtocolStructureChanged;
        }
        public string NewAlias { get; set; }
        public int SelectedAliasIndex { get; set; }
        public bool DragSelected { get; set; }
        public ICommand AddStructureAliasCommand
        {
            get { return new DelegateCommand(AddStructureAlias); }
        }
        private void AddStructureAlias(object param = null)
        {
            Ctr.AddStructureAlias(Id, NewAlias);
            RaisePropertyChangedEvent(nameof(Aliases));
        }
        public ICommand RemoveStructureAliasCommand
        {
            get { return new DelegateCommand(RemoveStructureAlias); }
        }
        private void RemoveStructureAlias(object param = null)
        {
            Ctr.RemoveStructureAlias(Id, param as string);
            RaisePropertyChangedEvent(nameof(Aliases));
        }
        public ICommand PinOptionsCommand
        {
            get { return new DelegateCommand(PinOptions); }
        }
        private void PinOptions(object param = null)
        {
            Pinned ^= true;
        }
        public wpfcolor? StructureColor
        {
            get
            {
                if (Ctr.CurrentStructureSet != null)
                {
                    return E.GetStructureColor(Ctr.CurrentStructureSet.UID);
                }
                else return null;
            }

        }
        private void OnProtocolStructureChanged(object sender, PropertyChangedEventArgs e)
        {
            RaisePropertyChangedEvent(e.PropertyName);
            if (e.PropertyName == nameof(Ctr.ProtocolStructure.AssignedStructureId))
            {
                RaisePropertyChangedEvent("StructureColor"); // this is for when the Assigned Eclipse structure itself is changed
                RaisePropertyChangedEvent("LabelIsConsistent"); // this is for when the Assigned Eclipse structure itself is changed
                RaisePropertyChangedEvent("LabelMismatchTooltip");
            }
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class ComponentSelector : ObservableObject
    {
        private bool isSelected { get; set; }
        public bool Pinned { get; set; } = false;
        public int DisplayHeight { get; } = 100;
        private Ctr.Component Comp;
        public int Id
        {
            get { return Comp.ID; }
        }
        public string ComponentName
        {
            get { return Comp.ComponentName; }
            set { Comp.ComponentName = value; }
        }
        public ComponentTypes ComponentType
        {
            get { return Comp.ComponentType.Value; }
            set { Comp.ComponentType.Value = value; }
        }
        public double ReferenceDose
        {
            get { return Comp.ReferenceDose; }
            set
            {
                SetReferenceDose(value);
                RaisePropertyChangedEvent("ComponentTDFDescription"); // Fody will update the primary property, this is to update the description
            }
        }

        private int _numFractions;
        public int NumFractions
        {
            get { return _numFractions; }
            set
            {
                _numFractions = value; // necessary because the await in SetFractions may result in the UI retrieving Comp.NumFractions before it has been updated
                SetFractions(value);
                RaisePropertyChangedEvent("ComponentTDFDescription"); // Fody will update the primary property, this is to update the description
            }
        }
        public string ComponentTDFDescription
        {
            get { return string.Format("({0:0.#} Gy in {1} fractions)", ReferenceDose / 100, NumFractions); }
        }
        public ComponentSelector(Ctr.Component Compin)
        {
            Comp = Compin;
            _numFractions = Comp.NumFractions;
            AvailableComponentTypes.Clear();
            foreach (ComponentTypes T in Enum.GetValues(typeof(ComponentTypes)))
            {
                AvailableComponentTypes.Add(T);
            }
        }
        private async void SetFractions(int NumFractions)
        {
            await Task.Run(() =>
            {
                Comp.NumFractions = NumFractions;
                Ctr.UpdateComponentConstraints(Comp.ID);
            });
        }
        private async void SetReferenceDose(double dose)
        {
            await Task.Run(() =>
            {
                Comp.ReferenceDose = dose;
                Ctr.UpdateComponentConstraints(Comp.ID);
            });
        }
        public ObservableCollection<ComponentTypes> AvailableComponentTypes { get; set; } = new ObservableCollection<ComponentTypes>() { ComponentTypes.Plan };
    }

    [AddINotifyPropertyChangedInterface]
    public class ConstraintSelector : ObservableObject
    {
        //        public event PropertyChangedEventHandler PropertyChanged;
        public bool RefreshFlag { get; set; } // this bool updates
        //public bool RefreshRowHeader { get; set; }
        public bool ConstraintInfoVisibility { get; set; } = false;
        public string ChangeDescription
        {
            get
            {
                return ""; // not implemented
            }
            set
            {
                // removed temporarily while refactoring constraintview
            }
        } // only used for entry when users changes constraint within the AdminWindow.
        public ObservableCollection<Ctr.ConstraintChangelog> ConstraintChangelogs
        {
            get
            {
                return new ObservableCollection<Ctr.ConstraintChangelog>(); // temp disabled while refactoring constraintview
                //if (Comp == null)
                //{
                //    return new ObservableCollection<Ctr.ConstraintChangelog>() { new Ctr.ConstraintChangelog(), new Ctr.ConstraintChangelog() };
                //}
                //else
                //{
                //    return new ObservableCollection<Ctr.ConstraintChangelog>(Comp.GetChangeLogs());
                //}
            }
        }
        private Ctr.Constraint Con;
        private StructureSelector _SS;
        public StructureSelector SS
        {
            get
            {
                return _SS;
            }
            set
            {
                if (_SS != value)
                {
                    _SS = value;
                    if (_SS != null)
                        Con.PrimaryStructureID = _SS.Id;
                }
            }
        }
        public int Id
        {
            get { return Con.ID; }
        }
        public bool Pinned { get; set; } = false;
        public bool DragSelected { get; set; } = false;
        public wpfcolor Color { get; set; } = wpfcolors.PapayaWhip;
        public ComponentSelector Component
        {
            get { return Components.FirstOrDefault(x => x.Id == Con.ComponentID); }
            set { Con.ComponentID = value.Id; }
        }
        public string ComponentName
        {
            get { return Con.ComponentName; }
            set { }
        }
        public int DisplayOrder
        {
            get { return Con.DisplayOrder.Value; }
            set { Con.DisplayOrder.Value = value; }
        }
        public double ConstraintValue
        {
            get { return Con.ConstraintValue; }
            set
            {
                if (value != Con.ConstraintValue)
                {
                    Con.ConstraintValue = value;
                    UpdateConstraint();
                    RaisePropertyChangedEvent(nameof(ConstraintValueColor));
                }
            }
        }
        public wpfbrush ConstraintValueColor
        {
            get
            {
                if (Con.isModified(nameof(Con.ConstraintValue)) && !Con.isCreated)
                    return new wpfbrush(wpfcolors.DarkOrange);
                else
                    return new wpfbrush(wpfcolors.Black);
            }
        }
        public double ReferenceValue
        {
            get { return Con.ReferenceValue; }
            set
            {
                if (Con.ReferenceValue != value)
                {
                    bool CalcAfter = false;
                    if (!Con.isValid())
                        CalcAfter = true;
                    Con.ReferenceValue = value;
                    if (CalcAfter)
                        UpdateConstraint(); // only do this if the constraint was previously invalid
                    RaisePropertyChangedEvent(nameof(ReferenceValueColor));
                }
            }
        }
        public wpfbrush ReferenceValueColor
        {
            get
            {
                if (Con.isModified(nameof(Con.ReferenceValue)) && !Con.isCreated)
                    return new wpfbrush(wpfcolors.DarkOrange);
                else
                    return new wpfbrush(wpfcolors.Black);
            }
        }
        public double? StopValue
        {
            get
            {
                return Con.Stop;
            }
            set
            {
                if (!Con.Stop.CloseEnough(value))
                {
                    Con.Stop = value;
                    UpdateConstraint();
                }
            }
        }
        public double? MinorViolation
        {
            get
            {
                return Con.MinorViolation;
            }
            set
            {

                if (!Con.MinorViolation.CloseEnough(value))
                {
                    Con.MinorViolation = value;
                    UpdateConstraint();
                }
            }
        }
        public double? MajorViolation
        {
            get
            {
                return Con.MajorViolation;
            }
            set
            {
                if (!Con.MajorViolation.CloseEnough(value))
                {
                    Con.MajorViolation = value;
                    UpdateConstraint();
                }
            }
        }
        public ConstraintTypeCodes ConstraintType
        {
            get { return Con.ConstraintType; }
            set
            {
                Con.ConstraintType = value;
                UpdateConstraint();
                SetComboBoxes();
            }
        }
        public ConstraintUnits ConstraintUnit
        {
            get { return Con.GetConstraintUnit(); }
            set
            {
                if (value != Con.GetConstraintUnit())
                {
                    switch (value)
                    {
                        case ConstraintUnits.Multiple:
                            Con.ConstraintScale = UnitScale.Absolute;
                            break;
                        case ConstraintUnits.cc:
                            Con.ConstraintScale = UnitScale.Absolute;
                            break;
                        case ConstraintUnits.cGy:
                            Con.ConstraintScale = UnitScale.Absolute;
                            break;
                        case ConstraintUnits.Percent:
                            Con.ConstraintScale = UnitScale.Relative;
                            break;
                    }
                    UpdateConstraint();
                }
            }
        }
        public ConstraintUnits ReferenceUnit
        {
            get { return Con.GetReferenceUnit(); }
            set
            {
                if (value != Con.GetReferenceUnit())
                {
                    bool CalcAfter = false;
                    if (!Con.isValid())
                        CalcAfter = true;
                    switch (value)
                    {
                        case ConstraintUnits.Multiple:
                            Con.ReferenceScale = UnitScale.Absolute;
                            break;
                        case ConstraintUnits.cc:
                            Con.ReferenceScale = UnitScale.Absolute;
                            break;
                        case ConstraintUnits.cGy:
                            Con.ReferenceScale = UnitScale.Absolute;
                            break;
                        case ConstraintUnits.Percent:
                            Con.ReferenceScale = UnitScale.Relative;
                            break;
                    }
                    if (CalcAfter)
                        UpdateConstraint(); // only do this if the constraint was previously invalid
                }

            }
        }
        public string StructureId
        {
            get { return Con.PrimaryStructureName; }
        }
        public string FullConstraintDefinition
        {
            get
            {
                return Con.GetConstraintString();
            }
        }
        public string ShortConstraintDefinition
        {
            get { return Con.GetConstraintStringNoStructure(); }
        }
        public string IsAddedStatus
        {
            get
            {
                if (Id < 0)
                    return "New";
                else
                    return "";
            }
        }
        public string ChangeStatusString
        {
            get
            {
                if (Con.isModified() && !Con.isCreated)
                    return ChangeStatus.Modified.Display();
                else
                    return "";
            }
        }
        public bool isModified
        {
            get
            {
                return (Con.isModified());
            }
        }
        public ReferenceTypes ReferenceType
        {
            get { return Con.ReferenceType; }
            set
            {
                if (value != Con.ReferenceType)
                {
                    bool CalcAfter = false;
                    if (!Con.isValid())
                        CalcAfter = true;
                    Con.ReferenceType = value;
                    if (CalcAfter)
                        UpdateConstraint();
                    //   RaisePropertyChangedEvent(nameof(ConstraintSelector.ShortConstraintDefinition));
                    //   RaisePropertyChangedEvent("OppositeReferenceTypeString"); // notify view to update the opposite as well
                }
            }
        }
        public string OppositeReferenceTypeString // this is used for thresholds.  
        {
            get
            {
                if (Con.ReferenceType == ReferenceTypes.Lower)
                    return "<";
                if (Con.ReferenceType == ReferenceTypes.Upper)
                    return ">";
                else
                    return "";
            }
        }
        public string GetResult(int AssessmentId)
        {
            Ctr.ConstraintResultView CRV = Con.GetResult(AssessmentId);
            if (CRV != null)
                return CRV.Result;
            else
                return "";
        }
        public List<ConstraintResultStatusCodes> GetStatusCodes(int AssessmentId)
        {
            Ctr.ConstraintResultView CRV = Con.GetResult(AssessmentId);
            if (CRV != null)
                return CRV.StatusCodes;
            else
                return null;
        }
        public bool isResultCalculating(int AssessmentId)
        {
            Ctr.ConstraintResultView CRV = Con.GetResult(AssessmentId);
            if (CRV != null)
                return CRV.isCalculating;
            else
                return false;
        }
        public ConstraintThresholdNames GetViolationStatus(int AssessmentId)
        {
            Ctr.ConstraintResultView CRV = Con.GetResult(AssessmentId);
            if (CRV != null)
                return CRV.ThresholdStatus;
            else
                return ConstraintThresholdNames.Unset;
        }
        private void SetComboBoxes()
        {
            ConstraintUnitTypes.Clear();
            List<ConstraintUnits> ConUnitList = new List<ConstraintUnits>();
            List<ConstraintUnits> RefUnitList = new List<ConstraintUnits>();
            ConUnitList.Add(ConstraintUnits.Percent);
            if (Con.isConstraintValueDose())
            {
                ConUnitList.Add(ConstraintUnits.cGy);
            }
            else
            {
                ConUnitList.Add(ConstraintUnits.cc);
            }
            foreach (ConstraintUnits U in ConUnitList)
            {
                ConstraintUnitTypes.Add(U);
            }

            RaisePropertyChangedEvent("ConstraintUnits");
            AvailableReferenceUnitTypes.Clear();


            RefUnitList.Add(ConstraintUnits.Percent);
            AvailableReferenceTypes.Clear();
            foreach (ReferenceTypes RT in Enum.GetValues(typeof(ReferenceTypes)).Cast<ReferenceTypes>())
            {
                if (RT == ReferenceTypes.Unset)
                    continue; // don't list this
                AvailableReferenceTypes.Add(RT);
            }
            if (Con.ConstraintType == ConstraintTypeCodes.CI)
            {
                RefUnitList.Add(ConstraintUnits.Multiple);
            }
            else
            {
                if (Con.isReferenceValueDose())
                {

                    RefUnitList.Add(ConstraintUnits.cGy);
                }
                else
                {
                    RefUnitList.Add(ConstraintUnits.cc);
                }
            }
            foreach (ConstraintUnits U in RefUnitList)
            {
                AvailableReferenceUnitTypes.Add(U);
            }
            RaisePropertyChangedEvent("ReferenceUnits");
            RaisePropertyChangedEvent("ReferenceTypes");
        }
        public ConstraintSelector(Ctr.Constraint ConIn, StructureSelector SSin)
        {
            Con = ConIn;
            SS = SSin;
            ConstraintTypes.Clear();
            foreach (ConstraintTypeCodes T in Enum.GetValues(typeof(ConstraintTypeCodes)).Cast<ConstraintTypeCodes>())
            {
                if (T == ConstraintTypeCodes.Unset)
                    continue;
                ConstraintTypes.Add(T);
            }
            SetComboBoxes();
            Components.Clear();
            foreach (var CN in Ctr.GetComponentList())
            {
                Components.Add(new ComponentSelector(CN));
            }
            Con.ConstraintEvaluating += OnConstraintEvaluating;
            Con.ConstraintEvaluated += OnConstraintEvaluated;
            Con.PropertyChanged += OnConstraintPropertyChanged;
            SSin.PropertyChanged += OnStructurePropertyChanged;
            RaisePropertyChangedEvent("ChangeStatusString");
            RaisePropertyChangedEvent(nameof(ConstraintValueColor));
            RaisePropertyChangedEvent(nameof(ReferenceValueColor));
        }
        // Event Handlers
        private async void UpdateConstraint()
        {
            await Task.Run(() => Con.EvaluateConstraint());
        }
        private void OnConstraintEvaluated(object sender, int AssessmentId)
        {
            RefreshFlag = !RefreshFlag;
        }
        private void OnConstraintEvaluating(object sender, int AssessmentId)
        {
            RefreshFlag = !RefreshFlag;
        }
        private void OnConstraintPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Ctr.Constraint.DisplayOrder):
                    RaisePropertyChangedEvent("DisplayOrder");
                    break;
                default:
                    RefreshFlag ^= true; // necessary for when these properties are updated internally to Squin, i.e. by an A/B ratio change
                    RaisePropertyChangedEvent(nameof(ConstraintSelector.ShortConstraintDefinition));
                    RaisePropertyChangedEvent(nameof(ChangeStatusString));
                    RaisePropertyChangedEvent(nameof(ConstraintSelector.ConstraintType));
                    RaisePropertyChangedEvent(nameof(ConstraintSelector.ConstraintValue));
                    RaisePropertyChangedEvent(nameof(ConstraintSelector.ConstraintUnit));
                    RaisePropertyChangedEvent(nameof(ConstraintSelector.ReferenceValue));
                    RaisePropertyChangedEvent(nameof(ConstraintSelector.ReferenceUnit));
                    break;
            }
        }
        private void OnStructurePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "EclipseStructureName":
                    RefreshFlag ^= true;
                    //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RefreshRowHeader"));
                    RaisePropertyChangedEvent("RefreshRowHeader");

                    break;
            }
        }
        public ObservableCollection<ConstraintTypeCodes> ConstraintTypes { get; private set; } = new ObservableCollection<ConstraintTypeCodes>() { ConstraintTypeCodes.Unset };
        public ObservableCollection<ConstraintUnits> ConstraintUnitTypes { get; private set; } = new ObservableCollection<ConstraintUnits>() { ConstraintUnits.Unset };
        public ObservableCollection<ReferenceTypes> AvailableReferenceTypes { get; private set; } = new ObservableCollection<ReferenceTypes>() { ReferenceTypes.Unset };
        public ObservableCollection<ConstraintUnits> AvailableReferenceUnitTypes { get; private set; } = new ObservableCollection<ConstraintUnits>() { ConstraintUnits.Unset };
        public ObservableCollection<ComponentSelector> Components { get; private set; } = new ObservableCollection<ComponentSelector>() { };

    }
    [AddINotifyPropertyChangedInterface]
    public class AssignmentSelector
    {
        public int ComponentId { get; set; }
        public int PlanId { get; set; }
    }
    //[AddINotifyPropertyChangedInterface]
    //public class AssessmentPlanSelector
    //{
    //    public PatientView _P { get; set; }
    //    public ComponentSelector CS { get; set; }
    //    private CourseSelector _Course;
    //    public CourseSelector Course
    //    {
    //        get { return _Course; }
    //        set
    //        {
    //            if (value != _Course)
    //            {
    //                _Course = value;
    //                AvailablePlans.Clear();
    //                PlanTypes P = PlanTypes.Unset;
    //                switch (CS.ComponentType)
    //                {
    //                    case (ComponentTypes.Plan):
    //                        P = PlanTypes.Single;
    //                        break;
    //                    case (ComponentTypes.Sum):
    //                        P = PlanTypes.Sum;
    //                        break;

    //                }
    //                foreach (var A in _P.Courses.FirstOrDefault(x => x.CourseId == _Course.CourseId).Plans.Where(x => x.PlanType == P))
    //                {
    //                    AvailablePlans.Add(A);
    //                }
    //            }
    //        }
    //    }
    //    public ObservableCollection<PlanSelector> AvailablePlans = new ObservableCollection<PlanSelector>();
    //    public AssessmentPlanSelector(PatientView _P)
    //    {
    //        _P = _P;
    //    }
    //}
    [AddINotifyPropertyChangedInterface]
    public class PlanSelector
    {
        public string PlanId { get; private set; } = "";
        public string PlanUID { get; private set; } = "";
        public string StructureSetUID { get; private set; } = "";
        public string CourseId { get; private set; } = "";
        public PlanTypes PlanType { get; set; } = PlanTypes.Unset;
        public PlanSelector(string planId = "", string planUID = "", string courseId = "", string structureSetUID = "", AssessmentComponentView ACVinit = null)
        {
            PlanId = planId;
            StructureSetUID = structureSetUID;
            PlanUID = planUID;
            CourseId = courseId;
            ACV = ACVinit;
        }
        public AssessmentComponentView ACV = null;
        public Controls.Control_ViewModel VM = new Controls.Control_ViewModel();
    }
    [AddINotifyPropertyChangedInterface]
    public class CourseSelector
    {
        public string CourseId { get; set; }
        public CourseSelector(string courseId = "")
        {
            CourseId = courseId;
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class StructureSetSelector
    {
        private Ctr.StructureSetHeader _StS = null;
        public string StructureSetId
        {
            get
            {
                if (_StS == null)
                    return "";
                else
                    return _StS.StructureSetId;
            }
        }
        public string StructureSetUID
        {
            get
            {
                if (_StS == null)
                    return "";
                else
                    return _StS.StructureSetUID;
            }
        }

        public string ComboBoxDisplay
        {
            get
            {
                return string.Format("{0} ({1})", StructureSetId, LinkedPlanId);
            }
        }

        public string LinkedPlanId
        {
            get
            {
                return _StS.LinkedPlanId;
            }
        }
        public StructureSetSelector(Ctr.StructureSetHeader StS = null)
        {
            _StS = StS;
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class PatientView : ObservableObject, IDisposable
    {
        public string PatientId { get; set; } = "";
        public bool AutomaticStructureAliasingEnabled = true;
        public Presenter ParentView;
        public string FullPatientName { get; private set; }
        public System.Windows.Media.SolidColorBrush TextBox_Background_Color { get; set; } = new System.Windows.Media.SolidColorBrush(wpfcolors.AliceBlue);
        public ObservableCollection<CourseSelector> Courses { get; set; } = new ObservableCollection<CourseSelector>() { new CourseSelector() };
        public ObservableCollection<StructureSetSelector> StructureSets { get; set; } = new ObservableCollection<StructureSetSelector>() { new StructureSetSelector(null) };
        private StructureSetSelector _CurrentStructureSet;

        public bool CalculateOnUpdate = true;
        public StructureSetSelector CurrentStructureSet
        {
            get
            {
                return _CurrentStructureSet;
            }
            set
            {
                if (_CurrentStructureSet != value)
                {
                    _CurrentStructureSet = value;
                    // Apply structure aliasing
                    if (value != null)
                    {
                        SetCurrentStructureSet(_CurrentStructureSet.StructureSetUID, AutomaticStructureAliasingEnabled, CalculateOnUpdate);

                    }
                }
            }
        }
        private async void SetCurrentStructureSet(string structureSetId, bool ApplyAliasing = true, bool CalculatOnStructureSetUpdate = true)
        {
            if (ParentView != null)
            {
                ParentView.isLoading = true;
                ParentView.LoadingString = "Applying structure aliases...";
                bool success = await Task.Run(() => Ctr.SetCurrentStructureSet(_CurrentStructureSet.StructureSetUID, ApplyAliasing));
                if (CalculatOnStructureSetUpdate)
                    Ctr.UpdateAllConstraints();
                ParentView.isLoading = false;
            }
        }

        public PatientView(string patientId = "")
        {
            Ctr.PatientOpened += OnPatientOpened;
            Ctr.CurrentStructureSetChanged += OnCurrentStructureSetChanged;
            Ctr.AvailableStructureSetsChanged += OnAvailableStructureSetsChanged;
        }

        public void Dispose()
        {
            Ctr.PatientOpened -= OnPatientOpened;
            Ctr.AvailableStructureSetsChanged -= OnAvailableStructureSetsChanged;
            Ctr.CurrentStructureSetChanged -= OnCurrentStructureSetChanged;
        }

        private void OnPatientOpened(object sender, EventArgs e)
        {
            PatientId = Ctr.PatientID;
            FullPatientName = string.Format("{0}, {1}", Ctr.PatientLastName, Ctr.PatientFirstName);
            StructureSets.Clear();
            foreach (Ctr.StructureSetHeader StS in Ctr.GetAvailableStructureSets())
            {
                StructureSets.Add(new StructureSetSelector(StS));
            }
        }

        private void OnAvailableStructureSetsChanged(object sender, EventArgs e)
        {
            var A = Ctr.GetAvailableStructureSets();
            StructureSetSelector NewSSS = null;
            foreach (var SS in A) // Make new linked structure set available
            {
                if (!StructureSets.Select(x => x.StructureSetUID).Contains(SS.StructureSetUID))
                {
                    NewSSS = new StructureSetSelector(SS);
                    SquintScript.App.Current.Dispatcher.Invoke(() =>
                    {
                        StructureSets.Add(NewSSS);
                    });

                }
            }
            var ExistingStructureSets = new List<StructureSetSelector>(StructureSets);
            foreach (var SSS in ExistingStructureSets) // remove structure sets that are no longer linked
            {
                if (!A.Select(x => x.StructureSetUID).Contains(SSS.StructureSetUID))
                {
                    SquintScript.App.Current.Dispatcher.Invoke(() =>
                    {
                        StructureSets.Remove(SSS);
                    });
                }
            }
        }
        private void OnCurrentStructureSetChanged(object sender, EventArgs e)
        {
            _CurrentStructureSet = StructureSets.FirstOrDefault(x => x.StructureSetUID == Ctr.CurrentStructureSet.UID);
            RaisePropertyChangedEvent(nameof(CurrentStructureSet));
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class AssessmentComponentView : ObservableObject, IDisposable
    {
        public string ComponentName
        {
            get { return Comp.ComponentName; }
            set
            {
                if (value != Comp.ComponentName)
                {
                    Comp.ComponentName = value;
                }
            }
        }
        public bool Warning { get; set; } = false;
        public string WarningString { get; set; } = "";
        public bool EnableCourseSelection { get; set; } = true;
        public bool DisableAutomaticAssociation = false;
        private CourseSelector _SelectedCourse;
        public CourseSelector SelectedCourse
        {
            get { return _SelectedCourse; }
            set
            {
                RaisePropertyChangedEvent("EnableCourseSelection");
                if (_SelectedCourse != value && value != null) // refresh available plans
                {
                    _SelectedCourse = value;
                    EnableCourseSelection = false;
                    SetPlanIds(_SelectedCourse.CourseId);
                }

            }
        }
        private async void SetPlanIds(string CourseId)
        {
            try
            {
                ParentView.ParentView.ParentView.ParentView.isLoading = true;
                ParentView.ParentView.ParentView.ParentView.LoadingString = "Loading course plans...";
                List<Ctr.PlanDescriptor> result = await Ctr.GetPlanDescriptors(_SelectedCourse.CourseId);
                ObservableCollection<PlanSelector> NewPlans = new ObservableCollection<PlanSelector>();
                foreach (var d in result)
                {
                    NewPlans.Add(new PlanSelector(d.PlanId, d.PlanUID, CourseId, d.StructureSetUID, this));
                }
                Plans = NewPlans;
                ParentView.ParentView.ParentView.ParentView.isLoading = false;
                ParentView.ParentView.ParentView.ParentView.LoadingString = "";
                EnableCourseSelection = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0} {1} {2}", ex.Message, ex.InnerException, ex.StackTrace));
            }
        }
        private PlanSelector _SelectedPlan;
        public PlanSelector SelectedPlan
        {
            get { return _SelectedPlan; }
            set
            {
                if (value == null)
                {
                    if (!DisableAutomaticAssociation)
                        Ctr.GetAssessment(A.ID).ClearComponentAssociation(Comp.ID);
                }
                if (value != _SelectedPlan)
                {
                    _SelectedPlan = value;
                    if (value != null)
                        SetPlanAsync();
                    RaisePropertyChangedEvent("SelectedPlan");
                }
            }
        }
        private Ctr.Component Comp;
        private Ctr.Assessment A;
        private AssessmentView ParentView;
        public ObservableCollection<CourseSelector> Courses { get; set; } = new ObservableCollection<CourseSelector>();
        public ObservableCollection<PlanSelector> Plans { get; set; } = new ObservableCollection<PlanSelector>();
        public AssessmentComponentView(AssessmentView AV, Ctr.Component CompIn, Ctr.Assessment Ain)
        {
            Comp = CompIn;
            ParentView = AV;
            Comp.PropertyChanged += UpdateStatus;
            Ctr.CurrentStructureSetChanged += UpdateStatus;
            A = Ain;
            var _P = Ctr.GetPlan(Comp.ID, A.ID); // check if plan is associated
            if (_P != null)
                Warning = Ctr.GetPlan(Comp.ID, A.ID).LoadWarning; // initialize warning 
            foreach (string CourseName in Ctr.GetCourseNames())
            {
                Courses.Add(new CourseSelector(CourseName));
            }

        }
        public void Dispose()
        {
            Comp.PropertyChanged -= UpdateStatus;
            Ctr.CurrentStructureSetChanged -= UpdateStatus;
        }
        private async void SetPlanAsync()
        {
            if (!DisableAutomaticAssociation)
            {
                try
                {
                    ParentView.ParentView.ParentView.ParentView.isLoading = true;
                    ParentView.ParentView.ParentView.ParentView.LoadingString = "Loading plan...";
                    var CSC = await Ctr.AssociatePlanToComponent(A.ID, Comp.ID, _SelectedCourse.CourseId, _SelectedPlan.PlanId, Comp.ComponentType.Value, true);
                    Ctr.UpdateComponentConstraints(Comp.ID, A.ID);
                    UpdateWarning(CSC);
                    ParentView.ParentView.ParentView.ParentView.isLoading = false;
                    ParentView.ParentView.ParentView.ParentView.LoadingString = "";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2})", ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
            ParentView.UpdateWarning();
        }
        private void UpdateStatus(object sender, EventArgs e)
        {
            if (Comp != null)
            {
                var CSC = A.StatusCodes(Comp.ID);
                UpdateWarning(CSC);
            }
        }
        private void UpdateWarning(List<ComponentStatusCodes> CSC)
        {
            Warning = false;
            WarningString = "";
            if (CSC != null)
            {
                foreach (var status in CSC)
                {
                    if (status == ComponentStatusCodes.Evaluable)
                        continue;
                    else
                    {
                        Warning = true;
                        if (WarningString == "")
                            WarningString = status.Display();
                        else
                            WarningString = WarningString + System.Environment.NewLine + status.Display();
                    }
                }
                ParentView.UpdateWarning();
            }
        }
    }

    [AddINotifyPropertyChangedInterface]
    public class AssessmentView : ObservableObject
    {
        public string ContentControlDataField { get; set; }
        public wpfcolor Color { get; set; }
        public wpfcolor TextColor { get; set; }
        public string TextDataField { get; set; }
        public string AssessmentName { get; set; }
        public bool Pinned { get; set; } = false;
        public bool Warning
        {
            get
            {
                foreach (var ACV in ACVs)
                {
                    if (ACV.Warning)
                        return true;
                }
                return false;
            }
        }
        public void UpdateWarning()
        {
            RaisePropertyChangedEvent("Warning"); // this to update the warning symbol in the parent view;
        }

        public int AssessmentId
        {
            get { return A.ID; }
        }
        public AssessmentsView ParentView;
        private Ctr.Assessment A;
        public int ComponentCount
        {
            get { return ACVs.Count; }
        }
        public ObservableCollection<AssessmentComponentView> ACVs { get; set; } = new ObservableCollection<AssessmentComponentView>();
        public AssessmentView(wpfcolor Color_in, wpfcolor TextColor_in, AssessmentsView ParentView_in)
        {
            ParentView = ParentView_in;
            Color = Color_in;
            TextColor = TextColor_in;
            if (!Ctr.PatientLoaded)
            {
                MessageBox.Show("Please load patient first...");
                return;
            }
            A = Ctr.NewAssessment();
            AssessmentName = A.AssessmentName;
            foreach (Ctr.Component Comp in Ctr.GetComponentList())
            {
                ACVs.Add(new AssessmentComponentView(this, Comp, A));
            }
        }
        public AssessmentView(Ctr.Assessment Ain, wpfcolor Color_in, wpfcolor TextColor_in, AssessmentsView ParentView_in)
        {
            A = Ain;
            Color = Color_in;
            TextColor = TextColor_in;
            AssessmentName = Ain.AssessmentName;
            ParentView = ParentView_in;
            foreach (Ctr.Component Comp in Ctr.GetComponentList())
            {
                var ACV = new AssessmentComponentView(this, Comp, A);
                ACV.DisableAutomaticAssociation = true;
                var _P = Ctr.GetPlan(Comp.ID, A.ID);
                if (_P != null)
                {
                    ACV.WarningString = _P.LoadWarningString;
                    if (!_P.LoadWarning) // if there's a load warning (e.g can't find the file, don't set the combo boxes
                    {
                        ACV.SelectedCourse = ACV.Courses.FirstOrDefault(x => x.CourseId == _P.CourseId);
                        ACV.SelectedPlan = ACV.Plans.FirstOrDefault(x => x.PlanId == _P.PlanId);
                        foreach (ComponentStatusCodes code in _P.GetErrorCodes())
                        {
                            if (code == ComponentStatusCodes.Evaluable)
                                continue;
                            else
                            {
                                ACV.Warning = true;
                                ACV.WarningString = string.Join(System.Environment.NewLine, code.Display());
                            }
                        }
                    }
                    else
                        ACV.Warning = true;
                }
                ACV.DisableAutomaticAssociation = false;
                ACVs.Add(ACV);
            }

        }
        public void Delete()
        {
            foreach (var ACV in ACVs)
                ACV.Dispose();
            Ctr.RemoveAssessment(A.ID);
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class AssessmentsView : ObservableObject
    {
        public AssessmentsView(ProtocolView parentView)
        {
            ParentView = parentView;
        }
        public Progress<int> Progress { get; set; }
        public Thickness ColHeaderMargin { get; set; } = new Thickness(10, 5, 10, 5);
        //public DataGridLength RowHeaderWidth { get; set; } = new DataGridLength(1, DataGridLengthUnitType.Auto);
        public double RowHeaderWidth { get; set; } = double.NaN;
        public int AssessmentCounter { get; private set; } = 1;
        public bool WaitingForUpdate { get; set; } = false;
        public string WaitingDescription { get; set; } = "";

        public ProtocolView ParentView { get; set; }
        public double FontSize { get; set; } = 12;
        private List<wpfcolor> DefaultAssessmentColors = new List<wpfcolor> { wpfcolors.LightSteelBlue, wpfcolors.AliceBlue, wpfcolors.PapayaWhip, wpfcolors.PaleGoldenrod };
        private List<wpfcolor> DefaultAssessmentTextColors = new List<wpfcolor> { wpfcolors.White, wpfcolors.Black, wpfcolors.Black, wpfcolors.Black };

        public AssessmentView SelectedAssessment { get; set; }
        public ObservableCollection<AssessmentView> Assessments { get; set; } = new ObservableCollection<AssessmentView>();
        public ObservableCollection<SquintDataColumn> AssessmentColumns { get; set; } = new ObservableCollection<SquintDataColumn>();
        public void AddAssessment()
        {
            if (Ctr.PatientLoaded && Ctr.ProtocolLoaded)
            {
                int colindex = (AssessmentCounter - 1) % DefaultAssessmentColors.Count;
                AssessmentView AV = new AssessmentView(DefaultAssessmentColors[colindex], DefaultAssessmentTextColors[colindex], this);
                Assessments.Add(AV);
                //Add new column
                SquintDataColumn dgtc = new SquintDataColumn(AV)
                {
                    //HeaderTemplate = (DataTemplate)Resources["myColumnHeaderTemplate"],
                    HeaderStyle = (Style)Application.Current.FindResource("SquintColumnHeaderStyle"),
                    CellTemplate = (DataTemplate)Application.Current.FindResource("SquintCellTemplate"),
                    CellStyle = (Style)Application.Current.FindResource("SquintCellStyle"),
                    Header = AV.AssessmentName,
                    Width = DataGridLength.Auto,
                };
                AssessmentColumns.Add(dgtc);
                AssessmentCounter++;
            }
            else
            {
                MessageBox.Show("Please load patient and protocol first", "No open Protocol/Patient");
                return;
            }

        }
        public void LoadAssessmentViews()
        {
            if (Ctr.PatientLoaded && Ctr.ProtocolLoaded)
            {
                foreach (Ctr.Assessment A in Ctr.GetAssessmentList())
                {
                    int colindex = (AssessmentCounter - 1) % DefaultAssessmentColors.Count;
                    AssessmentView AV = new AssessmentView(A, DefaultAssessmentColors[colindex], DefaultAssessmentTextColors[colindex], this);
                    Assessments.Add(AV);
                    //Add new column
                    SquintDataColumn dgtc = new SquintDataColumn(AV)
                    {
                        HeaderStyle = (Style)Application.Current.FindResource("SquintColumnHeaderStyle"),
                        CellTemplate = (DataTemplate)Application.Current.FindResource("SquintCellTemplate"),
                        CellStyle = (Style)Application.Current.FindResource("SquintCellStyle"),
                        Header = AV.AssessmentName,
                        Width = DataGridLength.Auto,
                    };
                    AssessmentColumns.Add(dgtc);
                    AssessmentCounter++;
                }
            }

            //else
            //{
            //    MessageBox.Show("Please load patient and protocol first", "No open Protocol/Patient");
            //    return;
            //}
        }
        public void DeleteAssessment(AssessmentView AS)
        {
            var UpdatedAssessments = new ObservableCollection<AssessmentView>();
            foreach (AssessmentView AV in Assessments)
            {
                if (AV != AS)
                    UpdatedAssessments.Add(AV);
            }
            Assessments = UpdatedAssessments;
            AssessmentColumns.Remove(AssessmentColumns.FirstOrDefault(x => x.Header.ToString() == AS.AssessmentName));
        }
        private void AddAssessment(object param = null)
        {
            AddAssessment();
        }
        public ICommand AddAssessmentCommand
        {
            get { return new DelegateCommand(AddAssessment); }
        }

        public ICommand ChangeLinkVisibilityCommand
        {
            get { return new DelegateCommand(ChangeLinkProtocolVisibility); }
        }
        private void ChangeLinkProtocolVisibility(object param = null)
        {
            ParentView.ParentView.isLinkProtocolVisible = !ParentView.ParentView.isLinkProtocolVisible;
            // Add an assessment if there aren't any
            if (Assessments.Count == 0 && ParentView.ParentView.isLinkProtocolVisible == true)
                AddAssessment();
        }
        //private void UpdateAssessmentsView()
        //{
        //    AssessmentPresenter = new AssessmentsView(this);
        //    AssessmentPresenter.LoadAssessmentViews();
        //}
        public ICommand FontSizeIncreaseCommand
        {
            get { return new DelegateCommand(FontSizeIncrease); }
        }
        public ICommand FontSizeDecreaseCommand
        {
            get { return new DelegateCommand(FontSizeDecrease); }
        }
        private void FontSizeIncrease(object param = null)
        {
            var AP = param as AssessmentsView;
            if (AP != null)
                AP.FontSize = AP.FontSize + 1;
        }
        private void FontSizeDecrease(object param = null)
        {
            var AP = param as AssessmentsView;
            if (AP != null)
            {
                AP.FontSize = AP.FontSize - 1;
                foreach (SquintDataColumn C in AssessmentColumns)
                {
                    C.Width = new DataGridLength(0);
                    C.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
                }
                RowHeaderWidth = 0;
                RowHeaderWidth = double.NaN;
            }
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class ConstraintResultView
    {
        private Ctr.ConstraintResultView sCRV;
        public string Result
        {
            get { return sCRV.Result; }
        }
        public ConstraintResultView(Ctr.ConstraintResultView crv)
        {
            sCRV = crv;
        }
    }
    //[AddINotifyPropertyChangedInterface]
    //public class ConstraintResultsView
    //{
    //    public Ctr.Constraint Con;
    //    private int _UpdatedId { get; set; }
    //    public int UpdatedId
    //    {
    //        get { return _UpdatedId; }
    //        set { _UpdatedId = value; }
    //    }
    //    public Dictionary<int, ConstraintResultView> ConstraintResults;
    //    public ConstraintResultsView(Ctr.Constraint ConIn)
    //    {
    //        Con = ConIn;
    //        Con.ConstraintEvaluated += OnConstraintEvaluated;
    //    }
    //    private void OnConstraintEvaluated(object sender, int AssessmentId)
    //    {
    //        _UpdatedId = AssessmentId;
    //    }
    //}
    [AddINotifyPropertyChangedInterface]
    public class SessionsView
    {
        public ObservableCollection<Ctr.SessionView> SessionViews { get; private set; } = new ObservableCollection<Ctr.SessionView>();
        public string SessionComment { get; set; }
        public SessionsView()
        {
            Ctr.SessionsChanged -= OnSessionsChanged;
            Ctr.SessionsChanged += OnSessionsChanged;
            foreach (Ctr.SessionView E in Ctr.GetSessionViews())
                SessionViews.Add(E);
        }
        public void OnSessionsChanged(object sender, EventArgs e)
        {
            ObservableCollection<Ctr.SessionView> updatedSV = new ObservableCollection<Ctr.SessionView>();
            foreach (Ctr.SessionView E in Ctr.GetSessionViews())
                updatedSV.Add(E);
            SessionViews = updatedSV;
        }
    }
    [AddINotifyPropertyChangedInterface]
    public class Presenter
    {
        public Presenter()
        {
            Protocol = new ProtocolView(this);
        }
        [AddINotifyPropertyChangedInterface]
        public class FilterComboBox : ObservableObject
        {
            public FilterComboBox(ViewEnums FilterType)
            {
                _ComboBoxHeader = FilterType.Display();
                View = FilterType;
                var NewComboSourceList = new ObservableCollection<object>();
                Ctr.ProtocolOpened += OnProtocolOpened;
                switch (FilterType)
                {
                    case ViewEnums.Centre:
                        foreach (TreatmentCentres TC in Enum.GetValues(typeof(TreatmentCentres)))
                        {
                            if (TC == TreatmentCentres.Unset || TC == TreatmentCentres.All)
                                continue;
                            else
                                NewComboSourceList.Add(TC);
                        }
                        ComboSourceList = NewComboSourceList;
                        break;
                    case ViewEnums.Site:
                        foreach (TreatmentSites TC in Enum.GetValues(typeof(TreatmentSites)))
                        {
                            if (TC == TreatmentSites.Unset || TC == TreatmentSites.All)
                                continue;
                            else
                                NewComboSourceList.Add(TC);
                        }
                        ComboSourceList = NewComboSourceList;
                        break;
                    case ViewEnums.ProtocolType:
                        foreach (ProtocolTypes TC in Enum.GetValues(typeof(ProtocolTypes)))
                        {
                            if (TC == ProtocolTypes.Unset || TC == ProtocolTypes.All)
                                continue;
                            else
                                NewComboSourceList.Add(TC);
                        }
                        ComboSourceList = NewComboSourceList;
                        break;
                    case ViewEnums.Approval:
                        foreach (ApprovalLevels TC in Enum.GetValues(typeof(ApprovalLevels)))
                        {
                            if (TC == ApprovalLevels.Unset || TC == ApprovalLevels.All)
                                continue;
                            else
                                NewComboSourceList.Add(TC);
                        }
                        ComboSourceList = NewComboSourceList;
                        break;
                }
            }
            public ViewEnums View { get; private set; }
            private string _ComboBoxHeader = "Default";
            public string ComboBoxHeader
            {
                get
                {
                    return _ComboBoxHeader;
                }
                set
                {
                    _ComboBoxHeader = value;
                    RaisePropertyChangedEvent("ComboBoxHeader");
                }
            }
            public ObservableCollection<object> ComboSourceList { get; set; } = new ObservableCollection<object>() { "Test1", "Test2" };
            public object ComboSelectedItem
            {
                get
                {
                    switch (View)
                    {
                        case ViewEnums.Centre:
                            if (Ctr.ProtocolLoaded)
                                return Ctr.GetActiveProtocol().TreatmentCentre;
                            else
                                return null;
                        case ViewEnums.Site:
                            if (Ctr.ProtocolLoaded)
                                return Ctr.GetActiveProtocol().TreatmentSite;
                            else
                                return null;
                        case ViewEnums.ProtocolType:
                            if (Ctr.ProtocolLoaded)
                                return Ctr.GetActiveProtocol().ProtocolType;
                            else
                                return null;
                        case ViewEnums.Approval:
                            if (Ctr.ProtocolLoaded)
                                return Ctr.GetActiveProtocol().ApprovalLevel;
                            else
                                return null;
                        default:
                            return null;
                    }
                }
                set
                {
                    switch (View)
                    {
                        case ViewEnums.Centre:
                            if (Ctr.ProtocolLoaded)
                                Ctr.GetActiveProtocol().TreatmentCentre = (TreatmentCentres)value;
                            break;
                        case ViewEnums.Site:
                            if (Ctr.ProtocolLoaded)
                                Ctr.GetActiveProtocol().TreatmentSite = (TreatmentSites)value;
                            break;
                        case ViewEnums.ProtocolType:
                            if (Ctr.ProtocolLoaded)
                                Ctr.GetActiveProtocol().ProtocolType = (ProtocolTypes)value;
                            break;
                        case ViewEnums.Approval:
                            if (Ctr.ProtocolLoaded)
                                Ctr.GetActiveProtocol().ApprovalLevel = (ApprovalLevels)value;
                            break;
                    }
                }
            }
            private void OnProtocolOpened(object sender, EventArgs e)
            {
                RaisePropertyChangedEvent("ComboSelectedItem");
            }
        }


        //public Controls.Beam_ViewModel Beam_ViewModel { get; set; } = new Controls.Beam_ViewModel();
        public Controls.LoadingViewModel Loading_ViewModel { get; set; } = new Controls.LoadingViewModel();
        //public Controls.Control_ViewModel Objectives_ViewModel { get; set; } = new Controls.Control_ViewModel();
        //public Controls.Imaging_ViewModel Imaging_ViewModel { get; set; } = new Controls.Imaging_ViewModel();
        //public Controls.TestList_ViewModel Simulation_ViewModel { get; set; } = new Controls.TestList_ViewModel();
        //public Controls.TestList_ViewModel Targets_ViewModel { get; set; } = new Controls.TestList_ViewModel();
        //public Controls.TestList_ViewModel Calculation_ViewModel { get; set; } = new Controls.TestList_ViewModel();
        //public Controls.TestList_ViewModel Prescription_ViewModel { get; set; } = new Controls.TestList_ViewModel();
        public ProtocolView Protocol { get; set; }
        public PatientView PatientPresenter { get; set; } = new PatientView(null);
        public SessionsView SessionsPresenter { get; set; } = new SessionsView();
        public EclipseProtocolPopupViewModel EclipseProtocolView { get; set; }
        public AssessmentView NewAssessmentId { get; private set; }
        public bool isPIDVisible { get; set; } = false;
        public bool AdminOptionsToggle { get; set; } = false;
        public bool SquintIsBusy { get; set; } = false;
        public int NumAdminButtons { get; private set; } = 7;
        public bool PlanCheckVisible { get; set; } = false;
        public string PlanCheckLoadingMessage { get; set; } = "Checking plan, please wait...";
        public bool isPlanCheckCalculating { get; set; } = false;
        public bool ProtocolCheckVisible { get; set; } = true;
        public bool SessionSelectVisibility { get; set; } = false;
        public bool SessionSaveVisibility { get; set; } = false;
        public bool ConstraintInfoVisibility { get; set; } = false;
        public bool isLoading { get; set; } = false;
        public string LoadingString { get; set; } = "";
        public bool isLinkProtocolVisible { get; set; } = false;
        public bool isUserPanelVisible { get; set; } = false;
        public bool isConfigVisible { get; set; } = false;
        public bool isLoadProtocolPanelVisible { get; set; } = true;
        public bool isStructurePanelVisible { get; set; } = true;
        public ObservableCollection<string> Protocols { get; } = new ObservableCollection<string> { "Protocol1", "Protocol2" };

        //UI Commands
        public ICommand ChangeVisibilityCommand
        {
            get { return new DelegateCommand(ChangeVisibility); }
        }
        public ICommand SyncrhonizePatientCommand
        {
            get { return new DelegateCommand(SynchronizePatient); }
        }
        public ICommand ExpandLoadProtocolCommand
        {
            get { return new DelegateCommand(ExpandLoadProtocol); }
        }
        public ICommand ExpandStructuresCommand
        {
            get { return new DelegateCommand(ExpandStructures); }
        }



        public ICommand EnterKeyCommand_PID
        {
            get { return new DelegateCommand(LoadPatient); }
        }
        public ICommand ShowComponentCommand
        {
            get { return new DelegateCommand(ShowComponent); }
        }
        public ICommand AddConstraintCommand
        {
            get { return new DelegateCommand(AddConstraint); }
        }
        public ICommand AddStructureCommand
        {
            get { return new DelegateCommand(AddStructure); }
        }
        public ICommand ShowConfigCommand
        {
            get { return new DelegateCommand(ShowConfig); }
        }
        public ICommand ShiftConstraintUpCommand
        {
            get { return new DelegateCommand(ShiftConstraintUp); }
        }
        public ICommand ShiftConstraintDownCommand
        {
            get { return new DelegateCommand(ShiftConstraintDown); }
        }
        public ICommand AddConstraintBelowCommand
        {
            get { return new DelegateCommand(AddConstraintBelow); }
        }
        public ICommand DeleteConstraintCommand
        {
            get { return new DelegateCommand(DeleteConstraint); }
        }
        public ICommand ExpandUserPanelCommand
        {
            get { return new DelegateCommand(ExpandUserPanel); }
        }
        public ICommand LaunchAdminViewCommand
        {
            get { return new DelegateCommand(LaunchAdminView); }
        }
        public ICommand SaveWorkspaceCommand
        {
            get
            {
                return new DelegateCommand(SaveSessionDialog);
            }
        }
        public ICommand LoadWorkspaceCommand
        {
            get
            {
                return new DelegateCommand(LoadSession);
            }
        }
        public ICommand SaveSessionCommand
        {
            get
            {
                return new DelegateCommand(SaveSession);
            }
        }
        public ICommand LoadSelectedSessionCommand
        {
            get
            {
                return new DelegateCommand(LoadSelectedSession);
            }
        }
        public ICommand DeleteSelectedSessionCommand
        {
            get
            {
                return new DelegateCommand(DeleteSelectedSession);
            }
        }
        public ICommand ViewPlanCheckCommand
        {
            get
            {
                return new DelegateCommand(ViewPlanCheck);
            }
        }
        public ICommand CloseCheckListCommand
        {
            get { return new DelegateCommand(CloseCheckList); }
        }

        public ICommand ImportProtocolDirectoryCommand
        {
            get { return new DelegateCommand(ImportProtocolDirectory); }
        }
        private async void ImportProtocolDirectory(object param = null)
        {
            System.Windows.Forms.FolderBrowserDialog f = new System.Windows.Forms.FolderBrowserDialog();
            f.Description = "Please select import folder...";
            f.SelectedPath = @"\\srvnetapp02\bcca\docs\physics\cn\software\squint\xml protocol library\";
            SquintIsBusy = true;
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string[] filenames = System.IO.Directory.GetFiles(f.SelectedPath);
                    List<Task> importTasks = new List<Task>();
                    Task finaltask = new Task(new Action(() => { }));
                    int count = 0;
                    double total = filenames.Count();
                    int filecount = 0;
                    foreach (string file in filenames)
                    {
                        filecount++;
                        string ext = System.IO.Path.GetExtension(file);
                        if (ext == ".xml")
                            try
                            {
                                if (filecount == filenames.Count())
                                    finaltask = new Task(() => Ctr.ImportProtocolFromXML(file, true));
                                else
                                {
                                    importTasks.Add(Task.Run(() =>
                                    {
                                        Ctr.ImportProtocolFromXML(file, false);
                                    }
                                    ));
                                    count++;
                                }
                            }
                            catch
                            {
                                MessageBox.Show(string.Format("Error importing {0}", file));
                            }
                    }
                    await Task.WhenAll(importTasks);
                    finaltask.RunSynchronously();
                }
                catch (Exception ex)
                {
                    Helpers.Logger.AddLog(string.Format("Error in batch import \r\n {0} \r\n {1} \r\n {2}",
                        ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
            SquintIsBusy = false;
            MessageBox.Show("Complete!");
        }

        public ICommand ImportProtocolCommand
        {
            get { return new DelegateCommand(ImportProtocol); }
        }

        private async void ImportProtocol(object param = null)
        {
            if (Ctr.PatientLoaded || Ctr.NumAssessments > 0)
            {
                System.Windows.Forms.DialogResult DR = System.Windows.Forms.MessageBox.Show("This will close the current protocol and any assessments. Any unsaved changes will be lost. Continue?", "Import from XML", System.Windows.Forms.MessageBoxButtons.YesNo);
                if (DR == System.Windows.Forms.DialogResult.No)
                    return;
            }
            Ctr.ClosePatient();
            Ctr.CloseProtocol();
            SquintIsBusy = true;
            System.Windows.Forms.OpenFileDialog d = new System.Windows.Forms.OpenFileDialog();
            d.Title = "Open Ctr.GetProtocolView() File";
            d.Filter = "XML files|*.xml";
            d.InitialDirectory = @"\\srvnetapp02\bcca\docs\Physics\CN\Software\Squint\XML Protocol Library\v0.6 Library";
            if (d.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MessageBox.Show(d.FileName.ToString());
            }
            bool ImportSuccessful = await Task.Run(() => Ctr.ImportProtocolFromXML(d.FileName, true));
            SquintIsBusy = false;
            if (!ImportSuccessful)
            {
                MessageBox.Show("Error in importing protocol, please review XML file");
            }
        }

        public ICommand ImportEclipseCommand
        {
            get { return new DelegateCommand(ImportEclipseProtocol); }
        }

        public bool ImportEclipseProtocolVisibility { get; set; } = false;
        public void ImportEclipseProtocol(object param = null)
        {
            if (Ctr.PatientLoaded || Ctr.NumAssessments > 0)
            {
                System.Windows.Forms.DialogResult DR = System.Windows.Forms.MessageBox.Show("Please close patient before importing protocols...");
                return;
            }
            EclipseProtocolView = new EclipseProtocolPopupViewModel(this);
            ImportEclipseProtocolVisibility ^= true;
            //        SquintIsBusy = true;
        }

        private void CloseCheckList(object param = null)
        {
            ProtocolCheckVisible = true;
            PlanCheckVisible = false;
        }
        private async void ViewPlanCheck(object param = null)
        {
            var p = (param as PlanSelector);
            if (param == null)
                return;
            else
            {
                ProtocolCheckVisible = false;
                PlanCheckVisible = true;
                isPlanCheckCalculating = true;
                Loading_ViewModel = new Controls.LoadingViewModel() { LoadingMessage = @"Checking plan, please wait..." };

                await Protocol.ChecklistViewModel.DisplayChecksForPlan(p);
                isPlanCheckCalculating = false;
            }
        }
        public ICommand ToggleChecklistViewCommand
        {
            get { return new DelegateCommand(ToggleCheckListView); }
        }
        private void ToggleCheckListView(object param = null)
        {
            ProtocolCheckVisible ^= false;
            PlanCheckVisible ^= true;

        }
        public ICommand DuplicateProtocolCommand
        {
            get { return new DelegateCommand(DuplicateProtocol); }
        }
        private async void DuplicateProtocol(object param = null)
        {
            LoadingString = "Creating copy of current protocol";
            isLoading = true;
            await Task.Run(() => Ctr.Save_DuplicateProtocol());
            isLoading = false;
            LoadingString = "";
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
        private async void DeleteSelectedSession(object param = null)
        {
            if (isLoading == true)
                return;
            Ctr.SessionView E = param as Ctr.SessionView;
            if (param == null)
                return;
            isLoading = true;
            LoadingString = "Deleting session...";
            try
            {
                await Task.Run(() => Ctr.Delete_Session(E.ID));
            }
            catch (Exception ex)
            {
                var t = ex;
            }
            isLoading = false;
        }
        private void SaveSessionDialog(object param = null)
        {
            SessionSaveVisibility ^= true;
        }
        private async void LoadSelectedSession(object param = null)
        {
            if (isLoading == true)
                return;
            Ctr.SessionView E = param as Ctr.SessionView;
            if (param == null)
                return;
            isLoading = true;
            LoadingString = "Loading session...";
            if (await Task.Run(() => Ctr.Load_Session(E.ID)))
            {
                Protocol.UpdateProtocolView();
                Protocol.isProtocolLoaded = true;
                Ctr.UpdateAllConstraints();
                isLinkProtocolVisible = true;
            }
            else
                MessageBox.Show("Error loading session");
            isLoading = false;
            SessionSelectVisibility ^= true;
        }
        private void LoadSession(object param = null)
        {
            SessionSelectVisibility ^= true;
        }
        private async void SaveSession(object param = null)
        {
            LoadingString = "Saving Session...";
            isLoading = true;
            bool Success = await Task.Run(() => Ctr.Save_Session(SessionsPresenter.SessionComment)); // boolean return is in order to delay the "isLoading" return to False, so the load menu has a chance to include the latest save
            isLoading = false;
            SessionSaveVisibility ^= true;
        }
        private void LaunchAdminView(object param = null)
        {
            if (Ctr.PatientLoaded)
                MessageBox.Show("Please close the current patient before starting protocol administration.");
            else
                AdminOptionsToggle ^= true;
        }
        private void DeleteConstraint(object param = null)
        {
            var CS = param as ConstraintSelector;
            if (CS != null)
            {
                if (CS.Id > 0)
                {
                    var D = MessageBox.Show("Delete this constraint?", "Confirm deletion", MessageBoxButton.OKCancel);
                    if (D == MessageBoxResult.OK)
                        Ctr.DeleteConstraint(CS.Id);
                }
                else
                    Ctr.DeleteConstraint(CS.Id);
            }
        }
        private void AddConstraintBelow(object param = null)
        {
            ConstraintSelector CS = param as ConstraintSelector;
            if (CS != null)
            {
                Ctr.DuplicateConstraint(CS.Id);
            }
        }
        private void ShowComponent(object param = null)
        {
            var CS = (ComponentSelector)param;
            CS.Pinned = !CS.Pinned;
        }
        private void ShowConfig(object param = null)
        {
            if (Ctr.SquintUser == "nchng")
                isConfigVisible = !isConfigVisible;
        }
        private void AddConstraint(object param = null)
        {
            Protocol.AddConstraint();
        }
        private void AddStructure(object param = null)
        {
            Protocol.AddStructure();
        }
        private void ShiftConstraintUp(object param = null)
        {
            ConstraintSelector CS = param as ConstraintSelector;
            if (CS != null)
            {
                Ctr.ShiftConstraintUp(CS.Id);
            }
        }
        private void ShiftConstraintDown(object param = null)
        {
            ConstraintSelector CS = param as ConstraintSelector;
            if (CS != null)
            {
                Ctr.ShiftConstraintDown(CS.Id);
            }
        }
        public ICommand ShowAssessmentCommand
        {
            get { return new DelegateCommand(ShowAssessment); }
        }
        private void ShowAssessment(object param = null)
        {
            var AS = (AssessmentView)param;
            AS.Pinned = !AS.Pinned;
        }
        public ICommand GetConstraintInformationCommand
        {
            get { return new DelegateCommand(GetConstraintInformation); }
        }
        public void GetConstraintInformation(object param = null)
        {
            var CS = (ConstraintSelector)param;
            CS.ConstraintInfoVisibility ^= true;
        }
        public ICommand PinConstraintCommand
        {
            get { return new DelegateCommand(PinConstraintDetails); }
        }
        private void PinConstraintDetails(object param = null)
        {
            var CS = (ConstraintSelector)param;
            CS.Pinned = !CS.Pinned;
        }
        public ICommand DeleteAssessmentCommand
        {
            get { return new DelegateCommand(DeleteAssessment); }
        }
        private void DeleteAssessment(object param = null)
        {
            var AS = (AssessmentView)param;
            Protocol.AssessmentPresenter.DeleteAssessment(AS);
            AS.Delete();
        }

        public ICommand UpdateProtocolCommand
        {
            get { return new DelegateCommand(UpdateProtocol); }
        }
        private async void UpdateProtocol(object param = null)
        {
            LoadingString = "Updating Protocol";
            isLoading = true;
            await Task.Run(() => Ctr.Save_UpdateProtocol());
            isLoading = false;
            LoadingString = "";
        }

        public ICommand DeleteSelectedProtocolCommand
        {
            get { return new DelegateCommand(DeleteProtocol); }
        }
        private async void DeleteProtocol(object param = null)
        {
            if (Ctr.GetActiveProtocol() != null)
            {
                var result = System.Windows.Forms.MessageBox.Show("This will delete the current protocol. Are you sure?", "Confirm protocol deletion", System.Windows.Forms.MessageBoxButtons.OKCancel);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    LoadingString = "Deleting Protocol";
                    isLoading = true;
                    await Task.Run(() => Ctr.DeleteProtocol(Ctr.GetActiveProtocol().ID));
                    Protocol.Unsubscribe();
                    Protocol = new ProtocolView(this);
                    Ctr.CloseProtocol();
                    isLoading = false;
                    LoadingString = "";
                }
            }
            else if (Protocol.SelectedProtocol != null)
            {
                var result = System.Windows.Forms.MessageBox.Show("This will delete the SELECTED protocol. Are you sure?", "Confirm protocol deletion", System.Windows.Forms.MessageBoxButtons.OKCancel);
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    LoadingString = "Deleting Protocol";
                    isLoading = true;
                    await Task.Run(() => Ctr.DeleteProtocol(Protocol.SelectedProtocol.Id));
                    isLoading = false;
                    LoadingString = "";
                }
            }
        }

        private async void LoadPatient(object param = null)
        {
            try
            {
                if (PatientPresenter.ParentView == null)
                    PatientPresenter.ParentView = this; // pass the current view so it can access the isLoading variable
                isLoading = true;
                LoadingString = "Loading patient";
                if (Ctr.PatientLoaded)
                {
                    var Result = MessageBox.Show("Close current patient and all assessments?", "Close Patient?", MessageBoxButton.OKCancel);
                    if (Result == MessageBoxResult.Cancel)
                        return;
                    CloseCheckList();
                    Ctr.ClosePatient();
                    Protocol.Unsubscribe();
                    Protocol = new ProtocolView(this);
                    Ctr.CloseProtocol();
                    foreach (string CourseId in Ctr.GetCourseNames())
                    {
                        PatientPresenter.Courses.Add(new CourseSelector(CourseId));
                    }
                }
                AdminOptionsToggle = false;
                await Task.Run(() => Ctr.LoadPatientFromDatabase(PatientPresenter.PatientId));
                SessionsPresenter = new SessionsView();
                if (Ctr.PatientLoaded)
                    PatientPresenter.TextBox_Background_Color = new System.Windows.Media.SolidColorBrush(wpfcolors.AliceBlue);
                else
                    PatientPresenter.TextBox_Background_Color = new System.Windows.Media.SolidColorBrush(wpfcolors.DarkOrange);
                isLoading = false;
                LoadingString = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0} {1} {2}", ex.Message, ex.InnerException, ex.StackTrace));
            }
        }
        private static EventHandler SynchronizeHandler;
        private async void SynchronizePatient(object param = null)
        {
            if (Ctr.PatientLoaded && Ctr.ProtocolLoaded && Ctr.GetAssessmentList().Count > 0)
            {
                var W = new WaitWindow();
                W.Show();
                SynchronizeHandler = new EventHandler((sender, e) => OnSynchronizationComplete(sender, e, W));
                Ctr.SynchronizationComplete += SynchronizeHandler;
                Ctr.SynchronizePlans();
            }
        }
        private void OnSynchronizationComplete(object sender, EventArgs E, Window W)
        {
            Ctr.SynchronizationComplete -= SynchronizeHandler;
            // Refresh ViewModel
            PatientPresenter.CalculateOnUpdate = false; // disable automatic calculation while model is updated
            string CurrentStructureSetId = ""; // store these as clearing the list will null this as it is databound.
            string CurrentStructureSetUID = "";
            if (PatientPresenter.CurrentStructureSet != null)
            {
                CurrentStructureSetId = PatientPresenter.CurrentStructureSet.StructureSetId; // store these as clearing the list will null this as it is databound.
                CurrentStructureSetUID = PatientPresenter.CurrentStructureSet.StructureSetUID;
            }
            PatientPresenter.StructureSets.Clear();
            foreach (var StS in Ctr.GetAvailableStructureSets())
            {
                PatientPresenter.StructureSets.Add(new StructureSetSelector(StS));
            }
            if (CurrentStructureSetId != "")
            {
                // Set structure set back to pre-update
                var UnchangedStructureSet = PatientPresenter.StructureSets.FirstOrDefault(x => x.StructureSetId == CurrentStructureSetId && x.StructureSetUID == CurrentStructureSetUID);
                if (UnchangedStructureSet != null)
                {
                    PatientPresenter.AutomaticStructureAliasingEnabled = false;
                    PatientPresenter.CurrentStructureSet = UnchangedStructureSet;
                    PatientPresenter.AutomaticStructureAliasingEnabled = true;
                }
            }
            foreach (AssessmentView AV in Protocol.AssessmentPresenter.Assessments)
            {
                foreach (AssessmentComponentView ACV in AV.ACVs)
                {
                    ObservableCollection<CourseSelector> UpdatedCourses = new ObservableCollection<CourseSelector>();
                    foreach (string CourseName in Ctr.GetCourseNames())
                    {
                        UpdatedCourses.Add(new CourseSelector(CourseName));
                    }
                    string PrevSelectedCourseId = "";
                    string PrevSelectedPlanUID = "";
                    if (ACV.SelectedCourse != null)
                        PrevSelectedCourseId = ACV.SelectedCourse.CourseId;
                    if (ACV.SelectedPlan != null)
                        PrevSelectedPlanUID = ACV.SelectedPlan.PlanUID;
                    ACV.Courses = UpdatedCourses;
                    if (PrevSelectedCourseId != "")
                        ACV.SelectedCourse = UpdatedCourses.FirstOrDefault(x => PrevSelectedCourseId == x.CourseId);
                    if (PrevSelectedPlanUID != "")
                        ACV.SelectedPlan = ACV.Plans.FirstOrDefault(x => PrevSelectedPlanUID == x.PlanUID);
                }
            }
            PatientPresenter.CalculateOnUpdate = true; // disable automatic calculation while model is updated
            //Ctr.UpdateAllConstraints();
            W.Close();
        }


        private void ChangeVisibility(object param = null)
        {
            if (isPIDVisible && Ctr.PatientLoaded)
            {
                var Result = MessageBox.Show("Close current patient and all assessments?", "Close Patient?", MessageBoxButton.OKCancel);
                if (Result == MessageBoxResult.Cancel)
                    return;
                Ctr.ClosePatient();
                CloseCheckList();
                PatientPresenter.Dispose();
                PatientPresenter = new PatientView();
                Protocol.Unsubscribe();
                Protocol = new ProtocolView(this);
                Ctr.CloseProtocol();

            }
            isPIDVisible = !isPIDVisible;
        }

        private void ExpandLoadProtocol(object param = null)
        {
            isLoadProtocolPanelVisible = !isLoadProtocolPanelVisible;
        }
        private void ExpandStructures(object param = null)
        {
            isStructurePanelVisible = !isStructurePanelVisible;
        }
        private void ExpandUserPanel(object param = null)
        {
            isUserPanelVisible = !isUserPanelVisible;
        }
    }
}
