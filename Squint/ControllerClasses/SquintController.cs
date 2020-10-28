using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using System.Xml;
using System.Xml.Serialization;
using VMSTypes = VMS.TPS.Common.Model.Types;
using ESAPI = VMS.TPS.Common.Model.API.Application;
using System.Data.Entity;
using SquintScript.ViewModels;
using SquintScript.Extensions;
using System.Windows.Threading;
using System.Runtime.Remoting.Contexts;
using VMSTemplates;
using System.Reflection;
using AutoMapper;
//using System.Windows.Forms;

namespace SquintScript
{
    public static partial class Ctr
    {
        private static Dispatcher _uiDispatcher;
        public static AsyncESAPI ESAPIContext { get; private set; }
        // State Data
        public static string SquintUser { get; private set; }
        public static Protocol CurrentProtocol { get { return CurrentSession.SessionProtocol; } }
        public static Session CurrentSession { get; private set; } = new Session();

        private static AsyncStructureSet _CurrentStructureSet;
        public static AsyncStructureSet CurrentStructureSet
        {
            get { return _CurrentStructureSet; }
            private set { _CurrentStructureSet = value; CurrentStructureSetChanged?.Invoke(null, EventArgs.Empty); }
        }

        private static List<BeamGeometryDefinition> _BeamGeometryDefinitions;
        public static Dictionary<int, Component> ComponentViews { get; private set; } = new Dictionary<int, Component>();
        public static Dictionary<int, AssessmentView> AssessmentViews { get; private set; } = new Dictionary<int, AssessmentView>();
        public static bool SavingNewProtocol { get; } = false;
        public static bool PatientOpen { get; private set; } = false;
        private static int _NewStructureCounter = 1;
        private static int _NewPlanCounter = 1;
        private static int _AssessmentNameIterator = 1;
        public static string DefaultNewStructureName = "UserStructure";
        public static SquintConfiguration Config;
        //static public bool PatientOpen { get; private set; } = false;
        public static bool ProtocolLoaded { get; private set; } = false;
        // Events

        //public static event EventHandler CurrentStructureSetChanged;
        public static event EventHandler Initialized;
        public static event EventHandler DatabaseInitializing;
        public static event EventHandler DatabaseCreating;
        public static event EventHandler ESAPIInitializing;
        public static event EventHandler AvailableStructureSetsChanged;
        public static event EventHandler SynchronizationComplete;
        public static event EventHandler ProtocolListUpdated;
        public static event EventHandler ProtocolUpdated;
        public static event EventHandler CurrentStructureSetChanged;
        public static event EventHandler ProtocolConstraintOrderChanged;
        public static event EventHandler ProtocolOpened;
        public static event EventHandler ProtocolClosed;
        public static event EventHandler PatientOpened;
        public static event EventHandler PatientClosed;
        public static event EventHandler StartingLongProcess;
        public static event EventHandler EndingLongProcess;
        public static event EventHandler SessionsChanged;
        public static event EventHandler<int> ConstraintAdded;

        // Initialization
        public static bool Initialize(Dispatcher uiDispatcher)
        {
            try
            {
                // Initialize automapper
                
                //Get configuration
                XmlSerializer ConfigSer = new XmlSerializer(typeof(SquintConfiguration));
                var AssemblyName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var configfilename = AssemblyName + @"_Config.xml";
                using (StreamReader configfile = new StreamReader(Path.Combine(path, configfilename)))
                {
                    try
                    {
                        Config = (SquintConfiguration)ConfigSer.Deserialize(configfile);
                    }
                    catch
                    {
                        throw new Exception("Unable to configuration file.  Please review the configuration file, which should be named (Squint exe file name)_Config.xml");
                    }
                }
                _uiDispatcher = uiDispatcher;
                InitializeESAPI();
                InitializeDatabase();
                InitializeDefinitions();
                CurrentSession = new Session();
                _uiDispatcher.Invoke(Initialized, new object[] { null, EventArgs.Empty });
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }
        private static void InitializeDefinitions()
        {
            // Automapper.Initialize();  // unable to get this to work globally due to limitations with ConvertUsing in the version of Automapper that supports .net 4.5
            _BeamGeometryDefinitions = DbController.GetBeamGeometryDefinitions();
        }
        private static void InitializeESAPI()
        {
            _uiDispatcher.Invoke(ESAPIInitializing, new object[] { null, EventArgs.Empty });
            ESAPIContext = new AsyncESAPI();
            SquintUser = ESAPIContext.CurrentUserId(); // Get user from ESAPI;
            
        }
        private static void InitializeDatabase()
        {
            _uiDispatcher.Invoke(DatabaseInitializing, new object[] { null, EventArgs.Empty });
            var database = Config.Databases.FirstOrDefault(x => x.Site == Config.Site.CurrentSite);
            DbController.SetDatabaseName(database.DatabaseName);
            var DBStatus = DbController.TestDbConnection();
            if (DBStatus == DatabaseStatus.NonExistent)
            {
                _uiDispatcher.Invoke(DatabaseCreating, new object[] { null, EventArgs.Empty });
                DbController.InitializeDatabase();
            }
            DbController.RegisterUser(SquintUser, ESAPIContext.CurrentUserId(), ESAPIContext.CurrentUserName());
        }

        public static string PatientFirstName
        {
            get
            {
                if (ReferenceEquals(null, ESAPIContext.Patient))
                    return string.Empty;
                else
                    return ESAPIContext.Patient.FirstName;
            }
        }
        public static string PatientLastName
        {
            get
            {
                if (ReferenceEquals(null, ESAPIContext.Patient))
                    return string.Empty;
                else
                    return ESAPIContext.Patient.LastName;
            }
        }
        public static string PatientID
        {
            get
            {
                if (ReferenceEquals(null, Ctr.ESAPIContext.Patient))
                    return string.Empty;
                else
                    return Ctr.ESAPIContext.Patient.Id;
            }
        }
        public async static Task<AsyncPlan> GetAsyncPlan(string CourseId, string PlanId)
        {
            AsyncCourse C = await ESAPIContext.Patient.GetCourse(CourseId);
            if (C == null)
                return null;
            else
            {
                AsyncPlan P = await C.GetPlan(PlanId);
                return P;
            }
        }
        public async static Task<AsyncPlan> GetAsyncPlanByUID(string CourseId, string PlanUID)
        {
            AsyncCourse C = await ESAPIContext.Patient.GetCourse(CourseId);
            if (C == null)
                return null;
            else
            {
                AsyncPlan P = await C.GetPlanByUID(PlanUID);
                return P;
            }
        }
        public static List<StructureSetHeader> GetAvailableStructureSets()
        {
            var L = new List<StructureSetHeader>();
            foreach (var p in CurrentSession.PlanAssociations)
            {
                if (p.Linked)
                    L.Add(new StructureSetHeader(p.StructureSetId, p.StructureSetUID, p.PlanId));
            }
            return L;
        }
        public static AsyncStructureSet GetStructureSet(string ssuid)
        {
            var ass = ESAPIContext.Patient.GetStructureSet(ssuid);
            if (ass != null)
            {
                return ass;
            }
            else return null;
        }

        public async static Task<List<ObjectiveDefinition>> GetOptimizationObjectiveList(string CourseId, string PlanId)
        {
            if (!PatientOpen)
                return null;
            else
            {
                AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
                if (P.ComponentType == ComponentTypes.Phase)
                {
                    var Objectives = await P.GetObjectiveItems();
                    return Objectives;
                }
                else return new List<ObjectiveDefinition>();
            }
        }
        public async static Task<double> GetSliceSpacing(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetSliceSpacing();
            else
                return double.NaN;
        }
        public async static Task<double?> GetDoseGridResolution(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetDoseGridResolution();
            else
                return null;
        }
        public async static Task<FieldNormalizationTypes> GetFieldNormalizationMode(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
            {
                string output = await P.GetFieldNormalizationMode();
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
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetPNV();
            else return double.NaN;
        }
        public static async Task<double?> GetRxDose(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P != null)
                return P.Dose;
            else
            {
                return double.NaN;
            }
        }
        public static async Task<int?> GetNumFractions(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return P.NumFractions;
            else
                return null;
        }
        public async static Task<double> GetPrescribedPercentage(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetPrescribedPercentage();
            else
                return double.NaN;
        }
        public async static Task<string> GetCourseIntent(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            return await P.GetCourseIntent();
        }
        public async static Task<string> GetAlgorithmModel(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetAlgorithmModel();
            else
                return "";
        }
        public async static Task<string> GetVMATAlgorithmModel(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetVMATAlgorithmModel();
            else
                return "";
        }
        public async static Task<string> GetAirCavityCorrectionVMAT(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetVMATAirCavityOption();
            else
                return null;
        }
        public async static Task<string> GetIMRTAlgorithmModel(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetIMRTAlgorithmModel();
            else
                return "";
        }
        public async static Task<string> GetAirCavityCorrectionIMRT(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetIMRTAirCavityOption();
            else
                return null;
        }
        public async static Task<double> GetCouchSurface(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetCouchSurface();
            else
                return double.NaN;
        }

        public static double GetAssignedHU(string ESId, string structureSetUID)
        {
            var AS = GetStructureSet(structureSetUID).GetStructure(ESId);
            if (AS != null)
                return AS.HU;
            else
                return double.NaN;
        }
        public async static Task<double> GetCouchInterior(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetCouchInterior();
            else return double.NaN;
        }
        public async static Task<bool?> GetHeterogeneityOn(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Sum)
                return null;
            string HeteroStatus = await P.GetHeterogeneityOn();
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
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetStudyId();
            else return "";
        }
        public async static Task<string> GetSeriesId(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetSeriesId();
            else return "";
        }
        public async static Task<string> GetSeriesComments(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetSeriesComments();
            else
                return "";
        }
        public async static Task<double> GetBolusThickness(string CourseId, string PlanId, string BolusId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            var Bolus = P.Structures.Values.FirstOrDefault(x => x.DicomType == @"BOLUS" && x.Id == BolusId);
            if (Bolus != null)
            {
                var thick = await P.GetBolusThickness(Bolus.Id);
                return thick;
            }
            return double.NaN;
        }

        public async static Task<string> GetImageComments(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetImageComments();
            else return "";
        }
        public async static Task<int?> GetNumSlices(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetNumSlices();
            else return null;
        }
        public async static Task<List<TxFieldItem>> GetTxFieldItems(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            return await P.GetTxFields();
        }
        public async static Task<List<ImagingFieldItem>> GetImagingFieldList(string CourseId, string PlanId)
        {
            if (!PatientOpen)
                return new List<ImagingFieldItem>();
            else
            {
                AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
                if (P.ComponentType != ComponentTypes.Phase)
                    return new List<ImagingFieldItem>();
                var Isocentre = await P.GetPlanIsocentre();
                var PatientCentre = await P.GetPatientCentre();
                var ImagingFields = await P.GetImagingFields();
                var TxFields = await P.GetTxFields();
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
            foreach (ImagingProtocols IP in CV.ImagingProtocols)
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
            if (CV.ImagingProtocols.Contains(ImagingProtocols.PostCBCT) && CV.ImagingProtocols.Contains(ImagingProtocols.PreCBCT))
            {
                if (IF.Where(x => x.Type == FieldType.CBCT).Count() < 2)
                {
                    Errors[ImagingProtocols.PostCBCT].Add("Insufficient CBCT fields for both pre and post imaging");
                    Errors[ImagingProtocols.PreCBCT].Add("Insufficient CBCT fields for both pre and post imaging");
                }
            }
            return Errors;
        }
        public async static Task<NTODefinition> GetNTOObjective(string CourseId, string PlanId)
        {
            if (!PatientOpen)
                return null;
            else
            {
                AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
                if (P.ComponentType == ComponentTypes.Phase)
                    return await P.GetNTOObjective();
                else
                    return null;
            }
        }
        public static List<ProtocolPreview> GetProtocolPreviewList()
        {
            return DbController.GetProtocolPreviews().Where(x => x.ID > 1).OrderBy(x => x.ProtocolName).ToList();
        }
        public static Protocol GetProtocol(int Id)
        {
            return DbController.GetProtocol(Id);
        }

        public static PlanAssociation GetPlanAssociation(int ComponentID, int AssessmentID)
        {
            PlanAssociation ECP = CurrentSession.PlanAssociations.FirstOrDefault(x => x.ComponentID == ComponentID && x.AssessmentID == AssessmentID);
            if (ECP != null)
                return ECP;
            else
                return null;
        }

        public static List<PlanAssociation> GetPlanAssociations()
        {
            return CurrentSession.PlanAssociations;
        }
        public static void ClearPlanAssociation(int componentId, int assessmentId)
        {
            foreach (var PA in CurrentSession.PlanAssociations)
            {
                PA.UpdateLinkedPlan(null, true);
            }
        }
        public static List<Assessment> GetAssessmentList()
        {
            return CurrentSession.Assessments.OrderBy(x => x.DisplayOrder).ToList();
        }
        public static List<AssessmentPreviewViewModel> GetAssessmentPreviews(string PID)
        {
            //return DataCache.GetAssessmentPreviews(PID);
            return null;

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

        public static int NumAssessments
        {
            get { return CurrentSession.Assessments.Count(); }
        }

        public static void SetCurrentStructureSet(string ssuid)
        {
            if (ssuid == null)
                CurrentStructureSet = null;
            else
                CurrentStructureSet = GetStructureSet(ssuid);
            CurrentStructureSetChanged?.Invoke(null, EventArgs.Empty);
        }
        public static List<string> GetCurrentStructures()
        {
            if (CurrentStructureSet != null)
                return CurrentStructureSet.GetAllStructures().Select(x => x.Id).ToList();
            else return new List<string>();
        }
        public static List<string> GetAvailableStructureSetIds()
        {
            var SSs = GetAvailableStructureSets();
            return SSs.Select(x => x.StructureSetId).ToList();
        }


        public static Constraint AddConstraint(ConstraintTypeCodes TypeCode, int ComponentID = 0, int StructureId = 1)
        {
            if (!ProtocolLoaded)
                return null;
            Component parentComponent = CurrentProtocol.Components.FirstOrDefault(x => x.ID == ComponentID);
            ProtocolStructure primaryStructure = CurrentProtocol.Structures.FirstOrDefault(x => x.ID == StructureId);
            Constraint Con = new Constraint(parentComponent, primaryStructure, null, TypeCode);
            CurrentProtocol.Components.FirstOrDefault(x => x.ID == ComponentID).Constraints.Add(Con);
            return Con;
        }
        public static void DeleteConstraint(int Id)
        {
            var AllConstraints = GetAllConstraints();
            var Con = AllConstraints.FirstOrDefault(x => x.ID == Id);
            Con.FlagForDeletion();
            int NewDisplayOrder = 1;
            foreach (Constraint C in CurrentProtocol.Components.FirstOrDefault(x => x.ID == Con.ComponentID).Constraints.Where(x => !x.ToRetire))
            {
                Con.DisplayOrder.Value = NewDisplayOrder++;
            }
        }
        public static List<Constraint> GetAllConstraints()
        {
            return CurrentProtocol.Components.SelectMany(x => x.Constraints).ToList();
        }
        public static Constraint GetConstraint(int Id)
        {
            return GetAllConstraints().FirstOrDefault(x => x.ID == Id);
        }
        public static void DeleteComponent(int Id)
        {
            if (CurrentProtocol.Components.Count() > 1)
            {
                GetComponent(Id).FlagForDeletion();
                //Re-index displayorder
                int NewDisplayOrder = 1;
                foreach (Component Comp in CurrentProtocol.Components.OrderBy(x => x.DisplayOrder))
                {
                    Comp.DisplayOrder = NewDisplayOrder++;
                }
            }
        }
        public static void DeleteStructure(int Id)
        {
            if (CurrentProtocol.Structures.Count() > 1)
            {
                GetProtocolStructure(Id).FlagForDeletion();
                int NewDisplayOrder = 1;
                foreach (ProtocolStructure PS in CurrentProtocol.Structures.OrderBy(x => x.DisplayOrder))
                {
                    PS.DisplayOrder = NewDisplayOrder++;
                }
            }

        }
        public static void ShiftConstraintUp(int Id)
        {
            Constraint Con = GetConstraint(Id);
            if (Con != null)
            {
                Constraint ConSwitch = GetAllConstraints().FirstOrDefault(x => x.DisplayOrder.Value == Con.DisplayOrder.Value - 1);
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
            Constraint Con = GetConstraint(Id);
            if (Con != null)
            {
                Constraint ConSwitch = GetAllConstraints().FirstOrDefault(x => x.DisplayOrder.Value == Con.DisplayOrder.Value + 1);
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
            if (CurrentProtocol == null)
                return;
            Constraint Con2Dup = GetConstraint(ConstraintID);
            foreach (Constraint Con in GetAllConstraints().Where(x => x.DisplayOrder.Value > Con2Dup.DisplayOrder.Value))
            {
                Con.DisplayOrder.Value = Con.DisplayOrder.Value + 1;
            }
            Constraint DupCon = new Constraint(Con2Dup);
            CurrentProtocol.Components.FirstOrDefault(x => x.ID == Con2Dup.ComponentID).Constraints.Add(DupCon);
            ConstraintAdded?.Invoke(null, DupCon.ID);
        }

        public static Component GetComponent(int ID)
        {
            return CurrentProtocol.Components.FirstOrDefault(x => x.ID == ID);
        }
        public static Component AddComponent()
        {
            if (!ProtocolLoaded)
                return null;
            string ComponentName = string.Format("NewPhase{0}", _NewPlanCounter++);
            int DisplayOrder = CurrentProtocol.Components.Count() + 1;
            var newComponent = new Component(CurrentProtocol.ID, ComponentName, DisplayOrder);
            CurrentProtocol.Components.Add(newComponent);
            foreach (Assessment SA in CurrentSession.Assessments)
                CurrentSession.PlanAssociations.Add(new PlanAssociation(newComponent, SA));
            return newComponent;
        }
        public async static Task<ProtocolStructure> AddNewStructure()
        {
            if (CurrentProtocol == null)
                return null;
            else
            {
                string NewStructureName = string.Format("{0}{1}", DefaultNewStructureName, _NewStructureCounter++);
                while (CurrentProtocol.Structures.Select(x=>x.ProtocolStructureName).Any(x=> x.Equals(NewStructureName)))
                {
                    NewStructureName = string.Format("{0}{1}", DefaultNewStructureName, _NewStructureCounter++);
                }
                StructureLabel SL = await DbController.GetStructureLabel(1);
                ProtocolStructure newProtocolStructure = new ProtocolStructure(SL, NewStructureName);
                newProtocolStructure.DisplayOrder = CurrentProtocol.Structures.Count() + 1;
                newProtocolStructure.ProtocolID = CurrentProtocol.ID;
                CurrentProtocol.Structures.Add(newProtocolStructure);
                return newProtocolStructure;
            }
        }
        public static void AddConstraintThreshold(ReferenceThresholdTypes Name, int ConstraintID, double ThresholdValue)
        {
            // ConstraintThreshold CT = new ConstraintThreshold(Name, ConstraintID, ThresholdValue);
        }

        public static async Task<bool> ImportEclipseProtocol(VMSTemplates.Protocol P)
        {
            // Reset adjusted protocol flag
            ClosePatient();
            CloseProtocol();
            StartNewSession();
            //load the XML document
            try
            {
                Protocol EclipseProtocol = new Protocol();
                EclipseProtocol.ProtocolName = P.Preview.ID;
                var EclipseStructureIdToSquintStructureIdMapping = new Dictionary<string, int>();
                foreach (VMSTemplates.ProtocolStructureTemplateStructure ECPStructure in P.StructureTemplate.Structures)
                {
                    StructureLabel SL = await DbController.GetStructureLabel(1);
                    var S = new ProtocolStructure(SL, ECPStructure.ID);
                    S.DefaultEclipseAliases.Add(ECPStructure.ID);
                    EclipseProtocol.Structures.Add(S);
                    EclipseStructureIdToSquintStructureIdMapping.Add(ECPStructure.ID.ToLower(), S.ID);
                }
                int ComponentDisplayOrder = 1;
                foreach (ProtocolPhase Ph in P.Phases)
                {

                    int numFractions = (int)Ph.FractionCount;
                    double totalDose = (double)Ph.PlanTemplate.DosePerFraction * numFractions * 100; // convert to cGy;
                    Component SC = new Component(EclipseProtocol.ID, Ph.ID, ComponentDisplayOrder++, numFractions, totalDose);
                    EclipseProtocol.Components.Add(SC);
                    if (Ph.Prescription.Item != null)
                    {
                        foreach (ProtocolPhasePrescriptionItem Item in Ph.Prescription.Item)
                        {
                            int structureId = 1;
                            if (Item.Primary)
                                SC.TotalDose = (double)Item.TotalDose * 100;
                            if (EclipseStructureIdToSquintStructureIdMapping.ContainsKey(Item.ID.ToLower()))
                                structureId = EclipseStructureIdToSquintStructureIdMapping[Item.ID.ToLower()];
                            var primaryStructure = EclipseProtocol.Structures.FirstOrDefault(x => x.ID == structureId);
                            Constraint Con = new Constraint(SC, primaryStructure, Item);
                            SC.Constraints.Add(Con);
                        }
                    }
                    if (Ph.Prescription.MeasureItem != null)
                    {
                        foreach (ProtocolPhasePrescriptionMeasureItem MI in Ph.Prescription.MeasureItem)
                        {
                            int structureId = 1;
                            if (EclipseStructureIdToSquintStructureIdMapping.ContainsKey(MI.ID.ToLower()))
                                structureId = EclipseStructureIdToSquintStructureIdMapping[MI.ID.ToLower()];
                            var primaryStructure = EclipseProtocol.Structures.FirstOrDefault(x => x.ID == structureId);
                            Constraint Con = new Constraint(SC, primaryStructure, MI);
                            SC.Constraints.Add(Con);
                        }
                    }
                }
                CurrentSession.SessionProtocol = EclipseProtocol;
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
            if (ESAPIContext != null)
                ESAPIContext.Dispose();
        }
        public static bool SaveXMLProtocolToDatabase(SquintProtocolXML _XMLProtocol)
        {
            List<string> ExistingProtocolNames;
            using (SquintDBModel LocalContext = new SquintDBModel())
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
                DbLibraryProtocol P = LocalContext.DbLibraryProtocols.Create();
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
                P.ID = IDGenerator.GetUniqueId();
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
                    if (S.Label == null)
                    {
                        MessageBox.Show(string.Format("Error: Structure {1} in protocol {0} is missing a label", _XMLProtocol.ProtocolMetaData.ProtocolName, S.ProtocolStructureName));
                        return false;
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
                    ProtocolStructure.ID = IDGenerator.GetUniqueId();
                    LocalContext.DbProtocolStructures.Add(ProtocolStructure);
                    ProtocolStructureNameToID.Add(ProtocolStructure.ProtocolStructureName, ProtocolStructure.ID);
                }

                // Add checklist 
                if (_XMLProtocol.ProtocolChecklist != null) // import default component checklist
                {
                    DbProtocolChecklist Checklist = LocalContext.DbProtocolChecklists.Create();
                    LocalContext.DbProtocolChecklists.Add(Checklist);
                    Checklist.ID = IDGenerator.GetUniqueId();
                    Checklist.DbProtocol = P;
                    if (_XMLProtocol.ProtocolChecklist.Calculation != null)
                    {
                        AlgorithmVolumeDoseTypes AlgorithmType;
                        if (Enum.TryParse(_XMLProtocol.ProtocolChecklist.Calculation.Algorithm, true, out AlgorithmType))
                            Checklist.AlgorithmVolumeDose = (int)AlgorithmType;
                        else
                            Checklist.AlgorithmVolumeDose = (int)AlgorithmVolumeDoseTypes.Unset;
                        FieldNormalizationTypes FNM;
                        if (Enum.TryParse(_XMLProtocol.ProtocolChecklist.Calculation.FieldNormalizationMode, true, out FNM))
                            Checklist.FieldNormalizationMode = (int)FNM;
                        else
                            Checklist.FieldNormalizationMode = (int)FieldNormalizationTypes.Unset;
                        Checklist.HeterogeneityOn = string.IsNullOrEmpty(_XMLProtocol.ProtocolChecklist.Calculation.HeterogeneityOn) ? null : (bool?)bool.Parse(_XMLProtocol.ProtocolChecklist.Calculation.HeterogeneityOn);
                        Checklist.AlgorithmResolution = string.IsNullOrEmpty(_XMLProtocol.ProtocolChecklist.Calculation.AlgorithmResolution) ? null : (double?)double.Parse(_XMLProtocol.ProtocolChecklist.Calculation.AlgorithmResolution);

                        AlgorithmVMATOptimizationTypes VMATOptimizationType;
                        if (Enum.TryParse(_XMLProtocol.ProtocolChecklist.Calculation.VMATOptimizationAlgorithm, true, out VMATOptimizationType))
                            Checklist.AlgorithmVMATOptimization = (int)VMATOptimizationType;
                        else
                            Checklist.AlgorithmVMATOptimization = (int)AlgorithmVMATOptimizationTypes.Unset;

                        AlgorithmIMRTOptimizationTypes IMRTOptimizationType;
                        if (Enum.TryParse(_XMLProtocol.ProtocolChecklist.Calculation.IMRTOptimizationAlgorithm, true, out IMRTOptimizationType))
                            Checklist.AlgorithmIMRTOptimization = (int)IMRTOptimizationType;
                        else
                            Checklist.AlgorithmIMRTOptimization = (int)AlgorithmIMRTOptimizationTypes.Unset;

                        Checklist.AirCavityCorrectionVMAT = _XMLProtocol.ProtocolChecklist.Calculation.VMATAirCavityCorrection;
                        Checklist.AirCavityCorrectionIMRT = _XMLProtocol.ProtocolChecklist.Calculation.IMRTAirCavityCorrection;
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
                            if (ProtocolStructureNameToID.ContainsKey(A.ProtocolStructureName))
                                DbA.ProtocolStructure_ID = ProtocolStructureNameToID[A.ProtocolStructureName];
                            else
                            {
                                MessageBox.Show(string.Format("Error importing Protocol {0}: Artifact check references a structure that can't be found. Possible reasons are that it  is not defined, or that there are capitalization errors in the defintiion.", P.ProtocolName));
                                return false;
                            }
                        }
                    }
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
                        DbCI.ID = IDGenerator.GetUniqueId();
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
                            B.DbBeamGeometries = new List<DbBeamGeometry>();
                            foreach (var G in b.ValidGeometries.Geometry)
                            {
                                DbBeamGeometry DbAG = LocalContext.DbBeamGeometries.FirstOrDefault(x => x.GeometryName == G.GeometryName);
                                if (DbAG != null)
                                    B.DbBeamGeometries.Add(DbAG);
                                //    DbAG.DbBeams = B;
                                //DbAG.GeometryName = ArcG.GeometryName;
                                //Trajectories T;
                                //if (Enum.TryParse(ArcG.Trajectory, true, out T))
                                //    DbAG.Trajectory = (int)T;
                                //else
                                //    DbAG.Trajectory = (int)Trajectories.Unset;
                                //DbAG.StartAngle = ArcG.StartAngle;
                                //DbAG.EndAngle = ArcG.EndAngle;
                                //DbAG.StartAngleTolerance = ArcG.StartAngleTolerance;
                                //DbAG.EndAngleTolerance = ArcG.EndAngleTolerance;
                                //LocalContext.DbBeamGeometries.Add(DbAG);
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
            foreach (var S in CurrentProtocol.Structures.OrderBy(x => x.DisplayOrder))
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
                    ProtocolStructureName = Ctr.GetProtocolStructure(A.ProtocolStructureId.Value).ProtocolStructureName,
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


            foreach (var C in CurrentProtocol.Components)
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
                cd.Prescription.ReferenceDose = C.TotalDose.ToString();
                cd.Beams.MaxBeams = C.MaxBeams.Value.ToString();
                cd.Beams.MinBeams = C.MinBeams.Value.ToString();
                cd.Beams.NumIso = C.NumIso.Value.ToString();
                cd.Beams.MinColOffset = C.MinColOffset.Value.ToString();
                foreach (var B in C.Beams)
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
                            GeometryName = vg.DisplayName
                        });
                    }
                }
                XMLProtocol.Components.Component.Add(cd);

                // Constraints
                foreach (var Con in C.Constraints.OrderBy(x => x.DisplayOrder))
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
                                ReferenceStructureName = Con.ReferenceStructureName,
                                DataTablePath = Con.ThresholdDataPath,
                                Stop = Con.Stop.ToString(),
                                ProtocolStructureName = Con.ProtocolStructureName
                            };
                            XMLProtocol.Constraints.ConstraintList.Add(XMLCon);

                            break;
                    }
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
        public async static Task<bool> LoadProtocolFromDb(string ProtocolName, IProgress<int> progress = null) // this method reloads the context.
        {
            try
            {
                StartNewSession();
                CurrentSession.SessionProtocol = await DbController.LoadProtocol(ProtocolName);
                if (CurrentProtocol != null)
                {
                    ProtocolLoaded = true;
                    _uiDispatcher.Invoke(ProtocolOpened, new object[] { null, EventArgs.Empty });
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\r\n {1}\r\n {2}", ex.Message, ex.InnerException, ex.StackTrace));
                return false;
            }
        }


        public static async Task Save_UpdateProtocol()
        {
            CurrentProtocol.LastModifiedBy = SquintUser;
            await DbController.Save_UpdateProtocol();
            string ProtocolNameToReload = CurrentProtocol.ProtocolName;
            await LoadProtocolFromDb(ProtocolNameToReload);
            _uiDispatcher.Invoke(ProtocolUpdated, new[] { null, EventArgs.Empty });
            ProtocolListUpdated?.Invoke(null, EventArgs.Empty);
        }
        public static void Save_DuplicateProtocol()
        {
            DbController.Save_DuplicateProtocol();
            ProtocolListUpdated?.Invoke(null, EventArgs.Empty);
        }
        public static void DeleteProtocol(int Id)
        {
            bool Deleted = DbController.Delete_Protocol(Id);
            if (ProtocolLoaded)
            {
                if (CurrentProtocol.ID == Id)
                {
                    
                    StartNewSession();
                }
            }
            if (Deleted)
            {
                ProtocolListUpdated?.Invoke(null, EventArgs.Empty);
            }
        }
        public static async Task<bool> Save_Session(string SessionComment)
        {
            if (PatientOpen & ProtocolLoaded)
            {
                try
                {
                    bool Success = await DbController.Save_Session(SessionComment);
                    if (Success)
                    {
                        SessionsChanged?.Invoke(null, EventArgs.Empty);
                        return true;
                    }
                    else
                        return false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("Error saving session \r\n StackTrace: {0}", ex.StackTrace));
                    return false;
                }
            }
            else
                return false;
        }
        public static void Delete_Session(int ID)
        {
            DbController.Delete_Session(ID);
            SessionsChanged?.Invoke(null, EventArgs.Empty);
        }
        public static async Task<bool> Load_Session(int ID)
        {
            if (PatientOpen)
            {
                if (ProtocolLoaded)
                {
                    CloseProtocol();
                    StartNewSession();
                }
                Session LoadedSession = await DbController.Load_Session(ID);
                if (LoadedSession != null)  // true if successful load
                {
                    CurrentSession = LoadedSession;
                    ProtocolLoaded = true;
                    AvailableStructureSetsChanged?.Invoke(null, EventArgs.Empty);
                    CurrentStructureSetChanged?.Invoke(null, EventArgs.Empty);
                    _uiDispatcher.Invoke(ProtocolOpened, new object[] { null, EventArgs.Empty });
                    foreach (ProtocolStructure PS in CurrentSession.SessionProtocol.Structures)
                    {
                        UpdateConstraintThresholds(PS);
                    }
                }
                else
                    Ctr.ProtocolLoaded = false;
                return true;
            }
            else return false;
        }
        public static async void SynchronizePlans()
        {
            await Task.Run(() => SynchronizePlansInternal());
            SynchronizationComplete?.Invoke(null, EventArgs.Empty);
        }
        public static void RefreshPatient()
        {
            if (PatientOpen)
            {
                string PID = PatientID;
                if (ESAPIContext != null)
                    ESAPIContext.ClosePatient();
                ESAPIContext.OpenPatient(PID);
            }
        }
        private static async Task SynchronizePlansInternal()
        {
            if (ESAPIContext.Patient != null)
            {
                string PID = ESAPIContext.Patient.Id;
                RefreshPatient();
                // Disable autorefresh (see below for reasons)
                try
                {
                    foreach (PlanAssociation ECP in CurrentSession.PlanAssociations)
                    {
                        if (!ECP.Linked)
                        {
                            continue; // Usually an assessment loaded without a valid plan
                        }
                        AsyncPlan P = await GetAsyncPlanByUID(ECP.CourseId, ECP.PlanUID); // note the ECP.PlanUID & StructureSetUID are not updated till the updatelinkedplan method is called below
                        if (P == null)
                        {
                            if (ECP.PlanType == ComponentTypes.Phase)
                                MessageBox.Show(string.Format("Plan {0} in Course {1} is no longer found", ECP.PlanId, ECP.CourseId));

                        }
                        else
                        {
                            if (P.HistoryDateTime != ECP.LastModified)
                            {
                                MessageBox.Show(string.Format("Note: Dose distribution in plan {0} has changed since last evaluation", ECP.PlanId));
                            }
                            if (P.StructureSetUID != ECP.StructureSetUID)
                            {
                                MessageBox.Show(string.Format("Note: Structure set in plan {0} has changed since last evaluation", ECP.PlanId));
                            }
                        }
                        ECP.UpdateLinkedPlan(P, false);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(String.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                }
            }
        }

        public static void CloseProtocol()
        {
            CurrentSession.SessionProtocol = null;
            ProtocolLoaded = false;
            ProtocolClosed?.Invoke(null, EventArgs.Empty);
        }
        public static void StartNewSession()
        {
            CurrentSession = new Session();
            SetCurrentStructureSet(null);
            _AssessmentNameIterator = 1;
        }

        public static void ClosePatient()
        {
            CurrentSession.PlanAssociations.Clear();
            CurrentStructureSet = null;
            ESAPIContext.ClosePatient();
            PatientOpen = false;
            PatientClosed?.Invoke(null, EventArgs.Empty);
        }
        public static async Task<bool> OpenPatient(string PID)
        {
            PatientOpen = await Task.Run(() => Ctr.ESAPIContext.OpenPatient(PID));
            if (PatientOpen)
                PatientOpened?.Invoke(null, EventArgs.Empty);
            SessionsChanged?.Invoke(null, EventArgs.Empty);
            return PatientOpen;
            //_uiDispatcher.Invoke(PatientOpened, new[] { null, EventArgs.Empty });
        }
        public static void ChangeConstraintComponent(int ConId, int CompId)
        {
            Constraint Con = GetAllConstraints().FirstOrDefault(x => x.ID == ConId);
            CurrentProtocol.Components.FirstOrDefault(x => x.ID == Con.ComponentID).Constraints.Remove(Con);
            Component newComponent = CurrentProtocol.Components.FirstOrDefault(x => x.ID == CompId);
            newComponent.Constraints.Add(Con);
            Con.parentComponent = newComponent;
        }

        public async static void UpdateConstraint(int ConId)
        {
            Constraint Con = GetAllConstraints().FirstOrDefault(x => x.ID == ConId);
            foreach (var PA in CurrentSession.PlanAssociations)
            {
                if (PA.Linked)
                {
                    await Con.EvaluateConstraint(PA);
                }
            }
        }
        public async static void UpdateConstraints()
        {
            foreach (var PA in CurrentSession.PlanAssociations)
            {
                if (PA.Linked)
                {
                    foreach (Constraint Con in CurrentProtocol.Components.First(x => x.ID == PA.ComponentID).Constraints)
                        await Con.EvaluateConstraint(PA);
                }
            }
        }

        public async static Task<bool> UpdateConstraints(int CompId, int? AssessmentId)
        {

            var PAs = CurrentSession.PlanAssociations.Where(x => x.ComponentID == CompId);
            if (AssessmentId != null)
                PAs = PAs.Where(x => x.AssessmentID == AssessmentId);
            foreach (var PA in PAs.ToList())
            {
                if (PA.Linked)
                {
                    foreach (Constraint Con in CurrentProtocol.Components.First(x => x.ID == PA.ComponentID).Constraints)
                        await Con.EvaluateConstraint(PA);
                }
            }
            return true;
        }

        public async static void UpdateConstraints(ProtocolStructure PS)
        {

            foreach (var PA in CurrentSession.PlanAssociations)
            {
                if (PA.Linked)
                {
                    foreach (Constraint Con in CurrentProtocol.Components.First(x => x.ID == PA.ComponentID).Constraints.Where(x => x.PrimaryStructureId == PS.ID || x.ReferenceStructureId == PS.ID))
                        await Con.EvaluateConstraint(PA);
                }
            }
        }

        public static void UpdateConstraintThresholds(ProtocolStructure E) // update interpolated thresholds based on change to ProtocolStructureId
        {
            foreach (Constraint Con in GetAllConstraints().Where(x => x.ReferenceStructureId == E.ID))
            {
                string SSUID = Ctr.CurrentStructureSet.UID;
                if (!string.IsNullOrEmpty(SSUID))
                    Con.UpdateThresholds(E.Volume(SSUID));
            }
        }
        public static bool MatchStructuresByAlias()
        {
            foreach (ProtocolStructure E in CurrentProtocol.Structures) // notify structures
            {
                E.ApplyAliasing(CurrentStructureSet);
                UpdateConstraintThresholds(E);
            }
            return true;
        }

        public static ProtocolStructure GetProtocolStructure(int Id)
        {
            return CurrentProtocol.Structures.FirstOrDefault(x => x.ID == Id);
        }

        public async static Task<StructureLabel> GetStructureLabel(int? Id = null)
        {
            if (Id == null)
                return await DbController.GetStructureLabel(1);
            else
                return await DbController.GetStructureLabel((int)Id);
        }

        public async static Task<string> GetStructureCode(int? Id = null)
        {
            if (Id == null)
                return await DbController.GetStructureCode(1);
            else
                return await DbController.GetStructureCode((int)Id);
        }
        public static IEnumerable<StructureLabel> GetStructureLabels()
        {
            return DbController.GetAllStructureLabels();
        }

        public async static Task<List<ComponentStatusCodes>> AssociatePlanToComponent(int AssessmentID, int ComponentID, string CourseId, string PlanId, ComponentTypes Type, bool ClearWarnings) // ClearWarnings
        {
            try
            {
                StartingLongProcess?.Invoke(null, EventArgs.Empty);
                AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
                return await Task.Run(() =>
                {
                    PlanAssociation PA = CurrentSession.PlanAssociations.FirstOrDefault(x => x.AssessmentID == AssessmentID && x.ComponentID == ComponentID);
                    PA.UpdateLinkedPlan(P, ClearWarnings);
                    AvailableStructureSetsChanged?.Invoke(null, EventArgs.Empty);
                    if (CurrentStructureSet == null)
                    {
                        SetCurrentStructureSet(P.StructureSetUID);
                        MatchStructuresByAlias();
                    }
                    else if (!GetAvailableStructureSets().Select(x => x.StructureSetUID).Contains(CurrentStructureSet.UID))
                    {
                        SetCurrentStructureSet(P.StructureSetUID);
                        MatchStructuresByAlias();
                    }
                    Ctr.EndingLongProcess?.Invoke(null, EventArgs.Empty);
                    return PA.GetErrorCodes();
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2})", ex.Message, ex.InnerException, ex.StackTrace));
                return null;
            }
        }
        public static List<SessionView> GetSessionViews()
        {
            List<SessionView> SVs = new List<SessionView>();
            if (ESAPIContext != null)
            {
                foreach (Session S in DbController.GetSessions())
                {
                    SVs.Add(new SessionView(S));
                }
            }
            return SVs;
        }
        public static Assessment NewAssessment()
        {
            Assessment NewAssessment = new Assessment(CurrentSession.ID, string.Format("Assessment#{0}", _AssessmentNameIterator), _AssessmentNameIterator);
            CurrentSession.Assessments.Add(NewAssessment);
            foreach (Component SC in CurrentProtocol.Components)
            {
                CurrentSession.PlanAssociations.Add(new PlanAssociation(SC, NewAssessment));
            }
            _AssessmentNameIterator++;
            return NewAssessment;
        }
        public static void RemoveAssessment(int AssessmentID)
        {
            foreach (PlanAssociation PL in CurrentSession.PlanAssociations.Where(x => x.AssessmentID == AssessmentID).ToList())
            {
                CurrentSession.PlanAssociations.Remove(PL);
            }
            CurrentSession.Assessments.Remove(CurrentSession.Assessments.FirstOrDefault(x => x.ID == AssessmentID));
        }

        public async static Task<List<PlanDescriptor>> GetPlanDescriptors(string CourseId)
        {
            AsyncCourse C = await ESAPIContext.Patient.GetCourse(CourseId);
            if (C != null)
            {
                return C.PlanDescriptors;
            }
            else return null;
        }
        public static List<string> GetCourseNames()
        {
            return Ctr.ESAPIContext.Patient.CourseIds;
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
            CurrentProtocol.Components.FirstOrDefault(x => x.ID == ComponentId).Beams.Add(new Beam(ComponentId));
        }

        public static void AddStructureAlias(int StructureId, string NewAlias)
        {
            GetProtocolStructure(StructureId).DefaultEclipseAliases.Add(NewAlias);
        }
        public static void RemoveStructureAlias(int StructureId, string AliasToRemove)
        {
            GetProtocolStructure(StructureId).DefaultEclipseAliases.Remove(AliasToRemove);
        }

        public static List<BeamGeometryDefinition> GetBeamGeometryDefinitions()
        {
            return new List<BeamGeometryDefinition>(_BeamGeometryDefinitions);
        }

        
    }
}
