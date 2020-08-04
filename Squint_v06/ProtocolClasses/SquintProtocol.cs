using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VMS.TPS.Common.Model.API;
using System.Data.Entity;
using PropertyChanged;
using SquintScript.Extensions;
using SquintScript.Converters;
using System.IO.Ports;
using System.Net.Http.Headers;
using System.Xml.Serialization;
using System.IO;
using System.Windows;
using System.Security.Permissions;

namespace SquintScript
{
    public static partial class Ctr
    {
        public class ImagingProtocol
        {
            public int ID { get; set; }
            public ImagingProtocols Type { get; set; }
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
                ThresholdStatus = ReferenceThresholdTypes.Unset;
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
            public ReferenceThresholdTypes ThresholdStatus;
            private void OnConstraintPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                SetResultString();
            }
        }
        //[AddINotifyPropertyChangedInterface]
        //public class ConstraintThreshold
        //{
        //    public event EventHandler AssociatedConstraintDeleted;
        //    //public event EventHandler NewConstraintThreshold;
        //    private Constraint Con;
        //    public ConstraintThreshold(DbConThreshold DbO_in, Constraint Con_in)
        //    {
        //        ID = DbO_in.ID;
        //        Con = Con_in;
        //        ThresholdValue = DbO_in.ThresholdValue;
        //        _OriginalThresholdValue = DbO_in.ThresholdValue;
        //        ThresholdName = (ReferenceThresholdTypes)DbO_in.DbConThresholdDef.Threshold;
        //        ThresholdType = (ConstraintThresholdTypes)DbO_in.DbConThresholdDef.ThresholdType;
        //    }
        //    public ConstraintThreshold(DbConThresholdDef DbCTdef, Constraint Constraint, double ThresholdValue_in)
        //    {
        //        ID = Ctr.IDGenerator();
        //        Con = Constraint;
        //        isCreated = true;
        //        ThresholdValue = ThresholdValue_in;
        //        _OriginalThresholdValue = ThresholdValue_in;
        //        ThresholdName = (ReferenceThresholdTypes)DbCTdef.Threshold;
        //        ThresholdType = (ConstraintThresholdTypes)DbCTdef.ThresholdType;
        //    }
        //    public ConstraintThreshold(ReferenceThresholdTypes Name, ConstraintThresholdTypes Type, Constraint Constraint, double ThresholdValue_in)
        //    {
        //        ID = Ctr.IDGenerator();
        //        Con = Constraint;
        //        isCreated = true;
        //        ThresholdValue = ThresholdValue_in;
        //        _OriginalThresholdValue = ThresholdValue_in;
        //        ThresholdName = Name;
        //        ThresholdType = Type;
        //    }
        //    public int ID { get; private set; }
        //    public int ConstraintID { get { return Con.ID; } }
        //    public ReferenceThresholdTypes ThresholdName { get; set; }
        //    public ConstraintThresholdTypes ThresholdType { get; set; }
        //    private double _OriginalThresholdValue;
        //    public double ThresholdValue { get; set; }
        //    public string Description { get; set; }
        //    public bool isCreated { get; private set; } = false;
        //    public bool isModified
        //    {
        //        get
        //        {
        //            if (Math.Abs(_OriginalThresholdValue - ThresholdValue) > 1E-5)
        //                return true;
        //            else
        //                return false;
        //        }
        //    }
        //}
        [AddINotifyPropertyChangedInterface]
        public class Artifact
        {
            public Artifact(DbArtifact DbA)
            {
                RefHU = new TrackedValue<double?>(DbA.HU);
                ToleranceHU = new TrackedValue<double?>(DbA.ToleranceHU);
                E = DataCache.GetProtocolStructure(DbA.DbProtocolStructure.ID);
            }
            public TrackedValue<double?> RefHU { get; set; }
            public TrackedValue<double?> ToleranceHU { get; set; }
            public ProtocolStructure E { get; set; }
        }
        [AddINotifyPropertyChangedInterface]
        public class BolusDefinition
        {
            public BolusDefinition(DbBolus DbB)
            {
                HU = new TrackedValue<double>(DbB.HU);
                Thickness = new TrackedValue<double>(DbB.Thickness);
                ToleranceThickness = new TrackedValue<double>(DbB.ToleranceThickness);
                ToleranceHU = new TrackedValue<double>(DbB.ToleranceHU);
                Indication = new TrackedValue<ParameterOptions>((ParameterOptions)DbB.Indication);
            }
            public TrackedValue<double> HU { get; set; }
            public TrackedValue<double> Thickness { get; set; }
            public TrackedValue<ParameterOptions> Indication { get; set; }
            public TrackedValue<double> ToleranceThickness { get; set; }
            public TrackedValue<double> ToleranceHU { get; set; }
        }

        public class ConstituentException
        {
            public string PropertyName;
            public double ChangeValue;
            public string ChangeString;
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
        [AddINotifyPropertyChangedInterface]
        public class StructureCheckList
        {
            public StructureCheckList()
            {

            }
            public StructureCheckList(DbStructureChecklist DbO)
            {
                isPointContourChecked = new TrackedValue<bool?>(DbO.isPointContourChecked);
                PointContourVolumeThreshold = new TrackedValue<double?>(DbO.PointContourThreshold);
            }
            public TrackedValue<bool?> isPointContourChecked { get; set; } = new TrackedValue<bool?>(false);
            public TrackedValue<double?> PointContourVolumeThreshold { get; set; } = new TrackedValue<double?>(double.NaN);
        }

        public interface IReferenceThreshold : ITrackedValue
        {
            ReferenceThresholdCalculationTypes ReferenceThresholdCalculationType { get; }
            double ReferenceValue { get; }
            double? Stop { get; set; }
            double? MajorViolation { get; set; }
            double? MinorViolation { get; set; }
            double? ReferenceStop { get; }
            double? ReferenceMajorViolation { get;}
            double? ReferenceMinorViolation { get; }

            string DataPath { get; }
        }


        public class XYPoint
        {
            [XmlAttribute]
            public double X { get; set; }
            [XmlAttribute]
            public double Y { get; set; }
        }
        public class ThresholdInterpolator
        {
            [XmlArray]
            public XYPoint[] MajorDeviation { get; set; }
            [XmlArray]
            public XYPoint[] MinorDeviation { get; set; }
            [XmlArray]
            public XYPoint[] Stop { get; set; }
        }
        public class FixedThreshold : IReferenceThreshold, ITrackedValue
        {
            public ReferenceThresholdCalculationTypes ReferenceThresholdCalculationType { get; private set; } = ReferenceThresholdCalculationTypes.Fixed;
            
            public bool IsChanged
            {
                get
                {
                    if (_MajorViolation.IsChanged || _MinorViolation.IsChanged || _Stop.IsChanged)
                        return true;
                    else
                        return false;
                }
            }
            public double ReferenceValue 
            {
                get
                {
                    if (MinorViolation != null)
                        return (double)MinorViolation;
                    if (MajorViolation != null)
                        return (double)MajorViolation;
                    else
                        return double.NaN;
                } 
            }

            private TrackedValue<double?> _MajorViolation { get; set; } 
            private TrackedValue<double?> _MinorViolation { get; set; }
            private TrackedValue<double?> _Stop { get; set; }

            public double? ReferenceMajorViolation { get { return _MajorViolation.ReferenceValue; } }
            public double? ReferenceMinorViolation { get { return _MinorViolation.ReferenceValue; } }
            public double? ReferenceStop { get { return _Stop.ReferenceValue; } }

            public double? MajorViolation
            {
                get
                { 
                    return _MajorViolation.Value; 
                }
                set
                {
                    _MajorViolation.Value = value;
                }
            }
            public double? MinorViolation
            {
                get
                {
                    return _MinorViolation.Value;
                }
                set
                {
                    _MinorViolation.Value = value;
                }
            }
            public double? Stop
            {
                get
                {
                    return _Stop.Value;
                }
                set
                {
                    _Stop.Value = value;
                }
            }
            public string DataPath { get; } = "";

            public FixedThreshold(double? major = null, double? minor = null, double? stop = null)
            {
                _MajorViolation = new TrackedValue<double?>(major);
                _MinorViolation = new TrackedValue<double?>(minor);
                _Stop = new TrackedValue<double?>(stop); 
            }
        }
        public class InterpolatedThreshold : IReferenceThreshold, ITrackedValue
        {
            public ReferenceThresholdCalculationTypes ReferenceThresholdCalculationType { get; private set; } = ReferenceThresholdCalculationTypes.Interpolated;
            private double Interpolate(double Xi, double[] X, double[] Y)
            {
                if (X.Length < 2)
                {
                    return Y.FirstOrDefault();
                }
                else
                {
                    double X1 = X[0];
                    double X2 = X[1];
                    double Y1 = Y[0];
                    double Y2 = Y[1];
                    int c = 2;
                    while (Xi >= X2 && c < X.Length)
                    {
                        X1 = X2;
                        X2 = X[c];
                        Y1 = Y2;
                        Y2 = Y[c];
                        c++;
                    }
                    double lever = (Xi - X1) / (X2 - X1);
                    return Math.Round( ((Y2 - Y1) * lever + Y1) *100)/100;
                }
            }

            public bool IsChanged
            {
                get { return false; } // cannot change this threshold type
            }
            public double ReferenceValue
            {
                get
                {
                    if (MinorViolation != null)
                        return (double)_MinorViolation;
                    if (MajorViolation != null)
                        return (double)_MajorViolation;
                    else
                        return double.NaN;
                }
            }
            public double? ReferenceMajorViolation { get { return _MajorViolation; } }
            public double? ReferenceMinorViolation { get { return _MinorViolation; } }
            public double? ReferenceStop { get { return _Stop; } }

            private double? _MajorViolation { get; set; }
            private double? _MinorViolation { get; set; }
            private double? _Stop { get; set; }
            public double? MajorViolation
            {
                get
                {
                    return _MajorViolation;
                }
                set { }
            }
        
            public double? MinorViolation { get { return _MinorViolation; } set { } }
            public double? Stop { get { return _Stop; } set { } }
            public int ReferenceProtocolStructureId;
            public InterpolatedThreshold(string dataPath, double? xi = null)
            {
                DataPath = dataPath;

                // Load data
                XmlSerializer ser = new XmlSerializer(typeof(ThresholdInterpolator));
                ThresholdInterpolator Data;
                using (var datastream = new StreamReader(DataPath))
                {
                    Data = (ThresholdInterpolator)ser.Deserialize(datastream);
                }
                if (Data != null)
                {
                    if (Data.MajorDeviation != null)
                    {
                        XMajor = Data.MajorDeviation.OrderBy(x=>x.X).Select(x => x.X).ToArray();
                        YMajor = Data.MajorDeviation.OrderBy(x=>x.X).Select(x => x.Y).ToArray();
                    }
                    if (Data.MinorDeviation != null)
                    {
                        XMinor = Data.MinorDeviation.OrderBy(x => x.X).Select(x => x.X).ToArray();
                        YMinor = Data.MinorDeviation.OrderBy(x => x.X).Select(x => x.Y).ToArray();
                    }
                    if (Data.Stop != null)
                    {
                        XStop = Data.Stop.OrderBy(x => x.X).Select(x => x.X).ToArray();
                        YStop = Data.Stop.OrderBy(x => x.X).Select(x => x.Y).ToArray();
                    }
                }
                if (xi != null)
                {
                    Xi = (double)xi;
                }

            }

            public string DataPath { get; private set; } = @"H:\Physics\CN\Software\Squint\XML ConstraintData\SABR_Lung_CI.xml";
            private double _Xi;
            private double[] XMajor;
            private double[] YMajor;
            private double[] XMinor;
            private double[] YMinor;
            private double[] XStop;
            private double[] YStop;
            public double Xi
            {
                get { return _Xi; }
                set
                {
                    if (_Xi != value)
                    {
                        _Xi = value;
                        // recalc
                        if (XMajor != null && YMajor != null)
                            _MajorViolation = Interpolate(_Xi, XMajor, YMajor);
                        if (XMinor != null && YMinor != null)
                            _MinorViolation = Interpolate(_Xi, XMinor, YMinor);
                        if (XStop != null && YStop != null)
                            _Stop = Interpolate(_Xi, XStop, YStop);
                    }
                }
            }
        }
    }
}

