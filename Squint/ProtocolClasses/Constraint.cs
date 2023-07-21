using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.ComponentModel;
using VMS.TPS.Common.Model.Types;
using System.Runtime.CompilerServices;
using System.Data;
using Squint.Extensions;
using System.Runtime.InteropServices;
using System.Windows.Markup;

namespace Squint
{

    public partial class Constraint : INotifyPropertyChanged
    {
        public class ConstraintReferenceValues
        {
            public int NumFractions;
            public int PrimaryStructureId;
            public int ReferenceStructureId;
            public double ReferenceValue;
            public ReferenceTypes ReferenceType;
            public ConstraintTypes ConstraintType;
            public UnitScale ConstraintScale;
            public UnitScale ReferenceScale;
            public double ConstraintValue;
        }
        //Classes
        //Required notification class
        public virtual event PropertyChangedEventHandler PropertyChanged;

        private TrackedValue<ProtocolStructure> _primaryStructure = new TrackedValue<ProtocolStructure>(null);
        private TrackedValue<ProtocolStructure> _referenceStructure = new TrackedValue<ProtocolStructure>(null);
        private TrackedValue<Component> _parentComponent;
        private ProtocolStructure primaryStructure { get { return _primaryStructure.Value; } }
        public Component parentComponent
        {
            get { return _parentComponent.Value; }
            set 
            { 
                _parentComponent.Value = value;
                NotifyPropertyChanged(nameof(parentComponent));
            }
        }
        private ProtocolStructure referenceStructure
        {
            get
            {
                return _referenceStructure.Value;
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            switch (propertyName)
            {
                case "ID":
                    break;
                case "ExceptionType":
                    break;
                default:
                    var test = ID;
                    //UpdateOnEvaluatedPropertyChange(propertyName);
                    break;
            }
        }
        public Constraint(Component parentComponent_in, ProtocolStructure primaryStructure_in, ProtocolStructure referenceStructure_in, DbConstraint DbO)
        {
            ID = DbO.ID;
            DisplayOrder = new TrackedValue<int>(DbO.DisplayOrder);

            _parentComponent = new TrackedValue<Component>(parentComponent_in);
            _referenceStructure = new TrackedValue<ProtocolStructure>(referenceStructure_in);
            _primaryStructure = new TrackedValue<ProtocolStructure>(primaryStructure_in);

            var DbOS = DbO as DbSessionConstraint;
            if (DbOS != null) // set current session values
            {

                _ConstraintType = new TrackedValue<ConstraintTypes>((ConstraintTypes)DbOS.OriginalConstraintType);
                if (DbOS.ConstraintType != DbOS.OriginalConstraintType)
                    ConstraintType = (ConstraintTypes)DbOS.ConstraintType;
                // Initialize thresholds
                if (!string.IsNullOrEmpty(DbOS.ThresholdDataPath))
                    _ThresholdCalculator = new InterpolatedThreshold(DbOS.ThresholdDataPath);
                else
                    _ThresholdCalculator = new FixedThreshold(DbOS.MajorViolation, DbOS.MinorViolation, DbOS.Stop);
                _ReferenceType = new TrackedValue<ReferenceTypes>((ReferenceTypes)DbOS.OriginalReferenceType);
                if (DbOS.ReferenceType != DbOS.OriginalReferenceType)
                    ReferenceType = (ReferenceTypes)DbOS.ReferenceType;
                _ReferenceScale = new TrackedValue<UnitScale>((UnitScale)DbOS.OriginalReferenceScale);
                if (DbOS.ReferenceScale != DbOS.OriginalReferenceScale)
                    ReferenceScale = (UnitScale)DbOS.ReferenceScale;
                _ConstraintValue = new TrackedValue<double>(DbOS.OriginalConstraintValue);
                if (DbOS.ConstraintValue != DbOS.OriginalConstraintValue)
                    ConstraintValue = DbO.ConstraintValue;
                _NumFractions = new TrackedValue<int>(DbOS.OriginalNumFractions);
                if (DbOS.Fractions != DbOS.OriginalNumFractions)
                    NumFractions = DbOS.Fractions;
                _ConstraintScale = new TrackedValue<UnitScale>((UnitScale)DbOS.OriginalConstraintScale);
                if (DbOS.ConstraintScale != DbOS.OriginalConstraintScale)
                    ConstraintScale = (UnitScale)DbOS.ConstraintScale;
            }
            else
            {
                _ConstraintType = new TrackedValue<ConstraintTypes>((ConstraintTypes)DbO.ConstraintType);
                //_ReferenceValue = new TrackedValueWithReferences<double>(DbO.ReferenceValue);
                _ReferenceType = new TrackedValue<ReferenceTypes>((ReferenceTypes)DbO.ReferenceType);
                _ReferenceScale = new TrackedValue<UnitScale>((UnitScale)DbO.ReferenceScale);
                _ConstraintValue = new TrackedValue<double>(DbO.ConstraintValue);
                _NumFractions = new TrackedValue<int>(DbO.Fractions);
                _ConstraintScale = new TrackedValue<UnitScale>((UnitScale)DbO.ConstraintScale);
                DisplayOrder = new TrackedValue<int>(DbO.DisplayOrder);
                // Look up thresholds
                if (!string.IsNullOrEmpty(DbO.ThresholdDataPath))
                    _ThresholdCalculator = new InterpolatedThreshold(DbO.ThresholdDataPath);
                else
                    _ThresholdCalculator = new FixedThreshold(DbO.MajorViolation, DbO.MinorViolation, DbO.Stop);
            }

            primaryStructure.PropertyChanged += OnProtocolStructureChanged;
            parentComponent.ReferenceDoseChanged += OnComponentDoseChanging;
            parentComponent.ReferenceFractionsChanged += OnComponentFractionsChanging;

        }
        public Constraint(Component parentComponent_in, ProtocolStructure primaryStructure_in, ProtocolStructure referenceStructure_in, ConstraintTypes TypeCode)
        {
            // This method creates a new ad-hoc constraint
            isCreated = true;
            ID = IDGenerator.GetUniqueId();
            _parentComponent = new TrackedValue<Component>(parentComponent_in);
            _referenceStructure = new TrackedValue<ProtocolStructure>(referenceStructure_in);
            _primaryStructure = new TrackedValue<ProtocolStructure>(primaryStructure_in);
            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Unset);
            _ConstraintValue = new TrackedValue<double>(0);
            _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Unset);
            _ConstraintType = new TrackedValue<ConstraintTypes>(TypeCode);
            //_ReferenceValue = new TrackedValueWithReferences<double>(0);
            _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Unset);
            DisplayOrder = new TrackedValue<int>(parentComponent.Constraints.Count() + 1);

            primaryStructure.PropertyChanged += OnProtocolStructureChanged;
            if (referenceStructure != null)
                referenceStructure_in.PropertyChanged += ReferenceStructure_in_PropertyChanged;
            parentComponent.ReferenceDoseChanged += OnComponentDoseChanging;
            parentComponent.ReferenceFractionsChanged += OnComponentFractionsChanging;

            // Initialize thresholds
            if (ConstraintType == ConstraintTypes.CI)
            {
                _ThresholdCalculator = (new InterpolatedThreshold(""));
            }
            else
            {
                _ThresholdCalculator = (new FixedThreshold());

            }
        }

        private void ReferenceStructure_in_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public Constraint(Constraint Con)
        {
            isCreated = true;
            ID = IDGenerator.GetUniqueId();
            _parentComponent = new TrackedValue<Component>(Con.parentComponent);
            _referenceStructure = new TrackedValue<ProtocolStructure>(Con.referenceStructure);
            _primaryStructure = new TrackedValue<ProtocolStructure>(Con.primaryStructure);
            ConstraintScale = Con.ConstraintScale;
            ConstraintValue = Con.ConstraintValue;
            ConstraintType = Con.ConstraintType;
            ReferenceType = Con.ReferenceType;
            ReferenceScale = Con.ReferenceScale;
            DisplayOrder = new TrackedValue<int>(Con.DisplayOrder.Value + 1);

            primaryStructure.PropertyChanged += OnProtocolStructureChanged;
            if (referenceStructure != null)
                referenceStructure.PropertyChanged += ReferenceStructure_in_PropertyChanged;
            parentComponent.ReferenceDoseChanged += OnComponentDoseChanging;
            parentComponent.ReferenceFractionsChanged += OnComponentFractionsChanging;
        }

        public Constraint(Component parentComponent_in, ProtocolStructure primaryStructure_in, VMS_XML.MeasureItem MI)
        {
            // This constructor creates a constraint from an Eclipse Clinical Protocol MeasureItem
            ID = IDGenerator.GetUniqueId();
            isCreated = true;

            _parentComponent = new TrackedValue<Component>(parentComponent_in);
            _primaryStructure = new TrackedValue<ProtocolStructure>(primaryStructure_in);

            primaryStructure.PropertyChanged += OnProtocolStructureChanged;
            if (referenceStructure != null)
                referenceStructure.PropertyChanged += ReferenceStructure_in_PropertyChanged;
            parentComponent.ReferenceDoseChanged += OnComponentDoseChanging;
            parentComponent.ReferenceFractionsChanged += OnComponentFractionsChanging;

            DisplayOrder = new TrackedValue<int>(parentComponent.Constraints.Count + 1);
            if (MI.Modifier.Equals("5", StringComparison.OrdinalIgnoreCase))
                return; // this is a reference point which is ignored by Squint
                        //_DbO = DataCache.SquintDb.Context.DbSessionConstraints.Create();
                        //DataCache.SquintDb.Context.DbConstraints.Add(_DbO);
                        //_DbO.ID = IDGenerator.GetUniqueId();
                        //_DbO.SecondaryStructureID = 1; // temp.. need to set this for Eclipse CI
                        //_DbO.PrimaryStructureId = ProtocolStructures.Values.Where(x => x.EclipseStructureName == Item.ID).SingleOrDefault().ID;
                        // Reference TYpe
            switch (Convert.ToInt32(MI.Modifier))
            {
                case 0: // lower constraint
                    _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Lower);
                    //if (V > (double)MI.Value)
                    //    ConstraintMet = true;
                    //ConstraintString = string.Format("{0} (Id={5}): V{1:0.#} [{2}] > {3:0.#} [{4}]", MI.ID, MI.TypeSpecifier, ConUnit, MI.Value, RefUnit, S.Id);
                    break;
                case 1: // upper constraint
                    _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Upper);
                    //if (V < (double)MI.Value)
                    //    ConstraintMet = true;
                    //ConstraintString = string.Format("{0} (Id={5}): V{1:0.#} [{2}] < {3:0.#} [{4}]", MI.ID, MI.TypeSpecifier, ConUnit, MI.Value, RefUnit, S.Id);
                    break;
                case 2: // equality
                    _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Unset);
                    //if (Math.Abs(V - (double)MI.Value) < 1E-5)
                    //    ConstraintMet = true;
                    //ConstraintString = string.Format("{0} (Id={5}): V{1:0.#} [{2}] = {3:0.#} [{4}]", MI.ID, MI.TypeSpecifier, ConUnit, MI.Value, RefUnit, S.Id);
                    break;
            }
            if ((bool)MI.ReportDQPValueInAbsoluteUnits)
                _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
            else
                _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Relative);
            if (MI != null)
            {
                switch (Convert.ToInt32(MI.Type)) // type of DVH constraint
                {
                    case 0: // Conformity Index less than
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.CI);
                        _ConstraintValue = new TrackedValue<double>(100);
                        _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Upper);
                        //_ReferenceValue = new TrackedValue<double>((double)MI.Value);
                        _ThresholdCalculator = (new FixedThreshold(ReferenceValue));
                        break;
                    case 2: // this is a Vx[%] type constraint
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.V);
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative); // ConUnit = "%";
                        if (MI.Value == null)
                        {
                            _ThresholdCalculator = (new FixedThreshold());
                        }
                        else
                        {
                            var val = (double)MI.Value;
                            if ((bool)MI.ReportDQPValueInAbsoluteUnits)
                                val = val / 1000; // convert to cc
                            _ThresholdCalculator = (new FixedThreshold(val));
                        }

                        if (MI.TypeSpecifier == null)
                            _ConstraintValue = new TrackedValue<double>(double.NaN);
                        else
                            _ConstraintValue = new TrackedValue<double>((double)MI.TypeSpecifier);
                        _ThresholdCalculator = (new FixedThreshold(ReferenceValue));
                        break;
                    case 3: // this is a Vx[Gy] type constraint
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.V);
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Absolute);

                        if (MI.Value == null)
                            _ThresholdCalculator = (new FixedThreshold());
                        else
                        {
                            var val = (double)MI.Value;
                            if ((bool)MI.ReportDQPValueInAbsoluteUnits)
                                val = val / 1000; // convert to cc
                            _ThresholdCalculator = (new FixedThreshold(val));
                        }
                        if (MI.TypeSpecifier == null)
                            _ConstraintValue = new TrackedValue<double>(double.NaN);
                        else
                            _ConstraintValue = new TrackedValue<double>((double)MI.TypeSpecifier * 100);
                        break;
                    case 4: // is is  Dx% type constraint
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.D);
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                        if (MI.Value == null)
                            _ThresholdCalculator = (new FixedThreshold());
                        else
                        {
                            var val = (double)MI.Value;
                            if ((bool)MI.ReportDQPValueInAbsoluteUnits)
                                val = val * 100; // convert to cGy
                            _ThresholdCalculator = (new FixedThreshold(val));
                        }
                        if (MI.TypeSpecifier == null)
                            _ConstraintValue = new TrackedValue<double>(double.NaN);
                        else
                            _ConstraintValue = new TrackedValue<double>((double)MI.TypeSpecifier);
                        break;
                    case 5: // this is a Dx cc type constraint
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.D);
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        if (MI.Value == null)
                            _ThresholdCalculator = (new FixedThreshold());
                        else
                        {
                            var val = (double)MI.Value;
                            if ((bool)MI.ReportDQPValueInAbsoluteUnits)
                                val = val * 100; // convert to cGy
                            _ThresholdCalculator = (new FixedThreshold(val));
                        }
                        if (MI.TypeSpecifier == null)
                            _ConstraintValue = new TrackedValue<double>(double.NaN);
                        else
                            _ConstraintValue = new TrackedValue<double>((double)MI.TypeSpecifier);
                        _ThresholdCalculator = (new FixedThreshold(ReferenceValue));
                        break;
                    default:
                        {
                            MessageBox.Show(string.Format("MI.Type = {0} not handled", MI.Type));
                            _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.Unset);
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Unset);
                            _ThresholdCalculator = (new FixedThreshold(null));
                            _ConstraintValue = new TrackedValue<double>(double.NaN);
                            break;
                        }
                }
            }
        }
        public Constraint(Component parentComponent_in, ProtocolStructure primaryStructure_in, VMS_XML.Item PI)
        {
            // This constructor creates a constraint from an Eclipse Clinical Protocol MeasureItem
            ID = IDGenerator.GetUniqueId();
            isCreated = true;
            _parentComponent = new TrackedValue<Component>(parentComponent_in);
            _primaryStructure = new TrackedValue<ProtocolStructure>(primaryStructure_in);
            DisplayOrder = new TrackedValue<int>(parentComponent.Constraints.Count + 1);

            primaryStructure.PropertyChanged += OnProtocolStructureChanged;
            if (referenceStructure != null)
                referenceStructure.PropertyChanged += ReferenceStructure_in_PropertyChanged;
            parentComponent.ReferenceDoseChanged += OnComponentDoseChanging;
            parentComponent.ReferenceFractionsChanged += OnComponentFractionsChanging;


            if (PI != null) // is RxItem
            {
                switch (Convert.ToInt32(PI.Modifier))
                {
                    case 0: // Dx% > y cGy
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.D);
                        _ConstraintValue = new TrackedValue<double>((double)PI.Parameter);
                        _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Lower);
                        //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                        _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                        break;
                    case 1: // Dx% < y cGy
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.D);
                        _ConstraintValue = new TrackedValue<double>((double)PI.Parameter);
                        _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Upper);
                        //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                        _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                        break;
                    case 3: // Dx = y cGy
                        MessageBox.Show(string.Format(@"Squint does not support [Maximum dose is], replacing with [Maximum dose less than]", PI.Modifier));
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.D);
                        _ConstraintValue = new TrackedValue<double>((double)0);
                        _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Upper);
                        //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                        _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                        break;
                    case 4: // Dx = y cGy
                        MessageBox.Show(string.Format(@"Squint does not support [Minimum dose is], replacing with [Minimum dose greater than]", PI.Modifier));
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.D);
                        _ConstraintValue = new TrackedValue<double>((double)100);
                        _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Lower);
                        //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                        _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                        break;
                    case 5: // refence point
                        MessageBox.Show(string.Format("Reference point objective for structure {0} is ignored", PI.ID));
                        break;
                    case 7: // Mean is more than cGy
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.M);
                        //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                        _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                        _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Lower);
                        _ConstraintValue = new TrackedValue<double>(0);
                        break;
                    case 8: // Mean is less than cGy
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.M);
                        //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                        _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                        _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Upper);
                        _ConstraintValue = new TrackedValue<double>(0);
                        break;
                    case 9: // Minimum is more than cGy
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.D);
                        //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                        _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                        _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Lower);
                        _ConstraintValue = new TrackedValue<double>(100);
                        break;
                    case 10: // Maximum is less than cGy
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.D);
                        //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                        _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                        _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                        _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Upper);
                        _ConstraintValue = new TrackedValue<double>(0);
                        break;
                    default:
                        MessageBox.Show(string.Format("PI.Type = {0} not handled", PI.Modifier));
                        _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.Unset);
                        _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Unset);
                        _ThresholdCalculator = (new FixedThreshold(null));
                        _ConstraintValue = new TrackedValue<double>(double.NaN);
                        break;
                }
            }
            else
                MessageBox.Show(@"Error: Eclipse prescription item in protocol was null!");
        }
        //Events
        public static readonly object Lock = new object();
        public event EventHandler<int> ConstraintFlaggedForDeletion;
        public event EventHandler<int> ConstraintEvaluating; //argument is the assessment ID
        public event EventHandler<int> ConstraintEvaluated;
        public event EventHandler AssociatedThresholdChanged;
        //Properties
        private Dictionary<int, ConstraintResult> ConstraintResults = new Dictionary<int, ConstraintResult>();
        public List<ConstraintChangelog> GetChangeLogs()
        {
            if (ID > 0)
                return DbController.GetConstraintChangelogs(ID);
            else
                return new List<ConstraintChangelog>() { new ConstraintChangelog(this) };
        }
        public bool isCreated { get; private set; } = false;
        public bool isModified(string propertyname = "")
        {
            if (propertyname == "")
                return _isModified();
            switch (propertyname)
            {
                case nameof(NumFractions):
                    return _NumFractions.IsChanged;
                case nameof(ComponentID):
                    return _parentComponent.IsChanged;
                case nameof(PrimaryStructureId):
                    return _primaryStructure.IsChanged;
                case nameof(ReferenceStructureId):
                    return _referenceStructure.IsChanged;
                case nameof(ReferenceValue):
                    return _ThresholdCalculator.IsChanged;
                case nameof(ReferenceType):
                    return _ReferenceType.IsChanged;
                case nameof(ConstraintType):
                    return _ConstraintType.IsChanged;
                case nameof(ConstraintScale):
                    return _ConstraintScale.IsChanged;
                case nameof(ReferenceScale):
                    return _ReferenceScale.IsChanged;
                case nameof(ConstraintValue):
                    return _ConstraintValue.IsChanged;
                default:
                    MessageBox.Show("Unrecognized property passed to Constraint.isModified");
                    return true;
            }
        }
        private TrackedValue<int> _NumFractions = new TrackedValue<int>(0);
        public int NumFractions { get { return _NumFractions.Value; } private set { _NumFractions.Value = value; } }
        public int ComponentID { get { return parentComponent.ID; } }
        public string ComponentName { get { return parentComponent.ComponentName; } }
        public double ReferenceValue
        {
            get
            {
                if (_ThresholdCalculator == null)
                    return double.NaN;
                else
                    return _ThresholdCalculator.ReferenceValue;
            }
        }
        //private bool _wasSessionModified { get; set; } = false;  // this is true if the loaded constraint was modified in its session
        private TrackedValue<ReferenceTypes> _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Unset);
        public ReferenceTypes ReferenceType { get { return _ReferenceType.Value; } set { _ReferenceType.Value = value; } }
        private TrackedValue<ConstraintTypes> _ConstraintType = new TrackedValue<ConstraintTypes>(ConstraintTypes.Unset);
        public ConstraintTypes ConstraintType { get { return _ConstraintType.Value; } set { _ConstraintType.Value = value; } }
        private TrackedValue<UnitScale> _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Unset);
        public UnitScale ConstraintScale { get { return _ConstraintScale.Value; } set { _ConstraintScale.Value = value; } }
        private TrackedValue<UnitScale> _ReferenceScale { get; set; } = new TrackedValue<UnitScale>(UnitScale.Unset);// the constraint unit 
        public UnitScale ReferenceScale { get { return _ReferenceScale.Value; } set { _ReferenceScale.Value = value; } }
        private TrackedValue<double> _ConstraintValue = new TrackedValue<double>(double.NaN);
        public double ConstraintValue
        {
            get { return _ConstraintValue.Value; }
            set { _ConstraintValue.Value = value; }
        }
        public TrackedValue<int> DisplayOrder { get; set; }

        public int PrimaryStructureId
        {
            get { return primaryStructure.ID; }
        }
        public int ReferenceStructureId
        {
            get
            {
                if (referenceStructure != null)
                    return referenceStructure.ID;
                else
                    return 1;
            }
        }
        public bool isValid()
        {
            if (ConstraintType == ConstraintTypes.Unset)
                return false;
            else
            {
                if (PrimaryStructureId != 1
                       && (ConstraintScale != UnitScale.Unset || ConstraintType == ConstraintTypes.M)
                       && (ConstraintType != ConstraintTypes.CI || ReferenceStructureId != 1)
                       && ConstraintValue >= 0
                       && (ReferenceValue >= 0 || (double.IsNaN(ReferenceValue) && _ThresholdCalculator is InterpolatedThreshold))
                       && ReferenceType != ReferenceTypes.Unset
                       && ComponentID != 1
                       && ReferenceScale != UnitScale.Unset)
                    return true;
                else
                    return false;
            }
        }
        public int ID { get; private set; }
        public string ProtocolStructureName
        {
            get
            {
                return primaryStructure.ProtocolStructureName;
            }
        }
        public string PrimaryStructureName
        {
            get
            {

                var E = primaryStructure;
                if (E != null)
                    return E.AssignedStructureId;
                else
                    return "Cannot find referenced structure";
            }
        }
        public string ReferenceStructureName
        {
            get
            {
                if (referenceStructure != null)
                    return referenceStructure.AssignedStructureId;
                else
                    return "Cannot find referenced structure";
            }
        }
        //DVH constraint type properties and methods
        public bool isConstraintValueDose()
        {
            if (ConstraintType == ConstraintTypes.V
                || ConstraintType == ConstraintTypes.CV || ConstraintType == ConstraintTypes.CI)
                return true;
            else
                return false;
        }
        public bool isReferenceValueDose()
        {
            if (ConstraintType == ConstraintTypes.D
                || ConstraintType == ConstraintTypes.M)
                return true;
            else
                return false;
        }
        public DvhTypes GetDvhConstraintType()
        {
            switch (ConstraintType)
            {
                case ConstraintTypes.V:
                    return DvhTypes.V;
                case ConstraintTypes.D:
                    return DvhTypes.D;
                case ConstraintTypes.CV:
                    return DvhTypes.CV;
                case ConstraintTypes.M:
                    return DvhTypes.M;
                default:
                    return DvhTypes.Unset;
            }
        }
        public ConstraintUnits GetConstraintUnit()
        {
            if (ConstraintScale == UnitScale.Unset)
                return ConstraintUnits.Unset;
            switch (ConstraintType)
            {
                case ConstraintTypes.CV:
                    {
                        if (ConstraintScale == UnitScale.Relative)
                            return ConstraintUnits.Percent;
                        else
                            return ConstraintUnits.cGy;
                    }
                case ConstraintTypes.V:
                    {
                        if (ConstraintScale == UnitScale.Relative)
                            return ConstraintUnits.Percent;
                        else
                            return ConstraintUnits.cGy;
                    }
                case ConstraintTypes.D:
                    {
                        if (ConstraintScale == UnitScale.Relative)
                            return ConstraintUnits.Percent;
                        else
                            return ConstraintUnits.cc;
                    }
                case ConstraintTypes.M:
                    return ConstraintUnits.Unset;
                case ConstraintTypes.Unset:
                    return ConstraintUnits.Unset;
                case ConstraintTypes.CI:
                    {
                        if (ConstraintScale == UnitScale.Relative)
                            return ConstraintUnits.Percent;
                        else
                            return ConstraintUnits.cGy;
                    }
                default:
                    MessageBox.Show("Error in GetConstraintUnit display");
                    return ConstraintUnits.Unset;
            }
        }
        public ConstraintUnits GetReferenceUnit()
        {
            if (ReferenceScale == UnitScale.Unset)
                return ConstraintUnits.Unset;
            switch (ConstraintType)
            {
                case ConstraintTypes.CV:
                    {
                        if (ReferenceScale == UnitScale.Relative)
                            return ConstraintUnits.Percent;
                        else
                            return ConstraintUnits.cc;
                    }
                case ConstraintTypes.V:
                    {
                        if (ReferenceScale == UnitScale.Relative)
                            return ConstraintUnits.Percent;
                        else
                            return ConstraintUnits.cc;
                    }
                case ConstraintTypes.D:
                    {
                        if (ReferenceScale == UnitScale.Relative)
                            return ConstraintUnits.Percent;
                        else
                            return ConstraintUnits.cGy;
                    }
                case ConstraintTypes.M:
                    {
                        if (ReferenceScale == UnitScale.Relative)
                            return ConstraintUnits.Percent;
                        else
                            return ConstraintUnits.cGy;
                    }
                case ConstraintTypes.Unset:
                    return ConstraintUnits.Unset;
                case ConstraintTypes.CI:
                    {
                        if (ReferenceScale == UnitScale.Relative)
                            return ConstraintUnits.Percent;
                        else
                            return ConstraintUnits.Multiple;
                    }
                default:
                    MessageBox.Show("Error in GetReferenceUnit display");
                    return ConstraintUnits.Unset;
            }
        }
        public string GetConstraintStringWithFractions()
        {
            return string.Format("{0} in {1} fractions", GetConstraintString(), parentComponent.NumFractions);
        }
        public string GetConstraintString(bool GetParentValues = false)
        {
            if (PrimaryStructureName == "")
            {
                return string.Format("{0}: {1}", ProtocolStructureName, GetConstraintStringNoStructure(GetParentValues));
            }
            return string.Format(@"{0} (""{1}"") : {2}", ProtocolStructureName, PrimaryStructureName, GetConstraintStringNoStructure(GetParentValues));
        }
        public string GetConstraintStringNoStructure(bool GetParentValues = false)
        {
            //if (GetParentValues == true)
            //    ReturnReferenceValues = true;
            //else
            //    ReturnReferenceValues = false;
            if (PrimaryStructureId == 1)
                return "Not assigned to structure";
            if (!isValid())
                return "(Invalid Definition)";
            string ReturnString = null;
            //string ConString = null;
            switch (ConstraintType)
            {
                case ConstraintTypes.CV:
                    ReturnString = string.Format("CV{0:0.###} [{1}] {2} {3:0.###} [{4}]", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());
                    break;
                case ConstraintTypes.V:
                    if (ConstraintScale == UnitScale.Relative)
                        ReturnString = string.Format("V{0:0.###} [{1}] {2} {3:0.###} [{4}]", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());
                    else
                        ReturnString = string.Format("V{0:0.#} [{1}] {2} {3:0.###} [{4}]", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());
                    break;
                case ConstraintTypes.D:
                    //Exception for min and max dose
                    if (ConstraintValue < 1E-5)
                    { // max dose
                        if (ReferenceScale == UnitScale.Relative)
                            ReturnString = string.Format("Max Dose {0} {1:0.###} [{2}]", ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());
                        else
                            ReturnString = string.Format("Max Dose {0} {1:0} [{2}]", ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());

                        break;
                    }
                    else if (ConstraintValue > (100 - 1E-5) && ConstraintScale == UnitScale.Relative)
                    {
                        if (ReferenceScale == UnitScale.Relative)
                            ReturnString = string.Format("Min Dose {0} {1:0.###} [{2}]", ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());
                        else
                            ReturnString = string.Format("Min Dose {0} {1:0} [{2}]", ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());
                        break;
                    }
                    else
                    {
                        if (ReferenceScale == UnitScale.Relative)
                            ReturnString = string.Format("D{0:0.###} [{1}] {2} {3:0.#} [{4}]", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());
                        else
                            ReturnString = string.Format("D{0:0.###} [{1}] {2} {3:0} [{4}]", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());
                        break;
                    }
                case ConstraintTypes.M:
                    ReturnString = string.Format("Mean Dose {0} {1:0} [{2}]", ReferenceType.Display(), ReferenceValue, GetReferenceUnit());
                    break;
                case ConstraintTypes.CI:
                    if (ConstraintValue == 0) // Display exception if it's the whole volume for readability
                        ReturnString = string.Format("Total volume is {0} {1} % of {2} volume", ReferenceType.Display(), ReferenceValue, ReferenceStructureName);
                    else
                    {
                        if (ReferenceScale == UnitScale.Relative)
                            ReturnString = string.Format("V{0}[{1}] {2} {3}{4} of the {5} volume", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display(), ReferenceStructureName);
                        else if (ReferenceScale == UnitScale.Absolute)
                            ReturnString = string.Format("V{0}[{1}] {2} {3} {4} the {5} volume", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display(), ReferenceStructureName);
                        break;
                    }
                    break;
                case ConstraintTypes.Unset:
                    ReturnString = "Constraint definition incomplete";
                    break;
                default:
                    throw new Exception("Error in formatting constraint string");
            }
            return ReturnString;
        }
        public double GetConstraintAbsDose()
        {

            if (isConstraintValueDose())
            {
                if (ConstraintScale == UnitScale.Relative)
                    return ConstraintValue * parentComponent.TotalDose / 100;
                else
                    return ConstraintValue;
            }
            else
            {
                if (isReferenceValueDose())
                {
                    if (ReferenceScale == UnitScale.Relative)
                    {
                        return ReferenceValue * parentComponent.TotalDose / 100;
                    }
                    else
                    {
                        return ReferenceValue;
                    }
                }
                else
                    return double.NaN;
            }
        }
        public bool LowerConstraintDoseScalesWithComponent = true;
        private async void ApplyBEDScaling(int prevFractions)
        {
            Component SC = parentComponent;
            ProtocolStructure E = primaryStructure;
            //var StructureLabel = Ctr.GetStructureLabel(E.StructureLabelID);
            double abRatio;
            if (E.AlphaBetaRatioOverride == null)
                abRatio = (await Ctr.GetStructureLabel(E.StructureLabelID)).AlphaBetaRatio;
            else
                abRatio = (double)E.AlphaBetaRatioOverride;
            if (abRatio > 0)
            {
                if (isConstraintValueDose())
                {
                    if (ConstraintScale == UnitScale.Relative)
                    {
                        ConstraintValue = DoseFunctions.BED(prevFractions, SC.NumFractions, _ConstraintValue.Value / 100 * SC.TotalDose, abRatio) * 100 / SC.TotalDose;
                    }
                    else
                    {
                        ConstraintValue = Math.Round(DoseFunctions.BED(prevFractions, SC.NumFractions, _ConstraintValue.Value, abRatio) / 100) * 100;
                    }
                }
                else if (isReferenceValueDose())
                {
                    if (ReferenceScale == UnitScale.Relative)
                    {
                        if (_ThresholdCalculator.MajorViolation != null && _ThresholdCalculator.MajorViolation != null)
                            _ThresholdCalculator.MajorViolation = Math.Round(DoseFunctions.BED(prevFractions, SC.NumFractions, (double)_ThresholdCalculator.MajorViolation / 100 * SC.TotalDose, abRatio)) * 100 / SC.TotalDose;
                        if (_ThresholdCalculator.MinorViolation != null && _ThresholdCalculator.MinorViolation != null)
                            _ThresholdCalculator.MinorViolation = Math.Round(DoseFunctions.BED(prevFractions, SC.NumFractions, (double)_ThresholdCalculator.MinorViolation / 100 * SC.TotalDose, abRatio)) * 100 / SC.TotalDose;
                        if (_ThresholdCalculator.Stop != null && _ThresholdCalculator.Stop != null)
                            _ThresholdCalculator.Stop = Math.Round(DoseFunctions.BED(prevFractions, SC.NumFractions, (double)_ThresholdCalculator.Stop / 100 * SC.TotalDose, abRatio)) * 100 / SC.TotalDose;
                    }
                    else
                    {
                        if (_ThresholdCalculator.MajorViolation != null && _ThresholdCalculator.MajorViolation != null)
                        {
                            _ThresholdCalculator.MajorViolation = Math.Round(DoseFunctions.BED(prevFractions, SC.NumFractions, (double)_ThresholdCalculator.MajorViolation, abRatio) / 100) * 100;
                        }
                        if (_ThresholdCalculator.MinorViolation != null && _ThresholdCalculator.MinorViolation != null)
                            _ThresholdCalculator.MinorViolation = Math.Round(DoseFunctions.BED(prevFractions, SC.NumFractions, (double)_ThresholdCalculator.MinorViolation, abRatio) / 100) * 100;
                        if (_ThresholdCalculator.Stop != null && _ThresholdCalculator.Stop != null)
                            _ThresholdCalculator.Stop = Math.Round(DoseFunctions.BED(prevFractions, SC.NumFractions, (double)_ThresholdCalculator.Stop, abRatio) / 100) * 100;
                    }
                }
            }
            NumFractions = SC.NumFractions;
        }
        //Evaluation routines
        //private List<Assessment> RegisteredAssessments = new List<Assessment>();
        private bool _isModified()
        {
            return _NumFractions.IsChanged
            || _parentComponent.IsChanged
            || _primaryStructure.IsChanged
            || _referenceStructure.IsChanged
            || _ThresholdCalculator.IsChanged
            || _ReferenceType.IsChanged
            || _ConstraintType.IsChanged
            || _ConstraintScale.IsChanged
            || _ReferenceScale.IsChanged
            || _ConstraintValue.IsChanged;
        }
    

        public void ChangePrimaryStructure(ProtocolStructure primaryStructure_in)
        {
            primaryStructure.PropertyChanged -= OnProtocolStructureChanged;
            primaryStructure_in.PropertyChanged += OnProtocolStructureChanged;
            _primaryStructure.Value = primaryStructure_in;
            NotifyPropertyChanged(nameof(PrimaryStructureId));
        }
        public void ChangeComponent(Component newParentComponent)
        {
            parentComponent.ReferenceDoseChanged -= OnComponentDoseChanging;
            parentComponent.ReferenceFractionsChanged -= OnComponentFractionsChanging;
            _parentComponent.Value = newParentComponent;
            newParentComponent.ReferenceDoseChanged += OnComponentDoseChanging;
            newParentComponent.ReferenceFractionsChanged += OnComponentFractionsChanging;
        }
        public void OnComponentUnlinked(object sender, int ComponentID)
        {
            Assessment SA = (sender as Assessment);
            if (ConstraintResults.ContainsKey(SA.ID))
                ConstraintResults[SA.ID].AddStatusCode(ConstraintResultStatusCodes.NotLinked);
        }

        public async Task EvaluateConstraint(PlanAssociation PA)
        {
            try
            {
                ConstraintResult CR = null;
                if (ConstraintResults.ContainsKey(PA.AssessmentID))
                    CR = ConstraintResults[PA.AssessmentID];
                if (CR == null)
                {
                    CR = new ConstraintResult(PA.AssessmentID, this);
                    ConstraintResults.Add(PA.AssessmentID, CR);
                }
                CR.isCalculating = true;
                CR.ClearStatusCodes();
                if (!isValid())
                {
                    CR.AddStatusCode(ConstraintResultStatusCodes.ConstraintUndefined);
                    CR.isCalculating = false;
                    ConstraintEvaluated?.Raise(new ConstraintResultViewModel(CR), PA.AssessmentID); // this notifies the view class, no need to raise to UI
                    return;
                }
               
                if (PA.LoadWarning)
                {
                    CR.AddStatusCode(ConstraintResultStatusCodes.NotLinked);
                    CR.isCalculating = false;
                    ConstraintEvaluated?.Raise(new ConstraintResultViewModel(CR), PA.AssessmentID); // this notifies the view class, no need to raise to UI

                    return;
                }
                List<ComponentStatusCodes> ComponentStatus = PA.GetErrorCodes();
                if (!ComponentStatus.Contains(ComponentStatusCodes.Evaluable))
                { // this constraint is not evaluable because of an error int he component link
                    CR.AddStatusCode(ConstraintResultStatusCodes.ErrorUnspecified);
                    CR.isCalculating = false;
                    ConstraintEvaluated?.Raise(new ConstraintResultViewModel(CR), PA.AssessmentID); // this notifies the view class, no need to raise to UI

                    return;
                }
                string targetId = primaryStructure.AssignedStructureId;
                DoseValue doseQuery; // = new DoseValue(Dvh_Val, DoseValue.DoseUnit.Percent); // set a default
                double rawresult = double.NaN;
                Component comp = parentComponent;
                VolumePresentation volPresentationQuery;
                VolumePresentation volPresentationReturn;
                DoseValuePresentation dosePresentationReturn;
                AsyncPlan p = PA.LinkedPlan;
                if (p.Dose == null)
                {
                    CR.ResultValue = Double.NaN;
                    CR.AddStatusCode(ConstraintResultStatusCodes.NoDoseDistribution);
                    //UpdateProgress();
                    CR.isCalculating = false;
                    ConstraintEvaluated?.Raise(new ConstraintResultViewModel(CR), PA.AssessmentID); // this notifies the view class, no need to raise to UI
                    return;
                }
                bool targetExists = p.StructureIds.Contains(targetId);
                if (!targetExists)
                {
                    CR.AddStatusCode(ConstraintResultStatusCodes.StructureNotFound);
                    //UpdateProgress();
                    CR.isCalculating = false;
                    ConstraintEvaluated?.Raise(new ConstraintResultViewModel(CR), PA.AssessmentID); // this notifies the view class, no need to raise to UI
                    return;
                }
                else
                    CR.LinkedLabelName = DbController.GetLabelByCode(p.Structures[targetId].Code);
                // Constraint is evaluable
                ConstraintEvaluating?.Raise(new ConstraintResultViewModel(CR), PA.AssessmentID); // this notifies the view class, no need to raise to UI
                string structureCode = await Ctr.GetStructureCode(primaryStructure.StructureLabelID);
                if (p.Structures[targetId].Code != structureCode)
                {
                    CR.AddStatusCode(ConstraintResultStatusCodes.LabelMismatch);
                }
                double binWidth = 0.01;
                if (ConstraintScale == UnitScale.Absolute)
                {
                    doseQuery = new DoseValue(ConstraintValue, DoseValue.DoseUnit.cGy);
                    volPresentationQuery = VolumePresentation.AbsoluteCm3;
                }
                else
                {
                    doseQuery = new DoseValue(ConstraintValue/100 * parentComponent.TotalDose, DoseValue.DoseUnit.cGy);
                    volPresentationQuery = VolumePresentation.Relative;
                }
                if (ReferenceScale == UnitScale.Absolute)
                {
                    dosePresentationReturn = DoseValuePresentation.Absolute;
                    volPresentationReturn = VolumePresentation.AbsoluteCm3;
                    dosePresentationReturn = DoseValuePresentation.Absolute;
                }
                else
                {
                    dosePresentationReturn = DoseValuePresentation.Relative;
                    volPresentationReturn = VolumePresentation.Relative;
                    dosePresentationReturn = DoseValuePresentation.Relative;
                }
                bool SumAndRefIsRelative = false;
                if (p.ComponentType == ComponentTypes.Sum)
                {
                    if (isConstraintValueDose() && ConstraintScale == UnitScale.Relative)
                    {
                        if (parentComponent.TotalDose > 0)
                        {
                            doseQuery = new DoseValue(ConstraintValue / 100 * parentComponent.TotalDose, DoseValue.DoseUnit.cGy);
                            CR.AddStatusCode(ConstraintResultStatusCodes.RelativeDoseForSum);
                        }
                        else
                        {
                            CR.ResultValue = Double.NaN;
                            CR.AddStatusCode(ConstraintResultStatusCodes.RefDoseInValid);
                            //UpdateProgress();
                            CR.isCalculating = false;
                            ConstraintEvaluated?.Invoke(new ConstraintResultViewModel(CR), PA.AssessmentID);
                            return;
                        }
                    }
                    else if ((isReferenceValueDose() && ReferenceScale == UnitScale.Relative))
                    {
                        SumAndRefIsRelative = true;
                        dosePresentationReturn = DoseValuePresentation.Absolute;
                        if (parentComponent.TotalDose > 0)
                        {
                            doseQuery = new DoseValue(ReferenceValue / 100 * parentComponent.TotalDose, DoseValue.DoseUnit.cGy);
                            CR.AddStatusCode(ConstraintResultStatusCodes.RelativeDoseForSum);
                        }
                        else
                        {
                            CR.ResultValue = Double.NaN;
                            CR.AddStatusCode(ConstraintResultStatusCodes.RefDoseInValid);
                            //UpdateProgress();
                            CR.isCalculating = false;
                            ConstraintEvaluated?.Invoke(new ConstraintResultViewModel(CR), PA.AssessmentID);
                            return;
                        }
                    }
                }
                switch (ConstraintType)
                {
                    case ConstraintTypes.CV: // critical volume
                        rawresult = p.Structures[targetId].Volume - await p.GetVolumeAtDose(targetId, doseQuery, VolumePresentation.AbsoluteCm3);
                        break;
                    case ConstraintTypes.V:
                        rawresult = await p.GetVolumeAtDose(targetId, doseQuery, volPresentationReturn);
                        break;
                    case ConstraintTypes.D:
                        {
                            rawresult = await p.GetDoseAtVolume(targetId, ConstraintValue, volPresentationQuery, dosePresentationReturn);
                            if (SumAndRefIsRelative)
                            {
                                rawresult = rawresult / parentComponent.TotalDose * 100;
                            }
                            break;
                        }
                    case ConstraintTypes.M:
                        rawresult = await p.GetMeanDose(targetId, volPresentationReturn, dosePresentationReturn, binWidth);
                        if (SumAndRefIsRelative)
                        {
                            rawresult = rawresult / parentComponent.TotalDose * 100;
                        }
                        break;
                    case ConstraintTypes.CI:
                        if (referenceStructure != null)
                        {
                            if (p.StructureIds.Contains(referenceStructure.AssignedStructureId))
                            {
                                double dose = GetConstraintAbsDose();
                                doseQuery = new DoseValue(dose, DoseValue.DoseUnit.cGy);
                                if (p.Structures[targetId].Volume < 0.035)
                                    rawresult = 0;
                                else
                                    rawresult = await (p.GetVolumeAtDose(targetId, doseQuery, VolumePresentation.AbsoluteCm3)) / p.Structures[referenceStructure.AssignedStructureId].Volume;
                                if (ReferenceScale == UnitScale.Relative)
                                {
                                    rawresult = rawresult * 100;
                                }
                            }
                            else
                            {
                                CR.AddStatusCode(ConstraintResultStatusCodes.StructureNotFound);
                                //UpdateProgress();
                                CR.isCalculating = false;
                                ConstraintEvaluated?.Invoke(new ConstraintResultViewModel(CR), PA.AssessmentID);
                                return;
                            }
                        }
                        else
                        {
                            CR.AddStatusCode(ConstraintResultStatusCodes.StructureNotFound);
                            //UpdateProgress();
                            CR.isCalculating = false;
                            ConstraintEvaluated?.Invoke(new ConstraintResultViewModel(CR), PA.AssessmentID);
                            return;
                        }
                        break;
                    default:
                        CR.AddStatusCode(ConstraintResultStatusCodes.ErrorUnspecified);
                        //UpdateProgress();
                        CR.isCalculating = false;
                        ConstraintEvaluated?.Invoke(new ConstraintResultViewModel(CR), PA.AssessmentID);
                        break;
                }
                if (!double.IsNaN(rawresult) && rawresult > -1) // <0 is flag for no structure in mean dose calculation
                {
                    CR.ResultValue = rawresult;
                    CR.ThresholdStatus = ThresholdStatus(CR);
                }
                else
                {
                    CR.ResultValue = rawresult;
                    CR.AddStatusCode(ConstraintResultStatusCodes.StructureEmpty); // temp
                }
                //UpdateProgress();
                CR.isCalculating = false;
                ConstraintEvaluated?.Invoke(new ConstraintResultViewModel(CR), PA.AssessmentID);
                return;
            }

            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error in EvaluateConstraint \r\n{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
            }
        }
        private ReferenceThresholdTypes ThresholdStatus(ConstraintResult CR)
        {
            if (CR.StatusCodes.Where(x => x != ConstraintResultStatusCodes.LabelMismatch 
                && x != ConstraintResultStatusCodes.RelativeDoseForSum 
                && x != ConstraintResultStatusCodes.Loaded).Count() > 0)
                return ReferenceThresholdTypes.Unset;
            if (ReferenceType == ReferenceTypes.Upper)
            {
                if (_ThresholdCalculator.Stop != null)
                    if (CR.ResultValue <= Stop)
                        return ReferenceThresholdTypes.Stop;
                if (_ThresholdCalculator.MajorViolation != null)
                    if (CR.ResultValue > MajorViolation)
                        return ReferenceThresholdTypes.MajorViolation;
                if (_ThresholdCalculator.MinorViolation != null)
                    if (CR.ResultValue > MinorViolation)
                        return ReferenceThresholdTypes.MinorViolation;
            }
            else
            {
                if (_ThresholdCalculator.Stop != null)
                    if (CR.ResultValue >= Stop)
                        return ReferenceThresholdTypes.Stop;
                if (_ThresholdCalculator.MajorViolation != null)
                    if (CR.ResultValue < MajorViolation)
                        return ReferenceThresholdTypes.MajorViolation;
                if (_ThresholdCalculator.MinorViolation != null)
                    if (CR.ResultValue < MinorViolation)
                        return ReferenceThresholdTypes.MinorViolation;
            }
            return ReferenceThresholdTypes.None;
        }
        public ConstraintResultViewModel GetResult(int AssessmentID)
        {
            if (ConstraintResults.ContainsKey(AssessmentID))
                return new ConstraintResultViewModel(ConstraintResults[AssessmentID]);
            else
                return null;
        }
        public List<ConstraintResultViewModel> GetAllResults()
        {
            List<ConstraintResultViewModel> returnList = new List<ConstraintResultViewModel>();
            foreach (ConstraintResult CR in ConstraintResults.Values)
            {
                returnList.Add(new ConstraintResultViewModel(CR));
            }
            return returnList;
        }
        public string GetResultString(Assessment SA)
        {
            if (ConstraintResults.ContainsKey(SA.ID))
                return ConstraintResults[SA.ID].ResultString;
            else
                return "";
        }
        public double GetResultValue(Assessment SA)
        {
            if (ConstraintResults.ContainsKey(SA.ID))
                return ConstraintResults[SA.ID].ResultValue;
            else
                return double.NaN;
        }
        public List<ConstraintResultStatusCodes> GetResultStatusCodes(ConstraintResult CR)
        {
            return CR.StatusCodes;
        }
        public void RefreshResultThresholdStatus()
        {
            foreach (ConstraintResult CR in ConstraintResults.Values)
            {
                CR.ThresholdStatus = ThresholdStatus(CR);
            }
        }
        public void AcceptChanges()
        {
            _ConstraintScale.AcceptChanges();
            _ConstraintType.AcceptChanges();
            _ConstraintValue.AcceptChanges();
            _ReferenceScale.AcceptChanges();
            _ReferenceType.AcceptChanges();
            //_ReferenceValue.AcceptChanges();
        }

        private IReferenceThreshold _ThresholdCalculator = new FixedThreshold(null);

        public string ThresholdDataPath
        {
            get
            {
                if (_ThresholdCalculator != null)
                    return _ThresholdCalculator.DataPath;
                else
                    return "";
            }
        }
        public double? Stop
        {
            get { return _ThresholdCalculator.Stop; }
            set
            {
                if (ConstraintType != ConstraintTypes.CI)
                {
                    if (value != _ThresholdCalculator.Stop)
                    {
                        if (double.IsNaN((double)value))
                            _ThresholdCalculator.Stop = null;
                        else
                        {
                            if (ReferenceType == ReferenceTypes.Lower)
                            {
                                if (double.IsNaN((double)value) || (value > MinorViolation || MinorViolation == null) && (value > MajorViolation || MajorViolation == null))
                                    _ThresholdCalculator.Stop = value;
                            }
                            else if ((value < MinorViolation || MinorViolation == null) && (value < MajorViolation || MajorViolation == null))
                                _ThresholdCalculator.Stop = value;
                        }
                    }
                }
            }
        }
        public void UpdateThresholds(double? newParam)
        {
            if (_ThresholdCalculator != null && newParam != null)
            {
                if (_ThresholdCalculator.ReferenceThresholdCalculationType == ReferenceThresholdCalculationTypes.Interpolated)
                {
                    ((InterpolatedThreshold)_ThresholdCalculator).Xi = (double)newParam;
                    NotifyPropertyChanged(nameof(ReferenceValue));
                    NotifyPropertyChanged(nameof(MajorViolation));
                    NotifyPropertyChanged(nameof(MinorViolation));
                    NotifyPropertyChanged(nameof(Stop));
                }
            }
        }



        public double? MinorViolation
        {
            get { return _ThresholdCalculator.MinorViolation; }
            set
            {
                if (ConstraintType != ConstraintTypes.CI)
                {
                    if (value != _ThresholdCalculator.MinorViolation)
                        if (double.IsNaN((double)value))
                            _ThresholdCalculator.MinorViolation = null;
                        else
                        {
                            if (ReferenceType == ReferenceTypes.Lower)
                            {
                                if ((value < Stop || Stop == null) && (value > MajorViolation || MajorViolation == null))
                                    _ThresholdCalculator.MinorViolation = value;
                            }
                            else if ((value > Stop || Stop == null) && (value < MajorViolation || MajorViolation == null))
                                _ThresholdCalculator.MinorViolation = value;
                        }
                }
            }
        }

        public double? MajorViolation
        {
            get { return _ThresholdCalculator.MajorViolation; }
            set
            {
                if (ConstraintType != ConstraintTypes.CI)
                {
                    if (value != _ThresholdCalculator.MajorViolation)
                        if (ReferenceType == ReferenceTypes.Lower)
                        {
                            if ((value < Stop || Stop == null) && (value < MinorViolation || MinorViolation == null))
                                _ThresholdCalculator.MajorViolation = value;
                        }
                        else if ((value > Stop || Stop == null) && (value > MinorViolation || MinorViolation == null))
                            _ThresholdCalculator.MajorViolation = value;
                }
            }
        }

        public bool ToRetire { get; private set; } = false;
        public void FlagForDeletion()
        {
            ToRetire = true;
            ConstraintFlaggedForDeletion?.Invoke(this, ID);
        }
        private void OnProtocolStructureChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ES":
                    {
                        if ((sender as ProtocolStructure).ID == PrimaryStructureId)
                            NotifyPropertyChanged("PrimaryStructureName");
                        else
                            NotifyPropertyChanged("SecondaryStructureName");
                        break;
                    }
            }

        }
        public void OnComponentDoseChanging(object sender, EventArgs e)
        {
            double oldDose = (double)sender;
            if (isConstraintValueDose())
            {
                if (ConstraintScale == UnitScale.Relative)
                {
                    NotifyPropertyChanged("ConstraintValue");
                }
                else if (ConstraintScale == UnitScale.Absolute && ReferenceType == ReferenceTypes.Lower && LowerConstraintDoseScalesWithComponent)
                {
                    if (_ConstraintScale.Value == UnitScale.Absolute)
                        ConstraintValue = _ConstraintValue.Value * parentComponent.TotalDose / oldDose;
                    else if (_ConstraintScale.Value == UnitScale.Relative)
                    {
                        ConstraintValue = _ConstraintValue.Value/100 * parentComponent.TotalDose;
                    }
                    NotifyPropertyChanged("ConstraintValue");
                }
            }
            else if (isReferenceValueDose())
            {
                if (ReferenceScale == UnitScale.Relative)
                {
                    NotifyPropertyChanged("ReferenceValue");
                }
                else if (ReferenceScale == UnitScale.Absolute && ReferenceType == ReferenceTypes.Lower && LowerConstraintDoseScalesWithComponent)
                {
                    if (_ReferenceScale.Value == UnitScale.Absolute)
                    {
                        _ThresholdCalculator.MajorViolation = _ThresholdCalculator.MajorViolation * parentComponent.TotalDose / oldDose;
                        _ThresholdCalculator.MinorViolation = _ThresholdCalculator.MinorViolation * parentComponent.TotalDose / oldDose;
                        _ThresholdCalculator.Stop = _ThresholdCalculator.Stop * parentComponent.TotalDose / oldDose;
                    }
                    else
                    {
                        _ThresholdCalculator.MajorViolation = _ThresholdCalculator.MajorViolation/100 * parentComponent.TotalDose;
                        _ThresholdCalculator.MinorViolation = _ThresholdCalculator.MinorViolation/100 * parentComponent.TotalDose;
                        _ThresholdCalculator.Stop = _ThresholdCalculator.Stop/100 * parentComponent.TotalDose;
                    }
                    NotifyPropertyChanged(nameof(IReferenceThreshold.MajorViolation));
                    NotifyPropertyChanged(nameof(IReferenceThreshold.MinorViolation));
                    NotifyPropertyChanged(nameof(IReferenceThreshold.Stop));
                    NotifyPropertyChanged(nameof(IReferenceThreshold.ReferenceValue));
                }
            }
        }
        private void OnComponentFractionsChanging(object sender, EventArgs e)
        {
            if (ReferenceType == ReferenceTypes.Upper)
            {
                ApplyBEDScaling((int)sender);
            }
            else
            {
                NumFractions = parentComponent.NumFractions;
            }
            //UpdateProgress();
        }
        private void OnConstraintThresholdChanged(object sender, EventArgs e)
        {
            AssociatedThresholdChanged?.Invoke(this, EventArgs.Empty);
        }

        public ConstraintReferenceValues GetConstraintReferenceValues()
        {
            int referenceStructureReferenceId = 1;
            if (referenceStructure != null)
                referenceStructureReferenceId = _referenceStructure.ReferenceValue.ID;
            return new ConstraintReferenceValues()
            {
                NumFractions = _NumFractions.ReferenceValue,
                PrimaryStructureId = _primaryStructure.ReferenceValue.ID,
                ReferenceStructureId = referenceStructureReferenceId,
                ReferenceType = _ReferenceType.ReferenceValue,
                ConstraintType = _ConstraintType.ReferenceValue,
                ConstraintScale = _ConstraintScale.ReferenceValue,
                ReferenceScale = _ReferenceScale.ReferenceValue,
                ConstraintValue = _ConstraintValue.ReferenceValue
            };
        }

    }

}

