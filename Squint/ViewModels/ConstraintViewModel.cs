using PropertyChanged;
using Squint.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using wpfbrush = System.Windows.Media.SolidColorBrush;
using wpfcolor = System.Windows.Media.Color;
using wpfcolors = System.Windows.Media.Colors;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ConstraintViewModel : ObservableObject
    {

        private SquintModel _model;
        public bool RefreshFlag { get; set; } // this bool updates

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
        public ObservableCollection<ConstraintChangelogViewModel> ConstraintChangelogs
        {
            get
            {
                return new ObservableCollection<ConstraintChangelogViewModel>(); // temp disabled while refactoring constraintview
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
        private ConstraintModel Con;
        private StructureViewModel _SS;
        public StructureViewModel SS
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
                    {
                        Con.ChangePrimaryStructure(_model.GetProtocolStructure(_SS.Id));
                        UpdateConstraint();
                    }
                }
            }
        }
        public int Id
        {
            get { return Con.ID; }
        }
        public bool Pinned { get; set; } = false;
        public bool DragSelected { get; set; } = false;
        public bool isCreated { get { return Con.isCreated; } }
        public bool ThresholdIsInterpolated
        {
            get
            {
                return !string.IsNullOrEmpty(Con.ThresholdDataPath);
            }
        }
        public wpfcolor Color { get; set; } = wpfcolors.PapayaWhip;
        public ComponentModel Component
        {
            get { return ComponentModels.FirstOrDefault(x => x.Id == Con.ComponentID); }
            set
            {
                if (Con.ComponentID != value.Id)
                {
                    _model.ChangeConstraintComponent(Con.ID, value.Id);
                    UpdateConstraint();
                }
            }
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
        public wpfbrush ConstraintValueColor // Bound by SquintRowHeaderStyle to change and to ConstraintValue textbox when editing
        {
            get
            {
                if (Con.isCreated)
                    return new wpfbrush(wpfcolors.ForestGreen);
                if (Con.isModified(nameof(Con.ConstraintValue)))
                    return new wpfbrush(wpfcolors.DarkOrange);
                else
                    return new wpfbrush(wpfcolors.Black);
            }
        }
        public double ReferenceValue
        {
            get { return Con.ReferenceValue; }
            //set
            //{
            //    if (Con.ReferenceValue != value)
            //    {
            //        bool CalcAfter = false;
            //        if (!Con.isValid())
            //            CalcAfter = true;
            //        Con.ReferenceValue = value;
            //        if (CalcAfter)
            //            UpdateConstraint(); // only do this if the constraint was previously invalid
            //        RaisePropertyChangedEvent(nameof(ReferenceValueColor));
            //    }
            //}
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
        public ConstraintTypes ConstraintType
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
                }
                UpdateConstraint();

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
                if (Con.isCreated)
                    return ChangeStatus.New.Display();
                if (Con.isModified())
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
            ConstraintResultViewModel CRV = Con.GetResult(AssessmentId);
            if (CRV != null)
                return CRV.Result;
            else
                return "";
        }
        public List<ConstraintResultStatusCodes> GetStatusCodes(int AssessmentId)
        {
            ConstraintResultViewModel CRV = Con.GetResult(AssessmentId);
            if (CRV != null)
                return CRV.StatusCodes;
            else
                return null;
        }
        public bool isResultCalculating(int AssessmentId)
        {
            ConstraintResultViewModel CRV = Con.GetResult(AssessmentId);
            if (CRV != null)
                return CRV.isCalculating;
            else
                return false;
        }
        public ReferenceThresholdTypes GetViolationStatus(int AssessmentId)
        {
            ConstraintResultViewModel CRV = Con.GetResult(AssessmentId);
            if (CRV != null)
                return CRV.ThresholdStatus;
            else
                return ReferenceThresholdTypes.Unset;
        }
        private void SetComboBoxes()
        {
            ConstraintUnitTypes.Clear();
            List<ConstraintUnits> ConUnitList = new List<ConstraintUnits>();
            List<ConstraintUnits> RefUnitList = new List<ConstraintUnits>();
            if (ConstraintType == Squint.ConstraintTypes.M)
            {
                ConUnitList.Add(ConstraintUnits.Unset);
                RefUnitList.Add(ConstraintUnits.cGy);
                RefUnitList.Add(ConstraintUnits.Percent);
                RaisePropertyChangedEvent("ReferenceUnits");
                RaisePropertyChangedEvent("ReferenceTypes");
                return;
            }
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
            if (Con.ConstraintType == Squint.ConstraintTypes.CI)
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
        public ConstraintViewModel(ConstraintModel ConIn, StructureViewModel SSin, SquintModel model)
        {
            // must unsubscribe when cleared!

            _model = model;
            Con = ConIn;
            SS = SSin;
            ConstraintTypes.Clear();
            foreach (ConstraintTypes T in Enum.GetValues(typeof(ConstraintTypes)).Cast<ConstraintTypes>())
            {
                if (T == Squint.ConstraintTypes.Unset)
                    continue;
                ConstraintTypes.Add(T);
            }
            SetComboBoxes();
            ComponentModels.Clear();
            foreach (var CN in _model.CurrentProtocol.Components)
            {
                ComponentModels.Add(CN);
            }
            Con.ConstraintEvaluating += OnConstraintEvaluating;
            Con.ConstraintEvaluated += OnConstraintEvaluated;
            Con.PropertyChanged += OnConstraintPropertyChanged;
            SSin.PropertyChanged += OnStructurePropertyChanged;
            RaisePropertyChangedEvent("ChangeStatusString");
            RaisePropertyChangedEvent(nameof(ConstraintValueColor));
            RaisePropertyChangedEvent(nameof(ReferenceValueColor));
        }
        public void Unsubscribe()
        {
            Con.ConstraintEvaluating -= OnConstraintEvaluating;
            Con.ConstraintEvaluated -= OnConstraintEvaluated;
            Con.PropertyChanged -= OnConstraintPropertyChanged;
            SS.PropertyChanged -= OnStructurePropertyChanged;
        }

        // Event Handlers
        private async void UpdateConstraint()
        {
            await Task.Run(() => _model.UpdateConstraint(Con.ID));
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
                case nameof(ConstraintModel.DisplayOrder):
                    RaisePropertyChangedEvent("DisplayOrder");
                    break;
                default:
                    RefreshFlag ^= true; // necessary for when these properties are updated internally to Squin, i.e. by an A/B ratio change
                    RaisePropertyChangedEvent(nameof(ConstraintViewModel.ShortConstraintDefinition));
                    RaisePropertyChangedEvent(nameof(ConstraintViewModel.ChangeStatusString));
                    RaisePropertyChangedEvent(nameof(ConstraintViewModel.ConstraintType));
                    RaisePropertyChangedEvent(nameof(ConstraintViewModel.ConstraintValue));
                    RaisePropertyChangedEvent(nameof(ConstraintViewModel.ConstraintUnit));
                    RaisePropertyChangedEvent(nameof(ConstraintViewModel.ReferenceValue));
                    RaisePropertyChangedEvent(nameof(ConstraintViewModel.ReferenceType));
                    RaisePropertyChangedEvent(nameof(ConstraintViewModel.ReferenceUnit));
                    RaisePropertyChangedEvent(nameof(ConstraintViewModel.MajorViolation));
                    RaisePropertyChangedEvent(nameof(ConstraintViewModel.MinorViolation));
                    RaisePropertyChangedEvent(nameof(ConstraintViewModel.StopValue));
                    break;
            }
        }
        private void OnStructurePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(StructureViewModel.AssignedStructureId):
                    RefreshFlag ^= true;
                    break;
            }
        }
        public ObservableCollection<ConstraintTypes> ConstraintTypes { get; private set; } = new ObservableCollection<ConstraintTypes>() { Squint.ConstraintTypes.Unset };
        public ObservableCollection<ConstraintUnits> ConstraintUnitTypes { get; private set; } = new ObservableCollection<ConstraintUnits>() { ConstraintUnits.Unset };
        public ObservableCollection<ReferenceTypes> AvailableReferenceTypes { get; private set; } = new ObservableCollection<ReferenceTypes>() { ReferenceTypes.Unset };
        public ObservableCollection<ConstraintUnits> AvailableReferenceUnitTypes { get; private set; } = new ObservableCollection<ConstraintUnits>() { ConstraintUnits.Unset };
        public ObservableCollection<ComponentModel> ComponentModels { get; private set; } = new ObservableCollection<ComponentModel>() { };

        public ICommand SetV95Command
        {
            get { return new DelegateCommand(SetV95); }
        }
        private void SetV95(object param = null)
        {
            ConstraintType = Squint.ConstraintTypes.V;
            Con.ReferenceType = ReferenceTypes.Lower;
            Con.MajorViolation = 98;
            Con.ReferenceScale = UnitScale.Relative;
            Con.ConstraintScale = UnitScale.Relative;
            Con.ConstraintValue = 95;
            UpdateConstraint();
        }

        public ICommand SetD0035Command
        {
            get { return new DelegateCommand(SetD0035); }
        }
        private void SetD0035(object param = null)
        {
            ConstraintType = Squint.ConstraintTypes.D;
            Con.ReferenceType = ReferenceTypes.Upper;
            Con.MajorViolation = 0;
            Con.ReferenceScale = UnitScale.Absolute;
            Con.ConstraintScale = UnitScale.Absolute;
            Con.ConstraintValue = 0.035;
            var test = AvailableReferenceUnitTypes;
            UpdateConstraint();
        }

        public ICommand SetDMinCommand
        {
            get { return new DelegateCommand(SetDMin); }
        }
        private void SetDMin(object param = null)
        {
            ConstraintType = Squint.ConstraintTypes.D;
            Con.ReferenceType = ReferenceTypes.Lower;
            Con.MajorViolation = 95;
            Con.ReferenceScale = UnitScale.Relative;
            Con.ConstraintScale = UnitScale.Relative;
            Con.ConstraintValue = 100;
            UpdateConstraint();
        }

        public ICommand SetMeanDoseCommand
        {
            get { return new DelegateCommand(SetMeanDose); }
        }
        private void SetMeanDose(object param = null)
        {
            ConstraintType = Squint.ConstraintTypes.M;
            Con.ReferenceType = ReferenceTypes.Upper;
            Con.MajorViolation = 0;
            Con.ReferenceScale = UnitScale.Absolute;
            Con.ConstraintScale = UnitScale.Relative;
            Con.ConstraintValue = 0;
            UpdateConstraint();
        }

    }
}
