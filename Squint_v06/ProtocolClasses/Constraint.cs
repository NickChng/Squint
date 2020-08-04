using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.ComponentModel;
using VMS.TPS.Common.Model.Types;
using System.Runtime.CompilerServices;
using System.Data;
using SquintScript.Extensions;
using System.Runtime.InteropServices;

namespace SquintScript
{
    public static partial class Ctr
    {
        public class Constraint : INotifyPropertyChanged
        {
            public class ConstraintReferenceValues
            {
                public int NumFractions;
                public int PrimaryStructureID;
                public int ReferenceStructureId;
                public double ReferenceValue;
                public ReferenceTypes ReferenceType;
                public ConstraintTypeCodes ConstraintType;
                public UnitScale ConstraintScale;
                public UnitScale ReferenceScale;
                public double ConstraintValue;
            }
            //Classes
            //Required notification class
            public virtual event PropertyChangedEventHandler PropertyChanged;
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
            public Constraint(DbConstraint DbO)
            {
                ID = DbO.ID;
                _ComponentID = new TrackedValue<int>(DbO.ComponentID);
                DisplayOrder = new TrackedValue<int>(DbO.DisplayOrder);

                var DbOS = DbO as DbSessionConstraint;
                if (DbOS != null) // set current session values
                {
                    _PrimaryStructureID = new TrackedValue<int>(DbOS.OriginalPrimaryStructureID);
                    PrimaryStructureID = DbOS.PrimaryStructureID;
                    _PrimaryStructureID.AcceptChanges(); // Don't flag new ID as a change;
                    _ReferenceStructureId = new TrackedValue<int>(DbOS.OriginalSecondaryStructureID);
                    ReferenceStructureId = DbOS.ReferenceStructureId;
                    _ReferenceStructureId.AcceptChanges();
                    _ConstraintType = new TrackedValue<ConstraintTypeCodes>((ConstraintTypeCodes)DbOS.OriginalConstraintType);
                    if (DbOS.ConstraintType != DbOS.OriginalConstraintType)
                        ConstraintType = (ConstraintTypeCodes)DbOS.ConstraintType;
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
                    _PrimaryStructureID = new TrackedValue<int>(DbO.PrimaryStructureID);
                    _ReferenceStructureId = new TrackedValue<int>(DbO.ReferenceStructureId);
                    _ConstraintType = new TrackedValue<ConstraintTypeCodes>((ConstraintTypeCodes)DbO.ConstraintType);
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

                var ProtocolStructure = DataCache.GetProtocolStructure(DbO.PrimaryStructureID);
                if (ProtocolStructure == null)
                {
                    throw new Exception("Referenced ProtocolStructure is not present in cache (Constraint constructor)");
                }
                else
                {
                    ProtocolStructure.PropertyChanged += OnProtocolStructureChanged;
                }
                var SC = DataCache.GetComponent(DbO.ComponentID);
                if (SC == null)
                {
                    throw new Exception("Referenced ProtocolStructure is not present in cache (Constraint constructor)");
                }
                else
                {

                    SC.ReferenceDoseChanged += OnComponentDoseChanging;
                    SC.ReferenceFractionsChanged += OnComponentFractionsChanging;
                }
            }
            public Constraint(ConstraintTypeCodes TypeCode, int ComponentID_in, int StructureId = 1, int LabelID_in = 0)
            {
                // This method creates a new ad-hoc constraint
                isCreated = true;
                ID = Ctr.IDGenerator();
                Component SC = DataCache.GetComponent(ComponentID_in);
                _ComponentID = new TrackedValue<int>(ComponentID_in); // necessary to create listeners
                _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Unset);
                _ConstraintValue = new TrackedValue<double>(0);
                _NumFractions = new TrackedValue<int>(SC.NumFractions);
                _PrimaryStructureID = new TrackedValue<int>(StructureId);
                _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Unset);
                _ConstraintType = new TrackedValue<ConstraintTypeCodes>(TypeCode);
                //_ReferenceValue = new TrackedValueWithReferences<double>(0);
                _ReferenceStructureId = new TrackedValue<int>(1);
                _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Unset);
                DisplayOrder = DataCache.GetAllConstraints().Select(x => x.DisplayOrder).Max();
                var ProtocolStructure = DataCache.GetProtocolStructure(PrimaryStructureID);
                if (ProtocolStructure == null)
                {
                    throw new Exception("Referenced ProtocolStructure is not present in cache (Constraint constructor)");
                }
                else
                {
                    ProtocolStructure.PropertyChanged += OnProtocolStructureChanged;
                }
                if (SC == null)
                {
                    throw new Exception("Referenced ProtocolStructure is not present in cache (Constraint constructor)");
                }
                else
                {

                    SC.ReferenceDoseChanged += OnComponentDoseChanging;
                    SC.ReferenceFractionsChanged += OnComponentFractionsChanging;
                }
                // Initialize thresholds
                if (ConstraintType == ConstraintTypeCodes.CI)
                {
                    _ThresholdCalculator = (new InterpolatedThreshold(""));
                }
                else
                {
                    _ThresholdCalculator = (new FixedThreshold());

                }
            }
            public Constraint(Constraint Con)
            {
                isCreated = true;
                ID = Ctr.IDGenerator();
                Component SC = DataCache.GetComponent(Con.ComponentID);
                _ComponentID = new TrackedValue<int>(SC.ID); // necessary to create listeners
                ConstraintScale = Con.ConstraintScale;
                ConstraintValue = Con.ConstraintValue;
                _NumFractions = new TrackedValue<int>(SC.NumFractions);
                PrimaryStructureID = Con.PrimaryStructureID;
                ReferenceStructureId = Con.ReferenceStructureId;
                ConstraintType = Con.ConstraintType;
                //ReferenceValue = Con.ReferenceValue;
                ReferenceType = Con.ReferenceType;
                ReferenceScale = Con.ReferenceScale;
                DisplayOrder = new TrackedValue<int>(Con.DisplayOrder.Value + 1);

                var ProtocolStructure = DataCache.GetProtocolStructure(PrimaryStructureID);
                if (ProtocolStructure == null)
                {
                    throw new Exception("Referenced ProtocolStructure is not present in cache (Constraint constructor)");
                }
                else
                {
                    ProtocolStructure.PropertyChanged += OnProtocolStructureChanged;
                }
                if (SC == null)
                {
                    throw new Exception("Referenced ProtocolStructure is not present in cache (Constraint constructor)");
                }
                else
                {
                    SC.ReferenceDoseChanged += OnComponentDoseChanging;
                    SC.ReferenceFractionsChanged += OnComponentFractionsChanging;
                }
            }

            public Constraint(VMSTemplates.ProtocolPhasePrescriptionMeasureItem MI, int componentID, int structureId, int displayOrder)
            {
                // This constructor creates a constraint from an Eclipse Clinical Protocol MeasureItem
                ID = IDGenerator();
                isCreated = true;
                _ComponentID = new TrackedValue<int>(componentID);
                _PrimaryStructureID = new TrackedValue<int>(structureId);
                DisplayOrder = new TrackedValue<int>(displayOrder);
                if (MI.Modifier == 5)
                    return; // this is a reference point which is ignored by Squint
                            //_DbO = DataCache.SquintDb.Context.DbSessionConstraints.Create();
                            //DataCache.SquintDb.Context.DbConstraints.Add(_DbO);
                            //_DbO.ID = Ctr.IDGenerator();
                            //_DbO.SecondaryStructureID = 1; // temp.. need to set this for Eclipse CI
                            //_DbO.PrimaryStructureID = ProtocolStructures.Values.Where(x => x.EclipseStructureName == Item.ID).SingleOrDefault().ID;
                            // Reference TYpe
                switch (MI.Modifier)
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
                    switch (MI.Type) // type of DVH constraint
                    {
                        case 0: // Conformity Index less than
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.CI);
                            _ReferenceStructureId = new TrackedValue<int>(structureId);
                            _ConstraintValue = new TrackedValue<double>(100);
                            _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                            _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Upper);
                            //_ReferenceValue = new TrackedValue<double>((double)MI.Value);
                            _ThresholdCalculator = (new FixedThreshold(ReferenceValue));
                            break;
                        case 2: // this is a Vx[%] type constraint
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.V);
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative); // ConUnit = "%";
                            if (MI.Value == null)
                            {
                                _ThresholdCalculator = (new FixedThreshold());
                            }
                            else
                            {
                                var val = (double)MI.Value;
                                if (MI.ReportDQPValueInAbsoluteUnits)
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
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.V);
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Absolute);

                            if (MI.Value == null)
                                _ThresholdCalculator = (new FixedThreshold());
                            else
                            {
                                var val = (double)MI.Value;
                                if (MI.ReportDQPValueInAbsoluteUnits)
                                    val = val / 1000; // convert to cc
                                _ThresholdCalculator = (new FixedThreshold(val));
                            }
                            if (MI.TypeSpecifier == null)
                                _ConstraintValue = new TrackedValue<double>(double.NaN);
                            else
                                _ConstraintValue = new TrackedValue<double>((double)MI.TypeSpecifier * 100);
                            break;
                        case 4: // is is  Dx% type constraint
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.D);
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                            if (MI.Value == null)
                                _ThresholdCalculator = (new FixedThreshold());
                            else
                            {
                                var val = (double)MI.Value;
                                if (MI.ReportDQPValueInAbsoluteUnits)
                                    val = val * 100; // convert to cGy
                                _ThresholdCalculator = (new FixedThreshold(val));
                            }
                            if (MI.TypeSpecifier == null)
                                _ConstraintValue = new TrackedValue<double>(double.NaN);
                            else
                                _ConstraintValue = new TrackedValue<double>((double)MI.TypeSpecifier);
                            break;
                        case 5: // this is a Dx cc type constraint
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.D);
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                            if (MI.Value == null)
                                _ThresholdCalculator = (new FixedThreshold());
                            else
                            {
                                var val = (double)MI.Value;
                                if (MI.ReportDQPValueInAbsoluteUnits)
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
                                _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.Unset);
                                _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Unset);
                                _ThresholdCalculator = (new FixedThreshold(null));
                                _ConstraintValue = new TrackedValue<double>(double.NaN);
                                break;
                            }
                    }
                }
                //Subscribe
                var PS = DataCache.GetProtocolStructure(structureId);
                if (PS != null)
                    PS.PropertyChanged += OnProtocolStructureChanged;
                else
                    MessageBox.Show(@"Error: Structure not found when trying to create constraint");

                var SC = DataCache.GetComponent(componentID);
                if (SC != null)
                {
                    SC.ReferenceDoseChanged += OnComponentDoseChanging;
                    SC.ReferenceFractionsChanged += OnComponentFractionsChanging;
                    _NumFractions = new TrackedValue<int>(SC.NumFractions);
                }
                else
                    MessageBox.Show(@"Error: Component not found when trying to create constraint");
            }
            public Constraint(VMSTemplates.ProtocolPhasePrescriptionItem PI, int componentID, int structureId, int displayOrder)
            {
                // This constructor creates a constraint from an Eclipse Clinical Protocol MeasureItem
                ID = IDGenerator();
                isCreated = true;
                _ComponentID = new TrackedValue<int>(componentID);
                _PrimaryStructureID = new TrackedValue<int>(structureId);
                DisplayOrder = new TrackedValue<int>(displayOrder);
                if (PI != null) // is RxItem
                {
                    switch (PI.Modifier)
                    {
                        case 0: // Dx% > y cGy
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.D);
                            _ConstraintValue = new TrackedValue<double>((double)PI.Parameter);
                            _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                            _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Lower);
                            //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                            _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                            break;
                        case 1: // Dx% < y cGy
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.D);
                            _ConstraintValue = new TrackedValue<double>((double)PI.Parameter);
                            _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                            _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Upper);
                            //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                            _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                            break;
                        case 3: // Dx = y cGy
                            MessageBox.Show(string.Format(@"Squint does not support [Maximum dose is], replacing with [Maximum dose less than]", PI.Modifier));
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.D);
                            _ConstraintValue = new TrackedValue<double>((double)0);
                            _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                            _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Upper);
                            //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                            _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                            break;
                        case 4: // Dx = y cGy
                            MessageBox.Show(string.Format(@"Squint does not support [Minimum dose is], replacing with [Minimum dose greater than]", PI.Modifier));
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.D);
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
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.M);
                            //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                            _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                            _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                            _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Lower);
                            _ConstraintValue = new TrackedValue<double>(0);
                            break;
                        case 8: // Mean is less than cGy
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.M);
                            //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                            _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                            _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                            _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Upper);
                            _ConstraintValue = new TrackedValue<double>(0);
                            break;
                        case 9: // Minimum is more than cGy
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Relative);
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.D);
                            //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                            _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                            _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                            _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Lower);
                            _ConstraintValue = new TrackedValue<double>(100);
                            break;
                        case 10: // Maximum is less than cGy
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.D);
                            //_ReferenceValue = new TrackedValue<double>((double)PI.TotalDose * 100);
                            _ThresholdCalculator = (new FixedThreshold((double)PI.TotalDose * 100));
                            _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Absolute);
                            _ReferenceType = new TrackedValue<ReferenceTypes>(ReferenceTypes.Upper);
                            _ConstraintValue = new TrackedValue<double>(0);
                            break;
                        default:
                            MessageBox.Show(string.Format("PI.Type = {0} not handled", PI.Modifier));
                            _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.Unset);
                            _ConstraintScale = new TrackedValue<UnitScale>(UnitScale.Unset);
                            _ThresholdCalculator = (new FixedThreshold(null));
                            _ConstraintValue = new TrackedValue<double>(double.NaN);
                            break;
                    }
                }
                else
                    MessageBox.Show(@"Error: Eclipse prescription item in protocol was null!");
                //Subscribe
                var PS = DataCache.GetProtocolStructure(structureId);
                if (PS != null)
                    PS.PropertyChanged += OnProtocolStructureChanged;
                else
                    MessageBox.Show(@"Error: Structure not found when trying to create constraint");


                var SC = DataCache.GetComponent(componentID);
                if (SC != null)
                {
                    SC.ReferenceDoseChanged += OnComponentDoseChanging;
                    SC.ReferenceFractionsChanged += OnComponentFractionsChanging;
                    _NumFractions = new TrackedValue<int>(SC.NumFractions);
                }
                else
                    MessageBox.Show(@"Error: Component not found when trying to create constraint");


            }
            //Events
            public static readonly object Lock = new object();
            public event EventHandler<int> ConstraintDeleted;
            public event EventHandler<int> ConstraintEvaluating; //argument is the assessment ID
            public event EventHandler<int> ConstraintEvaluated;
            public event EventHandler AssociatedThresholdChanged;
            //Properties
            private Dictionary<int, ConstraintResult> ConstraintResults = new Dictionary<int, ConstraintResult>();
            public List<ConstraintChangelog> GetChangeLogs()
            {
                if (ID > 0)
                    return DataCache.GetConstraintChangelogs(ID);
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
                        return _ComponentID.IsChanged;
                    case nameof(PrimaryStructureID):
                        return _PrimaryStructureID.IsChanged;
                    case nameof(_ReferenceStructureId):
                        return _ReferenceStructureId.IsChanged;
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
            private TrackedValue<int> _ComponentID = new TrackedValue<int>(1);
            public int ComponentID { get { return _ComponentID.Value; } set { _ComponentID.Value = value; } }
            public string ComponentName { get { return DataCache.GetComponent(ComponentID).ComponentName; } }
            private TrackedValue<int> _PrimaryStructureID = new TrackedValue<int>(1);
            public int PrimaryStructureID { get { return _PrimaryStructureID.Value; } set { _PrimaryStructureID.Value = value; } }
            private TrackedValue<int> _ReferenceStructureId = new TrackedValue<int>(1);
            public int ReferenceStructureId { get { return _ReferenceStructureId.Value; } set { _ReferenceStructureId.Value = value; } }
            //private TrackedValue<double> _ReferenceValue = new TrackedValue<double>(double.NaN);
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
            private TrackedValue<ConstraintTypeCodes> _ConstraintType = new TrackedValue<ConstraintTypeCodes>(ConstraintTypeCodes.Unset);
            public ConstraintTypeCodes ConstraintType { get { return _ConstraintType.Value; } set { _ConstraintType.Value = value; } }
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
            public bool isValid()
            {
                if (ConstraintType == ConstraintTypeCodes.Unset)
                    return false;
                else
                {
                    if (PrimaryStructureID != 1
                           && (ConstraintScale != UnitScale.Unset || ConstraintType == ConstraintTypeCodes.M)
                           && (ConstraintType != ConstraintTypeCodes.CI || ReferenceStructureId != 1)
                           && ConstraintValue >= 0
                           && ReferenceValue >= 0
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
                    var E = DataCache.GetProtocolStructure(PrimaryStructureID);
                    if (E != null)
                        return E.ProtocolStructureName;
                    else
                        return "Cannot find referenced structure";
                }
            }
            public string PrimaryStructureName
            {
                get
                {

                    var E = DataCache.GetProtocolStructure(PrimaryStructureID);
                    if (E != null)
                        return E.AssignedStructureId;
                    else
                        return "Cannot find referenced structure";
                }
            }
            public string SecondaryStructureName
            {
                get
                {

                    var E = DataCache.GetProtocolStructure(ReferenceStructureId);
                    if (E != null)
                        return E.AssignedStructureId;
                    else
                        return "Cannot find referenced structure";
                }
            }
            //DVH constraint type properties and methods
            public bool isConstraintValueDose()
            {
                if (ConstraintType == ConstraintTypeCodes.V
                    || ConstraintType == ConstraintTypeCodes.CV || ConstraintType == ConstraintTypeCodes.CI)
                    return true;
                else
                    return false;
            }
            public bool isReferenceValueDose()
            {
                if (ConstraintType == ConstraintTypeCodes.D
                    || ConstraintType == ConstraintTypeCodes.M)
                    return true;
                else
                    return false;
            }
            public Dvh_Types GetDvhConstraintType()
            {
                switch (ConstraintType)
                {
                    case ConstraintTypeCodes.V:
                        return Dvh_Types.V;
                    case ConstraintTypeCodes.D:
                        return Dvh_Types.D;
                    case ConstraintTypeCodes.CV:
                        return Dvh_Types.CV;
                    case ConstraintTypeCodes.M:
                        return Dvh_Types.M;
                    default:
                        return Dvh_Types.Unset;
                }
            }
            public ConstraintUnits GetConstraintUnit()
            {
                if (ConstraintScale == UnitScale.Unset)
                    return ConstraintUnits.Unset;
                switch (ConstraintType)
                {
                    case ConstraintTypeCodes.CV:
                        {
                            if (ConstraintScale == UnitScale.Relative)
                                return ConstraintUnits.Percent;
                            else
                                return ConstraintUnits.cGy;
                        }
                    case ConstraintTypeCodes.V:
                        {
                            if (ConstraintScale == UnitScale.Relative)
                                return ConstraintUnits.Percent;
                            else
                                return ConstraintUnits.cGy;
                        }
                    case ConstraintTypeCodes.D:
                        {
                            if (ConstraintScale == UnitScale.Relative)
                                return ConstraintUnits.Percent;
                            else
                                return ConstraintUnits.cc;
                        }
                    case ConstraintTypeCodes.M:
                        return ConstraintUnits.Unset;
                    case ConstraintTypeCodes.Unset:
                        return ConstraintUnits.Unset;
                    case ConstraintTypeCodes.CI:
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
                    case ConstraintTypeCodes.CV:
                        {
                            if (ReferenceScale == UnitScale.Relative)
                                return ConstraintUnits.Percent;
                            else
                                return ConstraintUnits.cc;
                        }
                    case ConstraintTypeCodes.V:
                        {
                            if (ReferenceScale == UnitScale.Relative)
                                return ConstraintUnits.Percent;
                            else
                                return ConstraintUnits.cc;
                        }
                    case ConstraintTypeCodes.D:
                        {
                            if (ReferenceScale == UnitScale.Relative)
                                return ConstraintUnits.Percent;
                            else
                                return ConstraintUnits.cGy;
                        }
                    case ConstraintTypeCodes.M:
                        {
                            if (ReferenceScale == UnitScale.Relative)
                                return ConstraintUnits.Percent;
                            else
                                return ConstraintUnits.cGy;
                        }
                    case ConstraintTypeCodes.Unset:
                        return ConstraintUnits.Unset;
                    case ConstraintTypeCodes.CI:
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
            public string GetRefStructureName()
            {
                if (ReferenceStructureId != 1)
                    return DataCache.GetProtocolStructure(ReferenceStructureId).ProtocolStructureName;
                else
                    return @"(ID not set)";
            }
            public string GetConstraintStringWithFractions()
            {
                return string.Format("{0} in {1} fractions", GetConstraintString(), DataCache.GetComponent(ComponentID).NumFractions);
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
                if (PrimaryStructureID == 1)
                    return "Not assigned to structure";
                if (!isValid())
                    return "(Invalid Definition)";
                string ReturnString = null;
                //string ConString = null;
                switch (ConstraintType)
                {
                    case ConstraintTypeCodes.CV:
                        ReturnString = string.Format("CV{0:0.###} [{1}] {2} {3:0.###} [{4}]", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());
                        break;
                    case ConstraintTypeCodes.V:
                        if (ConstraintScale == UnitScale.Relative)
                            ReturnString = string.Format("V{0:0.###} [{1}] {2} {3:0.###} [{4}]", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());
                        else
                            ReturnString = string.Format("V{0:0.#} [{1}] {2} {3:0.###} [{4}]", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display());
                        break;
                    case ConstraintTypeCodes.D:
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
                    case ConstraintTypeCodes.M:
                        ReturnString = string.Format("Mean Dose {0} {1:0} [{2}]", ReferenceType.Display(), ReferenceValue, GetReferenceUnit());
                        break;
                    case ConstraintTypeCodes.CI:
                        if (ConstraintValue == 0) // Display exception if it's the whole volume for readability
                            ReturnString = string.Format("Total volume is {0} {1} % of {2} volume", ReferenceType.Display(), ReferenceValue, GetRefStructureName());
                        else
                        {
                            if (ReferenceScale == UnitScale.Relative)
                                ReturnString = string.Format("V{0}[{1}] {2} {3}{4} of the {5} volume", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display(), GetRefStructureName());
                            else if (ReferenceScale == UnitScale.Absolute)
                                ReturnString = string.Format("V{0}[{1}] {2} {3} {4} the {5} volume", ConstraintValue, GetConstraintUnit().Display(), ReferenceType.Display(), ReferenceValue, GetReferenceUnit().Display(), GetRefStructureName());
                            break;
                        }
                        break;
                    case ConstraintTypeCodes.Unset:
                        ReturnString = "Constraint definition incomplete";
                        break;
                    default:
                        throw new Exception("Error in formatting constraint string");
                }
                return ReturnString;
            }
            public double GetConstraintAbsDose()
            {
                Component SC = DataCache.GetComponent(ComponentID);
                if (isConstraintValueDose())
                {
                    if (ConstraintScale == UnitScale.Relative)
                        return ConstraintValue * SC.ReferenceDose / 100;
                    else
                        return ConstraintValue;
                }
                else
                {
                    if (isReferenceValueDose())
                    {
                        if (ReferenceScale == UnitScale.Relative)
                        {
                            return ReferenceValue * SC.ReferenceDose / 100;
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
            //public int GetEclipseMeanDoseModifier()
            //{
            //    if (ReferenceType == ReferenceTypes.Upper)
            //        return 8;
            //    else
            //        return 7;
            //}
            //public int GetEclipseModifier()
            //{
            //    switch (ReferenceType)
            //    {
            //        case ReferenceTypes.Lower: // Eclipse code for minimum 
            //            return 0;
            //        case ReferenceTypes.Upper: // Eclipse code for minimum 
            //            return 1;
            //    }
            //    return 0;
            //    //case "8": // Eclipse code for mean dose
            //}
            //public int GetEclipseType()
            //{
            //    switch (ConstraintType)
            //    {
            //        case ConstraintTypeCodes.V:
            //            {
            //                if (ConstraintScale == UnitScale.Relative)
            //                    return 2;
            //                else
            //                    return 3;
            //            }
            //        case ConstraintTypeCodes.CV:
            //            {
            //                if (ConstraintScale == UnitScale.Relative)
            //                    return 2;
            //                else
            //                    return 3;
            //            }
            //        case ConstraintTypeCodes.D:
            //            {
            //                if (ConstraintScale == UnitScale.Relative)
            //                    return 4;
            //                else
            //                    return 5;
            //            }
            //    }
            //    return 0;
            //}
            //public double GetEclipseValue()
            //{
            //    switch (GetEclipseType())
            //    {
            //        case 0:
            //            return 0;
            //        case 4:
            //            {
            //                if (ReferenceScale == UnitScale.Absolute)
            //                    return ReferenceValue / 100; // return value in Gy
            //                else
            //                    return ReferenceValue; //return value in percent
            //            }
            //        case 5:
            //            {
            //                if (ReferenceScale == UnitScale.Absolute)
            //                    return ReferenceValue / 100; // return value in Gy
            //                else
            //                    return ReferenceValue; //return value in percent
            //            }
            //        case 3:
            //            {
            //                if (ReferenceScale == UnitScale.Absolute)
            //                    return ReferenceValue * 1000; // return value in 100ths of a cc
            //                else
            //                    return ReferenceValue; //return value in percent
            //            }
            //        case 2:
            //            {
            //                if (ReferenceScale == UnitScale.Absolute)
            //                    return ReferenceValue * 100; // return value in 100ths of a cc
            //                else
            //                    return ReferenceValue; //return value in percent
            //            }
            //        default:
            //            return 0;
            //    }
            //}
            //public double GetEclipseTypeSpecifier()
            //{
            //    switch (GetEclipseType())
            //    {
            //        case 0:
            //            return 0;
            //        case 5:
            //            {
            //                return ConstraintValue;
            //            }
            //        case 3:
            //            {
            //                return ConstraintValue / 100; // return value in Gy
            //            }
            //        case 4:
            //            {
            //                return ConstraintValue;
            //            }
            //        case 2:
            //            {
            //                return ConstraintValue;
            //            }
            //        default:
            //            return 0;
            //    }
            //}
            //public bool GetEclipseValueUnits()
            //{
            //    if (ReferenceScale == UnitScale.Absolute)
            //        return true;
            //    else
            //        return false;
            //}
            public bool LowerConstraintDoseScalesWithComponent = true;
            private void ApplyBEDScaling()
            {
                Component SC = DataCache.GetComponent(ComponentID);
                ProtocolStructure E = DataCache.GetProtocolStructure(PrimaryStructureID);
                var StructureLabel = DataCache.GetStructureLabel(E.StructureLabelID);
                double abRatio = DataCache.GetStructureLabel(E.StructureLabelID).AlphaBetaRatio;
                if (abRatio > 0)
                {
                    if (isConstraintValueDose())
                    {
                        if (ConstraintScale == UnitScale.Relative)
                        {
                            ConstraintValue = DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, _ConstraintValue.ReferenceValue / 100 * SC.ReferenceDose, abRatio) * 100 / SC.ReferenceDose;
                        }
                        else
                        {
                            ConstraintValue = Math.Round(DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, _ConstraintValue.ReferenceValue, abRatio) / 10) * 10;
                        }
                    }
                    else if (isReferenceValueDose())
                    {
                        if (ReferenceScale == UnitScale.Relative)
                        {
                            if (_ThresholdCalculator.MajorViolation != null && _ThresholdCalculator.MajorViolation != null)
                                MajorViolation = Math.Round(DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, (double)_ThresholdCalculator.ReferenceMajorViolation / 100 * SC.ReferenceDose, abRatio)) * 100 / SC.ReferenceDose;
                            if (_ThresholdCalculator.MinorViolation != null && _ThresholdCalculator.MinorViolation != null)
                                MinorViolation = Math.Round(DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, (double)_ThresholdCalculator.ReferenceMinorViolation / 100 * SC.ReferenceDose, abRatio)) * 100 / SC.ReferenceDose;
                            if (_ThresholdCalculator.Stop != null && _ThresholdCalculator.Stop != null)
                                Stop = Math.Round(DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, (double)_ThresholdCalculator.ReferenceStop / 100 * SC.ReferenceDose, abRatio)) * 100 / SC.ReferenceDose;
                        }
                        else
                        {
                            if (_ThresholdCalculator.MajorViolation != null && _ThresholdCalculator.MajorViolation != null)
                                MajorViolation = Math.Round(DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, (double)_ThresholdCalculator.ReferenceMajorViolation, abRatio) / 100) * 100;
                            if (_ThresholdCalculator.MinorViolation != null && _ThresholdCalculator.MinorViolation != null)
                                MinorViolation = Math.Round(DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, (double)_ThresholdCalculator.ReferenceMinorViolation, abRatio) / 100) * 100;
                            if (_ThresholdCalculator.Stop != null && _ThresholdCalculator.Stop != null)
                                Stop = Math.Round(DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, (double)_ThresholdCalculator.ReferenceStop, abRatio) / 100) * 100;
                        }
                    }
                }
                NumFractions = SC.NumFractions;
            }
            //Evaluation routines
            private List<int> RegisteredAssessmentIDs = new List<int>();
            private bool _isModified()
            {
                return _NumFractions.IsChanged
                || _ComponentID.IsChanged
                || _PrimaryStructureID.IsChanged
                || _ReferenceStructureId.IsChanged
                || _ThresholdCalculator.IsChanged
                || _ReferenceType.IsChanged
                || _ConstraintType.IsChanged
                || _ConstraintScale.IsChanged
                || _ReferenceScale.IsChanged
                || _ConstraintValue.IsChanged;
            }
            public void RegisterAssessment(Assessment SA)
            {
                //SA.ComponentAssociationChange += OnComponentAssociationChanged;
                //SA.ComponentUnlinked += OnComponentUnlinked;
                //SA.PlanMappingChanged += OnPlanMappingChanged;
                //SA.AssessmentDeleted += OnAssessmentDeleted;
                RegisteredAssessmentIDs.Add(SA.ID);
            }
            private void OnAssessmentDeleted(object sender, int AssessmentID)
            {
                RegisteredAssessmentIDs.Remove(AssessmentID);
            }
            public void OnComponentUnlinked(object sender, int ComponentID)
            {
                Assessment SA = (sender as Assessment);
                if (ConstraintResults.ContainsKey(SA.ID))
                    ConstraintResults[SA.ID].AddStatusCode(ConstraintResultStatusCodes.NotLinked);
            }
            public async void OnComponentAssociationChanged(object sender, int ChangedComponentID)
            {
                Assessment SA = (sender as Assessment);
                if (ComponentID == ChangedComponentID)
                {
                    // if (SA.AutoCalculate)
                    //await EvaluateConstraint(SA);
                }
            }
            public async void OnPlanMappingChanged(object sender, int ChangedComponentID)
            {
                Assessment SA = (sender as Assessment);
                string.Format("{0}", ID);
                if (ComponentID == ChangedComponentID)
                {
                    // if (SA.AutoCalculate)
                    //await Task.Run(() => EvaluateConstraint(SA));
                    //await EvaluateConstraint(SA);
                }
            }
            public async Task EvaluateConstraint()
            {
                foreach (int AssessmentId in RegisteredAssessmentIDs)
                {
                    await EvaluateConstraint(AssessmentId);
                }
            }
            public async Task EvaluateConstraint(int AssessmentId)
            {
                try
                {
                    ConstraintResult CR = null;
                    var SA = DataCache.GetAssessment(AssessmentId);
                    //lock (Ctr.LockConstraint)
                    //{
                    if (ConstraintResults.ContainsKey(SA.ID))
                        CR = ConstraintResults[SA.ID];
                    if (CR == null)
                    {
                        CR = new ConstraintResult(SA.ID, ID);
                        ConstraintResults.Add(SA.ID, CR);
                    }
                    CR.isCalculating = true;
                    //}
                    //lock (CR.LockSimultaneousEvaluation)
                    //{
                    //    while (CR.isCalculating) // wait for any other threads using this constraint to finish
                    //        Thread.Sleep(100);
                    //    CR.isCalculating = true; // this locks any other threads from passing this point if the UI invokes an update
                    //}
                    CR.ClearStatusCodes();
                    if (!isValid())
                    {
                        CR.AddStatusCode(ConstraintResultStatusCodes.ConstraintUndefined);
                        CR.isCalculating = false;
                        ConstraintEvaluated?.Raise(new ConstraintResultView(CR), SA.ID); // this notifies the view class, no need to raise to UI
                        return;
                    }
                    PlanAssociation ECP = GetPlanAssociation(ComponentID, SA.ID);
                    if (ECP == null)
                    {
                        CR.AddStatusCode(ConstraintResultStatusCodes.NotLinked);
                        CR.isCalculating = false;
                        ConstraintEvaluated?.Raise(new ConstraintResultView(CR), SA.ID); // this notifies the view class, no need to raise to UI
                        return;
                    }
                    if (ECP.LoadWarning)
                    {
                        CR.AddStatusCode(ConstraintResultStatusCodes.NotLinked);
                        CR.isCalculating = false;
                        ConstraintEvaluated?.Raise(new ConstraintResultView(CR), SA.ID); // this notifies the view class, no need to raise to UI

                        return;
                    }
                    List<ComponentStatusCodes> ComponentStatus = ECP.GetErrorCodes();
                    if (!ComponentStatus.Contains(ComponentStatusCodes.Evaluable))
                    { // this constraint is not evaluable because of an error int he component link
                        CR.AddStatusCode(ConstraintResultStatusCodes.ErrorUnspecified);
                        CR.isCalculating = false;
                        ConstraintEvaluated?.Raise(new ConstraintResultView(CR), SA.ID); // this notifies the view class, no need to raise to UI

                        return;
                    }
                    string targetId = DataCache.GetProtocolStructure(PrimaryStructureID).AssignedStructureId;
                    DoseValue doseQuery; // = new DoseValue(Dvh_Val, DoseValue.DoseUnit.Percent); // set a default
                    double rawresult = double.NaN;
                    Component comp = DataCache.GetComponent(ComponentID);
                    VolumePresentation volPresentationQuery;
                    VolumePresentation volPresentationReturn;
                    DoseValuePresentation dosePresentationReturn;
                    AsyncPlan p = ECP.LinkedPlan;
                    if (p.Dose == null)
                    {
                        CR.ResultValue = Double.NaN;
                        CR.AddStatusCode(ConstraintResultStatusCodes.NoDoseDistribution);
                        //UpdateProgress();
                        CR.isCalculating = false;
                        ConstraintEvaluated?.Raise(new ConstraintResultView(CR), SA.ID); // this notifies the view class, no need to raise to UI
                        return;
                    }
                    bool targetExists = p.StructureIds.Contains(targetId);
                    if (!targetExists)
                    {
                        CR.AddStatusCode(ConstraintResultStatusCodes.StructureNotFound);
                        //UpdateProgress();
                        CR.isCalculating = false;
                        ConstraintEvaluated?.Raise(new ConstraintResultView(CR), SA.ID); // this notifies the view class, no need to raise to UI
                        return;
                    }
                    else
                        CR.LinkedLabelName = DataCache.GetLabelByCode(p.Structures[targetId].Code);
                    // Constraint is evaluable
                    ConstraintEvaluating?.Raise(new ConstraintResultView(CR), SA.ID); // this notifies the view class, no need to raise to UI
                    if ((p.Structures[targetId].Code != DataCache.GetStructureCode(DataCache.GetProtocolStructure(PrimaryStructureID).StructureLabelID)))
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
                        doseQuery = new DoseValue(ConstraintValue, DoseValue.DoseUnit.Percent);
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
                            if (DataCache.GetComponent(ComponentID).ReferenceDose > 0)
                            {
                                doseQuery = new DoseValue(ConstraintValue / 100 * DataCache.GetComponent(ComponentID).ReferenceDose, DoseValue.DoseUnit.cGy);
                                CR.AddStatusCode(ConstraintResultStatusCodes.RelativeDoseForSum);
                            }
                            else
                            {
                                CR.ResultValue = Double.NaN;
                                CR.AddStatusCode(ConstraintResultStatusCodes.RefDoseInValid);
                                //UpdateProgress();
                                CR.isCalculating = false;
                                ConstraintEvaluated?.Invoke(new ConstraintResultView(CR), SA.ID);
                                return;
                            }
                        }
                        else if ((isReferenceValueDose() && ReferenceScale == UnitScale.Relative))
                        {
                            SumAndRefIsRelative = true;
                            dosePresentationReturn = DoseValuePresentation.Absolute;
                            if (DataCache.GetComponent(ComponentID).ReferenceDose > 0)
                            {
                                doseQuery = new DoseValue(ReferenceValue / 100 * DataCache.GetComponent(ComponentID).ReferenceDose, DoseValue.DoseUnit.cGy);
                                CR.AddStatusCode(ConstraintResultStatusCodes.RelativeDoseForSum);
                            }
                            else
                            {
                                CR.ResultValue = Double.NaN;
                                CR.AddStatusCode(ConstraintResultStatusCodes.RefDoseInValid);
                                //UpdateProgress();
                                CR.isCalculating = false;
                                ConstraintEvaluated?.Invoke(new ConstraintResultView(CR), SA.ID);
                                return;
                            }
                        }
                    }
                    switch (ConstraintType)
                    {
                        case ConstraintTypeCodes.CV: // critical volume
                            rawresult = p.Structures[targetId].Volume - await p.GetVolumeAtDose(targetId, doseQuery, VolumePresentation.AbsoluteCm3);
                            break;
                        case ConstraintTypeCodes.V:
                            rawresult = await p.GetVolumeAtDose(targetId, doseQuery, volPresentationReturn);
                            break;
                        case ConstraintTypeCodes.D:
                            {
                                rawresult = await p.GetDoseAtVolume(targetId, ConstraintValue, volPresentationQuery, dosePresentationReturn);
                                if (SumAndRefIsRelative)
                                {
                                    rawresult = rawresult / DataCache.GetComponent(ComponentID).ReferenceDose * 100;
                                }
                                break;
                            }
                        case ConstraintTypeCodes.M:
                            rawresult = await p.GetMeanDose(targetId, volPresentationReturn, dosePresentationReturn, binWidth);
                            if (SumAndRefIsRelative)
                            {
                                rawresult = rawresult / DataCache.GetComponent(ComponentID).ReferenceDose * 100;
                            }
                            break;
                        case ConstraintTypeCodes.CI:
                            string refStructId = DataCache.GetProtocolStructure(ReferenceStructureId).AssignedStructureId;
                            bool refStructExists = p.StructureIds.Contains(refStructId);
                            if (refStructExists)
                            {
                                double dose = GetConstraintAbsDose();
                                doseQuery = new DoseValue(dose, DoseValue.DoseUnit.cGy);
                                if (p.Structures[targetId].Volume < 0.035)
                                    rawresult = 0;
                                else
                                    rawresult = await (p.GetVolumeAtDose(targetId, doseQuery, VolumePresentation.AbsoluteCm3)) / p.Structures[refStructId].Volume;
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
                                ConstraintEvaluated?.Invoke(new ConstraintResultView(CR), SA.ID);
                                return;
                            }
                            break;
                        default:
                            CR.AddStatusCode(ConstraintResultStatusCodes.ErrorUnspecified);
                            //UpdateProgress();
                            CR.isCalculating = false;
                            ConstraintEvaluated?.Invoke(new ConstraintResultView(CR), SA.ID);
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
                    ConstraintEvaluated?.Invoke(new ConstraintResultView(CR), SA.ID);
                    return;
                }

                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error in EvaluateConstraint \r\n{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
            private ReferenceThresholdTypes ThresholdStatus(ConstraintResult CR)
            {
                Constraint Con = DataCache.GetConstraint(CR.ConstraintID);
                if (CR.StatusCodes.Where(x => x != ConstraintResultStatusCodes.LabelMismatch).Count() > 0)
                    return ReferenceThresholdTypes.Unset;
                if (Con.ReferenceType == ReferenceTypes.Upper)
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
            public ConstraintResultView GetResult(int AssessmentID)
            {
                if (ConstraintResults.ContainsKey(AssessmentID))
                    return new ConstraintResultView(ConstraintResults[AssessmentID]);
                else
                    return null;
            }
            public List<ConstraintResultView> GetAllResults()
            {
                List<ConstraintResultView> returnList = new List<ConstraintResultView>();
                foreach (ConstraintResult CR in ConstraintResults.Values)
                {
                    returnList.Add(new ConstraintResultView(CR));
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
                    if (ConstraintType != ConstraintTypeCodes.CI)
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
                    if (ConstraintType != ConstraintTypeCodes.CI)
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
                    if (ConstraintType != ConstraintTypeCodes.CI)
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


            //public double GetThreshold(ReferenceThresholdTypes Name)
            //{
            //    var CT = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == Name);
            //    if (CT != null)
            //        return CT.ThresholdValue;
            //    else
            //        return double.NaN;
            //}
            //public bool SetThreshold(ReferenceThresholdTypes Name, double value)
            //{
            //    var CTs = DataCache.GetConstraintThresholdByConstraintId(ID);
            //    switch (Name)
            //    {
            //        case ReferenceThresholdTypes.MajorViolation:
            //            if (double.IsNaN(value))
            //                return false;
            //            if (ReferenceType == ReferenceTypes.Lower)
            //            {
            //                if (value > ReferenceValue)
            //                    return false;
            //                var CT_minor = CTs.FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.MinorViolation);
            //                if (CT_minor != null)
            //                    if (CT_minor.ThresholdValue < value)
            //                        CT_minor.ThresholdValue = double.NaN;
            //                var CT_major = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.MajorViolation);
            //                if (CT_major != null)
            //                {
            //                    CT_major.ThresholdValue = value;
            //                    RefreshResultThresholdStatus();
            //                    return true;
            //                }
            //                else
            //                {
            //                    if (value != double.NaN)
            //                    {
            //                        DataCache.AddConstraintThreshold(ReferenceThresholdTypes.MajorViolation, ConstraintThresholdTypes.Violation, this, value);
            //                        RefreshResultThresholdStatus();
            //                        return true;
            //                    }
            //                    else
            //                        return false;
            //                }
            //            }
            //            else
            //            {
            //                if (value < ReferenceValue)
            //                    return false;
            //                var CT_minor = CTs.FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.MinorViolation);
            //                if (CT_minor != null)
            //                    if (CT_minor.ThresholdValue > value)
            //                        CT_minor.ThresholdValue = double.NaN;
            //                var CT_major = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.MajorViolation);
            //                if (CT_major != null)
            //                {
            //                    CT_major.ThresholdValue = value;
            //                    RefreshResultThresholdStatus();
            //                    return true;
            //                }
            //                else
            //                {
            //                    if (value != double.NaN)
            //                    {
            //                        DataCache.AddConstraintThreshold(ReferenceThresholdTypes.MajorViolation, ConstraintThresholdTypes.Violation, this, value);
            //                        RefreshResultThresholdStatus();
            //                        return true;
            //                    }
            //                    else
            //                        return false;
            //                }
            //            }
            //        case ReferenceThresholdTypes.MinorViolation:
            //            if (double.IsNaN(value)) //
            //            {
            //                var CT_minor = CTs.FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.MinorViolation);
            //                if (CT_minor != null)
            //                    CT_minor.ThresholdValue = value;
            //                return true;
            //            }
            //            if (ReferenceType == ReferenceTypes.Lower)
            //            {
            //                if (value > ReferenceValue)
            //                    return false;
            //                var CT_major = CTs.FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.MajorViolation);
            //                if (CT_major != null)
            //                    if (CT_major.ThresholdValue > value)
            //                        CT_major.ThresholdValue = double.NaN;
            //                var CT_minor = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.MinorViolation);
            //                if (CT_minor != null)
            //                {
            //                    CT_minor.ThresholdValue = value;
            //                    RefreshResultThresholdStatus();
            //                    return true;
            //                }
            //                else
            //                {
            //                    if (value != double.NaN)
            //                    {
            //                        DataCache.AddConstraintThreshold(ReferenceThresholdTypes.MinorViolation, ConstraintThresholdTypes.Violation, this, value);
            //                        RefreshResultThresholdStatus();
            //                        return true;
            //                    }
            //                    else
            //                        return false;
            //                }
            //            }
            //            else
            //            {
            //                if (value < ReferenceValue)
            //                    return false;
            //                var CT_major = CTs.FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.MajorViolation);
            //                if (CT_major != null)
            //                    if (CT_major.ThresholdValue < value)
            //                        CT_major.ThresholdValue = double.NaN;
            //                var CT_minor = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.MinorViolation);
            //                if (CT_minor != null)
            //                {
            //                    CT_minor.ThresholdValue = value;
            //                    RefreshResultThresholdStatus();
            //                    return true;
            //                }
            //                else
            //                {
            //                    if (value != double.NaN)
            //                    {
            //                        DataCache.AddConstraintThreshold(ReferenceThresholdTypes.MinorViolation, ConstraintThresholdTypes.Violation, this, value);
            //                        RefreshResultThresholdStatus();
            //                        return true;
            //                    }
            //                    else
            //                        return false;
            //                }
            //            }
            //        case ReferenceThresholdTypes.Stop:
            //            if (double.IsNaN(value)) //
            //            {
            //                var CT_stop = CTs.FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.Stop);
            //                if (CT_stop != null)
            //                    CT_stop.ThresholdValue = value;
            //                return true;
            //            }
            //            if (ReferenceType == ReferenceTypes.Lower)
            //            {
            //                if (value < ReferenceValue)
            //                    return false;
            //                var CT_stop = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.Stop);
            //                if (CT_stop != null)
            //                {
            //                    CT_stop.ThresholdValue = value;
            //                    RefreshResultThresholdStatus();
            //                    return true;
            //                }
            //                else
            //                {
            //                    if (value != double.NaN)
            //                    {
            //                        DataCache.AddConstraintThreshold(ReferenceThresholdTypes.Stop, ConstraintThresholdTypes.Goal, this, value);
            //                        RefreshResultThresholdStatus();
            //                        return true;
            //                    }
            //                    else
            //                        return false;
            //                }
            //            }
            //            else
            //            {
            //                if (value > ReferenceValue)
            //                    return false;
            //                var CT_stop = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ReferenceThresholdTypes.Stop);
            //                if (CT_stop != null)
            //                {
            //                    CT_stop.ThresholdValue = value;
            //                    RefreshResultThresholdStatus();
            //                    return true;
            //                }
            //                else
            //                {
            //                    if (value != double.NaN)
            //                    {
            //                        DataCache.AddConstraintThreshold(ReferenceThresholdTypes.Stop, ConstraintThresholdTypes.Goal, this, value);
            //                        RefreshResultThresholdStatus();
            //                        return true;
            //                    }
            //                    else
            //                        return false;
            //                }
            //            }
            //        default:
            //            return false;
            //    }
            //}
            public bool ToRetire { get; private set; } = false;
            public void FlagForDeletion()
            {
                ToRetire = true;
            }
            private void OnProtocolStructureChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "ES":
                        {
                            if ((sender as ProtocolStructure).ID == PrimaryStructureID)
                                NotifyPropertyChanged("PrimaryStructureName");
                            else
                                NotifyPropertyChanged("SecondaryStructureName");
                            break;
                        }
                }

            }
            private void OnComponentDoseChanging(object sender, EventArgs e)
            {
                if (isConstraintValueDose())
                {
                    if (ConstraintScale == UnitScale.Relative)
                    {
                        NotifyPropertyChanged("ConstraintValue");
                    }
                    else if (ConstraintScale == UnitScale.Absolute && ReferenceType == ReferenceTypes.Lower && LowerConstraintDoseScalesWithComponent)
                    {
                        double CompDose = DataCache.GetComponent(ComponentID).ReferenceDose;
                        double CompDoseRef = DataCache.GetComponent(ComponentID).ReferenceDoseOriginal;
                        ConstraintValue = _ConstraintValue.ReferenceValue * CompDose / DataCache.GetComponent(ComponentID).ReferenceDoseOriginal;
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

                        if (ReferenceType == ReferenceTypes.Upper)
                        {
                            double CompDose = DataCache.GetComponent(ComponentID).ReferenceDose;
                            _ThresholdCalculator.MajorViolation = _ThresholdCalculator.ReferenceMajorViolation * CompDose / DataCache.GetComponent(ComponentID).ReferenceDoseOriginal;
                            _ThresholdCalculator.MinorViolation = _ThresholdCalculator.ReferenceMinorViolation * CompDose / DataCache.GetComponent(ComponentID).ReferenceDoseOriginal;
                            _ThresholdCalculator.Stop = _ThresholdCalculator.ReferenceStop * CompDose / DataCache.GetComponent(ComponentID).ReferenceDoseOriginal;
                            NotifyPropertyChanged(nameof(IReferenceThreshold.MajorViolation));
                            NotifyPropertyChanged(nameof(IReferenceThreshold.MinorViolation));
                            NotifyPropertyChanged(nameof(IReferenceThreshold.Stop));
                            NotifyPropertyChanged(nameof(IReferenceThreshold.ReferenceValue));
                        }
                    }
                }
            }
            private void OnComponentFractionsChanging(object sender, EventArgs e)
            {
                if (ReferenceType == ReferenceTypes.Upper)
                {
                    ApplyBEDScaling();
                }
                else
                {
                    NumFractions = (sender as Component).NumFractions;
                }
                //UpdateProgress();
            }
            private void OnConstraintThresholdChanged(object sender, EventArgs e)
            {
                AssociatedThresholdChanged?.Invoke(this, EventArgs.Empty);
            }

            public ConstraintReferenceValues GetConstraintReferenceValues()
            {
                return new ConstraintReferenceValues()
                {
                    NumFractions = _NumFractions.ReferenceValue,
                    PrimaryStructureID = _PrimaryStructureID.ReferenceValue,
                    ReferenceStructureId = _ReferenceStructureId.ReferenceValue,
                    //ReferenceValue = _ReferenceValue.ReferenceValue,
                    ReferenceType = _ReferenceType.ReferenceValue,
                    ConstraintType = _ConstraintType.ReferenceValue,
                    ConstraintScale = _ConstraintScale.ReferenceValue,
                    ReferenceScale = _ReferenceScale.ReferenceValue,
                    ConstraintValue = _ConstraintValue.ReferenceValue
                };
            }


        }

    }
}

