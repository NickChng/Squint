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
using SquintScript.ViewModels;
using SquintScript.Extensions;
using System.Windows.Threading;
using System.Runtime.Remoting.Contexts;
using VMSTemplates;
using System.Reflection;
//using System.Windows.Forms;

namespace SquintScript
{
    public static class VersionContextConnection
    {
        private static string providerName = "Npgsql";
        public static string databaseName = "";
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

    public static class SiteConfiguration
    {

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
        private static Dispatcher _uiDispatcher;

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
                        BI.HU = Math.Round(Bolus.MaterialCTValue);
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
            public double GantryEnd { get; set; } = 0;
            public double GantryStart { get; set; } = 0;
            public double CollimatorAngle { get; set; } = 0;
            public double CouchRotation { get; set; } = 0;

            public VVector Isocentre { get; set; }
            public double Xmax { get; set; } = 0;
            public double Ymax { get; set; } = 0;
            public double Ymin { get; set; } = 0;
            public double Xmin { get; set; } = 0;
            public double X1max { get; set; } = 0;
            public double Y1max { get; set; } = 0;
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

        //public class PlanView
        //{
        //    private ECPlan ECP;
        //    public PlanView(ECPlan _ECP)
        //    {
        //        ECP = _ECP;
        //    }
        //    public string PlanName
        //    {
        //        get { return ECP.PlanName; }
        //    }
        //    public string CourseName
        //    {
        //        get { return ECP.CourseName; }
        //    }
        //    public bool LoadWarning { get { return ECP.LoadWarning; } }
        //    public string LoadWarningString { get { return ECP.LoadWarningString; } }
        //    public ComponentTypes PlanType
        //    {
        //        get { return ECP.PlanType; }
        //    }
        //    public IEnumerable<string> StructureNames
        //    {
        //        get { return ECP.GetStructureNames; }
        //    }
        //    public List<ComponentStatusCodes> ErrorCodes { get { return ECP.GetErrorCodes(); } }
        //    public bool isStructureEmpty(string StructureID)
        //    {
        //        var isEmpty = ECP.isStructureEmpty(StructureID);
        //        if (isEmpty != null)
        //            return (bool)isEmpty;
        //        else
        //        {
        //            MessageBox.Show("Error in isStructureEmpty, structure not found");
        //            return false;
        //        }
        //    }
        //}
        //public class ConstraintThresholdView
        //{
        //    private ConstraintThreshold CT;
        //    public ConstraintThresholdView(ConstraintThreshold _CT)
        //    {
        //        CT = _CT;
        //    }
        //    public int ID
        //    {
        //        get { return CT.ID; }
        //    }
        //    public double ThresholdValue
        //    {
        //        get { return CT.ThresholdValue; }
        //        set { CT.ThresholdValue = value; }
        //    }
        //    public ReferenceThresholdTypes ThresholdName
        //    {
        //        get { return CT.ThresholdName; }
        //    }
        //}
        [AddINotifyPropertyChangedInterface]
        public class SessionView
        {
            //public event PropertyChangedEventHandler PropertyChanged;
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
            public ProtocolPreview() { }
            public ProtocolPreview(int _ID, string ProtocolName_in)
            {
                ID = _ID;
                ProtocolName = ProtocolName_in;
            }

            public int ID { get; private set; } = 0;
            public string ProtocolName { get; private set; } = "None selected";
            public ProtocolTypes ProtocolType { get; set; } = ProtocolTypes.Unset;
            public TreatmentCentres TreatmentCentre { get; set; } = TreatmentCentres.Unset;
            public TreatmentSites TreatmentSite { get; set; } = TreatmentSites.Unset;
            public ApprovalLevels Approval { get; set; } = ApprovalLevels.Unset;
            public string LastModifiedBy { get; set; } = "";
        }

        //}
        //public class StructureLabelView
        //{
        //    private StructureLabel SL;
        //    public StructureLabelView(StructureLabel SLin)
        //    {
        //        SL = SLin;
        //    }
        //    public int ID
        //    {
        //        get { return SL.ID; }
        //    }
        //    public string Designator
        //    {
        //        get { return SL.Designator; }
        //    }
        //    public string LabelName
        //    {
        //        get { return SL.LabelName; }
        //    }
        //    public double AlphaBetaRatio
        //    {
        //        get { return SL.AlphaBetaRatio; }
        //    }
        //}
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
            public ReferenceThresholdTypes ThresholdStatus { get; private set; }
        }
        // State Data
        public static string SquintUser { get; private set; }
        private static Protocol CurrentProtocol
        {
            get { if (ProtocolLoaded) return DataCache.CurrentProtocol; else return null; }
        }
        public static AsyncStructureSet CurrentStructureSet { get; private set; }
        public static AsyncESAPI A { get; private set; }
        //private static Dictionary<int, StructureLabel> StructureLabels = new Dictionary<int, StructureLabel>();
        //public static Dictionary<int, ConstraintView> ConstraintViews { get; private set; } = new Dictionary<int, ConstraintView>();
        public static Dictionary<int, Component> ComponentViews { get; private set; } = new Dictionary<int, Component>();
        public static Dictionary<int, AssessmentView> AssessmentViews { get; private set; } = new Dictionary<int, AssessmentView>();
        //public static Dictionary<int, ConstituentView> ConstituentViews { get; private set; } = new Dictionary<int, ConstituentView>();
        private static Dictionary<int, ProtocolStructure> Structures = new Dictionary<int, ProtocolStructure>();





        //private static List<string> CourseNames = new List<string>();

        public static bool SavingNewProtocol { get; } = false;
        private static int _IDbase = -2;
        private static int _NewStructureCounter = 1;
        private static int _NewPlanCounter = 1;
        private static int _AssessmentNameIterator = 1;
        public static SquintConfiguration Config;
        static public bool PatientLoaded { get; private set; } = false;
        public static bool ProtocolLoaded { get; private set; } = false;
        // Events

        public static event EventHandler CurrentStructureSetChanged;
        public static event EventHandler AvailableStructureSetsChanged;
        public static event EventHandler SynchronizationComplete;
        public static event EventHandler ProtocolListUpdated;
        public static event EventHandler ProtocolUpdated;
        public static event EventHandler ProtocolConstraintOrderChanged;
        public static event EventHandler ProtocolOpened;
        public static event EventHandler ProtocolClosed;
        public static event EventHandler StartingLongProcess;
        public static event EventHandler EndingLongProcess;
        //public static event EventHandler<PlanAssociation> LinkedPlansChanged;
        public static event EventHandler SessionsChanged;
        public static event EventHandler<int> ConstraintAdded;
        //public static event EventHandler<int> ConstraintRemoved;
        public static event EventHandler<int> NewAssessmentAdded;
        //public static event EventHandler<int> AssessmentRemoved;

        // Initialization
        public static bool Initialize(Dispatcher uiDispatcher)
        {
            try
            {
                //Get configuration
                XmlSerializer ConfigSer = new XmlSerializer(typeof(SquintConfiguration));
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var configfilename = @"ConfigCN.xml";
                using (StreamReader configfile = new StreamReader(Path.Combine(path, configfilename)))
                {
                    Config = (SquintConfiguration)ConfigSer.Deserialize(configfile);
                    VersionContextConnection.databaseName = Config.Database.DatabaseName;
                }
                _uiDispatcher = uiDispatcher;
                DataCache.TestDbConnection();
                A = new AsyncESAPI();
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
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
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
                AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
                if (p.ComponentType == ComponentTypes.Phase)
                {
                    var Objectives = await p.GetObjectiveItems();
                    return Objectives;
                }
                else return new List<Controls.ObjectiveDefinition>();
            }
        }
        public async static Task<double> GetSliceSpacing(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetSliceSpacing();
            else
                return double.NaN;
        }
        public async static Task<double?> GetDoseGridResolution(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetDoseGridResolution();
            else
                return null;
        }
        public async static Task<FieldNormalizationTypes> GetFieldNormalizationMode(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
            {
                string output = await p.GetFieldNormalizationMode();
                switch (output)
                {
                    case "100% to isocenter":
                        return FieldNormalizationTypes.ISO;
                    case "No field normalization":
                        return FieldNormalizationTypes.None;
                    case "100% to central axis Dmax":
                        return FieldNormalizationTypes.CAX;
                    case "100% to field Dmax":
                        return FieldNormalizationTypes.field;
                    default:
                        return FieldNormalizationTypes.Unset;
                }
            }
            else
                return FieldNormalizationTypes.Unset;
        }
        public async static Task<double> GetPNV(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetPNV();
            else return double.NaN;
        }
        public static async Task<double?> GetRxDose(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p != null)
                return p.Dose;
            else
            {
                return double.NaN;
            }
        }
        public static async Task<int?> GetNumFractions(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return p.NumFractions;
            else
                return null;
        }
        public async static Task<double> GetPrescribedPercentage(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetPrescribedPercentage();
            else
                return double.NaN;
        }
        public async static Task<string> GetCourseIntent(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            return await p.GetCourseIntent();
        }
        public async static Task<string> GetAlgorithmModel(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetAlgorithmModel();
            else
                return "";
        }
        public async static Task<double> GetCouchSurface(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetCouchSurface();
            else
                return double.NaN;
        }

        public static double GetAssignedHU(string ESId, string structureSetUID)
        {
            var AS = DataCache.GetStructureSet(structureSetUID).GetStructure(ESId);
            if (AS != null)
                return AS.HU;
            else
                return double.NaN;
        }
        public async static Task<double> GetCouchInterior(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetCouchInterior();
            else return double.NaN;
        }
        public async static Task<bool?> GetHeterogeneityOn(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Sum)
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
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetStudyId();
            else return "";
        }
        public async static Task<string> GetSeriesId(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetSeriesId();
            else return "";
        }
        public async static Task<string> GetSeriesComments(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetSeriesComments();
            else
                return "";
        }
        public async static Task<double> GetBolusThickness(string CourseId, string PlanId, string BolusId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
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
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetImageComments();
            else return "";
        }
        public async static Task<int?> GetNumSlices(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            if (p.ComponentType == ComponentTypes.Phase)
                return await p.GetNumSlices();
            else return null;
        }
        public async static Task<List<TxFieldItem>> GetTxFieldItems(string CourseId, string PlanId)
        {
            AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
            return await p.GetTxFields();
        }
        public async static Task<List<ImagingFieldItem>> GetImagingFieldList(string CourseId, string PlanId)
        {
            if (!PatientLoaded)
                return new List<ImagingFieldItem>();
            else
            {
                AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
                if (p.ComponentType != ComponentTypes.Phase)
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
                AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, ComponentTypes.Phase);
                if (p.ComponentType == ComponentTypes.Phase)
                    return await p.GetNTOObjective();
                else
                    return null;
            }
        }
        public static List<ProtocolPreview> GetProtocolPreviewList()
        {
            return DataCache.GetProtocolPreviews().Where(x => x.ID > 1).OrderBy(x => x.ProtocolName).ToList();
        }
        public static Protocol GetProtocol(int Id)
        {
            return DataCache.GetProtocol(Id);
        }
        public static Protocol GetActiveProtocol()
        {
            if (ProtocolLoaded)
                return CurrentProtocol;
            else return null;
        }
        public static PlanAssociation GetPlanAssociation(int planID)
        {
            return DataCache.GetPlanAssociation(planID);
        }
        public static PlanAssociation GetPlanAssociation(int ComponentID, int AssessmentID)
        {
            PlanAssociation ECP = DataCache.GetPlanAssociations().Where(x => x.ComponentID == ComponentID && x.AssessmentID == AssessmentID).SingleOrDefault();
            if (ECP != null)
                return ECP;
            else
                return null;
        }
        public static void ClearPlanAssociation(int componentId, int assessmentId)
        {
            var ECP = DataCache.GetPlanAssociations().Where(x => x.ComponentID == componentId && x.AssessmentID == assessmentId).SingleOrDefault();
            if (ECP != null)
                DataCache.ClearPlan(ECP);
        }
        public static Component GetComponent(int ComponentID)
        {

            return DataCache.GetComponent(ComponentID);
        }
        public static Constraint GetConstraint(int ConId)
        {
            return DataCache.GetConstraint(ConId);
        }
        public static ProtocolStructure GetStructure(int StructureID)
        {
            return DataCache.GetProtocolStructure(StructureID);
        }
        //public static StructureLabel GetStructureLabelView(int StructureLabelID)
        //{
        //    return DataCache.GetStructureLabel(StructureLabelID);
        //}
        public static List<ProtocolStructure> GetStructureList()
        {
            return DataCache.GetAllProtocolStructures().ToList();
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
        //public static List<ConstraintThresholdView> GetConstraintThresholdViewList(int ConID)
        //{
        //    List<ConstraintThresholdView> returnList = new List<ConstraintThresholdView>();
        //    foreach (ConstraintThreshold CT in DataCache.GetConstraintThresholdByConstraintId(ConID))
        //    {
        //        returnList.Add(new ConstraintThresholdView(CT));
        //    }
        //    return returnList;
        //}
        //public static List<ECPlan> GetPlanViewList()
        //{
        //    List<ECPlan> returnList = new List<ECPlan>();
        //    foreach (ECPlan ECP in DataCache.GetAllPlans())
        //    {
        //        returnList.Add(ECP);
        //    }
        //    return returnList;
        //}
        public static List<Assessment> GetAssessmentList()
        {
            return DataCache.GetAllAssessments().OrderBy(x => x.DisplayOrder).ToList();
        }
        //public static List<StructureLabel> GetStructureLabelList()
        //{
        //    List<StructureLabel> returnList = new List<StructureLabel>();
        //    foreach (StructureLabel SL in DataCache.GetAllStructureLabels())
        //    {
        //        returnList.Add(SL);
        //    }
        //    return returnList;
        //}
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
        //public static bool isStructureIDUnique(string sID)
        //{
        //    if (DataCache.GetAllProtocolStructures().Select(x => x.EclipseStructureName).Contains(sID))
        //        return false;
        //    else
        //        return true;
        //}
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
        public static List<string> GetCurrentStructures()
        {
            return CurrentStructureSet.GetAllStructures().Select(x => x.Id).ToList();
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
            DataCache.GetConstraint(Id).FlagForDeletion();
            //Re-index displayorder
            int NewDisplayOrder = 1;
            foreach (Constraint Con in DataCache.GetAllConstraints().OrderBy(x => x.DisplayOrder))
            {
                Con.DisplayOrder.Value = NewDisplayOrder++;
            }
        }
        public static void DeleteComponent(int Id)
        {
            if (DataCache.GetAllComponents().Count() > 1)
            {
                DataCache.GetComponent(Id).FlagForDeletion();
                //Re-index displayorder
                int NewDisplayOrder = 1;
                foreach (Component Comp in DataCache.GetAllComponents().OrderBy(x => x.DisplayOrder))
                {
                    Comp.DisplayOrder = NewDisplayOrder++;
                }
            }
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
        public static Component AddComponent()
        {
            if (!ProtocolLoaded)
                return null;
            string ComponentName = string.Format("NewPhase{0}", _NewPlanCounter++);
            var newComponent = new Component(CurrentProtocol.ID, ComponentName);
            DataCache.AddComponent(newComponent);
            return newComponent;
        }
        public static ProtocolStructure AddNewStructure()
        {
            if (DataCache.CurrentProtocol == null)
                return null;
            else
            {
                string NewStructureName = string.Format("UserStructure{0}", _NewStructureCounter++);
                ProtocolStructure newProtocolStructure = new ProtocolStructure(NewStructureName, 1);
                DataCache.AddProtocolStructure(newProtocolStructure);
                return newProtocolStructure;
            }
        }
        public static void AddConstraintThreshold(ReferenceThresholdTypes Name, int ConstraintID, double ThresholdValue)
        {
            // ConstraintThreshold CT = new ConstraintThreshold(Name, ConstraintID, ThresholdValue);
        }

        public static bool ImportEclipseProtocol(VMSTemplates.Protocol P)
        {
            // Reset adjusted protocol flag
            ClosePatient();
            CloseProtocol();
            //load the XML document
            try
            {
                DataCache.CreateNewProtocol();
                DataCache.CurrentProtocol.ProtocolName = P.Preview.ID;
                var EclipseStructureIdToSquintStructureIdMapping = new Dictionary<string, int>();
                foreach (VMSTemplates.ProtocolStructureTemplateStructure ECPStructure in P.StructureTemplate.Structures)
                {
                    var S = new ProtocolStructure(ECPStructure.ID);
                    DataCache.AddProtocolStructure(S);
                    EclipseStructureIdToSquintStructureIdMapping.Add(ECPStructure.ID.ToLower(), S.ID);
                }
                int displayOrder = 0;
                foreach (ProtocolPhase Ph in P.Phases)
                {
                    int numFractions = (int)Ph.FractionCount;
                    double totalDose = (double)Ph.PlanTemplate.DosePerFraction * numFractions * 100; // convert to cGy;
                    Component SC = new Component(DataCache.CurrentProtocol.ID, Ph.ID, numFractions, totalDose);
                    DataCache.AddComponent(SC);
                    if (Ph.Prescription.Item != null)
                    {
                        foreach (VMSTemplates.ProtocolPhasePrescriptionItem Item in Ph.Prescription.Item)
                        {
                            int structureId = 1;
                            if (Item.Primary)
                                SC.ReferenceDose = (double)Item.TotalDose*100;
                            if (EclipseStructureIdToSquintStructureIdMapping.ContainsKey(Item.ID.ToLower()))
                                structureId = EclipseStructureIdToSquintStructureIdMapping[Item.ID.ToLower()];
                            Constraint Con = new Constraint(Item, SC.ID, structureId, displayOrder++);
                            DataCache.AddConstraint(Con);
                        }
                    }
                    if (Ph.Prescription.MeasureItem != null)
                    {
                        foreach (VMSTemplates.ProtocolPhasePrescriptionMeasureItem MI in Ph.Prescription.MeasureItem)
                        {
                            int structureId = 1;
                            if (EclipseStructureIdToSquintStructureIdMapping.ContainsKey(MI.ID.ToLower()))
                                structureId = EclipseStructureIdToSquintStructureIdMapping[MI.ID.ToLower()];
                            Constraint Con = new Constraint(MI, SC.ID, structureId, displayOrder++);
                            DataCache.AddConstraint(Con);
                        }
                    }
                }
                ProtocolLoaded = true;
                _uiDispatcher.Invoke(ProtocolOpened, new object[] { null, EventArgs.Empty });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Unspecified error importing Eclipse Protocol.");
                return false;
            }
        }
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
        //                ProtocolStructure ProtocolStructure = DataCache.GetProtocolStructure(Con.PrimaryStructureID);
        //                if (!ECProtocol.StructureTemplate.Structures.Structure.Select(x => x.ID).Contains(ProtocolStructure.EclipseStructureName))
        //                {
        //                    ECProtocol.StructureTemplate.Structures.Structure.Add(new SquintEclipseProtocol.StructureClass()
        //                    {
        //                        ID = ProtocolStructure.EclipseStructureName,
        //                        TypeIndex = 2,
        //                        ColorAndStyle = new SquintEclipseProtocol.EmptyClass(),
        //                        Identification = new SquintEclipseProtocol.Identification()
        //                    });
        //                }
        //                if (Con.ConstraintType == ConstraintTypeCodes.M)
        //                {
        //                    SquintEclipseProtocol.Item I = new SquintEclipseProtocol.Item()
        //                    {
        //                        ID = ProtocolStructure.EclipseStructureName,
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
        //                        ID = ProtocolStructure.EclipseStructureName,
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
        public static bool ImportProtocolFromXML(string filename, bool refresh)
        {
            SquintProtocolXML _XMLProtocol;
            if (filename == "") // no file selected
                return false;
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
            if (ReferenceEquals(null, _XMLProtocol.Constraints))
            {
                _XMLProtocol.Constraints = new SquintProtocolXML.ConstraintListDefinition();
                _XMLProtocol.Constraints.ConstraintList = new List<SquintProtocolXML.ConstraintDefinition>();
            }
            //if (ReferenceEquals(null, _XMLProtocol.ConformityIndexConstraints))
            //{
            //    _XMLProtocol.ConformityIndexConstraints = new SquintProtocolXML.ConformityIndexConstraintListDefinition();
            //    _XMLProtocol.ConformityIndexConstraints.ConformityIndexConstraintList = new List<SquintProtocolXML.ConformityIndexConstraintDefinition>();
            //}
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
            int ConstraintIndexCounter = 0;
            foreach (SquintProtocolXML.ConstraintDefinition con in _XMLProtocol.Constraints.ConstraintList)
            {
                if (con.ComponentName == null)
                {
                    MessageBox.Show(string.Format("DVH Constraint #{0} is missing a component reference", ConstraintIndexCounter));
                    return false;
                }
                if (!ComponentID2PK.ContainsKey(con.ComponentName))
                {
                    MessageBox.Show(string.Format("DVH Constraint #{0} does not reference a valid component", ConstraintIndexCounter));
                    return false;
                }
                con.ComponentID = ComponentID2PK[con.ComponentName.Trim()];
                ConstraintIndexCounter++;
            }
            bool ProtocolSavedCorrectly = SaveXMLProtocolToDatabase(_XMLProtocol);
            if (!ProtocolSavedCorrectly)
            {
                return false;
            }
            else
            {
                if (refresh)
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
                        MessageBox.Show(string.Format("A protocol with name {0} exists, creating a copy...", _XMLProtocol.ProtocolMetaData.ProtocolName));
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
                DbProtocol P = LocalContext.DbLibraryProtocols.Create();
                if (string.IsNullOrEmpty(_XMLProtocol.ProtocolMetaData.ProtocolName))
                {
                    MessageBox.Show(string.Format("Error: Protocol must have a ProtocolName"));
                    return false;
                }
                else
                {
                    P.ProtocolName = _XMLProtocol.ProtocolMetaData.ProtocolName;
                }
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
                    if (Enum.TryParse(_XMLProtocol.ProtocolMetaData.Intent, true, out Intent))
                    {
                        P.TreatmentIntent = (int)Intent;
                    }
                    else
                        P.TreatmentIntent = (int)TreatmentIntents.Unset; // unset
                }
                if (_XMLProtocol.ProtocolMetaData.ApprovalStatus != null)
                {
                    ApprovalLevels ApprovalLevel;
                    if (Enum.TryParse(_XMLProtocol.ProtocolMetaData.ApprovalStatus, true, out ApprovalLevel))
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
                    if (Enum.TryParse(_XMLProtocol.ProtocolMetaData.DiseaseSite, true, out DiseaseSite))
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
                    if (Enum.TryParse(_XMLProtocol.ProtocolMetaData.TreatmentCentre, true, out TreatmentCentre))
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
                    if (Enum.TryParse(_XMLProtocol.ProtocolMetaData.ProtocolType, true, out ProtocolType))
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
                int DisplayOrder = 1;
                Dictionary<string, int> ProtocolStructureNameToID = new Dictionary<string, int>();
                foreach (SquintProtocolXML.StructureDefinition S in _XMLProtocol.Structures.Structure)
                {
                    DbProtocolStructure ProtocolStructure = LocalContext.DbProtocolStructures.Create();
                    if (S.ProtocolStructureName == null)
                    {
                        MessageBox.Show(string.Format("Error: Structure in protocol {0} is missing the field ProtocolStructureName", _XMLProtocol.ProtocolMetaData.ProtocolName));
                        return false;
                    }
                    else
                    {
                        if (ProtocolStructureNameToID.ContainsKey(S.ProtocolStructureName))
                        {
                            MessageBox.Show(string.Format("Error: Multiple structures with same name ({1}) in protocol {0}, please remove duplication", _XMLProtocol.ProtocolMetaData.ProtocolName, S.ProtocolStructureName));
                            return false;
                        }
                        else
                            ProtocolStructure.ProtocolStructureName = S.ProtocolStructureName;
                    }
                    if (S.EclipseAliases != null)
                    {
                        int displayOrder = 1;
                        foreach (SquintProtocolXML.EclipseAlias EA in S.EclipseAliases.EclipseId)
                        {
                            DbStructureAlias DbSA = LocalContext.DbStructureAliases.Create();
                            DbSA.DbProtocolStructure = ProtocolStructure;
                            DbSA.EclipseStructureId = EA.Id;
                            DbSA.DisplayOrder = displayOrder++;
                            LocalContext.DbStructureAliases.Add(DbSA);
                        }
                    }
                    if (S.StructureChecklist != null)
                    {
                        DbStructureChecklist DbSC = LocalContext.DbStructureChecklists.Create();
                        var SC = S.StructureChecklist;
                        if (SC.PointContourCheck != null)
                        {
                            DbSC.isPointContourChecked = true;
                            DbSC.PointContourThreshold = string.IsNullOrEmpty(SC.PointContourCheck.Threshold) ? double.NaN : double.Parse(SC.PointContourCheck.Threshold);
                            LocalContext.DbStructureChecklists.Add(DbSC);
                        }
                        ProtocolStructure.DbStructureChecklist = DbSC;
                    }
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
                    ProtocolStructure.StructureLabelID = StructureLabelID;
                    ProtocolStructure.ProtocolID = P.ID;
                    ProtocolStructure.DisplayOrder = DisplayOrder++;
                    ProtocolStructure.ID = IDGenerator();
                    LocalContext.DbProtocolStructures.Add(ProtocolStructure);
                    ProtocolStructureNameToID.Add(ProtocolStructure.ProtocolStructureName, ProtocolStructure.ID);
                }
                int ComponentDisplayOrder = 1;
                foreach (SquintProtocolXML.ComponentDefinition comp in _XMLProtocol.Components.Component)
                {
                    DbComponent C = LocalContext.DbComponents.Create();
                    C.DisplayOrder = ComponentDisplayOrder++;
                    C.DbLibraryProtocol = P;
                    if (string.IsNullOrEmpty(comp.ComponentName))
                    {
                        MessageBox.Show(string.Format("Components must have a non-empty ComponentName attribute (Protocol: {0})", P.ProtocolName));
                        return false;
                    }
                    else
                        C.ComponentName = comp.ComponentName;
                    if (comp.Prescription != null)
                    {
                        int NumFractions;
                        if (!int.TryParse(comp.Prescription.NumFractions, out NumFractions))
                        {
                            MessageBox.Show(string.Format("Components must have an integer value in the NumFractions attribute (Protocol: {0} Component: {1})", P.ProtocolName, comp.ComponentName));
                            return false;
                        }
                        else
                            C.NumFractions = NumFractions;
                        double RefDose;
                        if (!double.TryParse(comp.Prescription.ReferenceDose, out RefDose))
                        {
                            MessageBox.Show(string.Format("Components must have a value in the ReferenceDose attribute (Protocol: {0} Component: {1})", P.ProtocolName, comp.ComponentName));
                            return false;
                        }
                        else
                            C.ReferenceDose = RefDose;
                        double PNVMax;
                        if (double.TryParse(comp.Prescription.PNVMax, out PNVMax))
                        {
                            C.PNVMax = PNVMax;
                        }
                        double PNVMin;
                        if (double.TryParse(comp.Prescription.PNVMin, out PNVMin))
                        {
                            C.PNVMin = PNVMin;
                        }
                        double RxPercentage;
                        if (double.TryParse(comp.Prescription.PrescribedPercentage, out RxPercentage))
                        {
                            C.PrescribedPercentage = RxPercentage;
                        }
                    }

                    ComponentTypes ComponentType;
                    if (!Enum.TryParse(comp.Type, true, out ComponentType))
                    {
                        MessageBox.Show(string.Format("Components must must specify either Plan or Sum in the Type attribute (Protocol: {0} Component: {1})", P.ProtocolName, comp.ComponentName));
                        return false;
                    }
                    else
                        C.ComponentType = (int)ComponentType;
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
                        C.MinBeams = string.IsNullOrEmpty(comp.Beams.MinBeams) ? null : (int?)int.Parse(comp.Beams.MinBeams);
                        C.MaxBeams = string.IsNullOrEmpty(comp.Beams.MaxBeams) ? null : (int?)int.Parse(comp.Beams.MaxBeams);
                        C.NumIso = string.IsNullOrEmpty(comp.Beams.NumIso) ? null : (int?)int.Parse(comp.Beams.NumIso);
                        C.MinColOffset = string.IsNullOrEmpty(comp.Beams.MinColOffset) ? null : (int?)int.Parse(comp.Beams.MinColOffset);
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
                                if (Enum.TryParse(ArcG.Trajectory, true, out T))
                                    DbAG.Trajectory = (int)T;
                                else
                                    DbAG.Trajectory = (int)Trajectories.Unset;
                                DbAG.StartAngle = ArcG.StartAngle;
                                DbAG.EndAngle = ArcG.EndAngle;
                                DbAG.StartAngleTolerance = ArcG.StartAngleTolerance;
                                DbAG.EndAngleTolerance = ArcG.EndAngleTolerance;
                                LocalContext.DbBeamGeometries.Add(DbAG);
                            }
                            B.DbComponent = C;
                            B.ProtocolBeamName = b.ProtocolBeamName;
                            B.CouchRotation = string.IsNullOrEmpty(b.CouchRotation) ? null : (double?)double.Parse(b.CouchRotation);
                            B.MaxColRotation = string.IsNullOrEmpty(b.MaxColRotation) ? null : (double?)double.Parse(b.MaxColRotation);
                            B.MinColRotation = string.IsNullOrEmpty(b.MinColRotation) ? null : (double?)double.Parse(b.MinColRotation);
                            B.MaxMUWarning = string.IsNullOrEmpty(b.MaxMUWarning) ? null : (double?)double.Parse(b.MaxMUWarning);
                            B.MinMUWarning = string.IsNullOrEmpty(b.MinMUWarning) ? null : (double?)double.Parse(b.MinMUWarning);
                            B.MinX = string.IsNullOrEmpty(b.MinX) ? null : (double?)double.Parse(b.MinX);
                            B.MaxX = string.IsNullOrEmpty(b.MaxX) ? null : (double?)double.Parse(b.MaxX);
                            B.MinY = string.IsNullOrEmpty(b.MinY) ? null : (double?)double.Parse(b.MinY);
                            B.MaxY = string.IsNullOrEmpty(b.MaxY) ? null : (double?)double.Parse(b.MaxY);
                            B.ToleranceTable = b.ToleranceTable;
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

                            FieldType Technique;
                            if (Enum.TryParse(b.Technique, true, out Technique))
                            {
                                B.Technique = (int)Technique;
                            }
                            else
                                B.Technique = (int)FieldType.Unset;
                            ParameterOptions JawTracking_Indication;
                            if (Enum.TryParse(b.JawTracking_Indication, true, out JawTracking_Indication))
                            {
                                B.JawTracking_Indication = (int)JawTracking_Indication;
                            }
                            else
                                B.JawTracking_Indication = (int)ParameterOptions.Unset;

                            if (b.BolusDefinitions != null)
                            {
                                foreach (var bolus in b.BolusDefinitions)
                                {
                                    if (bolus.HU == null)
                                        continue;
                                    DbBolus DbBolus = LocalContext.DbBoluses.Create();
                                    DbBolus.DbBeam = B;
                                    DbBolus.HU = double.Parse(bolus.HU);
                                    if (Enum.TryParse(bolus.Indication, true, out ParameterOptions BolusIndication))
                                        DbBolus.Indication = (int)BolusIndication;
                                    else
                                        DbBolus.Indication = (int)ParameterOptions.Unset;
                                    DbBolus.Thickness = string.IsNullOrEmpty(bolus.Thickness) ? double.NaN : double.Parse(bolus.Thickness);
                                    DbBolus.ToleranceThickness = string.IsNullOrEmpty(bolus.ToleranceThickness) ? double.NaN : double.Parse(bolus.ToleranceThickness);
                                    DbBolus.ToleranceHU = string.IsNullOrEmpty(bolus.ToleranceHU) ? double.NaN : double.Parse(bolus.ToleranceHU);
                                    LocalContext.DbBoluses.Add(DbBolus);
                                }
                            }
                        }
                    }
                    // Add checklist 
                    if (_XMLProtocol.ProtocolChecklist != null) // import default component checklist
                    {
                        DbProtocolChecklist Checklist = LocalContext.DbProtocolChecklists.Create();
                        LocalContext.DbProtocolChecklists.Add(Checklist);
                        Checklist.ID = IDGenerator();
                        Checklist.DbProtocol = P;
                        if (_XMLProtocol.ProtocolChecklist.Calculation != null)
                        {
                            AlgorithmTypes AlgorithmType;
                            if (Enum.TryParse(_XMLProtocol.ProtocolChecklist.Calculation.Algorithm, true, out AlgorithmType))
                                Checklist.Algorithm = (int)AlgorithmType;
                            else
                                Checklist.Algorithm = (int)AlgorithmTypes.Unset;
                            FieldNormalizationTypes FNM;
                            if (Enum.TryParse(_XMLProtocol.ProtocolChecklist.Calculation.FieldNormalizationMode, true, out FNM))
                                Checklist.FieldNormalizationMode = (int)FNM;
                            else
                                Checklist.FieldNormalizationMode = (int)FieldNormalizationTypes.Unset;
                            Checklist.HeterogeneityOn = string.IsNullOrEmpty(_XMLProtocol.ProtocolChecklist.Calculation.HeterogeneityOn) ? null : (bool?)bool.Parse(_XMLProtocol.ProtocolChecklist.Calculation.HeterogeneityOn);
                            Checklist.AlgorithmResolution = string.IsNullOrEmpty(_XMLProtocol.ProtocolChecklist.Calculation.AlgorithmResolution) ? null : (double?)double.Parse(_XMLProtocol.ProtocolChecklist.Calculation.AlgorithmResolution);
                        }
                        if (_XMLProtocol.ProtocolChecklist.Supports != null)
                        {
                            ParameterOptions SupportIndication;
                            if (Enum.TryParse(_XMLProtocol.ProtocolChecklist.Supports.Indication, true, out SupportIndication))
                            {
                                Checklist.SupportIndication = (int)SupportIndication;
                            }
                            else
                                Checklist.SupportIndication = (int)ParameterOptions.Unset;

                            Checklist.CouchInterior = string.IsNullOrEmpty(_XMLProtocol.ProtocolChecklist.Supports.CouchInterior) ? null : (double?)double.Parse(_XMLProtocol.ProtocolChecklist.Supports.CouchInterior);
                            Checklist.CouchSurface = string.IsNullOrEmpty(_XMLProtocol.ProtocolChecklist.Supports.CouchSurface) ? null : (double?)double.Parse(_XMLProtocol.ProtocolChecklist.Supports.CouchSurface);
                        }
                        if (_XMLProtocol.ProtocolChecklist.Simulation != null)
                        {
                            Checklist.SliceSpacing = string.IsNullOrEmpty(_XMLProtocol.ProtocolChecklist.Simulation.SliceSpacing) ? null : (double?)double.Parse(_XMLProtocol.ProtocolChecklist.Simulation.SliceSpacing);
                        }

                        if (_XMLProtocol.ProtocolChecklist.Artifacts != null)
                        {
                            foreach (SquintProtocolXML.ArtifactDefinition A in _XMLProtocol.ProtocolChecklist.Artifacts.Artifact)
                            {
                                DbArtifact DbA = LocalContext.DbArtifacts.Create();
                                LocalContext.DbArtifacts.Add(DbA);
                                DbA.HU = string.IsNullOrEmpty(A.HU) ? null : (double?)double.Parse(A.HU);
                                DbA.ToleranceHU = string.IsNullOrEmpty(A.ToleranceHU) ? null : (double?)double.Parse(A.ToleranceHU);
                                DbA.DbProtocolChecklist = Checklist;
                                DbA.ProtocolStructure_ID = ProtocolStructureNameToID[A.ProtocolStructureName];
                            }
                        }
                    }
                }
                int ConDisplayOrder = 1;
                foreach (SquintProtocolXML.ConstraintDefinition con in _XMLProtocol.Constraints.ConstraintList)
                {
                    // Input error checking
                    DbConstraint DbCon = LocalContext.DbConstraints.Create();
                    ConstraintTypeCodes ConstraintTypeCode;
                    if (!Enum.TryParse(con.ConstraintType, out ConstraintTypeCode))
                    {
                        MessageBox.Show(string.Format("Constraint number {0} in protocol {1} has an unrecognized ConstraintType", ConDisplayOrder, P.ProtocolName));
                        return false;
                    }
                    DbCon.ConstraintType = (int)ConstraintTypeCode;
                    UnitScale ConstraintUnitScale;
                    if (!Enum.TryParse(con.ConstraintUnit, true, out ConstraintUnitScale))
                    {
                        MessageBox.Show(string.Format("Constraint number {0} in protocol {1} has an unrecognized ConstraintUnit", ConDisplayOrder, P.ProtocolName));
                        return false;
                    }
                    DbCon.ConstraintScale = (int)ConstraintUnitScale;
                    UnitScale ReferenceUnitScale;
                    if (!Enum.TryParse(con.ReferenceUnit, true, out ReferenceUnitScale))
                    {
                        MessageBox.Show(string.Format("Constraint number {0} in protocol {1} has an unrecognized ReferenceUnit", ConDisplayOrder, P.ProtocolName));
                        return false;
                    }
                    DbCon.ReferenceScale = (int)ReferenceUnitScale;
                    ReferenceTypes ReferenceType;
                    if (!Enum.TryParse(con.ReferenceType, true, out ReferenceType))
                    {
                        MessageBox.Show(string.Format("Constraint number {0} in protocol {1} has an unrecognized ReferenceType", ConDisplayOrder, P.ProtocolName));
                        return false;
                    }
                    DbCon.ReferenceType = (int)ReferenceType;
                    double ConstraintValue;
                    if (!double.TryParse(con.ConstraintValue, out ConstraintValue))
                    {
                        MessageBox.Show(string.Format("Constraint number {0} in protocol {1} has an unrecognized ConstraintValue", ConDisplayOrder, P.ProtocolName));
                        return false;
                    }
                    DbCon.ConstraintValue = ConstraintValue;
                    if (!CompName2DB.ContainsKey(con.ComponentName))
                    {
                        MessageBox.Show(string.Format("Constraint number {0} in protocol {1} has an unrecognized ComponentName", ConDisplayOrder, P.ProtocolName));
                        return false;
                    }
                    DbCon.DbComponent = CompName2DB[con.ComponentName];
                    DbCon.Fractions = CompName2DB[con.ComponentName].NumFractions;
                    var DbProtocolStructure = LocalContext.DbProtocolStructures.Local.FirstOrDefault(x => x.ProtocolID == P.ID && x.ProtocolStructureName == con.ProtocolStructureName);
                    if (DbProtocolStructure != null)
                        DbCon.DbProtocolStructure_Primary = DbProtocolStructure;
                    else
                    {
                        MessageBox.Show(string.Format("Constraint references protocol structure {1} in protocol {0}, but this structure is not defined", P.ProtocolName, con.ProtocolStructureName));
                        return false;
                    }
                    if (!string.IsNullOrEmpty(con.ReferenceStructureName))
                    {
                        if (ProtocolStructureNameToID.ContainsKey(con.ReferenceStructureName))
                            DbCon.ReferenceStructureId = ProtocolStructureNameToID[con.ReferenceStructureName]; // default unset
                        else
                            DbCon.ReferenceStructureId = 1;
                    }
                    else
                    {
                        if (ConstraintTypeCode == ConstraintTypeCodes.CI)
                        {
                            MessageBox.Show(string.Format("Conformity constraint number {0} in protocol {1} does not have a ReferenceStructure", ConDisplayOrder, P.ProtocolName));
                            return false;
                        }
                        else
                            DbCon.ReferenceStructureId = 1;
                    }
                    DbCon.DisplayOrder = ConDisplayOrder++;
                    LocalContext.DbConstraints.Add(DbCon);

                    // Thresholds
                    if (!string.IsNullOrEmpty(con.DataTablePath))
                        DbCon.ThresholdDataPath = con.DataTablePath;
                    else
                    {
                        double majorViolation;
                        if (double.TryParse(con.MajorViolation, out majorViolation))
                            DbCon.MajorViolation = majorViolation;
                        else
                        {
                            MessageBox.Show(string.Format("Constraint number {0} in protocol {1} must have either a MajorViolation or DataTablePath with a MajorViolation defined", ConDisplayOrder, P.ProtocolName));
                            return false;
                        }
                        DbCon.MinorViolation = string.IsNullOrEmpty(con.MinorViolation) ? null : (double?)double.Parse(con.MinorViolation);
                        DbCon.Stop = string.IsNullOrEmpty(con.Stop) ? null : (double?)double.Parse(con.Stop);
                    }



                    DbConstraintChangelog DbCC = LocalContext.DbConstraintChangelogs.Create();

                    LocalContext.DbConstraintChangelogs.Add(DbCC);
                    DbCC.ChangeDescription = con.Description;
                    DbCC.ConstraintString = con.GetConstraintString();
                    DbCC.ChangeAuthor = Ctr.SquintUser;
                    DbCC.Date = DateTime.Now.ToBinary();
                    DbCC.DbConstraint = DbCon;
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

        public static bool ExportProtocolAsXML(string filename)
        {
            SquintProtocolXML XMLProtocol = new SquintProtocolXML();
            //Assign TEMPORARY primary keys to each constraint, since these are not defined in the XML protocol
            XMLProtocol.ProtocolMetaData = new SquintProtocolXML.ProtocolMetaDataDefinition()
            {
                ApprovalStatus = CurrentProtocol.ApprovalLevel.ToString(),
                DiseaseSite = CurrentProtocol.TreatmentSite.ToString(),
                Author = CurrentProtocol.Author,
                Intent = CurrentProtocol.TreatmentIntent.ToString(),
                ProtocolDate = DateTime.Now.ToShortDateString(),
                ProtocolName = CurrentProtocol.ProtocolName,
                ProtocolType = CurrentProtocol.ProtocolType.ToString(),
                TreatmentCentre = CurrentProtocol.TreatmentCentre.ToString()
            };
            XMLProtocol.Structures = new SquintProtocolXML.StructuresDefinition();
            foreach (var S in DataCache.GetAllProtocolStructures().OrderBy(x => x.DisplayOrder))
            {
                var XMLStructure = new SquintProtocolXML.StructureDefinition
                {
                    Label = S.StructureLabel.LabelName,
                    ProtocolStructureName = S.ProtocolStructureName,
                };
                foreach (string alias in S.DefaultEclipseAliases)
                {
                    XMLStructure.EclipseAliases.EclipseId.Add(new SquintProtocolXML.EclipseAlias()
                    {
                        Id = alias
                    });
                }
                if (S.CheckList.isPointContourChecked.isDefined)
                {
                    if ((bool)S.CheckList.isPointContourChecked.Value)
                    {
                        XMLStructure.StructureChecklist = new SquintProtocolXML.StructureChecklistDefinition()
                        {
                            PointContourCheck = new SquintProtocolXML.PointContourCheckDefinition()
                            {
                                Threshold = S.CheckList.PointContourVolumeThreshold.Value.ToString()
                            }
                        };
                    }
                }
                XMLProtocol.Structures.Structure.Add(XMLStructure);
            }
            foreach (var A in CurrentProtocol.Checklist.Artifacts)
            {
                XMLProtocol.ProtocolChecklist.Artifacts.Artifact.Add(new SquintProtocolXML.ArtifactDefinition
                {
                    HU = A.RefHU.Value.ToString(),
                    ProtocolStructureName = A.E.ProtocolStructureName,
                    ToleranceHU = A.ToleranceHU.Value.ToString()
                });
            }
            XMLProtocol.ProtocolChecklist.Calculation.Algorithm = CurrentProtocol.Checklist.Algorithm.Value.ToString();
            XMLProtocol.ProtocolChecklist.Calculation.AlgorithmResolution = CurrentProtocol.Checklist.AlgorithmResolution.Value.ToString();
            XMLProtocol.ProtocolChecklist.Calculation.FieldNormalizationMode = CurrentProtocol.Checklist.FieldNormalizationMode.Value.ToString();
            XMLProtocol.ProtocolChecklist.Calculation.HeterogeneityOn = CurrentProtocol.Checklist.HeterogeneityOn.Value.ToString();
            
            XMLProtocol.ProtocolChecklist.Simulation.SliceSpacing = CurrentProtocol.Checklist.SliceSpacing.Value.ToString();
            XMLProtocol.ProtocolChecklist.Supports.CouchInterior = CurrentProtocol.Checklist.CouchInterior.Value.ToString();
            XMLProtocol.ProtocolChecklist.Supports.CouchSurface = CurrentProtocol.Checklist.CouchSurface.Value.ToString();
            XMLProtocol.ProtocolChecklist.Supports.Indication = CurrentProtocol.Checklist.SupportIndication.Value.ToString();


            foreach (var C in DataCache.GetAllComponents())
            {
                SquintProtocolXML.ComponentDefinition cd = new SquintProtocolXML.ComponentDefinition()
                {
                    ComponentName = C.ComponentName,
                    Type = C.ComponentType.Value.ToString(),
                };
                cd.Prescription.PNVMax = C.PNVMax.Value.ToString();
                cd.Prescription.PNVMin = C.PNVMin.Value.ToString();
                cd.Prescription.PrescribedPercentage = C.PrescribedPercentage.Value.ToString();
                cd.Prescription.NumFractions = C.NumFractions.ToString();
                cd.Prescription.ReferenceDose = C.ReferenceDose.ToString();
                cd.Beams.MaxBeams = C.MaxBeams.Value.ToString();
                cd.Beams.MinBeams = C.MinBeams.Value.ToString();
                cd.Beams.NumIso = C.NumIso.Value.ToString();
                cd.Beams.MinColOffset = C.MinColOffset.Value.ToString();
                foreach (var B in DataCache.GetBeams(C.ID))
                {
                    var bd = new SquintProtocolXML.BeamDefinition()
                    {
                        CouchRotation = B.CouchRotation.Value.ToString(),
                        JawTracking_Indication = B.JawTracking_Indication.Value.ToString(),
                        MaxColRotation = B.MaxColRotation.Value.ToString(),
                        MaxX = B.MaxX.Value.ToString(),
                        MaxY = B.MaxY.Value.ToString(),
                        MinX = B.MinX.Value.ToString(),
                        MinY = B.MinY.Value.ToString(),
                        MinMUWarning = B.MinMUWarning.Value.ToString(),
                        MaxMUWarning = B.MaxMUWarning.Value.ToString(),
                        ProtocolBeamName = B.ProtocolBeamName,
                        Technique = B.Technique.ToString(),
                        ToleranceTable = B.ToleranceTable.Value.ToString(),
                        MinColRotation = B.MinColRotation.Value.ToString(),
                    };
                    cd.Beams.Beam.Add(bd);
                    bd.EclipseAliases = new SquintProtocolXML.EclipseAliases();
                    foreach (string alias in B.EclipseAliases)
                    {
                        bd.EclipseAliases.EclipseId.Add(new SquintProtocolXML.EclipseAlias() { Id = alias });
                    }
                    bd.Energies.Energy = new List<SquintProtocolXML.EnergyDefinition>();
                    foreach (Energies E in B.ValidEnergies)
                    {
                        bd.Energies.Energy.Add(new SquintProtocolXML.EnergyDefinition() { Mode = E.Display() });
                    }
                    var bolusDefinitionList = new List<SquintProtocolXML.BolusDefinition>();
                    foreach (var Bol in B.Boluses)
                    {
                        bolusDefinitionList.Add(new SquintProtocolXML.BolusDefinition()
                        {
                            HU = Bol.HU.Value.ToString(),
                            Indication = Bol.Indication.ToString(),
                            Thickness = Bol.Thickness.Value.ToString(),
                            ToleranceHU = Bol.ToleranceHU.Value.ToString(),
                            ToleranceThickness = Bol.ToleranceThickness.Value.ToString()
                        });
                    }
                    bd.ValidGeometries = new SquintProtocolXML.ValidGeometriesDefintiion();
                    foreach (var vg in B.ValidGeometries)
                    {
                        bd.ValidGeometries.Geometry.Add(new SquintProtocolXML.GeometryDefinition()
                        {
                            StartAngle = vg.StartAngle,
                            EndAngle = vg.EndAngle,
                            StartAngleTolerance = vg.StartAngleTolerance,
                            EndAngleTolerance = vg.EndAngleTolerance,
                            GeometryName = vg.GeometryName,
                            Trajectory = vg.Trajectory.ToString()
                        });
                    }
                }
                XMLProtocol.Components.Component.Add(cd);
            }

            foreach (var Con in DataCache.GetAllConstraints().OrderBy(x => x.DisplayOrder))
            {
                switch (Con.ConstraintType)
                {
                    case ConstraintTypeCodes.Unset:
                        continue;
                    default:
                        SquintProtocolXML.ConstraintDefinition XMLCon = new SquintProtocolXML.ConstraintDefinition()
                        {
                            ComponentName = Con.ComponentName,
                            ReferenceType = Con.ReferenceType.ToString(),
                            ReferenceUnit = Con.ReferenceScale.ToString(),
                            ConstraintType = Con.ConstraintType.ToString(),
                            ConstraintUnit = Con.ConstraintScale.ToString(),
                            ConstraintValue = Con.ConstraintValue.ToString(),
                            MajorViolation = Con.MajorViolation.ToString(),
                            MinorViolation = Con.MinorViolation.ToString(),
                            DataTablePath = Con.ThresholdDataPath,
                            Stop = Con.Stop.ToString(),
                            ProtocolStructureName = Con.ProtocolStructureName
                        };
                        XMLProtocol.Constraints.ConstraintList.Add(XMLCon);

                        break;
                }
            }

            Serializer ser = new Serializer();
            try
            {
                ser.Serialize<SquintProtocolXML>(XMLProtocol, filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(filename);
                MessageBox.Show(ex.InnerException.Message, "Error writing file");
                return false;
            }

            return true;
        }
        public static void LoadProtocolFromDb(string ProtocolName, IProgress<int> progress = null) // this method reloads the context.
        {
            try
            {
                CloseProtocol();
                DataCache.LoadProtocol(ProtocolName);
                ProtocolLoaded = true;
                _uiDispatcher.Invoke(ProtocolOpened, new object[] { null, EventArgs.Empty });
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\r\n {1}\r\n {2}", ex.Message, ex.InnerException, ex.StackTrace));
                return;
            }
        }


        public static async Task Save_UpdateProtocol()
        {
            await DataCache.Save_UpdateProtocol();
            _uiDispatcher.Invoke(ProtocolUpdated, new[] { null, EventArgs.Empty });
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
            if (ProtocolLoaded)
            {
                if (CurrentProtocol.ID == Id)
                    DataCache.CloseProtocol();
            }
            if (Deleted)
            {
                ProtocolListUpdated?.Invoke(null, EventArgs.Empty);
            }
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
                    foreach (Constraint Con in DataCache.GetAllConstraints())
                    {
                        foreach (Assessment A in DataCache.GetAllAssessments())
                        {
                            Con.RegisterAssessment(A);
                        }
                    }
                    foreach (PlanAssociation P in DataCache.GetPlanAssociations())
                    {
                        if (P.Linked)
                        {
                            var C = await DataCache.GetCourseAsync(P.CourseId);
                            if (P.Linked)
                            {
                                var AP = await C.GetPlan(P.PlanId);
                                if (AP != null)
                                    P.UpdateLinkedPlan(AP, false);
                            }
                        }
                    }
                    ProtocolLoaded = true;
                    _uiDispatcher.Invoke(ProtocolOpened, new object[] { null, EventArgs.Empty });
                }
                return true;
            }
            else return false;
        }
        public static async void SynchronizePlans()
        {
            await Task.Run(() => SynchronizePlansInternal());
            SynchronizationComplete?.Invoke(null, EventArgs.Empty);
        }
        private static async Task SynchronizePlansInternal()
        {
            if (DataCache.Patient != null)
            {
                string PID = DataCache.Patient.Id;
                DataCache.RefreshPatient();
                // Disable autorefresh (see below for reasons)
                try
                {
                    foreach (PlanAssociation ECP in DataCache.GetPlanAssociations())
                    {
                        AsyncPlan p = null;
                        if (!ECP.Linked)
                        {
                            continue; // Usually an assessment loaded without a valid plan
                        }
                        AsyncCourse C = await DataCache.GetCourse(ECP.CourseId);
                        if (C != null)
                        {
                            p = await DataCache.GetAsyncPlan(ECP.PlanUID, ECP.CourseId); // note the ECP.PlanUID & StructureSetUID are not updated till the updatelinkedplan method is called below
                            if (p == null)
                            {
                                if (ECP.PlanType == ComponentTypes.Phase)
                                    MessageBox.Show(string.Format("Plan {0} in Course {1} is no longer found", ECP.PlanId, ECP.CourseId));

                            }
                            else
                            {
                                if (p.HistoryDateTime != ECP.LastModified)
                                {
                                    MessageBox.Show(string.Format("Note: Dose distribution in plan {0} has changed since last evaluation", ECP.PlanId));
                                }
                                if (p.StructureSetUID != ECP.StructureSetUID)
                                {
                                    MessageBox.Show(string.Format("Note: Structure set in plan {0} has changed since last evaluation", ECP.PlanId));
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show(string.Format("Course {0} no longer exists", ECP.CourseId));
                        }
                        ECP.UpdateLinkedPlan(p, false);
                    }
                    //foreach (ProtocolStructure E in DataCache.GetAllProtocolStructures()) //This is a kludge while structures are derived from plans not structure sets.
                    //{
                    //    if (E.ES != null)
                    //        if (CurrentStructureSet.UID == E.ES.StructureSetUID)
                    //        {
                    //            var UpdatedStructure = CurrentStructureSet.GetStructure(E.ES.Id);
                    //            if (UpdatedStructure != null)
                    //                E.ES.Update(CurrentStructureSet.GetStructure(E.ES.Id)); // Have to disable autocalculation or this will provoke an update on constraints linked to plans that haven't been relinked yet
                    //            else
                    //                E.ES = new EclipseStructure(null);
                    //        }
                    //        else
                    //            E.ES = new EclipseStructure(null);
                    //}
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                }
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
        //    //        CurrentProtocol = new ProtocolView(DataCache.CurrentProtocol);
        //    //        foreach (Component Comp in DataCache.GetAllComponents())
        //    //        {
        //    //            new Component(Comp);
        //    //        }
        //    //        foreach (Constituent Cons in Constituents.Values)
        //    //        {
        //    //            new ConstituentView(Cons);
        //    //        }
        //    //        foreach (ProtocolStructure ProtocolStructure in DataCache.GetAllProtocolStructures())
        //    //        {
        //    //            new StructureView(ProtocolStructure);
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

        public async static void UpdateAllConstraints()
        {
            foreach (Constraint Con in DataCache.GetAllConstraints())
            {
                await Task.Run(() => Con.EvaluateConstraint());
            }
        }

        public async static void UpdateComponentConstraints(int CompId, int? AssessmentId = null)
        {
            foreach (Constraint Con in DataCache.GetConstraintsInComponent(CompId))
            {
                if (AssessmentId != null)
                    await Task.Run(() => Con.EvaluateConstraint((int)AssessmentId));
                else
                    await Task.Run(() => Con.EvaluateConstraint());
            }
        }

        public async static void UpdateConstraintsLinkedToStructure(int ProtocolStructureId)
        {
            foreach (Constraint Con in DataCache.GetAllConstraints().Where(x => x.PrimaryStructureID == ProtocolStructureId || x.ReferenceStructureId == ProtocolStructureId))
            {
                await Task.Run(() => Con.EvaluateConstraint());
            }
        }

        public static void UpdateConstraintThresholds(ProtocolStructure E) // update interpolated thresholds based on change to ProtocolStructureId
        {
            foreach (Constraint Con in DataCache.GetAllConstraints().Where(x => x.ReferenceStructureId == E.ID))
            {
                string SSUID = Ctr.CurrentStructureSet.UID;
                if (!string.IsNullOrEmpty(SSUID))
                    Con.UpdateThresholds(E.Volume(SSUID));
            }
        }
        public static bool SetCurrentStructureSet(string ssuid, bool PerformAliasing)
        {
            CurrentStructureSet = DataCache.GetStructureSet(ssuid);
            var test = DataCache.GetAllProtocolStructures().Count();
            if (PerformAliasing)
                foreach (ProtocolStructure E in DataCache.GetAllProtocolStructures()) // notify structures
                {
                    E.ApplyAliasing(CurrentStructureSet);
                    UpdateConstraintThresholds(E);
                }
            CurrentStructureSetChanged?.Invoke(null, EventArgs.Empty);
            return true;
        }

        public static StructureLabel GetStructureLabel(int? Id = null)
        {
            if (Id == null)
                return DataCache.GetStructureLabel(1);
            else
                return DataCache.GetStructureLabel((int)Id);
        }
        public static IEnumerable<StructureLabel> GetStructureLabels()
        {
            return DataCache.GetAllStructureLabels();
        }

        public async static Task<List<ComponentStatusCodes>> AssociatePlanToComponent(int AssessmentID, int ComponentID, string CourseId, string PlanId, ComponentTypes Type, bool ClearWarnings) // ClearWarnings
        {
            try
            {
                Ctr.StartingLongProcess?.Invoke(null, EventArgs.Empty);
                AsyncPlan p = await DataCache.GetAsyncPlan(PlanId, CourseId, Type);
                return await Task.Run(() =>
               {
                   PlanAssociation ECP = DataCache.GetPlanAssociations().Where(x => x.AssessmentID == AssessmentID && x.ComponentID == ComponentID).SingleOrDefault();
                   if (ECP == null)
                   {
                       ECP = new PlanAssociation(p, AssessmentID, ComponentID);
                       DataCache.AddPlan(ECP);
                       //DataCache.GetAssessment(AssessmentID).RegisterPlan(ECP); // this will fire PlanMappingChanged and update all DataCache.Constraints
                   }
                   else
                   {
                       ECP.UpdateLinkedPlan(p, ClearWarnings);
                   }
                   AvailableStructureSetsChanged?.Invoke(null, EventArgs.Empty);
                   UpdateStructureSetOptions(p.StructureSetUID);
                   Ctr.EndingLongProcess?.Invoke(null, EventArgs.Empty);
                   return ECP.GetErrorCodes();
               });

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2})", ex.Message, ex.InnerException, ex.StackTrace));
                return null;
            }
        }

        public static void UpdateStructureSetOptions(string SSUID)
        {
            var SSheaders = GetAvailableStructureSets();
            if (CurrentStructureSet == null)
            {
                SetCurrentStructureSet(SSUID, true);
            }
            else if (!SSheaders.Select(x => x.StructureSetUID).Contains(CurrentStructureSet.UID))
            {
                SetCurrentStructureSet(SSUID, true);
            }
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
            foreach (PlanAssociation PL in DataCache.GetPlanAssociations().Where(x => x.AssessmentID == AssessmentID).ToList())
            {
                DataCache.ClearPlan(PL);
            }
        }

        //DataCache.Patient Management
        public static event EventHandler PatientOpened;
        public static void ClosePatient()
        {
            DataCache.ClosePatient();
            PatientLoaded = false;
            DataCache.ClearEclipseData();
            CurrentStructureSet = null;
            _AssessmentNameIterator = 1;
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
                _uiDispatcher.Invoke(PatientOpened, new[] { null, EventArgs.Empty });
            }
        }
        public class PlanDescriptor
        {
            public string PlanId { get; private set; }
            public string PlanUID { get; private set; }

            public string StructureSetUID { get; private set; }
            public ComponentTypes Type { get; set; }
            public PlanDescriptor(ComponentTypes type, string planId, string planUID, string structureSetUID)
            {
                Type = type;
                PlanId = planId;
                PlanUID = planUID;
                StructureSetUID = structureSetUID;
            }
        }
        public async static Task<List<PlanDescriptor>> GetPlanDescriptors(string CourseName)
        {
            return await DataCache.GetPlanDescriptors(CourseName);
        }
        public static List<string> GetCourseNames()
        {
            return DataCache.Patient.CourseIds;
        }

        public static void AddNewContourCheck(ProtocolStructure S)
        {
            S.CheckList.isPointContourChecked.Value = true;
            S.CheckList.PointContourVolumeThreshold.Value = 0.05;
        }
        public static void RemoveNewContourCheck(ProtocolStructure S)
        {
            S.CheckList.isPointContourChecked.Value = false;
            S.CheckList.PointContourVolumeThreshold.Value = 0;
        }
        public static void AddNewBeamCheck(int ComponentId)
        {
            DataCache.AddBeam(new Beam(ComponentId));
        }

        public static void AddStructureAlias(int StructureId, string NewAlias)
        {
            DataCache.GetProtocolStructure(StructureId).DefaultEclipseAliases.Add(NewAlias);
        }
        public static void RemoveStructureAlias(int StructureId, string AliasToRemove)
        {
            DataCache.GetProtocolStructure(StructureId).DefaultEclipseAliases.Remove(AliasToRemove);
        }
    }
}
