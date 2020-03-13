using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Windows;
using System.Data.Common;
using System.Linq;
using System.IO;
using PropertyChanged;
using Npgsql;
using NpgsqlTypes;
using System.Xml;
using System.Xml.Serialization;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using VMSTypes = VMS.TPS.Common.Model.Types;
using ESAPI = VMS.TPS.Common.Model.API.Application;
using System.Text.RegularExpressions;
using System.Data.Entity;

namespace SquintScript
{

    public static class VersionContextConnection
    {
        //<add name="SquintdBModel.config" connectionString="Server=sprtqacn001;Port=5432;Database=Squint_v05_April_Clinical;User Id=postgres;Password=bccacn;" providerName="Npgsql" />
        private static string providerName = "Npgsql"; //Get this
        private static string databaseName = "Squint_v052_Clinical";
        private static string userName = "postgres";
        private static string password = "bccacn";
        private static string host = "sprtqacn001";
        private static int port = 5432;

        public static DbConnection GetDatabaseConnection()
        {
            var conn = DbProviderFactories.GetFactory(providerName).CreateConnection();
            conn.ConnectionString = $"Server={host}; " + $"Port={port}; " +
                $"User Id={userName};" + $"Password={password};" + $"Database={databaseName};";
            return conn;
        }
        public static string ConnectionString()
        {
            return $"Server={host}; " + $"Port={port}; " + $"User Id={userName};" + $"Password={password};" + $"Database={databaseName};";
        }
    }

    public static class UserProtocolFormat
    {
        public static int version = 1;
    }

    public static class ProtocolFilterSettings
    {
        public static TreatmentCentres TreatmentCentre = TreatmentCentres.Unset;
        public static TreatmentSites TreatmentSite = TreatmentSites.Unset;
        public static ApprovalLevels ApprovalLevel = ApprovalLevels.Unset;
        public static ProtocolTypes ProtocolType = ProtocolTypes.Unset;
        public static int CurrentAuthorId = 0;
    }

    public static partial class Ctr
    {
        private static void RaiseEventOnUIThread(Delegate theEvent, object[] args)  // class which helps pass events back to UI thread
        {
            foreach (Delegate d in theEvent.GetInvocationList())
            {
                ISynchronizeInvoke syncer = d.Target as ISynchronizeInvoke;
                if (syncer == null)
                {
                    d.DynamicInvoke(args);
                }
                else
                {
                    syncer.Invoke(d, args);  // cleanup omitted.  Changed from BeginInvoke as that crashes while UI held in ShowDialog for ProtocolBuilder
                }
            }
        }
    }

    public static partial class Ctr
    {
        //UI classes
        [AddINotifyPropertyChangedInterface]
        public class ProtocolView : ObservableObject
        {
            public ProtocolView()
            {
            }
            public ProtocolView(Protocol P)
            {
                _P = P;
                P.PropertyChanged += P_PropertyChanged;
            }
            private void P_PropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                RaisePropertyChangedEvent(e.PropertyName); // Protocol Node listens for this and redraws
            }
            private Protocol _P;
            public string ProtocolName
            {
                get
                {
                    if (_P != null)
                        return _P.ProtocolName;
                    else
                        return "";
                }
                set
                {
                    _P.ProtocolName = value;
                }
            }
            public int ProtocolID
            {
                get
                {
                    if (_P != null)
                        return _P.ID;
                    else
                        return 0;
                }
            }
            public string LastModifiedBy
            {
                get
                {
                    if (_P != null)
                        return _P.LastModifiedBy;
                    else
                        return "";
                }
            }
            public ApprovalLevels ApprovalLevel
            {
                get
                {
                    if (_P != null)
                        return _P.ApprovalLevel;
                    else
                        return ApprovalLevels.Unset;
                }
                set { _P.ApprovalLevel = value; }
            }
            public ProtocolTypes ProtocolType
            {
                get
                {
                    if (_P != null)
                        return _P.ProtocolType;
                    else
                        return ProtocolTypes.Unset;
                }
                set { _P.ProtocolType = value; }
            }
            public TreatmentCentres TreatmentCentre
            {
                get
                {
                    if (_P != null)
                        return _P.TreatmentCentre;
                    else
                        return TreatmentCentres.Unset;
                }
                set { _P.TreatmentCentre = value; }
            }
            public TreatmentSites TreatmentSite
            {
                get
                {
                    if (_P != null)
                        return _P.TreatmentSite;
                    else
                        return TreatmentSites.Unset;
                }
                set { _P.TreatmentSite = value; }
            }
            public TreatmentIntents TreatmentIntent { get { return _P.TreatmentIntent; } }
            public bool HeterogeneityOn { get { return _P.HeterogeneityOn; } }
            public AlgorithmTypes Algorithm { get { return _P.Algorithm; } }
            public FieldNormalizationTypes FieldNormalizationMode { get { return _P.FieldNormalizationMode; } }
            public double AlgorithmResolution { get { return _P.AlgorithmResolution; } }
            public double SliceSpacing { get { return _P.SliceSpacing; } }
            public double PNVMin { get { return _P.PNVMin; } }
            public double PNVMax { get { return _P.PNVMax; } }
        }
        [AddINotifyPropertyChangedInterface]
        public class EclipseStructure : ObservableObject
        {
            //public event PropertyChangedEventHandler PropertyChanged;
            public string Id
            {
                get
                {
                    if (_A == null)
                        return "";
                    return _A.Id;
                }
            }
            private AsyncStructure _A;
            public EclipseStructure(AsyncStructure A)
            {
                _A = A;
                RaisePropertyChangedEvent();
            }
            public string StructureSetUID
            {
                get
                {
                    if (_A == null)
                        return "";
                    return _A.StructureSetUID;
                }
            }
            public string LabelName
            {
                get
                {
                    if (_A == null)
                        return "";
                    else
                    {
                        if (_A.Code == null)
                            return "Unset";
                        else
                            return DataCache.GetLabelByCode(_A.Code);
                    }
                }
            }
            public async Task<int> VMS_NumParts()
            {
                if (_A == null)
                    return -1;
                return await _A.GetVMS_NumParts();
            }
            public async Task<List<double>> PartVolumes()
            {
                if (_A == null)
                    return null;
                return await _A.GetPartVolumes();

            }
            public async Task<int> NumParts()
            {
                if (_A == null)
                    return -1;
                return await _A.GetNumSeperateParts();
            }
            public double HU()
            {
                if (_A == null)
                    return -1;
                return _A.HU;
            }
            public void Update(AsyncStructure A)
            {
                _A = A;
                RaisePropertyChangedEvent();
            }
        }

        [AddINotifyPropertyChangedInterface]
        public class StructureSetHeader
        {
            public string LinkedPlanId { get; private set; }
            public string StructureSetId { get; private set; }
            public string StructureSetUID { get; private set; }
            public StructureSetHeader(string structureSetId, string structureSetUID, string linkedPlanId)
            {
                StructureSetId = structureSetId;
                StructureSetUID = structureSetUID;
                LinkedPlanId = linkedPlanId;
            }
        }
        //public class StructureView
        //{
        //    public PropertyChangedEventHandler StructureChanged;
        //    private ECSID _ECSID;
        //    public StructureView(ECSID ECSID)
        //    {
        //        _ECSID = ECSID;
        //        ECSID.PropertyChanged += ECSID_PropertyChanged;
        //        ECSID.ECSIDDeleting += OnECSIDDeleted;
        //        Structures.Add(ID, this);
        //    }
        //    private void ECSID_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //    {
        //        StructureChanged?.Invoke(this, e);
        //    }
        //    private void OnECSIDDeleted(object sender, int ID)
        //    {
        //        Structures.Remove(ID);
        //    }
        //    public string StructureDescription(bool GetParent = false)
        //    {
        //        return _ECSID.GetStructureDescription(GetParent);
        //    }
        //    public List<string> GetAliases()
        //    {
        //        return _ECSID.DefaultEclipseAliases;
        //    }
        //    public int ID
        //    {
        //        get
        //        { return _ECSID.ID; }
        //    }
        //    public int DisplayOrder
        //    {
        //        get
        //        {
        //            return _ECSID.DisplayOrder;
        //        }
        //    }
        //    public string ProtocolStructureName
        //    {
        //        get { return _ECSID.ProtocolStructureName; }
        //        set { _ECSID.ProtocolStructureName = value; }
        //    }
        //    public EclipseStructure EclipseStructure
        //    {
        //        get
        //        {
        //            return _ECSID.ES;
        //        }
        //        set
        //        {
        //            _ECSID.ES = value;
        //        }
        //    }
        //    public ExceptionTypes ExceptionType
        //    {
        //        get { return _ECSID.ExceptionType; }
        //    }
        //    public StructureLabelView StructureLabel
        //    {
        //        get
        //        {
        //            return GetStructureLabelView(_ECSID.StructureLabelID);
        //        }
        //    }
        //    public StructureLabelView AssignedStuctureLabel
        //    {
        //        get
        //        {
        //            return GetStructureLabelView(_ECSID.StructureLabelID);
        //        }
        //    }
        //    public StructureCheckList Checklist
        //    {
        //        get { return _ECSID.CheckList; }
        //    }
        //    public void SetStructureLabel(int StructureLabelID)
        //    {
        //        _ECSID.StructureLabelID = StructureLabelID;
        //    }

        //}
        //public class ComponentView
        //{
        //    public EventHandler ComponentChanged;
        //    public Component(Component _SC)
        //    {
        //        SC = _SC;
        //        Checklist = _SC.Checklist;
        //        SC.PropertyChanged += SC_PropertyChanged;
        //        SC.ComponentDeleted += OnComponentDeleted;
        //        ComponentViews.Add(SC.ID, this);
        //    }
        //    private void SC_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //    {
        //        ComponentChanged.Raise(this, EventArgs.Empty);
        //    }
        //    private Component SC;
        //    public int ID { get { return SC.ID; } }
        //    public int NumConstraints { get { return DataCache.GetConstraintsInComponent(ID).Count(); } }
        //    private bool _ApplyBEDScaling = true;
        //    public bool ApplyBEDScaling
        //    {
        //        get { return _ApplyBEDScaling; }
        //        set
        //        {
        //            foreach (Constraint Con in DataCache.GetConstraintsInComponent(ID))
        //            {
        //                Con.BEDScalesWithComponent = value;
        //            }
        //        }
        //    }
        //    public List<ImagingProtocols> ImagingProtocolsAttached { get { return SC.ImagingProtocolsAttached; } }
        //    private bool _ApplyAbsLowerConstraintScaling = true;
        //    public bool ApplyAbsLowerConstraintScaling
        //    {
        //        get { return _ApplyAbsLowerConstraintScaling; }
        //        set
        //        {
        //            foreach (Constraint Con in DataCache.GetConstraintsInComponent(ID))
        //            {
        //                Con.LowerConstraintDoseScalesWithComponent = value;
        //            }
        //        }
        //    }
        //    public ComponentTypes ComponentType { get { return SC.ComponentType; } }
        //    public List<Beam> GetBeams()
        //    {
        //        return DataCache.GetBeams(ID);
        //    }
        //    public string ComponentName
        //    {
        //        get { return SC.ComponentName; }
        //        set { SC.ComponentName = value; }
        //    }
        //    public async Task SetFractionsAsync(int NumFractions)
        //    {
        //        try
        //        {
        //            await Task.Run(() => SC.NumFractions = NumFractions);
        //        }
        //        catch (Exception ex)
        //        {
        //            string debugme = "hi";
        //        }
        //        //update constraint thresholds

        //    }
        //    public int NumFractions
        //    {
        //        get { return SC.NumFractions; }
        //    }

        //    public int NumIsocentres { get { return SC.NumIso; } }
        //    public int MinBeams { get { return SC.MinBeams; } }
        //    public int MaxBeams { get { return SC.MaxBeams; } }
        //    public int MinColOffset { get { return SC.MinColOffset; } }
        //    public double ReferenceDose
        //    {
        //        get { return SC.ReferenceDose; }
        //        set
        //        {
        //            SC.ReferenceDose = value;
        //        }
        //    }
        //    public int DisplayOrder
        //    {
        //        get { return SC.DisplayOrder; }
        //    }
        //    public ComponentChecklist Checklist;
        //    // Events
        //    private void OnComponentDeleted(object sender, int ID)
        //    {
        //        DataCache.ComponentDeleted -= OnComponentDeleted;
        //        ComponentViews.Remove(ID);
        //    }
        //}
        public class TxFieldItem
        {
            public class BolusInfo
            {
                public string Id;
                public double HU;
            }
            public TxFieldItem(string RefCourseId, string RefPlanId, VMS.TPS.Common.Model.API.Beam Field = null, VMS.TPS.Common.Model.Types.PatientOrientation O = VMS.TPS.Common.Model.Types.PatientOrientation.NoOrientation)
            {
                CourseId = RefCourseId;
                PlanId = RefPlanId;
                if (Field != null)
                {
                    Id = Field.Id;
                    switch (Field.Technique.Id)
                    {
                        case "ARC":
                            Type = FieldType.ARC;
                            break;
                        case "SRS_ARC":
                            Type = FieldType.ARC;
                            break;
                        case "STATIC":
                            Type = FieldType.STATIC;
                            break;
                    }
                    switch (Field.EnergyModeDisplayName)
                    {
                        case "6X":
                            Energy = Energies.Photons6;
                            break;
                        case "10X-FFF":
                            Energy = Energies.Photons10FFF;
                            break;
                        case "15X":
                            Energy = Energies.Photons15;
                            break;
                        case "10X":
                            Energy = Energies.Photons10;
                            break;
                    }
                    GantryDirection = Field.GantryDirection;
                    Isocentre = Field.IsocenterPosition;
                    CouchRotation = Field.ControlPoints.FirstOrDefault().PatientSupportAngle;
                    GantryStart = Field.ControlPoints.FirstOrDefault().GantryAngle;
                    GantryEnd = Field.ControlPoints.LastOrDefault().GantryAngle;
                    CollimatorAngle = Field.ControlPoints.FirstOrDefault().CollimatorAngle;
                    ToleranceTable = Field.ToleranceTableLabel;
                    foreach (Bolus Bolus in Field.Boluses)
                    {
                        BolusInfo BI = new BolusInfo();
                        BI.Id = Bolus.Id;
                        BI.HU = Bolus.MaterialCTValue;
                        BolusInfos.Add(BI);
                    }
                    Orientation = O;
                    var CP = Field.ControlPoints;
                    if (CP.Count > 0)
                    {
                        X1max = CP.Max(x => Math.Abs(x.JawPositions.X1)) / 10; // cm
                        X2max = CP.Max(x => x.JawPositions.X2) / 10;
                        Y1max = CP.Max(x => Math.Abs(x.JawPositions.Y1)) / 10;
                        Y2max = CP.Max(x => x.JawPositions.Y2) / 10;
                        X1min = CP.Min(x => Math.Abs(x.JawPositions.X1)) / 10;
                        X2min = CP.Min(x => x.JawPositions.X2) / 10;
                        Y1min = CP.Min(x => Math.Abs(x.JawPositions.Y1)) / 10;
                        Y2min = CP.Min(x => x.JawPositions.Y2) / 10;
                        Xmax = CP.Max(x => Math.Abs(x.JawPositions.X1) + x.JawPositions.X2) / 10;
                        Ymax = CP.Max(x => Math.Abs(x.JawPositions.Y1) + x.JawPositions.Y2) / 10;
                        Xmin = CP.Min(x => Math.Abs(x.JawPositions.X1) + x.JawPositions.X2) / 10;
                        Ymin = CP.Min(x => Math.Abs(x.JawPositions.Y1) + x.JawPositions.Y2) / 10;
                        if ((Math.Abs(X1max - X1min) > 1E-5) || (Math.Abs(X2max - X2min) > 1E-5) || (Math.Abs(Y1max - Y1min) > 1E-5) || (Math.Abs(Y2max - Y2min) > 1E-5))
                            isJawTracking = true;

                    }
                    if (Field.Meterset.Unit == DosimeterUnit.MU)
                        MU = Field.Meterset.Value;
                    else
                    {
                        throw new Exception(@"Meterset unit is not MU");
                    }

                }
                if (Type == FieldType.Unset)
                {
                    Warning = true;
                    WarningMessage = "Name / angle mismatch";
                }

            }
            private VMS.TPS.Common.Model.Types.PatientOrientation Orientation = VMS.TPS.Common.Model.Types.PatientOrientation.NoOrientation;
            private Regex AntField = new Regex("ANT");
            private Regex PostField = new Regex("POST");
            private Regex LeftField = new Regex("LLAT");
            private Regex RightField = new Regex("RLAT");
            private Regex CBCT = new Regex("CBCT");
            public FieldType Type { get; private set; } = FieldType.Unset;
            public string TypeString
            {
                get { return Type.Display(); }
            }
            public string CourseId;
            public string PlanId;
            public GantryDirection GantryDirection { get; set; }
            public Energies Energy { get; set; }
            public string Id { get; set; } = "Default Field";
            public double MU { get; set; } = 0;
            public double GantryEnd { get; set; }
            public double GantryStart { get; set; } = 0;
            public double CollimatorAngle { get; set; } = 0;
            public double CouchRotation { get; set; }

            public VVector Isocentre { get; set; }
            public double Xmax { get; set; }
            public double Ymax { get; set; }
            public double Ymin { get; set; }
            public double Xmin { get; set; }
            public double X1max { get; set; }
            public double Y1max { get; set; }
            public double X2max { get; set; }
            public double Y2max { get; set; }
            public double X1min { get; set; }
            public double Y1min { get; set; }
            public double X2min { get; set; }
            public double Y2min { get; set; }
            public string ToleranceTable { get; set; }
            public List<BolusInfo> BolusInfos { get; set; } = new List<BolusInfo>();
            public bool Warning { get; set; } = false;
            public string WarningMessage { get; set; } = "";
            public bool isJawTracking { get; set; } = false;
        }
        public class ImagingFieldItem
        {
            private List<string> MVmodes = new List<string>() { "X" };
            public ImagingFieldItem(VMS.TPS.Common.Model.API.Beam ImagingField = null, VMS.TPS.Common.Model.Types.PatientOrientation O = VMS.TPS.Common.Model.Types.PatientOrientation.NoOrientation)
            {
                if (ImagingField != null)
                {
                    Id = ImagingField.Id;
                    GantryAngle = ImagingField.ControlPoints.FirstOrDefault().GantryAngle;
                    CollimatorAngle = ImagingField.ControlPoints.FirstOrDefault().CollimatorAngle;
                    Orientation = O;
                    Type = IdentifyFieldType();
                }
                if (Type == FieldType.Unset)
                {
                    Warning = true;
                    WarningMessage = "Name / angle mismatch";
                }
                if (Type == FieldType.BolusSetup && ImagingField.MLC == null)
                {
                    Warning = true;
                    WarningMessage = "Confirm no MLC needed";
                }

            }
            private FieldType IdentifyFieldType()
            {
                if (Bolus.Match(Id.ToUpper()).Success)
                {
                    return FieldType.BolusSetup;
                }
                switch (GantryAngle)
                {
                    case 0:
                        if (CBCT.Match(Id.ToUpper()).Success)
                        {
                            return FieldType.CBCT;
                        }
                        else
                        {
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstSupine && AntField.Match(Id.ToUpper()).Success)
                                return FieldType.Ant_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstSupine && AntField.Match(Id.ToUpper()).Success)
                                return FieldType.Ant_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstProne && PostField.Match(Id.ToUpper()).Success)
                                return FieldType.Post_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstProne && PostField.Match(Id.ToUpper()).Success)
                                return FieldType.Post_kv;
                        }
                        return FieldType.Unset;
                    case 90:
                        {
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstSupine && LeftField.Match(Id.ToUpper()).Success)
                                return FieldType.LL_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstSupine && RightField.Match(Id.ToUpper()).Success)
                                return FieldType.RL_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstProne && RightField.Match(Id.ToUpper()).Success)
                                return FieldType.RL_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstProne && LeftField.Match(Id.ToUpper()).Success)
                                return FieldType.LL_kv;
                            return FieldType.Unset;
                        }
                    case 270:
                        {
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstSupine && RightField.Match(Id.ToUpper()).Success)
                                return FieldType.RL_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstSupine && LeftField.Match(Id.ToUpper()).Success)
                                return FieldType.LL_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstProne && LeftField.Match(Id.ToUpper()).Success)
                                return FieldType.LL_kv;
                            if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstProne && RightField.Match(Id.ToUpper()).Success)
                                return FieldType.RL_kv;
                            return FieldType.Unset;
                        }
                    case 180:
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstSupine && PostField.Match(Id.ToUpper()).Success)
                            return FieldType.Post_kv;
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstSupine && PostField.Match(Id.ToUpper()).Success)
                            return FieldType.Post_kv;
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.HeadFirstProne && AntField.Match(Id.ToUpper()).Success)
                            return FieldType.Ant_kv;
                        if (Orientation == VMS.TPS.Common.Model.Types.PatientOrientation.FeetFirstProne && AntField.Match(Id.ToUpper()).Success)
                            return FieldType.Ant_kv;
                        return FieldType.Unset;
                    default:
                        return FieldType.Unset;
                }
            }
            private VMS.TPS.Common.Model.Types.PatientOrientation Orientation = VMS.TPS.Common.Model.Types.PatientOrientation.NoOrientation;
            private Regex AntField = new Regex("ANT");
            private Regex PostField = new Regex("POST");
            private Regex LeftField = new Regex("LLAT");
            private Regex RightField = new Regex("RLAT");
            private Regex Bolus = new Regex("BOLUS");
            private Regex CBCT = new Regex("CBCT");
            public FieldType Type { get; private set; } = FieldType.Unset;
            public string TypeString
            {
                get { return Type.Display(); }
            }
            public string Id { get; set; } = "Default Field";
            public bool isMV { get; private set; } = false;
            public double GantryAngle { get; set; } = 0;
            public double CollimatorAngle { get; set; } = 0;
            public bool Warning { get; set; } = false;
            public string WarningMessage { get; set; } = "";
        }


        //[AddINotifyPropertyChangedInterface]
        //public class ConstraintView : INotifyPropertyChanged, IComparable<ConstraintView>
        //{
        //    //Required to allow sorting when bound to datagridview
        //    public int CompareTo(ConstraintView other)
        //    {
        //        return DisplayOrder.CompareTo(other.DisplayOrder);
        //    }
        //    //Required notification class
        //    public event PropertyChangedEventHandler PropertyChanged;
        //    public event EventHandler<int> ConstraintEvaluating;
        //    public event EventHandler<int> ConstraintEvaluated;
        //    public event EventHandler<int> ConstraintDeleted;
        //    private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        //    {
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //    }
        //    public EventHandler ConstraintViewChanged;
        //    //public ConstraintView(Constraint _Con)
        //    //{
        //    //    Con = _Con;
        //    //    ID = _Con.ID;
        //    //    ReferenceConstraintDefinition = Con.GetConstraintString(true);
        //    //    ConstraintDefinition = Con.GetConstraintString(false);
        //    //    ShortConstraintDefinition = Con.GetConstraintStringNoStructure(false);
        //    //    SC = DataCache.GetComponent(_Con.ComponentID);
        //    //    ComponentID = SC.ID;
        //    //    DisplayOrderComponent = SC.DisplayOrder;
        //    //    ConstraintUnit = Con.GetConstraintUnit();
        //    //    ReferenceUnit = Con.GetReferenceUnit();
        //    //    ComponentName = SC.ComponentName;
        //    //    PrimaryStructureName = Con.PrimaryStructureName;

        //    //    // Add subscriptions to domain object
        //    //    Con.PropertyChanged += OnConstraintPropertyChanged;
        //    //    Con.ConstraintEvaluating += OnConstraintEvaluating;
        //    //    Con.ConstraintEvaluated += OnConstraintEvaluated;
        //    //    Con.ConstraintDeleted += OnConstraintDeleted;
        //    //    // Add to state data
        //    //    lock (LockConstraint)
        //    //    {
        //    //        ConstraintViews.Add(Con.ID, this);
        //    //    }
        //    //}
        //    private Constraint Con;
        //    private Component SC;
        //    public string Status { get; private set; }
        //    public bool isModified(string PropertyName = "")
        //    {
        //        return Con.isModified(PropertyName);
        //    }
        //    public bool isCreated { get { return Con.isCreated; } }
        //    public string ReferenceConstraintDefinition { get; private set; }
        //    public string ShortConstraintDefinition { get; private set; }
        //    public string ConstraintDefinition { get; private set; }
        //    public string ChangeDescription { get; set; } = "";
        //    public int ID { get; private set; }
        //    public int DisplayOrder
        //    {
        //        get
        //        {
        //            return Con.DisplayOrder.Value;
        //        }
        //        set
        //        {
        //            Con.DisplayOrder.Value = value; 
        //        }
        //    }
        //    public string ComponentName { get; private set; }
        //    public int ComponentID
        //    {
        //        get { return Con.ComponentID; }
        //        set
        //        {
        //            Con.ComponentID = value;
        //        }
        //    }
        //    public int DisplayOrderComponent { get; private set; }
        //    public string PrimaryStructureName { get; set; }
        //    public List<System.Windows.Media.Color> StructureColors
        //    {
        //        get
        //        {
        //            var ECPs = DataCache.GetAllPlans().Where(x => x.ComponentID == ComponentID);
        //            List<System.Windows.Media.Color> Colors = new List<System.Windows.Media.Color>();
        //            foreach (var ECP in ECPs)
        //            {
        //                if (ECP != null)
        //                {
        //                    var C = ECP.GetStructureColor(PrimaryStructureName);
        //                    if (C != null)
        //                    {
        //                        if (!Colors.Contains(((System.Windows.Media.Color)C)))
        //                            Colors.Add((System.Windows.Media.Color)C);
        //                    }
        //                }
        //            }
        //            return Colors;
        //        }
        //    }
        //    public ConstraintUnits ConstraintUnit { get; private set; }
        //    public ConstraintUnits ReferenceUnit { get; private set; }
        //    public ConstraintTypeCodes ConstraintType
        //    {
        //        get
        //        {
        //            return Con.ConstraintType;
        //        }
        //        set
        //        {
        //            Con.ConstraintType = value; // Con will notify ConstraintView to update the private field
        //        }
        //    }
        //    public double ConstraintValue
        //    {
        //        get { return Con.ConstraintValue; }
        //        set
        //        {
        //            Con.ConstraintValue = value;// Con will notify ConstraintView to update the private field
        //        }
        //    }
        //    public double ReferenceValue
        //    {
        //        get { return Con.ReferenceValue; }
        //        set
        //        {
        //            Con.ReferenceValue = value; // Con will notify ConstraintView to update the private field
        //            StopValue = double.NaN;
        //            MajorViolation = value;
        //            MinorViolation = double.NaN;
        //        }
        //    }
        //    public double StopValue
        //    {
        //        get
        //        {
        //            return Con.GetThreshold(ConstraintThresholdNames.Stop);
        //        }
        //        set
        //        {
        //            Con.SetThreshold(ConstraintThresholdNames.Stop, value);
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Threshold"));
        //        }
        //    }
        //    public double MinorViolation
        //    {
        //        get
        //        {
        //            return Con.GetThreshold(ConstraintThresholdNames.MinorViolation);
        //        }
        //        set
        //        {
        //            Con.SetThreshold(ConstraintThresholdNames.MinorViolation, value);
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Threshold"));
        //        }
        //    }
        //    public double MajorViolation
        //    {
        //        get
        //        {
        //            return Con.GetThreshold(ConstraintThresholdNames.MajorViolation);
        //        }
        //        set
        //        {
        //            Con.SetThreshold(ConstraintThresholdNames.MajorViolation, value);
        //            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Threshold"));
        //        }
        //    }
        //    public ReferenceTypes ReferenceType
        //    {
        //        get { return Con.ReferenceType; }
        //        set
        //        {
        //            Con.ReferenceType = value; // Con will notify ConstraintView to update the private field
        //        }
        //    }
        //    public UnitScale ConstraintScale
        //    {
        //        get { return Con.ConstraintScale; }
        //        set
        //        {
        //            Con.ConstraintScale = value; // Con will notify ConstraintView to update the private field
        //        }
        //    }
        //    public UnitScale ReferenceScale
        //    {
        //        get { return Con.ReferenceScale; }
        //        set
        //        {
        //            Con.ReferenceScale = value; // Con will notify ConstraintView to update the private field
        //        }
        //    }
        //    public int PrimaryStructureID
        //    {
        //        get
        //        {
        //            return Con.PrimaryStructureID;
        //        }
        //        set
        //        {
        //            Con.PrimaryStructureID = value; // Con will notify ConstraintView to update the private field
        //        }
        //    }
        //    public ConstraintResultView GetResult(int AssessmentID)
        //    {
        //        return Con.GetResult(AssessmentID);
        //    }
        //    private List<ConstraintResultView> GetAllResults()
        //    {
        //        return Con.GetAllResults();
        //    }
        //    public List<ConstraintChangelog> GetChangeLogs()
        //    {
        //        return Con.GetChangeLogs();
        //    }
        //    public bool isBestResult(double testval)
        //    {
        //        if (Ctr.NumAssessments > 1 && !double.IsNaN(testval))
        //        {
        //            var Results = GetAllResults().Where(x => x != null);
        //            if (Results == null)
        //                return false;
        //            IEnumerable<double> ResultValues = Results.Select(x => x.ResultValue);
        //            double BestValue = 0;
        //            double Spread = 0;
        //            if (ReferenceType == ReferenceTypes.Lower)
        //            {
        //                BestValue = ResultValues.Max();
        //                var Dif = ResultValues.Select(x => x - BestValue);
        //                Spread = Math.Abs(ResultValues.Max() - ResultValues.Min());
        //            }
        //            else if (ReferenceType == ReferenceTypes.Upper)
        //            {
        //                BestValue = ResultValues.Min();
        //                var Dif = ResultValues.Select(x => x - BestValue);
        //                Spread = Math.Abs(ResultValues.Max() - ResultValues.Min());
        //            }
        //            if (Spread > 0.01 && testval == BestValue)
        //            {
        //                return true;
        //            }
        //            else
        //                return false;
        //        }
        //        else return false;
        //    }
        //    //View helpers
        //    public List<ConstraintUnits> GetAvailableConstraintUnitList()
        //    {
        //        List<ConstraintUnits> L = new List<ConstraintUnits>();
        //        L.Add(ConstraintUnits.Percent);
        //        if (Con.isConstraintValueDose())
        //        {
        //            L.Add(ConstraintUnits.cGy);
        //        }
        //        else
        //        {
        //            L.Add(ConstraintUnits.cc);
        //        }
        //        return L;
        //    }
        //    public List<ConstraintUnits> GetAvailableReferenceUnitList()
        //    {
        //        List<ConstraintUnits> L = new List<ConstraintUnits>();
        //        L.Add(ConstraintUnits.Percent);
        //        if (Con.ConstraintType == ConstraintTypeCodes.CI)
        //        {
        //            L.Add(ConstraintUnits.Multiple);
        //        }
        //        else
        //        {
        //            if (Con.isReferenceValueDose())
        //            {

        //                L.Add(ConstraintUnits.cGy);
        //            }
        //            else
        //            {
        //                L.Add(ConstraintUnits.cc);
        //            }
        //        }
        //        return L;
        //    }
        //    //Event Handlers
        //    private void OnConstraintPropertyChanged(object sender, PropertyChangedEventArgs e)
        //    {
        //        switch (e.PropertyName)
        //        {
        //            case "ID":
        //                ID = Con.ID;
        //                break;
        //            case "ReferenceScale":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                ShortConstraintDefinition = Con.GetConstraintStringNoStructure(false);
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReferenceScale"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReferenceUnit"));
        //                break;
        //            case "ConstraintScale":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                ShortConstraintDefinition = Con.GetConstraintStringNoStructure(false);
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintScale"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintUnit"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                break;
        //            case "ConstraintValue":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                ShortConstraintDefinition = Con.GetConstraintStringNoStructure(false);
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintValue"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                break;
        //            case "ReferenceValue":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                ShortConstraintDefinition = Con.GetConstraintStringNoStructure(false);
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReferenceValue"));
        //                break;
        //            case "ConstraintType":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                ShortConstraintDefinition = Con.GetConstraintStringNoStructure(false);
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintType"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                break;
        //            case "ReferenceType":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                ShortConstraintDefinition = Con.GetConstraintStringNoStructure(false);
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReferenceType"));
        //                break;
        //            case "PrimaryStructureID":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                PrimaryStructureName = Con.PrimaryStructureName;
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("PrimaryStructureID"));
        //                break;
        //            case "ExceptionType":
        //                break;
        //            case "ComponentID":
        //                ComponentName = SC.ComponentName;
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                break;
        //            case "PrimaryStructureName":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                PrimaryStructureName = Con.PrimaryStructureName;
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                break;
        //            case "NumFractions":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Threshold"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintValue"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReferenceValue"));
        //                break;
        //            case "ReferenceDose":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintValue"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ReferenceValue"));
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                break;
        //            case "SecondaryStructureName":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                PrimaryStructureName = Con.PrimaryStructureName;
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                break;
        //            case "ProtocolStructureName":
        //                ConstraintDefinition = Con.GetConstraintString(false);
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ConstraintDefinition"));
        //                break;
        //            case "Threshold":
        //                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Threshold"));
        //                break;
        //            default:
        //                MessageBox.Show("Error updating ConstraintView");
        //                break;
        //        }
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(e.PropertyName));
        //    }
        //    private void OnConstraintEvaluated(object sender, int AssessmentID)
        //    {

        //        //ConstraintEvaluated?.Invoke(this, AssessmentID);
        //        ConstraintEvaluated.Raise(this, AssessmentID);
        //        // if (ConstraintEvaluated != null)
        //        ///    RaiseEventOnUIThread(ConstraintEvaluated, new object[] { this, EventArgs.Empty }); // this is subscribed to by UI elements
        //    }
        //    private void OnConstraintEvaluating(object sender, int AssessmentID)
        //    {

        //        //ConstraintEvaluating?.Invoke(this, AssessmentID);
        //        ConstraintEvaluating.Raise(this, AssessmentID);
        //        //if (ConstraintEvaluating != null)
        //        //RaiseEventOnUIThread(ConstraintEvaluating, new object[] { this, EventArgs.Empty }); // this is subscribed to by UI elements
        //    }
        //    private void OnConstraintDeleted(object sender, int ID)
        //    {
        //        //Con.PropertyChanged -= OnConstraintPropertyChanged;
        //        //Con.ConstraintDeleted -= OnConstraintDeleted;
        //        //Con.ConstraintEvaluated -= OnConstraintEvaluated;
        //        //Con.ConstraintEvaluating -= OnConstraintEvaluating;
        //        DataCache.ConstraintDeleted -= OnConstraintDeleted;
        //        ConstraintViews.Remove(ID);
        //        ConstraintDeleted?.Raise(this, ID);
        //    }
        //}
        public class PlanView
        {
            private ECPlan ECP;
            public PlanView(ECPlan _ECP)
            {
                ECP = _ECP;
            }
            public string PlanName
            {
                get { return ECP.PlanName; }
            }
            public string CourseName
            {
                get { return ECP.CourseName; }
            }
            public bool LoadWarning { get { return ECP.LoadWarning; } }
            public string LoadWarningString { get { return ECP.LoadWarningString; } }
            public ComponentTypes PlanType
            {
                get { return ECP.PlanType; }
            }
            public IEnumerable<string> StructureNames
            {
                get { return ECP.GetStructureNames; }
            }
            public List<ComponentStatusCodes> ErrorCodes { get { return ECP.GetErrorCodes(); } }
            public bool isStructureEmpty(string StructureID)
            {
                var isEmpty = ECP.isStructureEmpty(StructureID);
                if (isEmpty != null)
                    return (bool)isEmpty;
                else
                {
                    MessageBox.Show("Error in isStructureEmpty, structure not found");
                    return false;
                }
            }
        }
        public class ConstraintThresholdView
        {
            private ConstraintThreshold CT;
            public ConstraintThresholdView(ConstraintThreshold _CT)
            {
                CT = _CT;
            }
            public int ID
            {
                get { return CT.ID; }
            }
            public double ThresholdValue
            {
                get { return CT.ThresholdValue; }
                set { CT.ThresholdValue = value; }
            }
            public ConstraintThresholdNames ThresholdName
            {
                get { return CT.ThresholdName; }
            }
        }
        public class SessionView : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            public int ID { get; private set; }
            public string SessionComment { get; private set; }
            public string SessionDisplay { get; private set; }
            public string SessionCreator { get; private set; }
            public string SessionDateTime { get; private set; }
            public SessionView(Session S)
            {
                ID = S.ID;
                SessionComment = S.SessionComment;
                SessionCreator = S.SessionCreator;
                SessionDateTime = S.SessionDateTime;
                SessionDisplay = string.Format("{0} {1} ({2})", S.ProtocolName, SessionDateTime, SessionCreator);
            }

        }
        //public class AssessmentView : INotifyPropertyChanged
        //{
        //    public event PropertyChangedEventHandler PropertyChanged;
        //    private void NotifyPropertyChanged([CallerMemberName] string PropertyName = "")
        //    {
        //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        //    }
        //    private Assessment SA;
        //    public AssessmentView(Assessment _SA)
        //    {
        //        SA = _SA;
        //        DisplayOrder = _SA.DisplayOrder;
        //        SA.AssessmentDeleted += OnAssessmentDeleted;
        //        AssessmentViews.Add(ID, this);
        //    }
        //    public int ID
        //    {
        //        get { return SA.ID; }
        //    }
        //    public int DisplayOrder;
        //    public string AssessmentName
        //    {
        //        get { return SA.AssessmentName; }
        //        set
        //        {
        //            SA.AssessmentName = value;
        //            NotifyPropertyChanged("AssessmentName");
        //        }
        //    }
        //    public string Comments
        //    {
        //        get { return SA.Comments; }
        //        set { SA.Comments = value; }
        //    }
        //    public void ClearComponentAssociation(int ComponentId)
        //    {
        //        ECPlan ECP = DataCache.GetAllPlans().Where(x => x.AssessmentID == ID && x.ComponentID == ComponentId).SingleOrDefault();
        //        if (ECP != null)
        //        {
        //            ECP.Delete();
        //        }
        //    }
        //    public List<ComponentStatusCodes> AssociatePlanToComponent(int ComponentID, string CourseId, string PlanId, bool ClearWarnings) // ClearWarnings
        //    {
        //        AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
        //        ECPlan ECP = DataCache.GetAllPlans().Where(x => x.AssessmentID == ID && x.ComponentID == ComponentID).SingleOrDefault();
        //        if (CurrentStructureSet == null)
        //            SetCurrentStructureSet(p.StructureSetId);
        //        if (ECP == null)
        //        {
        //            // If it meets these criteria the component is evaluable
        //            ECP = new ECPlan(p, ID, ComponentID);
        //            DataCache.AddPlan(ECP);
        //            DataCache.GetAssessment(ID).RegisterPlan(ECP); // this will fire PlanMappingChanged and update all DataCache.Constraints
        //        }
        //        else
        //        {
        //            ECP.Reassociate(p, ClearWarnings);
        //        }
        //        LinkedPlansChanged?.Invoke(this, EventArgs.Empty);
        //        return ECP.GetErrorCodes();
        //    }
        //    public List<ComponentStatusCodes> StatusCodes(int ComponentID)
        //    {
        //        ECPlan ECP = DataCache.GetAllPlans().Where(x => x.AssessmentID == ID && x.ComponentID == ComponentID).SingleOrDefault();
        //        if (ECP != null)
        //            return ECP.GetErrorCodes();
        //        else
        //            return null;
        //    }
        //    private void OnAssessmentDeleted(object sender, int ID)
        //    {
        //        Dispose();
        //    }
        //    public void Dispose()
        //    {
        //        SA.AssessmentDeleted -= OnAssessmentDeleted;
        //        AssessmentViews.Remove(ID);
        //    }
        //}
        public class AssessmentPreviewView
        {
            public AssessmentPreviewView(int _ID, string _AssessmentName, string _ReferenceProtocol, string _User, string _DateOfAssessment, string _Comments)
            {
                ID = _ID;
                AssessmentName = _AssessmentName;
                ReferenceProtocol = _ReferenceProtocol;
                User = _User;
                DateOfAssessment = _DateOfAssessment;
                Comments = _Comments;
            }
            public int ID { get; private set; }
            public string AssessmentName { get; private set; }
            public string ReferenceProtocol { get; private set; }
            public string User { get; private set; }
            public string DateOfAssessment { get; private set; }
            public string Comments { get; private set; }
        }
        public class ProtocolPreview
        {
            public ProtocolPreview(int _ID, string ProtocolName_in, TreatmentCentres TreatmentCentre_in, TreatmentSites TreatmentSite_in, ProtocolTypes ProtocolType_in, ApprovalLevels ApprovalLevel_in, string LastModifiedBy_in)
            {
                ID = _ID;
                ProtocolName = ProtocolName_in;
                ProtocolType = ProtocolType_in;
                TreatmentCentre = TreatmentCentre_in;
                TreatmentSite = TreatmentSite_in;
                Approval = ApprovalLevel_in;
                LastModifiedBy = LastModifiedBy_in;
            }

            public int ID { get; private set; }
            public string ProtocolName { get; private set; }
            public ProtocolTypes ProtocolType { get; set; }
            public TreatmentCentres TreatmentCentre { get; set; }
            public TreatmentSites TreatmentSite { get; set; }
            public ApprovalLevels Approval { get; set; }
            public string LastModifiedBy { get; set; }

        }
        public class StructureLabelView
        {
            private StructureLabel SL;
            public StructureLabelView(StructureLabel SLin)
            {
                SL = SLin;
            }
            public int ID
            {
                get { return SL.ID; }
            }
            public string Designator
            {
                get { return SL.Designator; }
            }
            public string LabelName
            {
                get { return SL.LabelName; }
            }
            public double AlphaBetaRatio
            {
                get { return SL.AlphaBetaRatio; }
            }
        }
        public class ConstraintResultView
        {
            public ConstraintResultView(ConstraintResult CR)
            {
                AssessmentID = CR.AssessmentID;
                ConstraintID = CR.ConstraintID;
                Result = CR.ResultString;
                ResultValue = CR.ResultValue;
                ThresholdStatus = CR.ThresholdStatus;
                StatusCodes = CR.StatusCodes;
                ReferenceType = DataCache.GetConstraint(CR.ConstraintID).ReferenceType;
                LabelName = CR.LinkedLabelName;
                isCalculating = CR.isCalculating;
            }
            public int AssessmentID { get; private set; }
            public int ConstraintID { get; private set; }
            public bool isCalculating { get; private set; }
            public double ResultValue { get; private set; }
            public string LabelName { get; private set; }
            public ReferenceTypes ReferenceType { get; private set; }
            public string Result { get; private set; }
            public List<ConstraintResultStatusCodes> StatusCodes { get; private set; } = new List<ConstraintResultStatusCodes>();
            public ConstraintThresholdNames ThresholdStatus { get; private set; }
        }
        // State Data
        public static string SquintUser { get; private set; }
        private static ProtocolView ActivePV;
        public static AsyncStructureSet CurrentStructureSet { get; private set; }
        public static AsyncESAPI A { get; private set; }
        private static Dictionary<int, StructureLabelView> StructureLabelViews = new Dictionary<int, StructureLabelView>();
        //public static Dictionary<int, ConstraintView> ConstraintViews { get; private set; } = new Dictionary<int, ConstraintView>();
        public static Dictionary<int, Component> ComponentViews { get; private set; } = new Dictionary<int, Component>();
        public static Dictionary<int, AssessmentView> AssessmentViews { get; private set; } = new Dictionary<int, AssessmentView>();
        //public static Dictionary<int, ConstituentView> ConstituentViews { get; private set; } = new Dictionary<int, ConstituentView>();
        private static Dictionary<int, ECSID> Structures = new Dictionary<int, ECSID>();





        private static List<string> CourseNames = new List<string>();

        public static bool SavingNewProtocol { get; } = false;
        private static int _IDbase = -2;
        private static int _NewStructureCounter = 1;
        private static int _NewPlanCounter = 1;
        private static int _AssessmentNameIterator = 1;
        static public bool PatientLoaded { get; private set; } = false;
        public static bool ProtocolLoaded { get; private set; } = false;
        // Events

        public static event EventHandler CurrentStructureSetChanged;
        public static event EventHandler<ECPlan> AvailableStructureSetsChanged;
        public static event EventHandler SynchronizationComplete;
        public static event EventHandler ProtocolListUpdated;
        public static event EventHandler ProtocolConstraintOrderChanged;
        public static event EventHandler ProtocolOpened;
        public static event EventHandler ProtocolClosed;
        public static event EventHandler<ECPlan> LinkedPlansChanged;
        public static event EventHandler SessionsChanged;
        public static event EventHandler<int> ConstraintAdded;
        public static event EventHandler<int> ConstraintRemoved;
        public static event EventHandler<int> NewAssessmentAdded;
        public static event EventHandler<int> AssessmentRemoved;
        public static event EventHandler<int> ComponentAdded;
        // Initialization
        public static bool Initialize(string username = null, string password = null)
        {
            try
            {
                A = new AsyncESAPI(username, password);
                SquintUser = A.CurrentUserId(); // Get user
                DataCache.RegisterUser(SquintUser);
                DataCache.CreateSession();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        // Public accessors
        public static string ProtocolName
        {
            get { return DataCache.CurrentProtocol.ProtocolName; }
        }
        public static string PatientFirstName
        {
            get
            {
                if (ReferenceEquals(null, DataCache.Patient))
                    return string.Empty;
                else
                    return DataCache.Patient.FirstName;
            }
        }
        public static string PatientLastName
        {
            get
            {
                if (ReferenceEquals(null, DataCache.Patient))
                    return string.Empty;
                else
                    return DataCache.Patient.LastName;
            }
        }
        public static string PatientID
        {
            get
            {
                if (ReferenceEquals(null, DataCache.Patient))
                    return string.Empty;
                else
                    return DataCache.Patient.Id;
            }
        }
        public async static Task<VMSTypes.VVector> GetPlanIsocentre(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetPlanIsocentre();
            else
                return new VMSTypes.VVector() { x = double.NaN, y = double.NaN, z = double.NaN };
        }
        public async static Task<List<Controls.ObjectiveDefinition>> GetOptimizationObjectiveList(string CourseId, string PlanId)
        {
            if (!PatientLoaded)
                return null;
            else
            {
                AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
                if (p.PlanType == ComponentTypes.Plan)
                {
                    var Objectives = await p.GetObjectiveItems();
                    return Objectives;
                }
                else return new List<Controls.ObjectiveDefinition>();
            }
        }
        public async static Task<double> GetSliceSpacing(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetSliceSpacing();
            else
                return double.NaN;
        }
        public async static Task<double> GetDoseGridResolution(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetDoseGridResolution();
            else
                return double.NaN;
        }
        public async static Task<string> GetFieldNormalizationMode(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetFieldNormalizationMode();
            else
                return "";
        }
        public async static Task<double> GetPNV(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetPNV();
            else return double.NaN;
        }
        public static double? GetRxDose(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return p.Dose;
            else
            {
                return double.NaN;
            }
        }
        public static int? GetNumFractions(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return p.NumFractions;
            else
                return null;
        }
        public async static Task<double> GetPrescribedPercentage(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetPrescribedPercentage();
            else
                return double.NaN;
        }
        public async static Task<string> GetCourseIntent(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            return await p.GetCourseIntent();
        }
        public async static Task<string> GetAlgorithmModel(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetAlgorithmModel();
            else
                return "";
        }
        public async static Task<double> GetCouchSurface(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetCouchSurface();
            else
                return double.NaN;
        }
        public async static Task<double> GetCouchInterior(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetCouchInterior();
            else return double.NaN;
        }
        public async static Task<bool?> GetHeterogeneityOn(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Sum)
                return null;
            string HeteroStatus = await p.GetHeterogeneityOn();
            switch (HeteroStatus)
            {
                case "ON":
                    return true;
                case "OFF":
                    return false;
                default:
                    return null;
            }
        }
        public async static Task<string> GetStudyId(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetStudyId();
            else return "";
        }
        public async static Task<string> GetSeriesId(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetSeriesId();
            else return "";
        }
        public async static Task<string> GetSeriesComments(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetSeriesComments();
            else
                return "";
        }
        public async static Task<double> GetBolusThickness(string CourseId, string PlanId, string BolusId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            var Bolus = p.Structures.Values.FirstOrDefault(x => x.DicomType == @"BOLUS" && x.Id == BolusId);
            if (Bolus != null)
            {
                var thick = await p.GetBolusThickness(Bolus.Id);
                return thick;
            }
            return double.NaN;
        }

        public async static Task<string> GetImageComments(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetImageComments();
            else return "";
        }
        public async static Task<int?> GetNumSlices(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            if (p.PlanType == ComponentTypes.Plan)
                return await p.GetNumSlices();
            else return null;
        }
        public async static Task<List<TxFieldItem>> GetTxFieldItems(string CourseId, string PlanId)
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            return await p.GetTxFields();
        }
        public async static Task<List<ImagingFieldItem>> GetImagingFieldList(string CourseId, string PlanId)
        {
            if (!PatientLoaded)
                return new List<ImagingFieldItem>();
            else
            {
                AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
                if (p.PlanType != ComponentTypes.Plan)
                    return new List<ImagingFieldItem>();
                var Isocentre = await p.GetPlanIsocentre();
                var PatientCentre = await p.GetPatientCentre();
                var ImagingFields = await p.GetImagingFields();
                var TxFields = await p.GetTxFields();
                foreach (ImagingFieldItem I in ImagingFields)
                {
                    if (I.Type == FieldType.Ant_kv && ((Isocentre.x - PatientCentre.x) < -50 || TxFields.All(x => (x.GantryStart < 40 || x.GantryStart > 180) && (x.GantryEnd < 40 || x.GantryEnd > 180))))
                    {
                        I.Warning = true;
                        I.WarningMessage = "Posterior preferred?";
                    }
                    if (I.Type == FieldType.Post_kv && ((Isocentre.x - PatientCentre.x) > 50 || TxFields.All(x => (x.GantryStart > 320 || x.GantryStart < 180) && (x.GantryEnd > 320 || x.GantryEnd < 180))))
                    {
                        I.Warning = true;
                        I.WarningMessage = "Anterior preferred?";
                    }
                }
                return ImagingFields;
            }
        }
        public static Dictionary<ImagingProtocols, List<string>> CheckImagingProtocols(Component CV, List<ImagingFieldItem> IF)
        {
            Dictionary<ImagingProtocols, List<string>> Errors = new Dictionary<ImagingProtocols, List<string>>();
            foreach (ImagingProtocols IP in CV.ImagingProtocolsAttached)
            {
                if (!Errors.ContainsKey(IP))
                    Errors.Add(IP, new List<string>());
                switch (IP)
                {
                    case ImagingProtocols.kV_2D:
                        if (!(IF.Any(x => x.Type == FieldType.Ant_kv) && (IF.Any(x => x.Type == FieldType.LL_kv) || IF.Any(x => x.Type == FieldType.RL_kv))) &&
                            !((IF.Any(x => x.Type == FieldType.Post_kv) && (IF.Any(x => x.Type == FieldType.LL_kv) || IF.Any(x => x.Type == FieldType.RL_kv)))))
                            Errors[IP].Add("Cannot find kv-pair");
                        break;
                    case ImagingProtocols.PreCBCT:
                        if (!IF.Any(x => x.Type == FieldType.CBCT))
                            Errors[IP].Add("Cannot find CBCT");
                        break;
                    case ImagingProtocols.PostCBCT:
                        if (!IF.Any(x => x.Type == FieldType.CBCT))
                            Errors[IP].Add("Cannot find CBCT");
                        break;
                }
            }
            if (CV.ImagingProtocolsAttached.Contains(ImagingProtocols.PostCBCT) && CV.ImagingProtocolsAttached.Contains(ImagingProtocols.PreCBCT))
            {
                if (IF.Where(x => x.Type == FieldType.CBCT).Count() < 2)
                {
                    Errors[ImagingProtocols.PostCBCT].Add("Insufficient CBCT fields for both pre and post imaging");
                    Errors[ImagingProtocols.PreCBCT].Add("Insufficient CBCT fields for both pre and post imaging");
                }
            }
            return Errors;
        }
        public async static Task<Controls.NTODefinition> GetNTOObjective(string CourseId, string PlanId)
        {
            if (!PatientLoaded)
                return null;
            else
            {
                AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
                if (p.PlanType == ComponentTypes.Plan)
                    return await p.GetNTOObjective();
                else
                    return null;
            }
        }
        public static List<ProtocolView> GetProtocolList()
        {
            return DataCache.GetProtocolList().Where(x => x.ProtocolID > 1).OrderBy(x => x.ProtocolName).ToList();
        }
        public static ProtocolView GetProtocolList(int Id)
        {
            return DataCache.GetProtocolList(Id);
        }
        public static ProtocolView GetProtocolView()
        {
            if (ProtocolLoaded)
                return ActivePV;
            else return new ProtocolView();
        }
        public static PlanView GetPlanView(int planID)
        {
            return new PlanView(DataCache.GetPlan(planID));
        }
        public static PlanView GetPlanView(int ComponentID, int AssessmentID)
        {
            ECPlan ECP = DataCache.GetAllPlans().Where(x => x.ComponentID == ComponentID && x.AssessmentID == AssessmentID).SingleOrDefault();
            if (ECP != null)
                return new PlanView(ECP);
            else
                return null;
        }
        public static Component GetComponentView(int ComponentID)
        {
            if (ComponentViews.ContainsKey(ComponentID))
                return ComponentViews[ComponentID];
            else
                return null;
        }
        public static Constraint GetConstraint(int ConId)
        {
            return DataCache.GetConstraint(ConId);
        }
        public static ECSID GetStructure(int StructureID)
        {
            if (Structures.ContainsKey(StructureID))
                return Structures[StructureID];
            else
                return null;
        }
        public static StructureLabelView GetStructureLabelView(int StructureLabelID)
        {
            return new StructureLabelView(DataCache.GetStructureLabel(StructureLabelID));
        }
        public static List<ECSID> GetStructureList()
        {
            return DataCache.GetAllECSIDs().ToList();
        }
        public static List<Constraint> GetConstraints(int? ComponentID = null)
        {
            if (ComponentID == null)
                return DataCache.GetAllConstraints().ToList();
            else
                return DataCache.GetAllConstraints().Where(x => x.ComponentID == ComponentID).ToList();
        }
        public static List<Component> GetComponentList()
        {
            return DataCache.GetAllComponents().OrderBy(x => x.DisplayOrder).ToList();
        }
        //public static List<ConstituentView> GetConstituentViewList(int ComponentId)
        //{
        //    return ConstituentViews.Values.Where(x => x.ComponentID == ComponentId).ToList();
        //}
        public static List<ConstraintThresholdView> GetConstraintThresholdViewList(int ConID)
        {
            List<ConstraintThresholdView> returnList = new List<ConstraintThresholdView>();
            foreach (ConstraintThreshold CT in DataCache.GetConstraintThresholdByConstraintId(ConID))
            {
                returnList.Add(new ConstraintThresholdView(CT));
            }
            return returnList;
        }
        public static List<PlanView> GetPlanViewList()
        {
            List<PlanView> returnList = new List<PlanView>();
            foreach (ECPlan ECP in DataCache.GetAllPlans())
            {
                returnList.Add(new PlanView(ECP));
            }
            return returnList;
        }
        public static List<Assessment> GetAssessmentList()
        {
            return DataCache.GetAllAssessments().OrderBy(x => x.DisplayOrder).ToList();
        }
        public static List<StructureLabelView> GetStructureLabelViewList()
        {
            List<StructureLabelView> returnList = new List<StructureLabelView>();
            foreach (StructureLabel SL in DataCache.GetAllStructureLabels())
            {
                returnList.Add(new StructureLabelView(SL));
            }
            return returnList;
        }
        public static List<AssessmentPreviewView> GetAssessmentPreviews(string PID)
        {
            //return DataCache.GetAssessmentPreviews(PID);
            return null;

        }
        // Public helper functions
        public static int IDGenerator()
        {
            lock (LockIDGen)
            {
                _IDbase--;
                return _IDbase;
            }
        }
        public static readonly Object LockConstraint = new Object();
        public static readonly Object LockSimultaneousEvaluation = new Object();
        public static readonly Object LockIDGen = new Object();
        public static readonly Object LockDb = new Object();
        private static Progress<int> _ProgressBar;
        public static Progress<int> ProgressBar
        {
            get { return _ProgressBar; }
            set
            {
                _ProgressBar = value;
            }
        }
        public static bool isStructureIDUnique(string sID)
        {
            if (DataCache.GetAllECSIDs().Select(x => x.EclipseStructureName).Contains(sID))
                return false;
            else
                return true;
        }
        public static int NumAssessments
        {
            get { return DataCache.GetAllAssessments().Count(); }
        }
        // Private helper functions

        //private static DbProtocolType GetDbProtocolType(ProtocolTypes ProtocolType = ProtocolTypes.Unset)
        //{

        //    DbProtocolType PT = SquintDb.Context.DbProtocolTypes.Where(x => x.ProtocolType == (int)ProtocolType).Single();
        //    if (PT == null)
        //    {
        //        PT = SquintDb.Context.DbProtocolTypes.Where(x => x.ProtocolType == (int)ProtocolTypes.Unset).Single();
        //    }
        //    return PT;

        //}
        //private static DbTreatmentCentre GetDbTreatmentCentre(TreatmentCentres TreatmentCentre = TreatmentCentres.Unset)
        //{

        //    DbTreatmentCentre TC = SquintDb.Context.DbTreatmentCentres.Where(x => x.TreatmentCentre == (int)TreatmentCentre).Single();
        //    if (TC == null)
        //    {
        //        TC = SquintDb.Context.DbTreatmentCentres.Where(x => x.TreatmentCentre == (int)TreatmentCentres.Unset).Single();
        //    }
        //    return TC;

        //}
        //private static DbTreatmentSite GetDbTreatmentSite(TreatmentSites TreatmentSite = TreatmentSites.Unset)
        //{
        //    DbTreatmentSite TS = SquintDb.Context.DbTreatmentSites.Where(x => x.TreatmentSite == (int)TreatmentSite).Single();
        //    if (TS == null)
        //    {
        //        TS = SquintDb.Context.DbTreatmentSites.Where(x => x.TreatmentSite == (int)TreatmentSites.Unset).Single();
        //    }
        //    return TS;
        //}

        //Squint structure helpers
        public static List<EclipseStructure> GetCurrentStructures()
        {
            List<EclipseStructure> StructureIds = new List<EclipseStructure>();
            foreach (AsyncStructure Structure in CurrentStructureSet.GetAllStructures())
            {
                StructureIds.Add(new EclipseStructure(Structure));

            }
            return StructureIds;
        }

        public static List<string> GetAvailableStructureSetIds()
        {
            var SSs = DataCache.GetAvailableStructureSets();
            return SSs.Select(x => x.StructureSetId).ToList();
        }
        public static List<StructureSetHeader> GetAvailableStructureSets()
        {
            return DataCache.GetAvailableStructureSets();

        }

        //Squint Protocol Control functions
        //public static bool CopyConstraintsBetweenProtocols(int ComponentID_Source, int ComponentID_Target)
        //{
        //    if (DataCache.CurrentProtocol == null)
        //        return false;
        //    int LastDisplayOrder = 1;
        //    Component SC_Source = DataCache.GetComponent(ComponentID_Source);
        //    Component SC_Target = DataCache.GetComponent(ComponentID_Target);
        //    if (SC_Source.ReferenceDose != SC_Target.ReferenceDose || SC_Source.NumFractions != SC_Target.NumFractions)
        //        return false;
        //    if (DataCache.GetConstraintsInComponent(SC_Target.ID).Count() > 0)
        //        LastDisplayOrder = DataCache.GetConstraintsInComponent(SC_Target.ID).Select(y => y.DisplayOrder).Max() + 1;
        //    List<Task> tasks = new List<Task>();
        //    foreach (Constraint Con in DataCache.GetConstraintsInComponent(SC_Source.ID).OrderBy(x => x.DisplayOrder).ToList())
        //    {
        //        Constraint DupCon = new Constraint(Con, SC_Target.ID);
        //        DataCache.AddConstraint(DupCon);
        //        new ConstraintView(DupCon);
        //        foreach (Assessment SA in DataCache.GetAllAssessments())
        //        {
        //            DupCon.RegisterAssessment(SA);
        //        }
        //        ConstraintAdded?.Raise(null, DupCon.ID);
        //    }
        //    return true;
        //}
        //public static ConstraintView CopySingleConstraintToNewComponent(int ComponentID_Target, int ConstraintID_Copied)
        //{
        //    if (DataCache.CurrentProtocol == null)
        //        return null;
        //    Component SC_Target = DataCache.GetComponent(ComponentID_Target);
        //    Constraint Con = DataCache.GetConstraint(ConstraintID_Copied);
        //    Constraint DupCon = new Constraint(Con, SC_Target.ID);
        //    foreach (Assessment SA in DataCache.GetAllAssessments())
        //    {
        //        DupCon.RegisterAssessment(SA);
        //    }
        //    ConstraintView CV = new ConstraintView(DupCon);
        //    ConstraintAdded?.Invoke(null, CV.ID);
        //    return CV;
        //}
        public static Constraint AddConstraint(ConstraintTypeCodes TypeCode, int ComponentID = 0, int StructureId = 1)
        {
            if (!ProtocolLoaded)
                return null;
            Constraint Con = new Constraint(TypeCode, ComponentID, StructureId);
            DataCache.AddConstraint(Con);
            foreach (Assessment SA in DataCache.GetAllAssessments())
            {
                Con.RegisterAssessment(SA);
            }
            return Con;
        }
        public static void DeleteConstraint(int Id)
        {
            DataCache.GetConstraint(Id).Delete();
            //Re-index displayorder
            int NewDisplayOrder = 1;
            foreach (Constraint Con in DataCache.GetAllConstraints().OrderBy(x => x.DisplayOrder))
            {
                Con.DisplayOrder.Value = NewDisplayOrder++;
            }
            ConstraintRemoved?.Invoke(null, Id);
        }
        public static void ShiftConstraintUp(int Id)
        {
            Constraint Con = DataCache.GetConstraint(Id);
            if (Con != null)
            {
                Constraint ConSwitch = DataCache.GetAllConstraints().FirstOrDefault(x => x.DisplayOrder.Value == Con.DisplayOrder.Value - 1);
                if (ConSwitch != null)
                {
                    ConSwitch.DisplayOrder = Con.DisplayOrder;
                    Con.DisplayOrder.Value = Con.DisplayOrder.Value - 1;
                }
                ProtocolConstraintOrderChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static void ShiftConstraintDown(int Id)
        {
            Constraint Con = DataCache.GetConstraint(Id);
            if (Con != null)
            {
                Constraint ConSwitch = DataCache.GetAllConstraints().FirstOrDefault(x => x.DisplayOrder.Value == Con.DisplayOrder.Value + 1);
                if (ConSwitch != null)
                {
                    ConSwitch.DisplayOrder = Con.DisplayOrder;
                    Con.DisplayOrder.Value = Con.DisplayOrder.Value + 1;
                }
                ProtocolConstraintOrderChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static void DuplicateConstraint(int ConstraintID)
        {
            if (DataCache.CurrentProtocol == null)
                return;
            Constraint Con2Dup = DataCache.GetConstraint(ConstraintID);
            foreach (Constraint Con in DataCache.GetAllConstraints().Where(x => x.DisplayOrder.Value > Con2Dup.DisplayOrder.Value))
            {
                Con.DisplayOrder.Value = Con.DisplayOrder.Value + 1;
            }
            Constraint DupCon = new Constraint(Con2Dup);
            DataCache.AddConstraint(DupCon);
            //Update DisplayOrder
            foreach (Assessment SA in DataCache.GetAllAssessments())
            {
                DupCon.RegisterAssessment(SA);
            }
            ConstraintAdded?.Invoke(null, DupCon.ID);
        }
        public static Component AddComponent(ComponentTypes Type_input, int ReferenceDose_input = 0, int NumFractions_input = 0, string ComponentName = "")
        {
            if (!ProtocolLoaded)
                return null;
            if (ComponentName == "")
            {
                if (Type_input == ComponentTypes.Plan)
                {
                    ComponentName = string.Format("Plan{0}", _NewPlanCounter++);
                }
                if (Type_input == ComponentTypes.Sum)
                {
                    ComponentName = string.Format("PlanSum{0}", _NewPlanCounter++);
                }
            }
            Component Comp = new Component(DataCache.CurrentProtocol.ID, ComponentName, NumFractions_input, ReferenceDose_input, Type_input);
            ComponentAdded?.Invoke(null, Comp.ID);
            Component CompView = new Component(Comp);
            return CompView;
        }
        public static ECSID AddNewStructure()
        {
            if (DataCache.CurrentProtocol == null)
                return null;
            else
            {
                string NewStructureName = string.Format("UserStructure{0}", _NewStructureCounter++);
                ECSID newECSID = new ECSID(NewStructureName, 1);
                DataCache.AddECSID(newECSID);
                return newECSID;
            }
        }
        public static void AddConstraintThreshold(ConstraintThresholdNames Name, int ConstraintID, double ThresholdValue)
        {
            // ConstraintThreshold CT = new ConstraintThreshold(Name, ConstraintID, ThresholdValue);
        }
        //public static bool ImportEclipseProtocol(SquintEclipseProtocol.Protocol ClinProtocol)
        //{
        //    // Reset adjusted protocol flag
        //    ClosePatient();
        //    CloseProtocol();
        //    //load the XML document
        //    try
        //    {
        //        DataCache.CreateNewProtocol();
        //        DataCache.CurrentProtocol.ProtocolName = ClinProtocol.Preview.ID;
        //        foreach (SquintEclipseProtocol.StructureClass ECPStructure in ClinProtocol.StructureTemplate.Structures.Structure)
        //        {
        //            new ECSID(ECPStructure.ID);
        //        }
        //        foreach (SquintEclipseProtocol.PhaseClass Phase in ClinProtocol.Phases.Phase)
        //        {
        //            Component SC = new Component(DataCache.CurrentProtocol.ID, Phase.ID);
        //            SC.NumFractions = Phase.PlanTemplate.FractionCount;
        //            SC.ReferenceDose = Phase.PlanTemplate.DosePerFraction * SC.NumFractions;
        //            foreach (SquintEclipseProtocol.Item Item in Phase.Prescription.Items)
        //            {
        //                Constraint Con = new Constraint(Item, SC.ID);
        //            }
        //            foreach (SquintEclipseProtocol.MeasureItem MI in Phase.Prescription.MeasureItems)
        //            {
        //                Constraint Con = new Constraint(MI, SC.ID);
        //            }
        //        }
        //        /*foreach (Constraint Con in DataCache.Constraints)
        //        {
        //            ConstraintAdded(null, CV.ID);
        //        } */
        //        var DbContext = SquintDb.Context;
        //        List<string> ExistingProtocolNames = DbContext.DbLibraryProtocols.Where(y => !y.isRetired).Select(x => x.ProtocolName).ToList();
        //        if (ExistingProtocolNames.Contains(DataCache.CurrentProtocol.ProtocolName))
        //        {
        //            MessageBox.Show(string.Format("A protocol of this name ({0}) already exists, please select a unique name for this protocol", DataCache.CurrentProtocol.ProtocolName));
        //            return false;
        //        }
        //        else
        //        {
        //            DbContext.SaveChanges();
        //        }
        //        ProtocolOpened(DataCache.CurrentProtocol, EventArgs.Empty);
        //        return true;
        //    }
        //    catch
        //    {
        //        MessageBox.Show("Error importing Clinical DataCache.CurrentProtocol. Check to make sure all DataCache.Constraints and structures are fully defined before importing.  No incomplete entries are allowed");
        //        return false;
        //    }
        //}
        //public static async Task DeleteComponent(int ComponentID)
        //{
        //    //// method reports progress as a percent of deleted DataCache.Constraints
        //    //int count = 1;
        //    //double total = DataCache.Constraints.Values.Where(x => x.ComponentID == ComponentID).Count();
        //    //Progress<int> LocalProgress = new Progress<int>((update) =>
        //    //{
        //    //    (Ctr.ProgressBar as IProgress<int>).Report(Convert.ToInt32(count++ / total * 100));
        //    //});
        //    //await Task.Run(() =>
        //    //{
        //    //    foreach (Constraint Con in DataCache.Constraints.Values.Where(x => x.ComponentID == ComponentID).ToList())
        //    //    {
        //    //        Con.Delete();
        //    //        (LocalProgress as IProgress<int>).Report(Con.ID);
        //    //    }
        //    //});
        //    //await Task.Run(() => Components[ComponentID].Delete());
        //}
        //public static void RemoveConstraintFromProtocol(int ConstraintID)
        //{
        //    // If constraint was added by user it is deleted, otherwise it is removed from calculation but logged as a protocol deviation
        //    DataCache.Constraints[ConstraintID].RemoveFromProtocol();
        //}
        //private static string ExportAssessmentToEclipse(int AssessmentID)
        //{
        //    Assessment SA = DataCache.GetAssessment(AssessmentID);
        //    if (SA == null)
        //        return "";
        //    SquintEclipseProtocol.Protocol ECProtocol = new SquintEclipseProtocol.Protocol();
        //    SquintEclipseProtocol.PreviewClass Preview = new SquintEclipseProtocol.PreviewClass();
        //    Preview.ID = string.Format("{0}_{1}_{2}", DataCache.Patient.Id, SA.AssessmentName, GetDbUser(SquintUser).ARIA_ID);
        //    Preview.ApprovalStatus = "Unapproved";
        //    Preview.Type = "Protocol";
        //    Preview.ApprovalHistory = @"Exported from Squint";
        //    Preview.LastModified = DateTime.Now.ToShortDateString();
        //    Preview.Diagnosis = "";
        //    ECProtocol.Preview = Preview;
        //    foreach (ECPlan ECP in DataCache.GetAllPlans().Where(x => x.AssessmentID == AssessmentID))
        //    {
        //        try
        //        {
        //            SquintEclipseProtocol.PhaseClass PC = new SquintEclipseProtocol.PhaseClass()
        //            {
        //                ID = ECP.PlanName,
        //                //Prescription = new List<SquintEclipseProtocol.MeasureItem>(),
        //                Prescription = new SquintEclipseProtocol.PrescriptionClass(),
        //                PlanTemplate = new SquintEclipseProtocol.PlanTemplate(),
        //                ObjectiveTemplate = new SquintEclipseProtocol.ObjectiveTemplate(),
        //                Mode = "Photon",
        //                DefaultEnergyKV = null,
        //                FractionCount = DataCache.GetComponent(ECP.ComponentID).NumFractions,
        //                FractionsPerDay = null,
        //                FractionsPerWeek = null,
        //                TreatmentStyle = "ARC",
        //                TreatmentUnit = "CNTB_FRASER",
        //            };
        //            foreach (Constraint Con in DataCache.GetAllConstraints())
        //            {
        //                if (Con.ComponentID != ECP.ComponentID)
        //                    continue;
        //                ECSID ECSID = DataCache.GetECSID(Con.PrimaryStructureID);
        //                if (!ECProtocol.StructureTemplate.Structures.Structure.Select(x => x.ID).Contains(ECSID.EclipseStructureName))
        //                {
        //                    ECProtocol.StructureTemplate.Structures.Structure.Add(new SquintEclipseProtocol.StructureClass()
        //                    {
        //                        ID = ECSID.EclipseStructureName,
        //                        TypeIndex = 2,
        //                        ColorAndStyle = new SquintEclipseProtocol.EmptyClass(),
        //                        Identification = new SquintEclipseProtocol.Identification()
        //                    });
        //                }
        //                if (Con.ConstraintType == ConstraintTypeCodes.M)
        //                {
        //                    SquintEclipseProtocol.Item I = new SquintEclipseProtocol.Item()
        //                    {
        //                        ID = ECSID.EclipseStructureName,
        //                        Modifier = Con.GetEclipseMeanDoseModifier(),
        //                        Type = 0, //Con.GetEclipseMeanDoseType(),
        //                        Dose = Con.ReferenceValue / 100 / DataCache.GetComponent(ECP.ComponentID).NumFractions
        //                    };
        //                    PC.Prescription.Items.Add(I);
        //                }
        //                else
        //                {
        //                    SquintEclipseProtocol.MeasureItem MI = new SquintEclipseProtocol.MeasureItem()
        //                    {
        //                        ID = ECSID.EclipseStructureName,
        //                        Modifier = Con.GetEclipseModifier(),
        //                        Type = Con.GetEclipseType(),
        //                        Value = Con.GetEclipseValue(),
        //                        TypeSpecifier = Con.GetEclipseTypeSpecifier(),
        //                        ReportDQPValueInAbsoluteUnits = Con.GetEclipseValueUnits()
        //                    };
        //                    PC.Prescription.MeasureItems.Add(MI);
        //                }
        //            }
        //            ECProtocol.Phases.Phase.Add(PC);
        //            Serializer ser = new Serializer();
        //            string user = GetDbUser(SquintUser).ARIA_ID;
        //            ser.Serialize<SquintEclipseProtocol.Protocol>(ECProtocol, string.Format("{0}{1}_{2}_{3}.xml", @"H:\Physics\CN\Software\Squint\XML Output\",
        //                DataCache.Patient.Id, SA.AssessmentName, user));
        //        }
        //        catch
        //        {

        //        }
        //    }
        //    return Preview.ID;
        //}
        public static bool ImportProtocolFromXML(string filename)
        {
            SquintProtocolXML _XMLProtocol;
            if (filename == "") // no file selected
                return false;
            int ConstraintPKiterator = 0;
            int ComponentPKiterator = 0;
            Dictionary<string, int> ComponentID2PK = new Dictionary<string, int>();
            //Prepare to read in new protocol
            string protocolInput = string.Empty;
            try { protocolInput = File.ReadAllText(filename); }
            catch { MessageBox.Show(@"Error opening file - it may be in use?", @"Error opening file..."); return false; }
            Serializer ser = new Serializer();
            try
            {
                _XMLProtocol = ser.Deserialize<SquintProtocolXML>(protocolInput);
            }
            catch (Exception ex)
            {
                MessageBox.Show(filename);
                MessageBox.Show(ex.InnerException.Message, "Error in Deserialize");
                return false;
            }
            //XML error checking - if any required fields weren't populated, create them
            if (ReferenceEquals(null, _XMLProtocol.DVHConstraints))
            {
                _XMLProtocol.DVHConstraints = new SquintProtocolXML.DVHConstraintListDefinition();
                _XMLProtocol.DVHConstraints.DVHConstraintList = new List<SquintProtocolXML.DVHConstraintDefinition>();
            }
            if (ReferenceEquals(null, _XMLProtocol.ConformityIndexConstraints))
            {
                _XMLProtocol.ConformityIndexConstraints = new SquintProtocolXML.ConformityIndexConstraintListDefinition();
                _XMLProtocol.ConformityIndexConstraints.ConformityIndexConstraintList = new List<SquintProtocolXML.ConformityIndexConstraintDefinition>();
            }
            //Assign TEMPORARY primary keys to each constraint, since these are not defined in the XML protocol
            foreach (SquintProtocolXML.ComponentDefinition cd in _XMLProtocol.Components.Component)
            {
                cd.ID = ComponentPKiterator; // assign primary keys
                bool ValidComponent = false;
                if (cd.ComponentName != null)
                {
                    if (cd.ComponentName != "")
                    {
                        ComponentID2PK.Add(cd.ComponentName, cd.ID);
                        ValidComponent = true;
                    }
                }
                if (!ValidComponent)
                {
                    MessageBox.Show(string.Format("Component {0} does not have a valid ComponentName", ComponentPKiterator));
                    return false;
                }
                ComponentPKiterator++;

            }
            int DVHConstraintIndexCounter = 0;
            int CIConstraintIndexCounter = 0;
            foreach (SquintProtocolXML.DVHConstraintDefinition con in _XMLProtocol.DVHConstraints.DVHConstraintList)
            {
                if (con.DisplayOrder == -1) // default display order to the XML list order
                    con.DisplayOrder = ConstraintPKiterator;
                if (con.ComponentName == null)
                {
                    MessageBox.Show(string.Format("DVH Constraint #{0} is missing a component reference", DVHConstraintIndexCounter));
                    return false;
                }
                if (!ComponentID2PK.ContainsKey(con.ComponentName))
                {
                    MessageBox.Show(string.Format("DVH Constraint #{0} does not reference a valid component", DVHConstraintIndexCounter));
                    return false;
                }
                con.ComponentID = ComponentID2PK[con.ComponentName.Trim()];
                DVHConstraintIndexCounter++;
                ConstraintPKiterator++;
            }
            foreach (SquintProtocolXML.ConformityIndexConstraintDefinition con in _XMLProtocol.ConformityIndexConstraints.ConformityIndexConstraintList)
            {
                if (con.DisplayOrder == -1) // default display order to the XML list order
                    con.DisplayOrder = ConstraintPKiterator;
                con.ID = ConstraintPKiterator;
                if (!ComponentID2PK.ContainsKey(con.ComponentName))
                {
                    MessageBox.Show(string.Format("Conformity constraint #{0} does not reference a valid component", DVHConstraintIndexCounter));
                    return false;
                }
                con.ComponentID = ComponentID2PK[con.ComponentName];
                CIConstraintIndexCounter++;
                ConstraintPKiterator++;
            }
            bool ProtocolSavedCorrectly = SaveXMLProtocolToDatabase(_XMLProtocol);
            if (!ProtocolSavedCorrectly)
            {
                return false;
            }
            else
            {
                ProtocolListUpdated?.Invoke(null, EventArgs.Empty);
                return true;
            }
        }
        public static void Dispose()
        {
            if (A != null)
                A.Dispose();
        }
        public static bool SaveXMLProtocolToDatabase(SquintProtocolXML _XMLProtocol)
        {
            List<string> ExistingProtocolNames;
            using (SquintdBModel LocalContext = new SquintdBModel())
            {
                if (LocalContext.DbLibraryProtocols.Where(y => !y.isRetired) != null)
                {
                    ExistingProtocolNames = LocalContext.DbLibraryProtocols.Where(y => !y.isRetired).Select(x => x.ProtocolName).ToList();
                    if (ExistingProtocolNames.Contains(_XMLProtocol.ProtocolMetaData.ProtocolName))
                    {
                        int renamecounter = 1;
                        while (ExistingProtocolNames.Contains(_XMLProtocol.ProtocolMetaData.ProtocolName))
                            _XMLProtocol.ProtocolMetaData.ProtocolName = _XMLProtocol.ProtocolMetaData.ProtocolName + (renamecounter++).ToString();
                    }
                }
                DbUser User = LocalContext.DbUsers.Where(x => x.ARIA_ID == SquintUser).SingleOrDefault();
                if (User == null)
                {
                    MessageBox.Show("User not recognized", "Error");
                    return false;
                }
                DbLibraryProtocol P = LocalContext.DbLibraryProtocols.Create();
                P.ProtocolName = _XMLProtocol.ProtocolMetaData.ProtocolName;
                P.DbUser_ProtocolAuthor = User;
                P.LastModifiedBy = User.ARIA_ID;
                P.TreatmentCentreID = 1;
                P.DbUser_Approver = User;
                P.CreationDate = _XMLProtocol.ProtocolMetaData.ProtocolDate;
                lock (LockDb)
                {
                    P.DbProtocolType = LocalContext.DbProtocolTypes.Where(x => x.ProtocolType == (int)ProtocolTypes.Unset).Single();
                }
                if (_XMLProtocol.ProtocolMetaData.Intent != null)
                {
                    TreatmentIntents Intent;
                    if (Enum.TryParse(_XMLProtocol.ProtocolMetaData.Intent, out Intent))
                    {
                        P.TreatmentIntent = (int)Intent;
                    }
                    else
                        P.TreatmentIntent = (int)TreatmentIntents.Unset; // unset
                }
                if (_XMLProtocol.ProtocolMetaData.ApprovalStatus != null)
                {
                    ApprovalLevels ApprovalLevel;
                    if (Enum.TryParse(_XMLProtocol.ProtocolMetaData.ApprovalStatus, out ApprovalLevel))
                    {
                        P.DbApprovalLevel = LocalContext.DbApprovalLevels.Where(x => x.ApprovalLevel == (int)ApprovalLevel).Single();
                    }
                    else
                        P.DbApprovalLevel = LocalContext.DbApprovalLevels.Where(x => x.ApprovalLevel == (int)ApprovalLevels.Unapproved).Single();
                }
                else
                {
                    P.DbApprovalLevel = LocalContext.DbApprovalLevels.Where(x => x.ApprovalLevel == (int)ApprovalLevels.Unapproved).Single();
                }
                if (_XMLProtocol.ProtocolMetaData.DiseaseSite != null)
                {
                    TreatmentSites DiseaseSite;
                    if (Enum.TryParse(_XMLProtocol.ProtocolMetaData.DiseaseSite, out DiseaseSite))
                    {
                        P.DbTreatmentSite = LocalContext.DbTreatmentSites.Where(x => x.TreatmentSite == (int)DiseaseSite).Single();
                    }
                    else
                        P.DbTreatmentSite = LocalContext.DbTreatmentSites.Where(x => x.TreatmentSite == (int)TreatmentSites.Unset).Single();
                }
                else
                {
                    P.DbTreatmentSite = LocalContext.DbTreatmentSites.Where(x => x.TreatmentSite == (int)TreatmentSites.Unset).Single();
                }
                if (_XMLProtocol.ProtocolMetaData.TreatmentCentre != null)
                {
                    TreatmentCentres TreatmentCentre;
                    if (Enum.TryParse(_XMLProtocol.ProtocolMetaData.TreatmentCentre, out TreatmentCentre))
                    {
                        P.DbTreatmentCentre = LocalContext.DbTreatmentCentres.Where(x => x.TreatmentCentre == (int)TreatmentCentre).Single();
                    }
                    else
                        P.DbTreatmentCentre = LocalContext.DbTreatmentCentres.Where(x => x.TreatmentCentre == (int)TreatmentCentres.Unset).Single();
                }
                else
                {
                    P.DbTreatmentCentre = LocalContext.DbTreatmentCentres.Where(x => x.TreatmentCentre == (int)TreatmentCentres.Unset).Single();
                }
                if (_XMLProtocol.ProtocolMetaData.ProtocolType != null)
                {
                    ProtocolTypes ProtocolType;
                    if (Enum.TryParse(_XMLProtocol.ProtocolMetaData.ProtocolType, out ProtocolType))
                    {
                        P.DbProtocolType = LocalContext.DbProtocolTypes.Where(x => x.ProtocolType == (int)ProtocolType).Single();
                    }
                    else
                        P.DbProtocolType = LocalContext.DbProtocolTypes.Where(x => x.ProtocolType == (int)ProtocolTypes.Unset).Single();
                }
                else
                {
                    P.DbProtocolType = LocalContext.DbProtocolTypes.Where(x => x.ProtocolType == (int)ProtocolTypes.Unset).Single();
                }
                P.ID = IDGenerator();
                LocalContext.DbLibraryProtocols.Add(P);
                try
                {
                    //   LocalContext.SaveChanges(); // protocol key is now available
                }
                catch (Exception ex)
                {
                    MessageBox.Show(P.ProtocolName);
                }
                Dictionary<string, DbComponent> CompName2DB = new Dictionary<string, DbComponent>();
                List<string> ComponentTypeText = new List<string>();
                foreach (ComponentTypes T in Enum.GetValues(typeof(ComponentTypes)))
                {
                    ComponentTypeText.Add(T.Display());
                }
                int DisplayOrder = 1;
                Dictionary<string, int> ProtocolStructureNameToID = new Dictionary<string, int>();
                foreach (SquintProtocolXML.StructureDefinition S in _XMLProtocol.Structures.Structure)
                {
                    DbECSID ECSID = LocalContext.DbECSIDs.Create();
                    if (S.ProtocolStructureName == null)
                    {
                        MessageBox.Show(string.Format("Error: Structure in protocol {0} is missing the field ProtocolStructureName", _XMLProtocol.ProtocolMetaData.ProtocolName));
                        return false;
                    }
                    else
                    {
                        ECSID.ProtocolStructureName = S.ProtocolStructureName;
                    }
                    if (S.EclipseAliases != null)
                    {
                        foreach (SquintProtocolXML.EclipseAlias EA in S.EclipseAliases.EclipseId)
                        {
                            if (ECSID.DefaultEclipseAliases == null)
                            {
                                ECSID.DefaultEclipseAliases = string.Format("{0};", EA.Id);
                            }
                            else
                                ECSID.DefaultEclipseAliases = ECSID.DefaultEclipseAliases + string.Format("{0};", EA.Id);
                        }
                    }
                    if (S.StructureChecklist != null)
                    {
                        DbStructureChecklist DbSC = LocalContext.DbStructureChecklists.Create();
                        var SC = S.StructureChecklist;
                        if (SC.PointContourCheck != null)
                        {
                            DbSC.isPointContourChecked = true;
                            var T = SC.PointContourCheck.Threshold as double?;
                            if (T != null)
                            {
                                DbSC.PointContourThreshold = SC.PointContourCheck.Threshold;
                            }
                            LocalContext.DbStructureChecklists.Add(DbSC);
                        }
                        ECSID.DbStructureChecklist = DbSC;
                    }
                    else
                        ECSID.StructureChecklistID = 1; // default;
                    int StructureLabelID = 1;
                    if (S.Label.ToUpper() == "BODY")
                    {
                        string schema = SchemaFullNames.VMS.Display().ToUpper();
                        List<string> test = LocalContext.DbStructureLabels.Where(x => x.StructureLabel.ToUpper() == S.Label.ToUpper()).Select(x => x.Designator).ToList();
                        StructureLabelID = LocalContext.DbStructureLabels.Where(x => x.StructureLabel.ToUpper() == S.Label.ToUpper() && x.Designator.ToUpper() == schema).SingleOrDefault().ID;
                    }
                    else
                    {
                        switch (LocalContext.DbStructureLabels.Where(x => x.StructureLabel.ToUpper() == S.Label.ToUpper()).Count())
                        {
                            case 0:
                                S.Label = "Unset";
                                StructureLabelID = 1;
                                break;
                            case 1:
                                StructureLabelID = LocalContext.DbStructureLabels.Where(x => x.StructureLabel.ToUpper() == S.Label.ToUpper()).SingleOrDefault().ID;
                                break;
                            default:
                                string schema = SchemaFullNames.VMS.Display().ToUpper();
                                StructureLabelID = LocalContext.DbStructureLabels.Where(x => x.StructureLabel.ToUpper() == S.Label.ToUpper() && x.Designator.ToUpper() == schema).SingleOrDefault().ID;
                                break;
                        }
                    }
                    ECSID.StructureLabelID = StructureLabelID;
                    ECSID.ProtocolID = P.ID;
                    ECSID.DisplayOrder = DisplayOrder++;
                    ECSID.ID = IDGenerator();
                    LocalContext.DbECSIDs.Add(ECSID);
                    ProtocolStructureNameToID.Add(ECSID.ProtocolStructureName, ECSID.ID);
                }
                int ComponentDisplayOrder = 1;
                foreach (SquintProtocolXML.ComponentDefinition comp in _XMLProtocol.Components.Component)
                {
                    DbComponent C = LocalContext.DbComponents.Create();
                    C.DisplayOrder = ComponentDisplayOrder++;
                    C.DbLibraryProtocol = P;
                    C.ComponentName = comp.ComponentName;
                    C.NumFractions = comp.NumFractions;
                    if (comp.Type == "Phase")
                        comp.Type = "Plan"; // backward compatibility
                    C.ComponentType = ComponentTypeText.IndexOf(comp.Type);
                    C.ReferenceDose = comp.ReferenceDose;
                    LocalContext.DbComponents.Add(C);
                    //LocalContext.SaveChanges(); // component Key available
                    CompName2DB.Add(comp.ComponentName, C);
                    //Add imaging
                    if (comp.ImagingProtocols.ImagingProtocol != null)
                    {
                        ImagingProtocols ProtocolType;
                        DbComponentImaging DbCI = LocalContext.DbComponentImagings.Create();
                        LocalContext.DbComponentImagings.Add(DbCI);
                        DbCI.ID = IDGenerator();
                        DbCI.DbComponent = C;
                        foreach (var IP in comp.ImagingProtocols.ImagingProtocol)
                        {
                            DbImaging DbI = LocalContext.DbImagings.Create();
                            if (Enum.TryParse(IP.Id, true, out ProtocolType))
                            {
                                DbI.ImagingProtocol = (int)ProtocolType;
                                DbI.ImagingProtocolName = ProtocolType.Display();
                                DbI.ComponentImagingID = DbCI.ID;
                            }
                            else
                            {
                                DbI.ImagingProtocol = (int)ProtocolTypes.Unset;
                                DbI.ImagingProtocolName = ProtocolTypes.Unset.Display();
                                DbI.ComponentImagingID = DbCI.ID;
                            }
                            LocalContext.DbImagings.Add(DbI);
                        }
                    }
                    if (comp.Beams.Beam != null)
                    {
                        C.MinBeams = comp.Beams.MinBeams;
                        C.MaxBeams = comp.Beams.MaxBeams;
                        C.NumIso = comp.Beams.NumIso;
                        C.MinColOffset = comp.Beams.MinColOffset;
                        foreach (var b in comp.Beams.Beam)
                        {
                            DbBeam B = LocalContext.DbBeams.Create();
                            LocalContext.DbBeams.Add(B);
                            foreach (var Alias in b.EclipseAliases.EclipseId)
                            {
                                DbBeamAlias DbBA = LocalContext.DbBeamAliases.Create();
                                DbBA.EclipseFieldId = Alias.Id;
                                DbBA.DbBeam = B;
                                LocalContext.DbBeamAliases.Add(DbBA);
                            }
                            foreach (var ArcG in b.ValidGeometries.Geometry)
                            {
                                DbBeamGeometry DbAG = LocalContext.DbBeamGeometries.Create();
                                DbAG.DbBeam = B;
                                DbAG.GeometryName = ArcG.GeometryName;
                                Trajectories T;
                                if (Enum.TryParse(ArcG.Trajectory, out T))
                                {
                                    DbAG.Trajectory = (int)T;
                                }
                                else
                                    DbAG.Trajectory = (int)Trajectories.Unset;
                                if (ArcG.MinStartAngle > -1)
                                    DbAG.MinStartAngle = ArcG.MinStartAngle;
                                else
                                    DbAG.MinStartAngle = ArcG.MaxStartAngle;
                                if (ArcG.MinEndAngle > -1)
                                    DbAG.MinEndAngle = ArcG.MinEndAngle;
                                else
                                    DbAG.MinEndAngle = DbAG.MaxEndAngle;
                                if (ArcG.MaxStartAngle > -1)
                                    DbAG.MaxStartAngle = ArcG.MaxStartAngle;
                                else
                                    DbAG.MaxStartAngle = DbAG.MinStartAngle;
                                if (ArcG.MaxEndAngle > -1)
                                    DbAG.MaxEndAngle = ArcG.MaxEndAngle;
                                else
                                    DbAG.MaxEndAngle = DbAG.MinEndAngle;
                                LocalContext.DbBeamGeometries.Add(DbAG);
                            }
                            B.DbComponent = C;
                            B.ProtocolBeamName = b.ProtocolBeamName;
                            B.CouchRotation = b.CouchRotation;
                            B.MaxColRotation = b.MaxColRotation;
                            B.MinColRotation = b.MinColRotation;
                            B.MaxMUWarning = b.MaxMUWarning;
                            B.MinMUWarning = b.MinMUWarning;
                            B.MinX = b.MinX;
                            B.MaxX = b.MaxX;
                            B.MinY = b.MinY;
                            B.MaxY = b.MaxY;
                            B.ToleranceTable = b.ToleranceTable;
                            B.BolusClinicalHU = b.RefBolusHU;
                            bool UnsetEnergyAdded = false;
                            foreach (var E in b.Energies.Energy)
                            {
                                DbEnergy DbEn = LocalContext.DbEnergies.FirstOrDefault(x => x.EnergyString == E.Mode);
                                if (DbEn != null)
                                {
                                    if (B.DbEnergies == null)
                                        B.DbEnergies = new List<DbEnergy>() { DbEn };
                                    else
                                        B.DbEnergies.Add(DbEn);
                                }
                                else
                                {
                                    if (!UnsetEnergyAdded)
                                    {
                                        if (B.DbEnergies == null)
                                            B.DbEnergies = new List<DbEnergy>() { LocalContext.DbEnergies.Find((int)Energies.Unset) };
                                        else
                                            B.DbEnergies.Add(LocalContext.DbEnergies.Find((int)Energies.Unset));
                                        UnsetEnergyAdded = true;
                                    }
                                }
                            }
                            ParameterOptions BolusIndication;
                            if (Enum.TryParse(b.Bolus.Indication, out BolusIndication))
                            {
                                B.BolusClinicalIndication = (int)BolusIndication;
                                B.BolusClinicalHU = b.Bolus.HU;
                                B.BolusClinicalMinThickness = b.Bolus.MinThickness;
                                B.BolusClinicalMaxThickness = b.Bolus.MaxThickness;
                            }
                            else
                                B.BolusClinicalIndication = (int)ParameterOptions.Unset;

                            FieldType Technique;
                            if (Enum.TryParse(b.Technique, out Technique))
                            {
                                B.Technique = (int)Technique;
                            }
                            else
                                B.Technique = (int)FieldType.Unset;
                            ParameterOptions VMAT_JawTracking;
                            if (Enum.TryParse(b.VMAT_JawTracking, out VMAT_JawTracking))
                            {
                                B.VMAT_JawTracking = (int)VMAT_JawTracking;
                            }
                            else
                                B.VMAT_JawTracking = (int)ParameterOptions.Unset;
                        }
                    }
                    // Add checklist 
                    if (_XMLProtocol.ComponentDefaults != null) // import default component checklist
                    {
                        DbComponentChecklist Checklist = LocalContext.DbComponentChecklists.Create();
                        LocalContext.DbComponentChecklists.Add(Checklist);
                        Checklist.ID = IDGenerator();
                        Checklist.DbComponent = C;
                        if (_XMLProtocol.ComponentDefaults.Calculation != null)
                        {
                            AlgorithmTypes AlgorithmType;
                            if (Enum.TryParse(_XMLProtocol.ComponentDefaults.Calculation.Algorithm, out AlgorithmType))
                                Checklist.Algorithm = (int)AlgorithmType;
                            else
                                Checklist.Algorithm = (int)AlgorithmTypes.Unset;
                            FieldNormalizationTypes FNM;
                            if (Enum.TryParse(_XMLProtocol.ComponentDefaults.Calculation.FieldNormalizationMode, out FNM))
                                Checklist.FieldNormalizationMode = (int)FNM;
                            else
                                Checklist.FieldNormalizationMode = (int)FieldNormalizationTypes.Unset;
                            Checklist.HeterogeneityOn = _XMLProtocol.ComponentDefaults.Calculation.HeterogeneityOn;
                            Checklist.AlgorithmResolution = _XMLProtocol.ComponentDefaults.Calculation.AlgorithmResolution;
                        }
                        if (_XMLProtocol.ComponentDefaults.Supports != null)
                        {
                            ParameterOptions SupportIndication;
                            if (Enum.TryParse(_XMLProtocol.ComponentDefaults.Supports.Indication, out SupportIndication))
                            {
                                Checklist.SupportIndication = (int)SupportIndication;
                            }
                            else
                                Checklist.SupportIndication = (int)ParameterOptions.Unset;

                            Checklist.CouchInterior = _XMLProtocol.ComponentDefaults.Supports.CouchInterior;
                            Checklist.CouchSurface = _XMLProtocol.ComponentDefaults.Supports.CouchSurface;
                        }
                        if (_XMLProtocol.ComponentDefaults.Simulation != null)
                        {
                            Checklist.SliceSpacing = _XMLProtocol.ComponentDefaults.Simulation.SliceSpacing;
                        }
                        if (_XMLProtocol.ComponentDefaults.Prescription != null)
                        {
                            Checklist.PNVMax = _XMLProtocol.ComponentDefaults.Prescription.PNVMax;
                            Checklist.PNVMin = _XMLProtocol.ComponentDefaults.Prescription.PNVMin;
                            Checklist.PrescribedPercentage = _XMLProtocol.ComponentDefaults.Prescription.PrescribedPercentage;
                        }

                        if (_XMLProtocol.ComponentDefaults.Artifacts != null)
                        {
                            foreach (SquintProtocolXML.ArtifactDefinition A in _XMLProtocol.ComponentDefaults.Artifacts.Artifact)
                            {
                                DbArtifact DbA = LocalContext.DbArtifacts.Create();
                                LocalContext.DbArtifacts.Add(DbA);
                                DbA.HU = A.HU;
                                DbA.ToleranceHU = A.ToleranceHU;
                                DbA.DbComponentChecklist = Checklist;
                                DbA.ECSID_ID = ProtocolStructureNameToID[A.ProtocolStructureName];
                            }
                        }
                    }
                }
                foreach (SquintProtocolXML.DVHConstraintDefinition con in _XMLProtocol.DVHConstraints.DVHConstraintList)
                {
                    UnitScale ConstraintScale = new UnitScale();
                    UnitScale ReferenceScale = new UnitScale();
                    ConstraintTypeCodes ConstraintType = new ConstraintTypeCodes();
                    ReferenceTypes ReferenceType = new ReferenceTypes();
                    switch (con.DvhType)
                    {
                        case "V":
                            ConstraintType = ConstraintTypeCodes.V;
                            break;
                        case "D":
                            ConstraintType = ConstraintTypeCodes.D;
                            break;
                        case "CV":
                            ConstraintType = ConstraintTypeCodes.CV;
                            break;
                        case "M":
                            ConstraintType = ConstraintTypeCodes.M;
                            break;
                    }
                    switch (con.DvhUnit)
                    {
                        case "relative":
                            ConstraintScale = UnitScale.Relative;
                            break;
                        case "absolute":
                            ConstraintScale = UnitScale.Absolute;
                            break;
                    }
                    switch (con.ConstraintType)
                    {
                        case "upper":
                            ReferenceType = ReferenceTypes.Upper;
                            break;
                        case "lower":
                            ReferenceType = ReferenceTypes.Lower;
                            break;
                    }
                    switch (con.ConstraintUnit)
                    {
                        case "relative":
                            ReferenceScale = UnitScale.Relative;
                            break;
                        case "absolute":
                            ReferenceScale = UnitScale.Absolute;
                            break;
                    }
                    DbConstraint dBconDVH = LocalContext.DbConstraints.Create();
                    dBconDVH.DbComponent = CompName2DB[con.ComponentName];
                    dBconDVH.DisplayOrder = con.DisplayOrder;
                    dBconDVH.ConstraintValue = con.DvhVal;
                    dBconDVH.ConstraintType = (int)ConstraintType;
                    var DbECSID = LocalContext.DbECSIDs.Local.FirstOrDefault(x => x.ProtocolID == P.ID && x.ProtocolStructureName == con.ProtocolStructureName);
                    if (DbECSID != null)
                        dBconDVH.DbECSID_Primary = DbECSID;
                    else
                    {
                        MessageBox.Show(string.Format("Constraint references protocol structure {0}, but this structure is not defined", con.ProtocolStructureName));
                        return false;
                    }
                    dBconDVH.SecondaryStructureID = 1; // default unset
                    dBconDVH.ReferenceValue = con.ConstraintVal;
                    dBconDVH.ReferenceType = (int)ReferenceType;
                    dBconDVH.ConstraintScale = (int)ConstraintScale;
                    dBconDVH.ReferenceScale = (int)ReferenceScale;
                    dBconDVH.Fractions = CompName2DB[con.ComponentName].NumFractions;
                    LocalContext.DbConstraints.Add(dBconDVH);
                    //LocalContext.SaveChanges();
                    List<DbConThresholdDef> test = LocalContext.DbConThresholdDefs.ToList();
                    if (con.MajorViolation != null)
                    {
                        DbConThreshold DbConThreshold = new DbConThreshold()
                        {
                            DbConstraint = dBconDVH,
                            ThresholdValue = Double.Parse(con.MajorViolation),
                            DbConThresholdDef = LocalContext.DbConThresholdDefs.Where(x => x.Threshold == (int)ConstraintThresholdNames.MajorViolation).Single()
                        };
                        LocalContext.DbConThresholds.Add(DbConThreshold);
                        //LocalContext.SaveChanges();
                    }
                    if (con.MinorViolation != null)
                    {
                        DbConThreshold DbConThreshold = new DbConThreshold()
                        {
                            DbConstraint = dBconDVH,
                            ThresholdValue = Double.Parse(con.MinorViolation),
                            DbConThresholdDef = LocalContext.DbConThresholdDefs.Where(x => x.Threshold == (int)ConstraintThresholdNames.MinorViolation).Single()
                        };
                        LocalContext.DbConThresholds.Add(DbConThreshold);
                        //LocalContext.SaveChanges();
                    }
                    else
                    {
                        if (con.MajorViolation == null) // both major and minor DataCache.Constraints are null
                        {
                            DbConThreshold DbConThreshold = new DbConThreshold()
                            {
                                DbConstraint = dBconDVH,
                                ThresholdValue = con.ConstraintVal,
                                DbConThresholdDef = LocalContext.DbConThresholdDefs.Where(x => x.Threshold == (int)ConstraintThresholdNames.MinorViolation).Single()
                            };
                            LocalContext.DbConThresholds.Add(DbConThreshold);
                            // LocalContext.SaveChanges();
                        }
                        else
                        {
                            if (Math.Abs(Double.Parse(con.MajorViolation) - con.ConstraintVal) > 0.01)
                            {
                                DbConThreshold DbConThreshold = new DbConThreshold()
                                {
                                    DbConstraint = dBconDVH,
                                    ThresholdValue = con.ConstraintVal,
                                    DbConThresholdDef = LocalContext.DbConThresholdDefs.Where(x => x.Threshold == (int)ConstraintThresholdNames.MinorViolation).Single()
                                };
                                LocalContext.DbConThresholds.Add(DbConThreshold);
                                // LocalContext.SaveChanges();
                            }
                        }
                    }
                    if (con.Stop != null)
                    {
                        DbConThreshold DbConThreshold = new DbConThreshold()
                        {
                            DbConstraint = dBconDVH,
                            ThresholdValue = Double.Parse(con.Stop),
                            DbConThresholdDef = LocalContext.DbConThresholdDefs.Where(x => x.Threshold == (int)ConstraintThresholdNames.Stop).Single()
                        };
                        LocalContext.DbConThresholds.Add(DbConThreshold);
                        // LocalContext.SaveChanges();
                    }
                    // Create initial log
                    DbConstraintChangelog DbCC = LocalContext.DbConstraintChangelogs.Create();
                    LocalContext.DbConstraintChangelogs.Add(DbCC);
                    DbCC.ChangeDescription = con.Description;
                    DbCC.ConstraintString = con.GetConstraintString();
                    DbCC.ChangeAuthor = Ctr.SquintUser;
                    DbCC.Date = DateTime.Now.ToBinary();
                    DbCC.DbConstraint = dBconDVH;
                    DbCC.ParentLogID = 1; // the root dummy log
                }
                foreach (SquintProtocolXML.ConformityIndexConstraintDefinition con in _XMLProtocol.ConformityIndexConstraints.ConformityIndexConstraintList)
                {
                    ConstraintTypeCodes ConstraintType = ConstraintTypeCodes.CI;
                    ReferenceTypes ReferenceType = new ReferenceTypes();
                    UnitScale ConstraintScale = new UnitScale();
                    UnitScale ReferenceScale = new UnitScale();
                    switch (con.ConstraintType)
                    {
                        case "upper":
                            ReferenceType = ReferenceTypes.Upper;
                            break;
                        case "lower":
                            ReferenceType = ReferenceTypes.Lower;
                            break;
                    }
                    switch (con.DoseUnit)
                    {
                        case "relative":
                            ConstraintScale = UnitScale.Relative;
                            break;
                        case "absolute":
                            ConstraintScale = UnitScale.Absolute;
                            break;
                    }
                    switch (con.ConstraintUnit)
                    {
                        case "percent":
                            ReferenceScale = UnitScale.Relative;
                            break;
                        case "multiple":
                            ReferenceScale = UnitScale.Absolute;
                            break;
                    }

                    DbConstraint DbConCI = LocalContext.DbConstraints.Create();
                    DbConCI.DbComponent = CompName2DB[con.ComponentName];
                    DbConCI.ConstraintType = (int)ConstraintType;
                    DbConCI.DisplayOrder = con.DisplayOrder;
                    DbConCI.ReferenceValue = con.ConstraintVal;
                    DbConCI.ReferenceType = (int)ReferenceType;
                    try
                    {
                        DbConCI.DbECSID_Primary = LocalContext.DbECSIDs.Local.Where(x => x.ProtocolStructureName == con.PrimaryStructureName && x.ProtocolID == P.ID).Single();
                        DbConCI.DbECSID_Secondary = LocalContext.DbECSIDs.Local.Where(x => x.ProtocolStructureName == con.ReferenceStructureName && x.ProtocolID == P.ID).Single();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("Constraint references protocol structure {0} and {1}, but at least one of these structures is not defined", con.PrimaryStructureName, con.ReferenceStructureName));
                        return false;
                    }
                    DbConCI.ConstraintValue = con.DoseVal;
                    DbConCI.ConstraintScale = (int)ConstraintScale;
                    DbConCI.ReferenceScale = (int)ReferenceScale;
                    DbConCI.Fractions = CompName2DB[con.ComponentName].NumFractions;
                    LocalContext.DbConstraints.Add(DbConCI);
                    //LocalContext.SaveChanges();
                    if (con.MajorViolation != null)
                    {
                        DbConThreshold DbConThreshold = new DbConThreshold()
                        {
                            DbConstraint = DbConCI,
                            ThresholdValue = Double.Parse(con.MajorViolation),
                            DbConThresholdDef = LocalContext.DbConThresholdDefs.Where(x => x.Threshold == (int)ConstraintThresholdNames.MajorViolation).Single()
                        };
                        LocalContext.DbConThresholds.Add(DbConThreshold);
                    }
                    if (con.MinorViolation != null)
                    {
                        DbConThreshold DbConThreshold = new DbConThreshold()
                        {
                            DbConstraint = DbConCI,
                            ThresholdValue = Double.Parse(con.MinorViolation),
                            DbConThresholdDef = LocalContext.DbConThresholdDefs.Where(x => x.Threshold == (int)ConstraintThresholdNames.MinorViolation).Single()
                        };
                        LocalContext.DbConThresholds.Add(DbConThreshold);
                    }
                    if (con.Stop != null)
                    {
                        DbConThreshold DbConThreshold = new DbConThreshold()
                        {
                            DbConstraint = DbConCI,
                            ThresholdValue = Double.Parse(con.Stop),
                            DbConThresholdDef = LocalContext.DbConThresholdDefs.Where(x => x.Threshold == (int)ConstraintThresholdNames.Stop).Single()
                        };
                        LocalContext.DbConThresholds.Add(DbConThreshold);
                    }
                    // Create initial log
                    DbConstraintChangelog DbCC = LocalContext.DbConstraintChangelogs.Create();
                    LocalContext.DbConstraintChangelogs.Add(DbCC);
                    DbCC.ChangeDescription = con.Description;
                    DbCC.ConstraintString = con.GetConstraintString();
                    DbCC.ChangeAuthor = Ctr.SquintUser;
                    DbCC.Date = DateTime.Now.ToBinary();
                    DbCC.DbConstraint = DbConCI;
                    DbCC.ParentLogID = 1; // the root dummy log
                }
                //Commit changes
                try
                {
                    LocalContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0} {1} {2}", ex.Message, ex.InnerException, ex.StackTrace));
                }
                return true;
            }
        }
        public static void LoadProtocolFromDb(string ProtocolName, IProgress<int> progress = null) // this method reloads the context.
        {
            try
            {
                CloseProtocol();
                DataCache.LoadProtocol(ProtocolName);
                ActivePV = new ProtocolView(DataCache.CurrentProtocol);
                foreach (Component Comp in DataCache.GetAllComponents())
                {
                    new Component(Comp);
                }
                //foreach (Constraint Con in DataCache.GetAllConstraints())
                //{
                //    ConstraintView CV = new ConstraintView(Con);
                //}
                ProtocolLoaded = true;
                ProtocolOpened?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\r\n {1}\r\n {2}", ex.Message, ex.InnerException, ex.StackTrace));
                return;
            }
        }


        public static void Save_UpdateProtocol()
        {
            DataCache.Save_UpdateProtocol();
            ProtocolListUpdated?.Invoke(null, EventArgs.Empty);
        }
        public static void Save_DuplicateProtocol()
        {
            DataCache.Save_DuplicateProtocol();
            ProtocolListUpdated?.Invoke(null, EventArgs.Empty);
        }
        public static void DeleteProtocol(int Id)
        {
            bool Deleted = DataCache.Delete_Protocol(Id);
            if (Deleted)
                ProtocolListUpdated?.Invoke(null, EventArgs.Empty);
        }
        public static async Task<bool> Save_Session(string SessionComment)
        {
            if (PatientLoaded & ProtocolLoaded)
            {
                bool Success = await DataCache.Save_Session(SessionComment);
                if (Success)
                {
                    SessionsChanged?.Invoke(null, EventArgs.Empty);
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        public static void Delete_Session(int ID)
        {
            DataCache.Delete_Session(ID);
            SessionsChanged?.Invoke(null, EventArgs.Empty);
        }
        public static async Task<bool> Load_Session(int ID)
        {
            if (PatientLoaded)
            {
                if (ProtocolLoaded)
                    CloseProtocol();
                DataCache.ClearAssessments();
                if (await DataCache.Load_Session(ID))  // true if successful load
                {
                    ActivePV = new ProtocolView(DataCache.CurrentProtocol);
                    //foreach (Assessment A in DataCache.GetAllAssessments())
                    //{
                    //    new AssessmentView(A);
                    //}
                    foreach (Component Comp in DataCache.GetAllComponents())
                    {
                        new Component(Comp);
                    }
                    foreach (Constraint Con in DataCache.GetAllConstraints())
                    {
                        foreach (Assessment A in DataCache.GetAllAssessments())
                        {
                            Con.RegisterAssessment(A);
                        }
                    }
                    foreach (ECPlan P in DataCache.GetAllPlans())
                    {
                        var C = await DataCache.GetCourseAsync(P.CourseName);
                        DataCache.GetAssessment(P.AssessmentID).RegisterPlan(P);
                        if (P.Linked)
                        {
                            var AP = C.Plans.FirstOrDefault(x => x.Id == P.PlanName);
                            if (AP != null)
                                P.Reassociate(AP, false);
                        }
                    }
                    ProtocolLoaded = true;
                    ProtocolOpened?.Invoke(null, EventArgs.Empty);
                }
                return true;
            }
            else return false;
        }
        public static async void SynchronizePlans()
        {
            await SynchronizePlansInternal();
        }
        private static async Task SynchronizePlansInternal()
        {
            if (DataCache.Patient != null)
            {
                //
                string PID = DataCache.Patient.Id;
                Dictionary<int, DateTime> PlanLastModified = new Dictionary<int, DateTime>();
                Dictionary<int, string> IdToPlanName = new Dictionary<int, string>();
                Dictionary<int, string> IdToCourseName = new Dictionary<int, string>();
                Dictionary<int, int> IdtoHashId = new Dictionary<int, int>();
                List<string> PlanIDs = new List<string>();
                List<string> PlanCourse = new List<string>();
                foreach (ECPlan ECP in DataCache.GetAllPlans())
                {

                    if (!ECP.Linked)
                    {
                        continue;
                    }
                    PlanIDs.Add(ECP.LinkedPlan.Id);
                    PlanCourse.Add(ECP.LinkedPlan.Course.Id);
                    IdtoHashId.Add(ECP.ID, ECP.LinkedPlan.HashId);
                    PlanLastModified.Add(ECP.ID, ECP.LinkedPlan.HistoryDateTime);
                    IdToPlanName.Add(ECP.ID, ECP.LinkedPlan.Id); // plan id to eclipse hash id
                    IdToCourseName.Add(ECP.ID, ECP.LinkedPlan.Course.Id);
                }
                DataCache.RefreshPatient();
                // Disable autorefresh (see below for reasons)
                foreach (Assessment SA in DataCache.GetAllAssessments())
                {
                    SA.AutoCalculate = false;
                }
                try
                {
                    foreach (ECPlan ECP in DataCache.GetAllPlans())
                    {
                        AsyncPlan p = null;
                        if (!ECP.Linked)
                        {
                            continue; // Usually an assessment loaded without a valid plan
                        }
                        AsyncCourse C = await DataCache.GetCourseAsync(IdToCourseName[ECP.ID]);
                        if (C != null)
                        {
                            p = C.Plans.Find(x => x.Id == IdToPlanName[ECP.ID]);
                            if (p == null)
                            {
                                MessageBox.Show(string.Format("Plan {0} in Course {1} is no longer found", IdToPlanName[ECP.ID], IdToCourseName[ECP.ID]));
                                //ECP.Reassociate(null);
                            }
                            else
                            {
                                if (p.HistoryDateTime != PlanLastModified[ECP.ID])
                                {
                                    MessageBox.Show(string.Format("Note: Dose distribution in plan {0} has changed since last evaluation", IdToPlanName[ECP.ID]));
                                    ECP.Reassociate(p, false); // Reassociated fires PlanMappingChanged event which is subscribed to by the assessment, which is subscribed to by the DataCache.Constraints, which update in response
                                }
                                else
                                {
                                    ECP.LinkedPlan = p;
                                    // ECP.RefreshTime = DateTime.Now;
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(string.Format("Course {0} no longer exists", IdToCourseName[ECP.ID]));
                            ECP.Reassociate(p, false);
                        }
                    }
                    foreach (ECSID E in DataCache.GetAllECSIDs()) //This is a kludge while structures are derived from plans not structure sets.
                    {
                        if (E.ES != null)
                            if (CurrentStructureSet.UID == E.ES.StructureSetUID)
                            {
                                var UpdatedStructure = CurrentStructureSet.GetStructure(E.ES.Id);
                                if (UpdatedStructure != null)
                                    E.ES.Update(CurrentStructureSet.GetStructure(E.ES.Id)); // Have to disable autocalculation or this will provoke an update on constraints linked to plans that haven't been relinked yet
                                else
                                    E.ES = new EclipseStructure(null);
                            }
                            else
                                E.ES = new EclipseStructure(null);
                    }
                    foreach (Assessment SA in DataCache.GetAllAssessments())
                    {
                        SA.AutoCalculate = true;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                }
                SynchronizationComplete?.Invoke(null, EventArgs.Empty);
            }
        }
        //public static async void LoadAssessmentFromDb(string AssessmentName, bool AskToLoadAssociatedAssessments = true)
        //{
        //    //if (DataCache.Patient == null)
        //    //    return;
        //    //else
        //    //{
        //    //    (ProgressBar as IProgress<int>).Report(10);
        //    //    CloseProtocol();
        //    //    ProtocolClosed?.Raise(null, EventArgs.Empty);
        //    //    (ProgressBar as IProgress<int>).Report(50);
        //    //    DbAssessment DbSA = SquintDb.Context.DbAssessments.Where(x => x.PID == DataCache.Patient.Id && x.AssessmentName == AssessmentName).SingleOrDefault();
        //    //    if (DbSA != null)
        //    //    {
        //    //        ProtocolLoaded = LoadProtocol(DbSA.DbLibraryProtocol); // this subroutine doesn't announce constraint creation since DataCache.Assessments may include user-added DataCache.Constraints
        //    //        if (DbSA.DbSession != null)
        //    //            if (DataCache.CurrentSession == null)
        //    //                DataCache.CurrentSession = new Session(DbSA.DbSession); // this applies all exceptions to the protocol
        //    //            else
        //    //            {
        //    //                //DataCache.ConstraintThresholds
        //    //                if (DataCache.CurrentSession.DbO != DbSA.DbSession)
        //    //                    MessageBox.Show("Protocol Exceptions don't match");
        //    //            }
        //    //        Assessment SA = new Assessment(DbSA);
        //    //        //Now load and attach plans
        //    //        foreach (DbPlan DbP in SquintDb.Context.DbPlans.Where(x => x.AssessmentID == SA.ID).ToList())
        //    //        {
        //    //            if (!isPlanLoaded(DbP.CourseName, DbP.PlanName))
        //    //            {
        //    //                await GetCourseByName(DbP.CourseName, null, false);
        //    //            }
        //    //            ECPlan ECP = new ECPlan(DbP);
        //    //            Plans.Add(ECP.ID,ECP);// add plan to cache if not already loaded
        //    //            SA.RegisterPlan(ECP);
        //    //        }
        //    //        foreach (Constraint Con in DataCache.Constraints.Values)
        //    //        {
        //    //            DbConstraintResult DbCR = DbSA.ConstraintResults.Where(x => x.SessionConstraintID == Con.ID).SingleOrDefault();
        //    //            if (DbCR == null)
        //    //            {
        //    //                //MessageBox.Show("Evaluating loaded constraint.  This is a bug as some constraintresult should have been saved");
        //    //            }
        //    //            else
        //    //            {
        //    //                Con.ImportEvaluatedConstraint(DbCR);
        //    //            }
        //    //        }
        //    //        //Generate views
        //    //        ActivePV = new ProtocolView(DataCache.CurrentProtocol);
        //    //        foreach (Component Comp in DataCache.GetAllComponents())
        //    //        {
        //    //            new Component(Comp);
        //    //        }
        //    //        foreach (Constituent Cons in Constituents.Values)
        //    //        {
        //    //            new ConstituentView(Cons);
        //    //        }
        //    //        foreach (ECSID ECSID in DataCache.GetAllECSIDs())
        //    //        {
        //    //            new StructureView(ECSID);
        //    //        }
        //    //        foreach (Constraint Con in DataCache.Constraints.Values)
        //    //        {
        //    //            ConstraintView CV = new ConstraintView(Con);
        //    //            Con.RegisterAssessment(SA);
        //    //            //RaiseEventOnUIThread(ConstraintAdded, new object[] { null, CV.ID });
        //    //            ConstraintAdded?.Raise(null, CV.ID);
        //    //        }
        //    //        new AssessmentView(SA);
        //    //        //
        //    //        ICollection<DbAssessment> DbSA_Associated = DbSA.DbSession.AssociatedAssessments;
        //    //        if (DbSA_Associated.Count > 1 && AskToLoadAssociatedAssessments)
        //    //        {
        //    //            //Window.DialogResult DR = MessageBox.Show("Load associated DataCache.Assessments?", "Load", MessageBoxButtons.YesNo);
        //    //            MessageBoxResult D = MessageBox.Show("Load associated DataCache.Assessments?", "Load", MessageBoxButton.YesNo);
        //    //            if (D == MessageBoxResult.Yes)
        //    //            {
        //    //                foreach (DbAssessment DbSA2 in DbSA_Associated)
        //    //                {
        //    //                    if (DbSA != DbSA2)
        //    //                    {
        //    //                        new Assessment(DbSA2);
        //    //                        new AssessmentView(SA);
        //    //                    }
        //    //                }
        //    //            }
        //    //        }
        //    //        // Announcements
        //    //        (ProgressBar as IProgress<int>).Report(100);
        //    //        ProtocolOpened?.Raise(null, EventArgs.Empty); // now DataCache.Constraints are announced, announce protocol opening
        //    //        foreach (Assessment SA2 in DataCache.GetAllAssessments())
        //    //        {
        //    //            AssessmentLoaded?.Raise(null, SA2.ID);
        //    //        }
        //    //    }
        //    //    else
        //    //    {
        //    //        MessageBox.Show("Error: Assessment not found");
        //    //    }
        //    //}
        //}
        public static void CloseProtocol()
        {
            DataCache.CloseProtocol();
            CurrentStructureSet = null;
            ProtocolClosed?.Raise(null, EventArgs.Empty);
        }

        public static bool SetCurrentStructureSet(string ssuid, bool PerformAliasing)
        {
            CurrentStructureSet = DataCache.GetStructureSet(ssuid);
            if (PerformAliasing)
                foreach (ECSID E in DataCache.GetAllECSIDs()) // notify structures
                {
                    if (E.ES.Id == "")
                        E.ApplyAliasing(CurrentStructureSet);
                }
            CurrentStructureSetChanged?.Invoke(null, EventArgs.Empty);
            return true;
        }

        public static List<ComponentStatusCodes> AssociatePlanToComponent(int AssessmentID, int ComponentID, string CourseId, string PlanId, bool ClearWarnings) // ClearWarnings
        {
            AsyncPlan p = DataCache.GetAsyncPlans().FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseId);
            ECPlan ECP = DataCache.GetAllPlans().Where(x => x.AssessmentID == AssessmentID && x.ComponentID == ComponentID).SingleOrDefault();
            if (ECP == null)
            {
                // If it meets these criteria the component is evaluable
                ECP = new ECPlan(p, AssessmentID, ComponentID);
                DataCache.AddPlan(ECP);
                AvailableStructureSetsChanged?.Invoke(null, ECP);
                if (CurrentStructureSet == null)
                {
                    SetCurrentStructureSet(p.StructureSetUID, true);
                }
                DataCache.GetAssessment(AssessmentID).RegisterPlan(ECP); // this will fire PlanMappingChanged and update all DataCache.Constraints
            }
            else
            {
                ECP.Reassociate(p, ClearWarnings);
                AvailableStructureSetsChanged?.Invoke(null, ECP);
            }
            LinkedPlansChanged?.Invoke(null, ECP);
            return ECP.GetErrorCodes();
        }

        public static string GetStructureIDString(int ID)
        {
            return DataCache.GetECSID(ID).EclipseStructureName;
        }
        public static Assessment GetAssessment(int AssessmentID)
        {
            return DataCache.GetAssessment(AssessmentID);
        }
        public static List<SessionView> GetSessionViews()
        {
            List<SessionView> SVs = new List<SessionView>();
            foreach (Session S in DataCache.GetSessions())
            {
                SVs.Add(new SessionView(S));
            }
            return SVs;
        }
        public static Assessment NewAssessment()
        {
            Assessment NewAssessment = new Assessment(string.Format("Assessment#{0}", _AssessmentNameIterator), _AssessmentNameIterator);
            DataCache.AddAssessment(NewAssessment);
            foreach (Constraint Con in DataCache.GetAllConstraints())
            {
                Con.RegisterAssessment(NewAssessment);
            }
            _AssessmentNameIterator++;
            //Subscribe to updates
            //AssessmentView AV = new AssessmentView(NewAssessment);
            NewAssessmentAdded?.Invoke(null, NewAssessment.ID);
            return NewAssessment;
        }
        public static Assessment LoadAssessment(int AssessmentID)
        {
            Assessment A = DataCache.GetAssessment(AssessmentID);
            foreach (Constraint Con in DataCache.GetAllConstraints())
            {
                Con.RegisterAssessment(A);
            }
            //Subscribe to updates
            //AssessmentView AV = new AssessmentView(A);
            NewAssessmentAdded?.Invoke(null, A.ID);
            return A;
        }
        public static void RemoveAssessment(int AssessmentID)
        {
            foreach (ECPlan PL in DataCache.GetAllPlans().Where(x => x.AssessmentID == AssessmentID).ToList())
            {
                PL.Delete();
            }
            if (AssessmentRemoved != null)
                AssessmentRemoved?.Raise(null, AssessmentID);
        }

        //DataCache.Patient Management
        public static EventHandler PatientClosed;
        public static event EventHandler PatientOpened;
        public static void ClosePatient()
        {
            DataCache.ClosePatient();
            PatientLoaded = false;
            DataCache.ClearCourses();
            CurrentStructureSet = null;
            _AssessmentNameIterator = 1;
            PatientClosed?.Invoke(null, EventArgs.Empty); // this fires the event PatientClosed, which is subscribed to by the UI to clear any DataCache.Patient specific fields
        }
        public static void LoadPatientFromDatabase(string PID)
        {
            DataCache.OpenPatient(PID);
            if (DataCache.Patient == null)
            {
                PatientLoaded = false;
                return;
            }
            else
            {
                PatientLoaded = true;
                CourseNames = DataCache.Patient.CourseIds;
                PatientOpened?.Invoke(null, EventArgs.Empty);
            }
        }
        public async static Task<List<string>> GetPlanIdsByCourseName(string CourseName)
        {
            return await DataCache.GetPlanIdsByCourseName(CourseName);
        }

        public static List<string> GetCourseNames()
        {
            return DataCache.Patient.CourseIds;
        }
    }
}
