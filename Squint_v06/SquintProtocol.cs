using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Runtime.CompilerServices;
using System.Data.Entity;
using PropertyChanged;
using System.Data;


namespace SquintScript
{
    public static partial class Ctr
    {
        public class ImagingProtocol
        {
            public int ID { get; set; }
            public ImagingProtocols Type { get; set; }
        }
        public class Protocol : INotifyPropertyChanged
        {
            //Required notification class
            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            //Events
            public event EventHandler ProtocolNameChanged;
            public event EventHandler NewProtocolCommitting;
            public event EventHandler ProtocolUpdated;
            public event EventHandler ProtocolClosed;
            public class ProtocolArgs : EventArgs
            {
                public object Value;
            }
            public Protocol()
            {
                ID = Ctr.IDGenerator();
            }
            public Protocol(DbProtocol DbO)
            {
                ID = DbO.ID;
                ProtocolName = DbO.ProtocolName;
                CreationDate = DbO.CreationDate;
                ApprovalLevel = (ApprovalLevels)DbO.DbApprovalLevel.ApprovalLevel;
                Author = DbO.DbUser_ProtocolAuthor.ARIA_ID;
                LastModifiedBy = DbO.LastModifiedBy;
                ApprovingUser = DbO.DbUser_Approver.ARIA_ID;
                ProtocolType = (ProtocolTypes)DbO.DbProtocolType.ProtocolType;
                TreatmentCentre = (TreatmentCentres)DbO.DbTreatmentCentre.TreatmentCentre;
                TreatmentSite = (TreatmentSites)DbO.DbTreatmentSite.TreatmentSite;
                TreatmentIntent = (TreatmentIntents)DbO.TreatmentIntent;
            }
            //Properties
            public int ID { get; private set; }
            public string ProtocolName { get; set; }
            public ProtocolTypes ProtocolType { get; set; }
            public string CreationDate { get; set; }
            public ApprovalLevels ApprovalLevel { get; set; }
            public string Author { get; set; }
            public string ApprovingUser { get; set; }
            public string LastModifiedBy { get; set; }
            public TreatmentCentres TreatmentCentre { get; set; }
            public TreatmentSites TreatmentSite { get; set; }
            public TreatmentIntents TreatmentIntent { get; set; }
            // Protocol Wide Calc Defaults
            public AlgorithmTypes Algorithm { get; set; }
            public FieldNormalizationTypes FieldNormalizationMode { get; set; }
            public double AlgorithmResolution { get; set; }
            public double PNVMin { get; set; }
            public double PNVMax { get; set; }
            public double SliceSpacing { get; set; }
            public bool HeterogeneityOn { get; set; }

        }
        public class Session
        {
            //public event EventHandler NewProtocolExceptionCommitting;
            private Protocol SessionProtocol;
            public int ID { get; private set; }
            public int PatientId { get; set; } = 0;
            public string ProtocolName
            {
                get
                {
                    if (SessionProtocol != null)
                        return SessionProtocol.ProtocolName;
                    else
                        return "";
                }
            }
            public string SessionComment { get; set; }
            public string SessionCreator { get; set; }
            public string SessionDateTime { get; set; }
            public void AddProtocol(Protocol P)
            {
                SessionProtocol = P;
            }
            public void RemoveProtocol()
            {
                SessionProtocol = null;
            }
            public Session()
            {
                ID = Ctr.IDGenerator();
            }
            public Session(DbSession DbS)
            {
                ID = DbS.ID;
                SessionProtocol = new Protocol(DbS.DbSessionProtocol);
                SessionComment = DbS.SessionComment;
                SessionCreator = DbS.SessionCreator;
                SessionDateTime = DbS.SessionDateTime;
            }
        }

        [AddINotifyPropertyChangedInterface]
        public class ConstraintChangelog
        {
            public string DateString { get; private set; } = DateTime.Now.ToShortDateString();
            public string ChangeDescription { get; private set; } = "Long multi-line history for testing formatting of control. Long multi-line history for testing formatting of control. Long multi-line history for testing formatting of control.";
            public string ChangeAuthor { get; private set; } = "NoAuthor";
            public int ConstraintID { get; private set; }
            public string ConstraintString { get; private set; } = "Null Constraint String";
            public ConstraintChangelog(DbConstraintChangelog DbCC = null)
            {
                if (DbCC != null)
                {
                    DateString = string.Join(" ", DateTime.FromBinary(DbCC.Date).ToShortDateString(), DateTime.FromBinary(DbCC.Date).ToShortTimeString());
                    ChangeDescription = DbCC.ChangeDescription;
                    ChangeAuthor = DbCC.ChangeAuthor;
                    ConstraintID = DbCC.ConstraintID;
                    ConstraintString = DbCC.ConstraintString;
                }
            }
            public ConstraintChangelog(Constraint C)
            {
                DateString = string.Join(" ", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
                ChangeDescription = @"Ad-hoc constraint";
                ChangeAuthor = SquintUser;
                ConstraintID = C.ID;
                ConstraintString = "";
            }
        }

        public class ConstraintResult : INotifyPropertyChanged
        {
            //Events
            public event PropertyChangedEventHandler PropertyChanged;
            public event EventHandler<ConstraintResultRemovedArgs> ConstraintResultRemoved;
            public class ConstraintResultRemovedArgs : EventArgs
            {
                public int ConstraintID; // this will be -1 if constraint already deleted
                public int oldID; // 
                public int AssessmentID;
            }
            public ConstraintResult(int Id, int AssessmentId, int ConId)
            {
                ID = Id;
                ConstraintID = ConId;
                AssessmentID = AssessmentId;
                isLoaded = true;
            }
            public ConstraintResult(int _AssessmentID, int _ConstraintID)
            {
                isLoaded = false;
                ThresholdStatus = ConstraintThresholdNames.Unset;
                ID = (int)IDGenerator();
                AssessmentID = _AssessmentID;
                ConstraintID = _ConstraintID;
                SetResultString();
                // Subscribe to constraint
                DataCache.GetConstraint(ConstraintID).PropertyChanged += OnConstraintPropertyChanged;

            }
            public readonly Object LockSimultaneousEvaluation = new Object();
            public int ID { get; private set; }
            public int AssessmentID { get; private set; }
            public int ConstraintID { get; private set; }
            public bool isCalculating = false;
            public string LinkedLabelName = "Not Linked";
            private double _ResultValue;
            public double ResultValue
            {
                get
                {
                    return _ResultValue;
                }
                set
                {
                    _ResultValue = value;
                    SetResultString();
                    isLoaded = false;
                }
            }
            private Dictionary<ConstraintResultStatusCodes, int> _StatusCodes = new Dictionary<ConstraintResultStatusCodes, int>(); // code to entity ID
            public List<ConstraintResultStatusCodes> StatusCodes
            {
                get { return _StatusCodes.Keys.ToList(); }
            }
            public bool isLoaded { get; private set; }
            private string _ResultString;
            private void SetResultString()
            {
                Constraint Con = DataCache.GetConstraint(ConstraintID);
                if (!Con.isValid())
                    _ResultString = "";
                switch (Con.GetReferenceUnit())
                {
                    case ConstraintUnits.cc:
                        {
                            _ResultString = string.Format(@"{0:0.#} {1}", ResultValue, ConstraintUnits.cc.Display());
                            break;
                        }
                    case ConstraintUnits.cGy:
                        {
                            _ResultString = string.Format(@"{0:0} {1}", ResultValue, ConstraintUnits.cGy.Display());
                            break;
                        }
                    case ConstraintUnits.Percent:
                        {
                            _ResultString = string.Format(@"{0:0.#} {1}", ResultValue, ConstraintUnits.Percent.Display());
                            break;
                        }
                    case ConstraintUnits.Unset:
                        {
                            _ResultString = string.Format(@"{0} {1}", ResultValue, ConstraintUnits.Unset.Display());
                            break;
                        }
                    case ConstraintUnits.Multiple:
                        {
                            _ResultString = string.Format(@"{0:0.##} {1}", ResultValue, ConstraintUnits.Multiple.Display());
                            break;
                        }
                    default:
                        _ResultString = "";
                        break;
                }
            }
            public string ResultString
            {
                get
                {
                    if (StatusCodes.Contains(ConstraintResultStatusCodes.StructureEmpty) ||
                        StatusCodes.Contains(ConstraintResultStatusCodes.StructureNotFound) ||
                        StatusCodes.Contains(ConstraintResultStatusCodes.NotLinked) ||
                        StatusCodes.Contains(ConstraintResultStatusCodes.LinkedPlanError) ||
                        StatusCodes.Contains(ConstraintResultStatusCodes.ConstraintUndefined) ||
                        StatusCodes.Contains(ConstraintResultStatusCodes.ErrorUnspecified))
                        return "-";
                    else
                        return _ResultString;
                }
            }
            public void ClearStatusCodes()
            {
                _StatusCodes.Clear();
            }
            public void AddStatusCode(ConstraintResultStatusCodes Code)
            {
                if (_StatusCodes.ContainsKey(Code))
                    return;
                else
                {
                    _StatusCodes.Add(Code, IDGenerator());
                }

            }
            public bool HasActiveStatus()
            {
                if (StatusCodes.Count > 0)
                {
                    return true;
                }
                return false;
            }
            public string GetStatuses()
            {
                string returnString = "";
                foreach (ConstraintResultStatusCodes Code in _StatusCodes.Keys)
                {
                    returnString = returnString + Code.Display() + "\r\n";
                }
                return returnString;
            }
            public ConstraintThresholdNames ThresholdStatus;
            private void OnConstraintPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                SetResultString();
            }
        }
        public class Constraint : INotifyPropertyChanged
        {
            public class ConstraintReferenceValues
            {
                public int NumFractions;
                public int PrimaryStructureID;
                public int SecondaryStructureID;
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
                        UpdateOnEvaluatedPropertyChange(propertyName);
                        break;
                }
            }
            private async void UpdateOnEvaluatedPropertyChange(string propertyName)
            {

                foreach (int SA_ID in new List<int>(RegisteredAssessmentIDs)) // new list created to avoid user chaning the list while constraints being calculated
                    if (DataCache.GetAssessment(SA_ID).AutoCalculate)
                        await Task.Run(() => EvaluateConstraint(DataCache.GetAssessment(SA_ID))); // this has to be awaited so the while loop doesn't lock up the UI thread
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
                    _SecondaryStructureID = new TrackedValue<int>(DbOS.OriginalSecondaryStructureID);
                    SecondaryStructureID = DbOS.SecondaryStructureID;
                    _ConstraintType = new TrackedValue<ConstraintTypeCodes>((ConstraintTypeCodes)DbOS.OriginalConstraintType);
                    ConstraintType = (ConstraintTypeCodes)DbOS.ConstraintType;
                    _ReferenceValue = new TrackedValueWithReferences<double>(DbOS.OriginalReferenceValue);
                    _ReferenceValue.Value = DbO.ReferenceValue;
                    _ReferenceType = new TrackedValue<ReferenceTypes>((ReferenceTypes)DbOS.OriginalReferenceType);
                    ReferenceType = (ReferenceTypes)DbOS.ReferenceType;
                    _ReferenceScale = new TrackedValue<UnitScale>((UnitScale)DbOS.OriginalReferenceScale);
                    ReferenceScale = (UnitScale)DbOS.ReferenceScale;
                    _ConstraintValue = new TrackedValue<double>(DbOS.OriginalConstraintValue);
                    ConstraintValue = DbO.ConstraintValue;
                    _NumFractions = new TrackedValue<int>(DbOS.OriginalNumFractions);
                    NumFractions = DbOS.Fractions;
                    _ConstraintScale = new TrackedValue<UnitScale>((UnitScale)DbOS.OriginalConstraintScale);
                    ConstraintScale = (UnitScale)DbOS.ConstraintScale;
                }
                else
                {
                    _PrimaryStructureID = new TrackedValue<int>(DbO.PrimaryStructureID);
                    _SecondaryStructureID = new TrackedValue<int>(DbO.SecondaryStructureID);
                    _ConstraintType = new TrackedValue<ConstraintTypeCodes>((ConstraintTypeCodes)DbO.ConstraintType);
                    _ReferenceValue = new TrackedValueWithReferences<double>(DbO.ReferenceValue);
                    _ReferenceType = new TrackedValue<ReferenceTypes>((ReferenceTypes)DbO.ReferenceType);
                    _ReferenceScale = new TrackedValue<UnitScale>((UnitScale)DbO.ReferenceScale);
                    _ConstraintValue = new TrackedValue<double>(DbO.ConstraintValue);
                    _NumFractions = new TrackedValue<int>(DbO.Fractions);
                    _ConstraintScale = new TrackedValue<UnitScale>((UnitScale)DbO.ConstraintScale);
                    DisplayOrder = new TrackedValue<int>(DbO.DisplayOrder);
                }

                var ECSID = DataCache.GetECSID(DbO.PrimaryStructureID);
                if (ECSID == null)
                {
                    throw new Exception("Referenced ECSID is not present in cache (Constraint constructor)");
                }
                else
                {
                    ECSID.PropertyChanged += OnECSIDChanged;
                }
                var SC = DataCache.GetComponent(DbO.ComponentID);
                if (SC == null)
                {
                    throw new Exception("Referenced ECSID is not present in cache (Constraint constructor)");
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
                _ConstraintType = new TrackedValue<ConstraintTypeCodes>(TypeCode);
                _ReferenceValue = new TrackedValueWithReferences<double>(0);
                _SecondaryStructureID = new TrackedValue<int>(1);
                _ReferenceScale = new TrackedValue<UnitScale>(UnitScale.Unset);
                DisplayOrder = DataCache.GetAllConstraints().Select(x => x.DisplayOrder).Max();
                var ECSID = DataCache.GetECSID(PrimaryStructureID);
                if (ECSID == null)
                {
                    throw new Exception("Referenced ECSID is not present in cache (Constraint constructor)");
                }
                else
                {
                    ECSID.PropertyChanged += OnECSIDChanged;
                }
                if (SC == null)
                {
                    throw new Exception("Referenced ECSID is not present in cache (Constraint constructor)");
                }
                else
                {

                    SC.ReferenceDoseChanged += OnComponentDoseChanging;
                    SC.ReferenceFractionsChanged += OnComponentFractionsChanging;
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
                SecondaryStructureID = Con.SecondaryStructureID;
                ConstraintType = Con.ConstraintType;
                ReferenceValue = Con.ReferenceValue;
                ReferenceType = Con.ReferenceType;
                ReferenceScale = Con.ReferenceScale;
                DisplayOrder = new TrackedValue<int>(Con.DisplayOrder.Value + 1);

                var ECSID = DataCache.GetECSID(PrimaryStructureID);
                if (ECSID == null)
                {
                    throw new Exception("Referenced ECSID is not present in cache (Constraint constructor)");
                }
                else
                {
                    ECSID.PropertyChanged += OnECSIDChanged;
                }
                if (SC == null)
                {
                    throw new Exception("Referenced ECSID is not present in cache (Constraint constructor)");
                }
                else
                {

                    SC.ReferenceDoseChanged += OnComponentDoseChanging;
                    SC.ReferenceFractionsChanged += OnComponentFractionsChanging;
                }
            }
            public Constraint(SquintEclipseProtocol.Item Item, int ComponentID = -1)
            {
                //// This constructor duplications the input constraint 
                //if (Item.Modifier == 5)
                //    return; // this is a reference point which is ignored by Squint
                //if (Item.Modifier == 6) //unknown value
                //    return;
                //_DbO = DataCache.SquintDb.Context.DbSessionConstraints.Create();
                //DataCache.SquintDb.Context.DbConstraints.Add(_DbO);
                //_DbO.ID = Ctr.IDGenerator();
                //_DbO.SecondaryStructureID = 1; // temp.. need to set this for Eclipse CI
                //_DbO.PrimaryStructureID = ECSIDs.Values.Where(x => x.EclipseStructureName == Item.ID).SingleOrDefault().ID;
                //bool setThreshold = true;
                //if (Item.Dose == null)
                //{
                //    Item.Dose = 0; // if left empty, set to zero and don't set a major violation threshold because user left this blank in Eclipse
                //    setThreshold = false;
                //}
                //switch (Item.Modifier)
                //{
                //    case 0:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.V; // At least X % receives more than Y dose
                //        _DbO.ReferenceType = (int)ReferenceTypes.Lower;
                //        _DbO.ConstraintValue = Item.Parameter;
                //        _DbO.ConstraintScale = (int)UnitScale.Relative;
                //        _DbO.ReferenceScale = (int)UnitScale.Absolute;
                //        break;
                //    case 1:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.V; // At most X % receives more than Y dose
                //        _DbO.ReferenceType = (int)ReferenceTypes.Upper;
                //        _DbO.ConstraintValue = Item.Parameter;
                //        _DbO.ConstraintScale = (int)UnitScale.Relative;
                //        _DbO.ReferenceScale = (int)UnitScale.Absolute;
                //        break;
                //    case 2:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.M;  // Mean dose is
                //        _DbO.ReferenceType = (int)ReferenceTypes.Lower;
                //        _DbO.ConstraintValue = Item.Parameter;
                //        _DbO.ConstraintScale = (int)UnitScale.Absolute;
                //        _DbO.ReferenceScale = (int)UnitScale.Absolute;
                //        break;
                //    case 3:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.D; // Max dose is 
                //        _DbO.ReferenceType = (int)ReferenceTypes.Upper;
                //        _DbO.ConstraintValue = 0;
                //        _DbO.ConstraintScale = (int)UnitScale.Relative;
                //        _DbO.ReferenceScale = (int)UnitScale.Absolute;
                //        break;
                //    case 4:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.D; // Min dose is 
                //        _DbO.ReferenceType = (int)ReferenceTypes.Lower;
                //        _DbO.ConstraintValue = 100;
                //        _DbO.ConstraintScale = (int)UnitScale.Relative;
                //        _DbO.ReferenceScale = (int)UnitScale.Absolute;
                //        break;
                //    case 5:
                //        //reference point which is aborted above
                //        break;
                //    case 6:
                //        // unknown
                //        break;
                //    case 7:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.M; // Mean dose is greater than
                //        _DbO.ReferenceType = (int)ReferenceTypes.Lower;
                //        _DbO.ConstraintValue = Item.Parameter;
                //        _DbO.ConstraintScale = (int)UnitScale.Absolute;
                //        _DbO.ReferenceScale = (int)UnitScale.Absolute;
                //        break;
                //    case 8:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.M; // Mean dose is less than
                //        _DbO.ReferenceType = (int)ReferenceTypes.Upper;
                //        _DbO.ConstraintValue = Item.Parameter;
                //        _DbO.ConstraintScale = (int)UnitScale.Absolute;
                //        _DbO.ReferenceScale = (int)UnitScale.Absolute;
                //        break;
                //    case 9:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.D; // Min dose is greater than
                //        _DbO.ReferenceType = (int)ReferenceTypes.Lower;
                //        _DbO.ConstraintValue = 100;
                //        _DbO.ConstraintScale = (int)UnitScale.Relative;
                //        _DbO.ReferenceScale = (int)UnitScale.Absolute;
                //        break;
                //    case 10:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.D; // Max dose is  less than
                //        _DbO.ReferenceType = (int)ReferenceTypes.Upper;
                //        _DbO.ConstraintValue = 0;
                //        _DbO.ConstraintScale = (int)UnitScale.Relative;
                //        _DbO.ReferenceScale = (int)UnitScale.Absolute;
                //        break;
                //    default:
                //        string debug = "";
                //        break;
                //}
                //if (Item.Dose != null)
                //    _DbO.ReferenceValue = (double)Item.Dose * 100 * DataCache.GetComponent(ComponentID).NumFractions;
                //if (ComponentID != -1)
                //    _DbO.ComponentID = ComponentID;
                //lock (Lock)
                //{
                //    Constraints.Add(ID, this);
                //}
                //if (setThreshold)
                //{
                //    DbConThresholdDef MajorViolation = DataCache.SquintDb.Context.DbConThresholdDefs.Where(x => x.Threshold == (int)ConstraintThresholdNames.MajorViolation).Single();
                //    ConstraintThreshold newCT = new ConstraintThreshold(MajorViolation, _DbO.ID, _DbO.ReferenceValue);
                //}
                //Component SC = Components[_DbO.ComponentID];
                //ECSID ECSID_DvH = ECSIDs.Values.Where(x => x.ID == _DbO.PrimaryStructureID && x.ProtocolID == DataCache.CurrentProtocol.ID).SingleOrDefault();
                //ECSID_DvH.NewECSIDCommitting += OnNewECSIDChanged_Primary;
                //ECSID_DvH.ECSIDDeleting += OnECSIDDeleting_Primary;
                //ECSID_DvH.ECSIDChanged += OnECSIDChanged;
                //if (_DbO.SecondaryStructureID != 1)
                //{
                //    ECSID ECSID_CI = ECSIDs.Values.Where(x => x.ID == _DbO.SecondaryStructureID && x.ProtocolID == DataCache.CurrentProtocol.ID).SingleOrDefault();
                //    ECSID_CI.NewECSIDCommitting += OnNewECSIDChanged_Secondary;
                //    ECSID_CI.ECSIDDeleting += OnECSIDDeleting_Secondary;
                //    ECSID_CI.ECSIDChanged += OnECSIDChanged;
                //}
                //SC.ComponentDeleted += OnComponentDeleted;
                //SC.ReferenceDoseChanged += OnComponentDoseChanging;
                //SC.ReferenceFractionsChanged += OnComponentFractionsChanging;
                //SC.NewComponentCommitting += OnNewComponentCommitting;
                //ExceptionType = ExceptionTypes.None;
                //ManageSaveEvents();
                //ID = _DbO.ID;
            }
            public Constraint(SquintEclipseProtocol.MeasureItem MI, int ComponentID = -1)
            {
                //// This constructor duplications the input constraint 
                //if (MI.Type == 0 || MI.Type == 1)
                //{
                //    return; // conformity and gradient index not handled
                //}
                //_DbO = DataCache.SquintDb.Context.DbSessionConstraints.Create();
                //DataCache.SquintDb.Context.DbConstraints.Add(_DbO);
                //_DbO.ID = IDGenerator();
                //_DbO.SecondaryStructureID = 1; // temp.. need to set this for Eclipse CI
                //_DbO.PrimaryStructureID = ECSIDs.Values.Where(x => x.EclipseStructureName.ToUpper() == MI.ID.ToUpper()).SingleOrDefault().ID;
                //if (MI.ReportDQPValueInAbsoluteUnits)
                //    _DbO.ReferenceScale = (int)UnitScale.Absolute;
                //else
                //    _DbO.ReferenceScale = (int)UnitScale.Relative;
                //bool setThreshold = true;
                //if (MI.Value == null)
                //{
                //    MI.Value = 0; // if left empty, set to zero and don't set a major violation threshold because user left this blank in Eclipse
                //    setThreshold = false;
                //}
                //switch (MI.Type)
                //{
                //    case 2:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.V;
                //        if (ReferenceScale == UnitScale.Absolute)
                //            _DbO.ReferenceValue = (double)MI.Value / 1000;
                //        else
                //            _DbO.ReferenceValue = (double)MI.Value;
                //        _DbO.ConstraintScale = (int)UnitScale.Relative;
                //        _DbO.ConstraintValue = (double)MI.TypeSpecifier; //return value in percent
                //        break;
                //    case 3:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.V;
                //        if (ReferenceScale == UnitScale.Absolute)
                //            _DbO.ReferenceValue = (double)MI.Value / 1000;
                //        else
                //            _DbO.ReferenceValue = (double)MI.Value;
                //        _DbO.ConstraintScale = (int)UnitScale.Absolute;
                //        _DbO.ConstraintValue = (double)MI.TypeSpecifier * 100;
                //        break;
                //    case 4:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.D;
                //        _DbO.ConstraintScale = (int)UnitScale.Relative;
                //        _DbO.ConstraintValue = (double)MI.TypeSpecifier; //return value in percent
                //        if (ReferenceScale == UnitScale.Absolute)
                //            _DbO.ReferenceValue = (double)MI.Value * 100;
                //        else
                //            _DbO.ReferenceValue = (double)MI.Value;
                //        break;
                //    case 5:
                //        _DbO.ConstraintType = (int)ConstraintTypeCodes.D;
                //        _DbO.ConstraintScale = (int)UnitScale.Absolute;
                //        _DbO.ConstraintValue = (double)MI.TypeSpecifier; // convert to cGy
                //        if (ReferenceScale == UnitScale.Absolute)
                //            _DbO.ReferenceValue = (double)MI.Value * 100;
                //        else
                //            _DbO.ReferenceValue = (double)MI.Value;
                //        break;
                //    default:
                //        break;
                //}
                //switch (MI.Modifier)
                //{
                //    case 0:
                //        _DbO.ReferenceType = (int)ReferenceTypes.Lower;
                //        break;
                //    case 1:
                //        _DbO.ReferenceType = (int)ReferenceTypes.Upper;
                //        break;
                //    case 2:
                //        _DbO.ReferenceType = (int)ReferenceTypes.Lower; // this is an "is equal to" constraint (i.e. for normalization), which Squint treats as a lower constraint
                //        break;
                //}
                //if (MI.ReportDQPValueInAbsoluteUnits)
                //    _DbO.ReferenceScale = (int)UnitScale.Absolute;
                //else
                //    _DbO.ReferenceScale = (int)UnitScale.Relative;
                //if (ComponentID != -1)
                //    _DbO.ComponentID = ComponentID;
                //lock (Lock)
                //{
                //    Constraints.Add(_DbO.ID, this);
                //}
                //if (setThreshold)
                //{
                //    DbConThresholdDef MajorViolation = DataCache.SquintDb.Context.DbConThresholdDefs.Where(x => x.Threshold == (int)ConstraintThresholdNames.MajorViolation).Single();
                //    ConstraintThreshold newCT = new ConstraintThreshold(MajorViolation, _DbO.ID, _DbO.ReferenceValue);
                //}
                //Component SC = Components[_DbO.ComponentID];
                //ECSID ECSID_DvH = ECSIDs.Values.Where(x => x.ID == _DbO.PrimaryStructureID && x.ProtocolID == DataCache.CurrentProtocol.ID).SingleOrDefault();
                //ECSID_DvH.NewECSIDCommitting += OnNewECSIDChanged_Primary;
                //ECSID_DvH.ECSIDDeleting += OnECSIDDeleting_Primary;
                //ECSID_DvH.ECSIDChanged += OnECSIDChanged;
                //if (_DbO.SecondaryStructureID != 1)
                //{
                //    ECSID ECSID_CI = ECSIDs.Values.Where(x => x.ID == _DbO.SecondaryStructureID && x.ProtocolID == DataCache.CurrentProtocol.ID).SingleOrDefault();
                //    ECSID_CI.NewECSIDCommitting += OnNewECSIDChanged_Secondary;
                //    ECSID_CI.ECSIDDeleting += OnECSIDDeleting_Secondary;
                //    ECSID_CI.ECSIDChanged += OnECSIDChanged;
                //}
                //SC.ComponentDeleted += OnComponentDeleted;
                //SC.ReferenceDoseChanged += OnComponentDoseChanging;
                //SC.ReferenceFractionsChanged += OnComponentFractionsChanging;
                //SC.NewComponentCommitting += OnNewComponentCommitting;
                //ExceptionType = ExceptionTypes.None;
                //ManageSaveEvents();
                //ID = _DbO.ID;
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
                    case nameof(_SecondaryStructureID):
                        return _SecondaryStructureID.IsChanged;
                    case nameof(ReferenceValue):
                        return _ReferenceValue.IsChanged;
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
            private TrackedValue<int> _NumFractions;
            public int NumFractions { get { return _NumFractions.Value; } set { _NumFractions.Value = value; } }
            private TrackedValue<int> _ComponentID;
            public int ComponentID { get { return _ComponentID.Value; } set { _ComponentID.Value = value; } }
            public string ComponentName { get { return DataCache.GetComponent(ComponentID).ComponentName; } }
            private TrackedValue<int> _PrimaryStructureID { get; set; }
            public int PrimaryStructureID { get { return _PrimaryStructureID.Value; } set { _PrimaryStructureID.Value = value; } }
            private TrackedValue<int> _SecondaryStructureID { get; set; }
            public int SecondaryStructureID { get { return _SecondaryStructureID.Value; } set { _SecondaryStructureID.Value = value; } }
            private TrackedValueWithReferences<double> _ReferenceValue { get; set; }
            public double ReferenceValue
            {
                get { return _ReferenceValue.Value; }
                set { _ReferenceValue.Value = value; }
            }
            //private bool _wasSessionModified { get; set; } = false;  // this is true if the loaded constraint was modified in its session
            private TrackedValue<ReferenceTypes> _ReferenceType;
            public ReferenceTypes ReferenceType { get { return _ReferenceType.Value; } set { _ReferenceType.Value = value; } }
            private TrackedValue<ConstraintTypeCodes> _ConstraintType;
            public ConstraintTypeCodes ConstraintType { get { return _ConstraintType.Value; } set { _ConstraintType.Value = value; } }
            private TrackedValue<UnitScale> _ConstraintScale;
            public UnitScale ConstraintScale { get { return _ConstraintScale.Value; } set { _ConstraintScale.Value = value; } }
            private TrackedValue<UnitScale> _ReferenceScale { get; set; }// the constraint unit
            public UnitScale ReferenceScale { get { return _ReferenceScale.Value; } set { _ReferenceScale.Value = value; } }
            private TrackedValue<double> _ConstraintValue;
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
                           && (ConstraintType != ConstraintTypeCodes.CI || SecondaryStructureID != 1)
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
                    var E = DataCache.GetECSID(PrimaryStructureID);
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
                    var E = DataCache.GetECSID(PrimaryStructureID);
                    if (E != null)
                        return E.EclipseStructureName;
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
                if (SecondaryStructureID != 1)
                    return DataCache.GetECSID(SecondaryStructureID).ProtocolStructureName;
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
                        else if (ConstraintValue < 1E-5 && ConstraintScale == UnitScale.Relative)
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
            public bool BEDScalesWithComponent = true;
            public bool LowerConstraintDoseScalesWithComponent = true;
            private void ApplyBEDScaling()
            {
                Component SC = DataCache.GetComponent(ComponentID);
                ECSID E = DataCache.GetECSID(PrimaryStructureID);
                double abRatio = DataCache.GetStructureLabel(E.StructureLabelID).AlphaBetaRatio;
                if (abRatio > 0)
                {
                    if (isConstraintValueDose())
                    {
                        if (ConstraintScale == UnitScale.Relative)
                        {
                            ConstraintValue = DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, _ConstraintValue.ReferenceValue / 100 * SC.ReferenceDose, abRatio) * 100 / SC.ReferenceDose;
                            NumFractions = SC.NumFractions;
                        }
                        else
                        {
                            ConstraintValue = Math.Round(DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, _ConstraintValue.ReferenceValue, abRatio) / 100) * 100;
                            NumFractions = SC.NumFractions;
                        }
                    }
                    else if (isReferenceValueDose())
                    {
                        if (ReferenceScale == UnitScale.Relative)
                        {
                            ReferenceValue = Math.Round(DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, _ReferenceValue.ReferenceValue / 100 * SC.ReferenceDose, abRatio)) * 100 / SC.ReferenceDose;
                            NumFractions = SC.NumFractions;
                        }
                        else
                        {
                            ReferenceValue = Math.Round(DoseFunctions.BED(_NumFractions.ReferenceValue, SC.NumFractions, _ReferenceValue.ReferenceValue, abRatio) / 100) * 100;
                            NumFractions = SC.NumFractions;
                        }
                        // NotifyPropertyChanged("ReferenceValue");
                        foreach (ConstraintThreshold CT in DataCache.GetConstraintThresholdByConstraintId(ID).ToList())
                        {
                            switch (CT.ThresholdName)
                            {
                                case ConstraintThresholdNames.MinorViolation:
                                    DataCache.DeleteConstraintThreshold(CT);
                                    break;
                                case ConstraintThresholdNames.MajorViolation:
                                    CT.ThresholdValue = ReferenceValue;
                                    break;
                                case ConstraintThresholdNames.Stop:
                                    DataCache.DeleteConstraintThreshold(CT);
                                    break;
                            }
                        }
                    }
                }
                else
                {
                    NumFractions = SC.NumFractions;
                }
            }
            //Evaluation routines
            private List<int> RegisteredAssessmentIDs = new List<int>();
            private bool _isModified()
            {
                return _NumFractions.IsChanged
                || _ComponentID.IsChanged
                || _PrimaryStructureID.IsChanged
                || _SecondaryStructureID.IsChanged
                || _ReferenceValue.IsChanged
                || _ReferenceType.IsChanged
                || _ConstraintType.IsChanged
                || _ConstraintScale.IsChanged
                || _ReferenceScale.IsChanged
                || _ConstraintValue.IsChanged;
            }
            public void RegisterAssessment(Assessment SA)
            {
                SA.ComponentAssociationChange += OnComponentAssociationChanged;
                SA.ComponentUnlinked += OnComponentUnlinked;
                SA.PlanMappingChanged += OnPlanMappingChanged;
                RegisteredAssessmentIDs.Add(SA.ID);
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
                    if (SA.AutoCalculate)
                        await Task.Run(() => EvaluateConstraint(SA));
                }
            }
            public async void OnPlanMappingChanged(object sender, int ChangedComponentID)
            {
                Assessment SA = (sender as Assessment);
                string.Format("{0}", ID);
                if (ComponentID == ChangedComponentID)
                {
                    if (SA.AutoCalculate)
                        await Task.Run(() => EvaluateConstraint(SA));
                    //await EvaluateConstraint(SA);
                }
            }
            public async Task EvaluateConstraint(Assessment SA)
            {
                try
                {
                    ConstraintResult CR = null;
                    lock (Ctr.LockConstraint)
                    {
                        if (ConstraintResults.ContainsKey(SA.ID))
                            CR = ConstraintResults[SA.ID];
                        if (CR == null)
                        {
                            CR = new ConstraintResult(SA.ID, ID);
                            ConstraintResults.Add(SA.ID, CR);
                        }
                    }
                    lock (CR.LockSimultaneousEvaluation)
                    {
                        while (CR.isCalculating) // wait for any other threads using this constraint to finish
                            Thread.Sleep(100);
                        CR.isCalculating = true; // this locks any other threads from passing this point if the UI invokes an update
                    }
                    CR.ClearStatusCodes();
                    if (!isValid())
                    {
                        CR.AddStatusCode(ConstraintResultStatusCodes.ConstraintUndefined);
                        CR.isCalculating = false;
                        ConstraintEvaluated?.Raise(new ConstraintResultView(CR), SA.ID); // this notifies the view class, no need to raise to UI
                        return;
                    }
                    ECPlan ECP = DataCache.GetAllPlans().Where(x => x.AssessmentID == SA.ID && x.ComponentID == ComponentID).SingleOrDefault();
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
                    string targetId = DataCache.GetECSID(PrimaryStructureID).EclipseStructureName;
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
                    if ((p.Structures[targetId].Code != DataCache.GetStructureCode(DataCache.GetECSID(PrimaryStructureID).StructureLabelID)))
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
                    if (p.PlanType == ComponentTypes.Sum)
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
                            string refStructId = DataCache.GetECSID(SecondaryStructureID).EclipseStructureName;
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
            private ConstraintThresholdNames ThresholdStatus(ConstraintResult CR)
            {
                Constraint Con = DataCache.GetConstraint(CR.ConstraintID);
                if (CR.StatusCodes.Where(x => x != ConstraintResultStatusCodes.LabelMismatch).Count() > 0)
                    return ConstraintThresholdNames.Unset;
                ConstraintThresholdNames ThreshLevel = ConstraintThresholdNames.None;
                if (Con.ReferenceType == ReferenceTypes.Upper)
                {
                    foreach (ConstraintThreshold CT in DataCache.GetConstraintThresholdByConstraintId(Con.ID).Where(x => x.ThresholdType == ConstraintThresholdTypes.Violation).Where(x => !double.IsNaN(x.ThresholdValue)).OrderBy(x => x.ThresholdValue))
                        if (CR.ResultValue > CT.ThresholdValue)
                            ThreshLevel = CT.ThresholdName;
                    foreach (ConstraintThreshold CT in DataCache.GetConstraintThresholdByConstraintId(Con.ID).Where(x => x.ThresholdType == ConstraintThresholdTypes.Goal).Where(x => !double.IsNaN(x.ThresholdValue)).OrderByDescending(x => x.ThresholdValue))
                        if (CR.ResultValue < CT.ThresholdValue)
                            ThreshLevel = CT.ThresholdName;
                }
                else
                {
                    foreach (ConstraintThreshold CT in DataCache.GetConstraintThresholdByConstraintId(Con.ID).Where(x => x.ThresholdType == ConstraintThresholdTypes.Violation).Where(x => !double.IsNaN(x.ThresholdValue)).OrderByDescending(x => x.ThresholdValue))
                        if (CR.ResultValue < CT.ThresholdValue)
                            ThreshLevel = CT.ThresholdName;
                    foreach (ConstraintThreshold CT in DataCache.GetConstraintThresholdByConstraintId(Con.ID).Where(x => x.ThresholdType == ConstraintThresholdTypes.Goal).Where(x => !double.IsNaN(x.ThresholdValue)).OrderBy(x => x.ThresholdValue))
                        if (CR.ResultValue > CT.ThresholdValue)
                            ThreshLevel = CT.ThresholdName;
                }
                return ThreshLevel;
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
            public double StopValue
            {
                get
                {
                    return GetThreshold(ConstraintThresholdNames.Stop);
                }
                set
                {
                    SetThreshold(ConstraintThresholdNames.Stop, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Threshold"));
                }
            }
            public double MinorViolation
            {
                get
                {
                    return GetThreshold(ConstraintThresholdNames.MinorViolation);
                }
                set
                {
                    SetThreshold(ConstraintThresholdNames.MinorViolation, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Threshold"));
                }
            }
            public double MajorViolation
            {
                get
                {
                    return GetThreshold(ConstraintThresholdNames.MajorViolation);
                }
                set
                {
                    SetThreshold(ConstraintThresholdNames.MajorViolation, value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Threshold"));
                }
            }
            public double GetThreshold(ConstraintThresholdNames Name)
            {
                var CT = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == Name);
                if (CT != null)
                    return CT.ThresholdValue;
                else
                    return double.NaN;
            }
            public bool SetThreshold(ConstraintThresholdNames Name, double value)
            {
                var CTs = DataCache.GetConstraintThresholdByConstraintId(ID);
                switch (Name)
                {
                    case ConstraintThresholdNames.MajorViolation:
                        if (double.IsNaN(value))
                            return false;
                        if (ReferenceType == ReferenceTypes.Lower)
                        {
                            if (value > ReferenceValue)
                                return false;
                            var CT_minor = CTs.FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.MinorViolation);
                            if (CT_minor != null)
                                if (CT_minor.ThresholdValue < value)
                                    CT_minor.ThresholdValue = double.NaN;
                            var CT_major = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.MajorViolation);
                            if (CT_major != null)
                            {
                                CT_major.ThresholdValue = value;
                                RefreshResultThresholdStatus();
                                return true;
                            }
                            else
                            {
                                if (value != double.NaN)
                                {
                                    DataCache.AddConstraintThreshold(ConstraintThresholdNames.MajorViolation, ConstraintThresholdTypes.Violation, this, value);
                                    RefreshResultThresholdStatus();
                                    return true;
                                }
                                else
                                    return false;
                            }
                        }
                        else
                        {
                            if (value < ReferenceValue)
                                return false;
                            var CT_minor = CTs.FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.MinorViolation);
                            if (CT_minor != null)
                                if (CT_minor.ThresholdValue > value)
                                    CT_minor.ThresholdValue = double.NaN;
                            var CT_major = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.MajorViolation);
                            if (CT_major != null)
                            {
                                CT_major.ThresholdValue = value;
                                RefreshResultThresholdStatus();
                                return true;
                            }
                            else
                            {
                                if (value != double.NaN)
                                {
                                    DataCache.AddConstraintThreshold(ConstraintThresholdNames.MajorViolation, ConstraintThresholdTypes.Violation, this, value);
                                    RefreshResultThresholdStatus();
                                    return true;
                                }
                                else
                                    return false;
                            }
                        }
                    case ConstraintThresholdNames.MinorViolation:
                        if (double.IsNaN(value)) //
                        {
                            var CT_minor = CTs.FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.MinorViolation);
                            if (CT_minor != null)
                                CT_minor.ThresholdValue = value;
                            return true;
                        }
                        if (ReferenceType == ReferenceTypes.Lower)
                        {
                            if (value > ReferenceValue)
                                return false;
                            var CT_major = CTs.FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.MajorViolation);
                            if (CT_major != null)
                                if (CT_major.ThresholdValue > value)
                                    CT_major.ThresholdValue = double.NaN;
                            var CT_minor = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.MinorViolation);
                            if (CT_minor != null)
                            {
                                CT_minor.ThresholdValue = value;
                                RefreshResultThresholdStatus();
                                return true;
                            }
                            else
                            {
                                if (value != double.NaN)
                                {
                                    DataCache.AddConstraintThreshold(ConstraintThresholdNames.MinorViolation, ConstraintThresholdTypes.Violation, this, value);
                                    RefreshResultThresholdStatus();
                                    return true;
                                }
                                else
                                    return false;
                            }
                        }
                        else
                        {
                            if (value < ReferenceValue)
                                return false;
                            var CT_major = CTs.FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.MajorViolation);
                            if (CT_major != null)
                                if (CT_major.ThresholdValue < value)
                                    CT_major.ThresholdValue = double.NaN;
                            var CT_minor = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.MinorViolation);
                            if (CT_minor != null)
                            {
                                CT_minor.ThresholdValue = value;
                                RefreshResultThresholdStatus();
                                return true;
                            }
                            else
                            {
                                if (value != double.NaN)
                                {
                                    DataCache.AddConstraintThreshold(ConstraintThresholdNames.MinorViolation, ConstraintThresholdTypes.Violation, this, value);
                                    RefreshResultThresholdStatus();
                                    return true;
                                }
                                else
                                    return false;
                            }
                        }
                    case ConstraintThresholdNames.Stop:
                        if (double.IsNaN(value)) //
                        {
                            var CT_stop = CTs.FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.Stop);
                            if (CT_stop != null)
                                CT_stop.ThresholdValue = value;
                            return true;
                        }
                        if (ReferenceType == ReferenceTypes.Lower)
                        {
                            if (value < ReferenceValue)
                                return false;
                            var CT_stop = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.Stop);
                            if (CT_stop != null)
                            {
                                CT_stop.ThresholdValue = value;
                                RefreshResultThresholdStatus();
                                return true;
                            }
                            else
                            {
                                if (value != double.NaN)
                                {
                                    DataCache.AddConstraintThreshold(ConstraintThresholdNames.Stop, ConstraintThresholdTypes.Goal, this, value);
                                    RefreshResultThresholdStatus();
                                    return true;
                                }
                                else
                                    return false;
                            }
                        }
                        else
                        {
                            if (value > ReferenceValue)
                                return false;
                            var CT_stop = DataCache.GetConstraintThresholdByConstraintId(ID).FirstOrDefault(x => x.ThresholdName == ConstraintThresholdNames.Stop);
                            if (CT_stop != null)
                            {
                                CT_stop.ThresholdValue = value;
                                RefreshResultThresholdStatus();
                                return true;
                            }
                            else
                            {
                                if (value != double.NaN)
                                {
                                    DataCache.AddConstraintThreshold(ConstraintThresholdNames.Stop, ConstraintThresholdTypes.Goal, this, value);
                                    RefreshResultThresholdStatus();
                                    return true;
                                }
                                else
                                    return false;
                            }
                        }
                    default:
                        return false;
                }
            }
            public void Delete()
            {
                ConstraintDeleted?.Invoke(this, ID);
            }
            private void OnECSIDChanged(object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName)
                {
                    case "ES":
                        {
                            if ((sender as ECSID).ID == PrimaryStructureID)
                                NotifyPropertyChanged("PrimaryStructureName");
                            else
                                NotifyPropertyChanged("SecondaryStructureName");
                            break;
                        }
                }

            }
            private void OnComponentDoseChanging(object sender, double OldReferenceDose)
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
                        ConstraintValue = ConstraintValue * CompDose / OldReferenceDose;
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

                        double CompDose = DataCache.GetComponent(ComponentID).ReferenceDose;
                        ReferenceValue = ReferenceValue * CompDose / OldReferenceDose;
                        NotifyPropertyChanged("ReferenceValue");
                    }
                }
                //UpdateProgress();
            }
            private void OnComponentFractionsChanging(object sender, EventArgs e)
            {
                if (BEDScalesWithComponent)
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
                    SecondaryStructureID = _SecondaryStructureID.ReferenceValue,
                    ReferenceValue = _ReferenceValue.ReferenceValue,
                    ReferenceType = _ReferenceType.ReferenceValue,
                    ConstraintType = _ConstraintType.ReferenceValue,
                    ConstraintScale = _ConstraintScale.ReferenceValue,
                    ReferenceScale = _ReferenceScale.ReferenceValue,
                    ConstraintValue = _ConstraintValue.ReferenceValue
                };
            }
        }
        [AddINotifyPropertyChangedInterface]
        public class ConstraintThreshold
        {
            public event EventHandler AssociatedConstraintDeleted;
            //public event EventHandler NewConstraintThreshold;
            private Constraint Con;
            public ConstraintThreshold(DbConThreshold DbO_in, Constraint Con_in)
            {
                ID = DbO_in.ID;
                Con = Con_in;
                ThresholdValue = DbO_in.ThresholdValue;
                _OriginalThresholdValue = DbO_in.ThresholdValue;
                ThresholdName = (ConstraintThresholdNames)DbO_in.DbConThresholdDef.Threshold;
                ThresholdType = (ConstraintThresholdTypes)DbO_in.DbConThresholdDef.ThresholdType;
            }
            public ConstraintThreshold(DbConThresholdDef DbCTdef, Constraint Constraint, double ThresholdValue_in)
            {
                ID = Ctr.IDGenerator();
                Con = Constraint;
                isCreated = true;
                ThresholdValue = ThresholdValue_in;
                _OriginalThresholdValue = ThresholdValue_in;
                ThresholdName = (ConstraintThresholdNames)DbCTdef.Threshold;
                ThresholdType = (ConstraintThresholdTypes)DbCTdef.ThresholdType;
            }
            public ConstraintThreshold(ConstraintThresholdNames Name, ConstraintThresholdTypes Type, Constraint Constraint, double ThresholdValue_in)
            {
                ID = Ctr.IDGenerator();
                Con = Constraint;
                isCreated = true;
                ThresholdValue = ThresholdValue_in;
                _OriginalThresholdValue = ThresholdValue_in;
                ThresholdName = Name;
                ThresholdType = Type;
            }
            public int ID { get; private set; }
            public int ConstraintID { get { return Con.ID; } }
            public ConstraintThresholdNames ThresholdName { get; set; }
            public ConstraintThresholdTypes ThresholdType { get; set; }
            private double _OriginalThresholdValue;
            public double ThresholdValue { get; set; }
            public string Description { get; set; }
            public bool isCreated { get; private set; } = false;
            public bool isModified
            {
                get
                {
                    if (Math.Abs(_OriginalThresholdValue - ThresholdValue) > 1E-5)
                        return true;
                    else
                        return false;
                }
            }
        }
        [AddINotifyPropertyChangedInterface]
        public class Artifact
        {
            public Artifact(DbArtifact DbA)
            {
                RefHU = DbA.HU;
                ToleranceHU = DbA.ToleranceHU;
                E = DataCache.GetECSID(DbA.DbECSID.ID);
            }
            public double RefHU { get; set; }
            public double ToleranceHU { get; set; }
            public double CheckHU
            {
                get
                {
                    return E.AssignedHU;
                }
            }
            public ECSID E { get; set; }
        }
        [AddINotifyPropertyChangedInterface]
        public class ComponentChecklist
        {
            public ComponentChecklist(DbComponentChecklist DbO)
            {
                ID = DbO.ID;
                ComponentID = DbO.ComponentID;
                TreatmentTechniqueType = (TreatmentTechniques)DbO.TreatmentTechniqueType;
                MinFields = DbO.MinFields;
                MaxFields = DbO.MaxFields;
                VMAT_MinFieldColSeparation = DbO.VMAT_MinFieldColSeparation;
                NumIso = DbO.NumIso;
                MinXJaw = DbO.MinXJaw;
                MaxXJaw = DbO.MaxXJaw;
                MinYJaw = DbO.MinYJaw;
                MaxYJaw = DbO.MaxYJaw;
                VMAT_JawTracking = (ParameterOptions)DbO.VMAT_JawTracking;
                Algorithm = (AlgorithmTypes)DbO.Algorithm;
                FieldNormalizationMode = (FieldNormalizationTypes)DbO.FieldNormalizationMode;
                AlgorithmResolution = DbO.AlgorithmResolution;
                PNVMin = DbO.PNVMin;
                PNVMax = DbO.PNVMax;
                SliceSpacing = DbO.SliceSpacing;
                HeterogeneityOn = DbO.HeterogeneityOn;
                //Couch
                SupportIndication = (ParameterOptions)DbO.SupportIndication;
                CouchSurface = DbO.CouchSurface;
                CouchInterior = DbO.CouchInterior;
                //Artifact
                foreach (DbArtifact DbA in DbO.Artifacts)
                {
                    Artifact A = new Artifact(DbA);
                    Artifacts.Add(A);
                }
            }

            public int ID { get; set; }
            public int ComponentID { get; set; }
            public TreatmentTechniques TreatmentTechniqueType { get; set; }
            public double MinFields { get; set; }
            public double MaxFields { get; set; }
            public double MinMU { get; set; }
            public double MaxMU { get; set; }
            public double VMAT_MinColAngle { get; set; }
            public double VMAT_MinFieldColSeparation { get; set; }
            public int NumIso { get; set; }
            public bool VMAT_SameStartStop { get; set; }
            public double MinXJaw { get; set; }
            public double MaxXJaw { get; set; }
            public double MinYJaw { get; set; }
            public double MaxYJaw { get; set; }
            public ParameterOptions VMAT_JawTracking { get; set; }
            public AlgorithmTypes Algorithm { get; set; }
            public FieldNormalizationTypes FieldNormalizationMode { get; set; }
            public double AlgorithmResolution { get; set; }
            public double PNVMin { get; set; }
            public double PNVMax { get; set; }
            public double SliceSpacing { get; set; }
            public bool HeterogeneityOn { get; set; }
            public ParameterOptions SupportIndication { get; set; }
            public double CouchSurface { get; set; }
            public double CouchInterior { get; set; }
            public ParameterOptions BolusClinicalIndication { get; set; }
            public double BolusClinicalHU { get; set; }
            public double BolusClinicalThickness { get; set; }
            public List<Artifact> Artifacts { get; set; } = new List<Artifact>();
        }
        [AddINotifyPropertyChangedInterface]
        public class Component : INotifyPropertyChanged
        {
            //Required notification class
            public event PropertyChangedEventHandler PropertyChanged;
            public Component(int CompId, int ProtocolId)
            {
                ID = CompId;
                ProtocolID = ProtocolId;
            }
            public Component(Component SC) // shallow clone
            {
                ID = Ctr.IDGenerator();
                ProtocolID = SC.ProtocolID;
                ComponentName = SC.ComponentName + "Copy";
                ComponentType = SC.ComponentType;
                NumFractions = SC.NumFractions;
                ReferenceDose = SC.ReferenceDose;
                DisplayOrder = SC.DisplayOrder;
                Checklist = SC.Checklist;
            }
            public Component(DbComponent DbO)
            {
                ID = DbO.ID;
                MinColOffset = DbO.MinColOffset;
                MinBeams = DbO.MinBeams;
                MaxBeams = DbO.MaxBeams;
                ComponentName = DbO.ComponentName;
                ComponentType = (ComponentTypes)DbO.ComponentType;
                NumFractions = DbO.NumFractions;
                ReferenceDose = DbO.ReferenceDose;
                DisplayOrder = DbO.DisplayOrder;
                ProtocolID = DbO.ProtocolID;
            }
            public Component(int ProtocolID, string ComponentName_in, int ReferenceFractions_in = 0, double ReferenceDose_in = 0, ComponentTypes ComponentType_in = ComponentTypes.Plan)
            {
                ID = Ctr.IDGenerator();
                ComponentName = ComponentName_in;
                ComponentType = ComponentType_in;
                NumFractions = ReferenceFractions_in;
                ReferenceDose = ReferenceDose_in;
                if (DataCache.GetAllComponents().Count() > 0)
                    DisplayOrder = DataCache.GetAllComponents().Select(x => x.DisplayOrder).Max() + 1;
                else
                    DisplayOrder = 1;
                ProtocolID = DataCache.CurrentProtocol.ID;
            }
            public event EventHandler ComponentChanged;
            public event EventHandler<int> ComponentDeleted;
            public event EventHandler ComponentExceptionLoaded;
            public event EventHandler NewComponentCommitting;
            public event EventHandler NewComponentCommitted;
            public event EventHandler<double> ReferenceDoseChanged;
            public event EventHandler ReferenceFractionsChanged;
            public class ComponentArgs : EventArgs
            {
                public object Value;
            }
            public int ID { get; private set; }
            public int DisplayOrder { get; set; }
            public int ProtocolID { get; private set; }
            public string ComponentName { get; set; }
            public ComponentTypes ComponentType { get; set; }
            public int MinBeams { get; set; } = -1;
            public int MaxBeams { get; set; } = -1;
            public int NumIso { get; set; } = 1;
            public int MinColOffset { get; set; } = 0;

            public List<Beam> GetBeams()
            {
                return DataCache.GetBeams(ID);
            }
            public bool isTDFmodified { get; private set; }
            private double _ReferenceDose;
            public double ReferenceDose
            {
                get
                {
                    return _ReferenceDose;
                }
                set
                {
                    if (Math.Abs(ReferenceDose - value) > 1E-5 && value > 0)
                    {
                        double PrevDose = _ReferenceDose;
                        _ReferenceDose = value;
                        ReferenceDoseChanged?.Invoke(this, _ReferenceDose);
                    }
                }
            }
            private int _NumFractions;
            public int NumFractions
            {
                get
                {
                    return _NumFractions;
                }
                set
                {
                    if (value != NumFractions && value > 0)
                    {
                        _NumFractions = value;
                        ReferenceFractionsChanged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            // Component Calc Overrides
            public List<ImagingProtocols> ImagingProtocolsAttached { get; set; } = new List<ImagingProtocols>();
            public List<Beam> Beams()
            {
                return DataCache.GetBeams(ID);
            }
            public ComponentChecklist Checklist { get; set; }
            private Dictionary<int, EventHandler> HandlerRegister = new Dictionary<int, EventHandler>();
            //public List<Constituent> GetConstituents()
            //{
            //    // includes ones flagged as deleted
            //    return DataCache.GetConstituentsByComponentID(ID).ToList();
            //}
            public void Delete()
            {
                ComponentDeleted?.Invoke(this, ID);
            }
        }
        [AddINotifyPropertyChangedInterface]
        //public class Constituent : INotifyPropertyChanged
        //{
        //    //Required notification class
        //    public event PropertyChangedEventHandler PropertyChanged;
        //    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        //    {
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //    public event EventHandler<int> ConstituentDeleting;
        //    //public Constituent(DbConstituent DbO)
        //    //{
        //    //    ID = DbO.ID;
        //    //    ComponentID = DbO.ComponentID;
        //    //    ConstituentCompID = DbO.ConstituentCompID;

        //    //}
        //    public Constituent(int ComponentID_in, int ConstituentCompID_in)
        //    {
        //        ID = Ctr.IDGenerator();
        //        ComponentID = ComponentID_in;
        //        ConstituentCompID = ConstituentCompID_in;
        //    }
        //    public int ID { get; private set; }
        //    public int ComponentID { get; set; }
        //    public int ConstituentCompID { get; set; }
        //    public void Delete()
        //    {
        //        ConstituentDeleting?.Invoke(this, ID);
        //    }
        //}
        public class ConstituentException
        {
            public string PropertyName;
            public double ChangeValue;
            public string ChangeString;
        }
        [AddINotifyPropertyChangedInterface]
        public class Assessment
        {
            //Events
            //public event PropertyChangedEventHandler PropertyChanged;
            public event EventHandler<int> AssessmentDeleted;
            public event EventHandler<int> ComponentUnlinked;
            //public event EventHandler AssessmentOverwriting;
            public event EventHandler<int> PlanMappingChanged;
            //public event EventHandler NewAssessmentCommitted;
            public event EventHandler<int> ComponentAssociationChange;
            //public event EventHandler AssessmentNameChanged;
            public Assessment(DbAssessment DbO)
            {
                ID = DbO.ID;
                PID = DbO.PID;
                DisplayOrder = DbO.DisplayOrder;
                PatientName = DbO.PatientName;
                Comments = DbO.Comments;
                AssessmentName = DbO.AssessmentName;
                SquintUser = DbO.SquintUser;
            }
            public Assessment(string AssessmentName_in, int DisplayOrder_in)
            {
                ID = Ctr.IDGenerator();
                DisplayOrder = DisplayOrder_in;
                AssessmentName = AssessmentName_in;
                ProtocolID = DataCache.CurrentProtocol.ID;
                PID = DataCache.Patient.Id;
                PatientName = string.Format("{0},{1}", DataCache.Patient.LastName, DataCache.Patient.FirstName);
                DateOfAssessment = String.Format("{0:g}", DateTimeOffset.Now);
                SquintUser = Ctr.SquintUser;
            }
            public int DisplayOrder { get; set; } = 0;
            public bool AutoCalculate { get; set; } = true;
            private const double binWidthRel = 0.01;
            private const double binWidthAbs = 0.01;
            public int ID { get; private set; }
            public int ProtocolID { get; private set; }
            public string PID { get; private set; }
            public string PatientName { get; private set; }
            public string SquintUser { get; private set; }
            public string DateOfAssessment { get; private set; }
            public string AssessmentName { get; set; }
            public string Comments { get; set; }
            public void RegisterPlan(ECPlan ECP)
            {
                ECP.PlanMappingChanged += OnECPMappingChanged;
                ECP.PlanDeleting += OnECPDeleting;
                PlanMappingChanged?.Invoke(this, ECP.ComponentID);
            }
            public void ClearComponentAssociation(int ComponentId)
            {
                ECPlan ECP = DataCache.GetAllPlans().Where(x => x.AssessmentID == ID && x.ComponentID == ComponentId).SingleOrDefault();
                if (ECP != null)
                {
                    ECP.Delete();
                }
            }

            public List<ComponentStatusCodes> StatusCodes(int ComponentID)
            {
                ECPlan ECP = DataCache.GetAllPlans().Where(x => x.AssessmentID == ID && x.ComponentID == ComponentID).SingleOrDefault();
                if (ECP != null)
                    return ECP.GetErrorCodes();
                else
                    return null;
            }
            public void Delete()
            {
                AssessmentDeleted?.Invoke(this, ID);
            }

            public List<ComponentStatusCodes> GetStatusCodes(int ComponentID)
            {
                ECPlan ECP = DataCache.GetAllPlans().Where(x => x.AssessmentID == ID && x.ComponentID == ComponentID).SingleOrDefault();
                if (ECP != null)
                    return ECP.GetErrorCodes();
                else
                    return null;
            }

            /*private bool ValidateComponentAssociations(Component SC)
            {
                // This performs error checking on the plans linked to this component and returns a status code indicating an error. 0 is no error
                ECPlan ECP = Plans.Values.Where(x => x.AssessmentID == ID && x.ComponentID == SC.ID).SingleOrDefault();
                if (!ComponentValidity.Keys.Contains(SC.ID))
                {
                    ComponentValidity.Add(SC.ID, true);
                    ComponentErrorCodeList.Add(SC.ID, new List<ComponentStatusCodes>());
                }
                else
                {
                    ComponentValidity[SC.ID] = true;
                    ComponentErrorCodeList[SC.ID] = new List<ComponentStatusCodes>();
                }
                if (ECP == null)
                {
                    ComponentErrorCodeList[SC.ID].Add(ComponentStatusCodes.NotLinked);  // no other errors possible
                    ComponentValidity[SC.ID] = false;
                    return false;   // statuscode 1, component not linked
                }
                if (ECP.InternalPlanID == 0)
                {
                    ComponentErrorCodeList[SC.ID].Add(ComponentStatusCodes.NotLinked);
                    ComponentValidity[SC.ID] = false;
                    return false;
                }
                // If it meets these criteria the component is evaluable
                ComponentErrorCodeList[SC.ID].Add(ComponentStatusCodes.Evaluable);
                //
                if (SC.ComponentType == ComponentTypes.Plan)
                {
                    AsyncPlan CurrentPlan = AsyncPlans[ECP.InternalPlanID];
                    if (Math.Abs((double)CurrentPlan.Dose - SC.ReferenceDose) > 1)
                    {
                        // Reference dose mismatch
                        ComponentErrorCodeList[SC.ID].Add(ComponentStatusCodes.ReferenceDoseMismatch);
                        ComponentValidity[SC.ID] = true;
                    }
                    if (Math.Abs((int)CurrentPlan.NumFractions - SC.NumFractions) > 0)
                    {
                        // Num fraction mismatch
                        ComponentErrorCodeList[SC.ID].Add(ComponentStatusCodes.NumFractionsMismatch);
                        ComponentValidity[SC.ID] = true;
                    }
                }
                else if (SC.ComponentType == ComponentTypes.Sum)
                {
                    // First confirm that all constituent components do not have errors
                    foreach (Constituent SCs in Constituents.Values)
                    {
                        //bool ConstituentValid = true;
                        //if (!ComponentValidity.Keys.Contains(SCs.ConstituentCompID)) // not yet evaluated
                        bool ConstituentValid = ValidateComponentAssociations(Components[SCs.ConstituentCompID]);
                        if (!ConstituentValid)
                        {
                            ComponentErrorCodeList[SC.ID].Add(ComponentStatusCodes.ConstituentError);
                            ComponentValidity[SC.ID] = false;
                            return false; // No further errors possible
                        }
                    }
                    AsyncPlan CurrentPlansum = AsyncPlans[ECP.InternalPlanID];
                    int numValidAssociations = 0;
                    foreach (int ConstituentCompID in Constituents.Values.Select(x => x.ConstituentCompID).ToList())
                    {
                        ECP = Plans.Values.Where(x => x.ComponentID == ConstituentCompID && x.AssessmentID == ID).SingleOrDefault();
                        if (ECP == null)
                        {
                            ComponentErrorCodeList[SC.ID].Add(ComponentStatusCodes.ConstituentNotInSum);
                            ComponentValidity[SC.ID] = false;
                        }
                        else
                        {
                            if (CurrentPlansum.ConstituentPlans.Select(x => x.UID).ToList().Contains(ECP.UID))
                            {
                                numValidAssociations++;
                            }
                            else
                            {
                                ComponentErrorCodeList[SC.ID].Add(ComponentStatusCodes.ConstituentNotInSum);
                                ComponentValidity[SC.ID] = false;
                            }
                        }
                    }
                    List<string> PlanSumUIDs = CurrentPlansum.ConstituentPlans.Select(x => x.UID).ToList();
                    if (numValidAssociations != PlanSumUIDs.Count)
                    {
                        ComponentErrorCodeList[SC.ID].Add(ComponentStatusCodes.PlansNotEqualToConstituents);// some constituents are linked to plans which are not in the sum
                        ComponentValidity[SC.ID] = false;
                    }
                }
                return ComponentValidity[SC.ID];
            }
            */
            //Events
            private void OnECPMappingChanged(object sender, int ComponentID)
            {
                PlanMappingChanged?.Invoke(this, ComponentID);
            }
            private void OnECPDeleting(object sender, int ComponentID)
            {
                ECPlan ECP = (sender as ECPlan);
                ECP.PlanMappingChanged -= OnECPMappingChanged;
                ECP.PlanDeleting -= OnECPDeleting;
            }
        }
        public class ECPlan : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private Dictionary<int, bool> ComponentValidity = new Dictionary<int, bool>(); // component ID to validity
            public event EventHandler<int> PlanDeleting;
            public event EventHandler<int> PlanMappingChanged;
            public ECPlan(DbPlan DbO)
            {
                ID = DbO.ID;
                LinkedPlan = DataCache.GetAsyncPlan(DbO.PlanName, DbO.CourseName, (ComponentTypes)DbO.PlanType);
                if (LinkedPlan == null)
                {
                    LoadWarning = true;
                    LoadWarningString = string.Format(@"{0}\{1} Not Found!", DbO.CourseName, DbO.PlanName);
                }
                else
                if (LinkedPlan.HistoryDateTime != DateTime.FromBinary(DbO.LastModified))
                {
                    LoadWarning = true;
                    LoadWarningString = ComponentStatusCodes.ChangedSinceLastSession.Display();
                }
                AssessmentID = DbO.AssessmentID;
                ComponentID = DbO.SessionComponentID;
                DataCache.GetComponent(ComponentID).PropertyChanged += OnComponentPropertyChanged;
            }
            public ECPlan(AsyncPlan p, int AssessmentID_in, int ComponentID_in)
            {
                ID = Ctr.IDGenerator();
                LinkedPlan = p;
                //if (p.PlanType == ComponentTypes.Plan)
                AssessmentID = AssessmentID_in;
                ComponentID = ComponentID_in;
                DataCache.GetComponent(ComponentID).PropertyChanged += OnComponentPropertyChanged;
            }
            public AsyncPlan LinkedPlan { get; set; }
            //public DateTime RefreshTime { get; set; }
            public bool LoadWarning { get; private set; } = false;
            public string LoadWarningString { get; private set; } = "";
            public string PID { get; private set; }
            public int AssessmentID { get; private set; }
            public int ComponentID { get; private set; }
            public ComponentTypes PlanType
            {
                get
                {
                    if (Linked)
                        return LinkedPlan.PlanType;
                    else
                        return ComponentTypes.Unset;
                }
            }
            public int ID { get; private set; }
            public string UID
            {
                get
                {
                    if (LinkedPlan != null)
                        return LinkedPlan.UID;
                    else
                        return "";
                }
            }
            public string PlanName
            {
                get
                {
                    if (LinkedPlan != null)
                        return LinkedPlan.Id;
                    else
                        return "";
                }
            }
            public string CourseName
            {
                get
                {
                    if (LinkedPlan != null)
                        return LinkedPlan.Course.Id;
                    else
                        return "";
                }
            }
            public IEnumerable<string> GetStructureNames
            {
                get
                {
                    if (LinkedPlan != null)
                        return LinkedPlan.StructureIds;
                    else
                        return Enumerable.Empty<string>();
                }
            }
            public bool? isStructureEmpty(string StructureID)
            {
                if (Linked)
                {
                    if (!LinkedPlan.StructureIds.Contains(StructureID))
                        return null;
                    else
                    {
                        if (LinkedPlan.Structures[StructureID].Volume == 0)
                            return true;
                        else
                            return false;
                    }
                }
                else return null;
            }
            public bool Linked
            {
                get { if (LinkedPlan != null) return true; else return false; }
            }
            //methods
            public System.Windows.Media.Color? GetStructureColor(string Id)
            {
                if (!Linked)
                    return null;
                if (LinkedPlan.Structures.ContainsKey(Id))
                    return LinkedPlan.Structures[Id].Color;
                else
                    return null;
            }
            public List<ComponentStatusCodes> GetErrorCodes()
            {
                return ValidateComponentAssociations();
            }
            public void Reassociate(AsyncPlan p, bool ClearWarnings)
            {
                if (ClearWarnings) // only want to do this if Reassociation is done by user
                {
                    LoadWarning = false;
                    LoadWarningString = "";
                }
                LinkedPlan = p;
                PlanMappingChanged?.Invoke(this, ComponentID);
                if (p == null)
                    return;
                ValidateComponentAssociations();
            }
            private List<ComponentStatusCodes> ValidateComponentAssociations()
            {
                // This performs error checking on the plans linked to this component and returns a status code indicating an error. 0 is no error
                List<ComponentStatusCodes> ErrorCodes = new List<ComponentStatusCodes>();
                if (!Linked)
                {
                    ErrorCodes.Add(ComponentStatusCodes.NotLinked);
                    return ErrorCodes;
                }
                if (!LinkedPlan.IsDoseValid)
                {
                    ErrorCodes.Add(ComponentStatusCodes.NoDoseDistribution);
                    return ErrorCodes;
                }
                ErrorCodes.Add(ComponentStatusCodes.Evaluable);
                Component SC = DataCache.GetComponent(ComponentID);
                if (SC.ComponentType == ComponentTypes.Plan)
                {
                    if (!LinkedPlan.IsDoseValid)
                        ErrorCodes.Add(ComponentStatusCodes.NoDoseDistribution);
                    if (Math.Abs((double)LinkedPlan.Dose - SC.ReferenceDose) > 1)
                    {
                        // Reference dose mismatch
                        ErrorCodes.Add(ComponentStatusCodes.ReferenceDoseMismatch);
                    }
                    if (Math.Abs((int)LinkedPlan.NumFractions - SC.NumFractions) > 0)
                    {
                        // Num fraction mismatch
                        ErrorCodes.Add(ComponentStatusCodes.NumFractionsMismatch);
                    }
                }
                if (LinkedPlan.StructureSetUID != CurrentStructureSet.UID)
                    ErrorCodes.Add(ComponentStatusCodes.StructureSetDiscrepancy);

                //else if (SC.ComponentType == ComponentTypes.Sum)
                //{
                //    // First confirm that all constituent components do not have errors
                //    int numValidAssociations = 0;
                //    //foreach (Constituent SCs in DataCache.GetConstituentsByComponentID(ComponentID))
                //    //{
                //    //    ECPlan ECPConstituent = DataCache.GetAllPlans().Where(x => x.ComponentID == SCs.ConstituentCompID && x.AssessmentID == AssessmentID).SingleOrDefault();
                //    //    if (ECPConstituent != null)
                //    //    {
                //    //        if (LinkedPlan.ConstituentPlans.Contains(ECPConstituent.LinkedPlan))
                //    //        {
                //    //            numValidAssociations++;
                //    //        }
                //    //        else
                //    //        {
                //    //            ErrorCodes.Add(ComponentStatusCodes.ConstituentNotInSum);
                //    //        }
                //    //    }
                //    //    else
                //    //    {
                //    //        ErrorCodes.Add(ComponentStatusCodes.ConstituentNotInSum);
                //    //    }

                //    //}
                //    List<string> PlanSumUIDs = LinkedPlan.ConstituentPlans.Select(x => x.UID).ToList();
                //    if (numValidAssociations != PlanSumUIDs.Count)
                //    {
                //        ErrorCodes.Add(ComponentStatusCodes.PlansNotEqualToConstituents);// some constituents are linked to plans which are not in the sum
                //    }
                //}
                return ErrorCodes;
            }
            public void Delete()
            {
                PlanDeleting?.Invoke(this, ID);
                PlanMappingChanged?.Invoke(this, ComponentID);
            }
            //Events
            private void OnComponentPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                ValidateComponentAssociations();
            }
        }
        public class StructureLabel : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public StructureLabel(DbStructureLabel DbO)
            {
                ID = DbO.ID;
                StructureType = (StructureTypes)DbO.StructureType;
                Code = DbO.Code;
                LabelName = DbO.StructureLabel;
                AlphaBetaRatio = DbO.AlphaBetaRatio;
                Description = DbO.Description;
                Designator = DbO.Designator;
            }
            public int ID { get; private set; }
            public StructureTypes StructureType { get; private set; }
            public string Code { get; private set; }
            public string LabelName { get; private set; }
            public double AlphaBetaRatio { get; private set; }
            public string Description { get; private set; }
            public string Designator { get; private set; }

        }
        //public class AssignedId : INotifyPropertyChanged
        //{
        //    public event PropertyChangedEventHandler PropertyChanged;
        //    public int SessionId { get; private set; }
        //    public int ECSID_Id { get; private set; }
        //    public string EclipseId { get; private set; }
        //    public string LabelName { get; private set; }
        //    public bool Valid { get; private set; } = false;
        //    public AssignedId(DbAssignedStructureId DbO)
        //    {
        //        SessionId = DbO.SessionId;
        //        ECSID_Id = DbO.DbECSID_Id;
        //        EclipseId = DbO.EclipseId;
        //    }
        //    public AssignedId(ECSID ECSID_in, AsyncPlan p, string StructureId)
        //    {
        //        if (!p.Structures.ContainsKey(StructureId))
        //        {
        //            LabelName = p.Structures[StructureId].Code;
        //            Valid = true;
        //        }
        //        else
        //        {
        //            Valid = false;
        //            LabelName = "";
        //        }
        //        SessionId = DataCache.CurrentSession.ID;
        //        EclipseId = StructureId;
        //        ECSID_Id = ECSID_in.ID;
        //    }
        //    public void UpdateAssignedId(ECSID ECSID_in, AsyncPlan p, string StructureId)
        //    {
        //        EclipseId = StructureId;
        //        ECSID_Id = ECSID_in.ID;
        //        if (!p.Structures.ContainsKey(StructureId))
        //        {
        //            LabelName = p.Structures[StructureId].Code;
        //            Valid = false;
        //        }
        //        else
        //        {
        //            Valid = false;
        //            LabelName = "";
        //        }

        //    }
        //}
        [AddINotifyPropertyChangedInterface]
        public class ECSID : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            public EventHandler NewECSIDCommitting;
            public EventHandler NewECSIDCommitted;
            public EventHandler ECSIDExceptionLoaded;
            public EventHandler ECSIDChanged;
            public EventHandler<int> ECSIDDeleting;
            public ECSID(DbECSID DbO)
            {
                ID = DbO.ID;
                ProtocolStructureName = DbO.ProtocolStructureName;
                StructureLabelID = DbO.StructureLabelID;
                DisplayOrder = DbO.DisplayOrder;
                if (DbO.DefaultEclipseAliases != null)
                    DefaultEclipseAliases = DbO.DefaultEclipseAliases.Split(';').ToList();
                if (DbO.DbStructureChecklist != null)
                {
                    CheckList = new StructureCheckList(DbO.DbStructureChecklist);
                }
                ES.PropertyChanged += OnESPropertyChanged;
            }
            public ECSID(string NewStructureName, int StructureLabelID_in = 1)
            {
                ID = Ctr.IDGenerator();
                ES.PropertyChanged += OnESPropertyChanged;
                CheckList = new StructureCheckList();
                ProtocolStructureName = NewStructureName;
                DisplayOrder = DataCache.GetAllECSIDs().Count() + 1;
                ProtocolID = DataCache.CurrentProtocol.ID;
                StructureLabelID = StructureLabelID_in;
                ES.PropertyChanged += OnESPropertyChanged;
            }
            public List<string> DefaultEclipseAliases { get; private set; } = new List<string>();
            public string GetStructureDescription(bool GetParentValues = false)
            {
                //if (GetParentValues)
                //{
                //    return
                //        string.Format("{0} (Label: {1}, \u03B1/\u03B2={2})", DbO.DbECSID_Parent.ProtocolStructureName,
                //        DataCache.SquintDb.Context.DbStructureLabels.Find(DbO.DbECSID_Parent.StructureLabelID).StructureLabel,
                //        DataCache.SquintDb.Context.DbStructureLabels.Find(DbO.DbECSID_Parent.StructureLabelID).AlphaBetaRatio);
                //}
                //else
                return string.Format("{0} (Label: {1}, \u03B1/\u03B2={2})", ProtocolStructureName,
                    DataCache.SquintDb.Context.DbStructureLabels.Find(StructureLabelID).StructureLabel,
                    DataCache.SquintDb.Context.DbStructureLabels.Find(StructureLabelID).AlphaBetaRatio);
            }
            public StructureLabel StructureLabel
            {
                get
                {
                    return DataCache.GetStructureLabel(StructureLabelID);
                }
            }
            public ExceptionTypes ExceptionType
            {
                get;
                private set;
            }
            public int ID { get; private set; }
            public int DisplayOrder { get; set; }
            public int ProtocolID { get; private set; }
            public int StructureLabelID { get; set; }
            public string ProtocolStructureName { get; set; }
            public StructureCheckList CheckList { get; set; }
            private EclipseStructure _ES = new EclipseStructure(null);
            public EclipseStructure ES
            {
                get
                {
                    return _ES;
                }
                set
                {
                    _ES.PropertyChanged -= OnESPropertyChanged;
                    _ES = value;
                    _ES.PropertyChanged += OnESPropertyChanged;
                }
            }
            public double AssignedHU
            {
                get
                {
                    return ES.HU();
                }
            }
            public string EclipseStructureLabel
            {
                get { return ES.LabelName; }
            }
            public string EclipseStructureName
            {
                get
                {
                    return ES.Id;
                }
            }
            public string EclipseStructureSetUID { get { return ES.StructureSetUID; } }
            public void Delete()
            {
                ECSIDDeleting?.Invoke(this, ID);
            }
            //public void ApplyAliasing(ECPlan ECP)
            //{
            //    // this will assign a structure Id if that structure exists in the plan ECP
            //    AsyncPlan P = ECP.LinkedPlan;
            //    List<int> rank = new List<int>();
            //    List<string> matchedIDs = new List<string>();
            //    int AliasRank = 0;
            //    foreach (string Alias in DefaultEclipseAliases)
            //    {
            //        foreach (string StructureId in P.StructureIds.OrderByDescending(x => x.Count()))
            //        {
            //            if (StructureId.ToUpper().Replace(@"_", @"").Replace(@"B_", @"") == Alias.ToUpper().Replace(@"_", @""))
            //            {
            //                rank.Add(AliasRank);
            //                matchedIDs.Add(StructureId);
            //                break;
            //            }
            //        }
            //        AliasRank++;
            //    }
            //    if (matchedIDs.Count > 0)
            //    {
            //        var sorted_Ids = matchedIDs.Zip(rank, (Ids, Order) => new { Ids, Order }).OrderBy(x => x.Order).Select(x => x.Ids);
            //        ES.Update(P.Structures[sorted_Ids.First()]);
            //    }
            //}
            public void ApplyAliasing(AsyncStructureSet SS)
            {
                // this will assign a structure Id if that structure exists in the plan ECP
                List<int> rank = new List<int>();
                List<string> matchedIDs = new List<string>();
                int AliasRank = 0;
                var ssid = SS.Id;
                var ssIds = SS.GetStructureIds();
                foreach (string Alias in DefaultEclipseAliases)
                {
                    foreach (string StructureId in SS.GetStructureIds().OrderByDescending(x => x.Count()))
                    {
                        if (StructureId.ToUpper().Replace(@"_", @"").Replace(@"B_", @"") == Alias.ToUpper().Replace(@"_", @""))
                        {
                            rank.Add(AliasRank);
                            matchedIDs.Add(StructureId);
                            break;
                        }
                    }
                    AliasRank++;
                }
                if (matchedIDs.Count > 0)
                {
                    var sorted_Ids = matchedIDs.Zip(rank, (Ids, Order) => new { Ids, Order }).OrderBy(x => x.Order).Select(x => x.Ids);
                    ES.Update(SS.GetStructure(sorted_Ids.First()));
                }
                else
                {
                    ES.Update(null);
                }
            }
            private void OnESPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ES")); // let subscribers know that the Eclipse structure internal properties have changed
            }
        }
        [AddINotifyPropertyChangedInterface]
        public class StructureCheckList
        {
            public StructureCheckList()
            {

            }
            public StructureCheckList(DbStructureChecklist DbO)
            {
                isPointContourChecked = DbO.isPointContourChecked;
                PointContourVolumeThreshold = DbO.PointContourThreshold;
            }
            public bool isPointContourChecked { get; set; } = false;
            public double PointContourVolumeThreshold { get; set; } = double.NaN;
        }
        public class ConThresholdDef
        {
            public ConThresholdDef(DbConThresholdDef DbO_in)
            {
                _DbO = DbO_in;
            }
            private DbConThresholdDef _DbO;
            public ConstraintThresholdNames ThresholdName
            {
                get
                {
                    return (ConstraintThresholdNames)_DbO.Threshold;
                }
            }
            public ConstraintThresholdTypes ThreshholdType
            {
                get
                {
                    return (ConstraintThresholdTypes)_DbO.ThresholdType;
                }
            }
        }
    }
}

