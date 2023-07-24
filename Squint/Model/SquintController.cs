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
using Squint.ViewModels;
using Squint.Extensions;
using System.Windows.Threading;
using System.Runtime.Remoting.Contexts;
using System.Reflection;
using AutoMapper;
using Squint.Helpers;
//using System.Windows.Forms;

namespace Squint
{
    public static partial class SquintModel
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
                // set dispatcher;
                _uiDispatcher = uiDispatcher;
                
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
                if (ReferenceEquals(null, SquintModel.ESAPIContext.Patient))
                    return string.Empty;
                else
                    return SquintModel.ESAPIContext.Patient.Id;
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

        public async static Task<string> GetCTDeviceId(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetCTDeviceId();
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
                    if (I.Type == FieldTechniqueType.Ant_kv && ((Isocentre.x - PatientCentre.x) < -50 || TxFields.All(x => (x.GantryStart < 40 || x.GantryStart > 180) && (x.GantryEnd < 40 || x.GantryEnd > 180))))
                    {
                        I.Warning = true;
                        I.WarningMessage = "Posterior preferred?";
                    }
                    if (I.Type == FieldTechniqueType.Post_kv && ((Isocentre.x - PatientCentre.x) > 50 || TxFields.All(x => (x.GantryStart > 320 || x.GantryStart < 180) && (x.GantryEnd > 320 || x.GantryEnd < 180))))
                    {
                        I.Warning = true;
                        I.WarningMessage = "Anterior preferred?";
                    }
                }
                return ImagingFields;
            }
        }
        public static Dictionary<ImagingProtocolTypes, List<string>> CheckImagingProtocols(Component CV, List<ImagingFieldItem> IF)
        {
            Dictionary<ImagingProtocolTypes, List<string>> Errors = new Dictionary<ImagingProtocolTypes, List<string>>();
            foreach (ImagingProtocolTypes IP in CV.ImagingProtocols)
            {
                if (!Errors.ContainsKey(IP))
                    Errors.Add(IP, new List<string>());
                switch (IP)
                {
                    case ImagingProtocolTypes.kV_2D:
                        if (!(IF.Any(x => x.Type == FieldTechniqueType.Ant_kv) && (IF.Any(x => x.Type == FieldTechniqueType.LL_kv) || IF.Any(x => x.Type == FieldTechniqueType.RL_kv))) &&
                            !((IF.Any(x => x.Type == FieldTechniqueType.Post_kv) && (IF.Any(x => x.Type == FieldTechniqueType.LL_kv) || IF.Any(x => x.Type == FieldTechniqueType.RL_kv)))))
                            Errors[IP].Add("Cannot find kv-pair");
                        break;
                    case ImagingProtocolTypes.PreCBCT:
                        if (!IF.Any(x => x.Type == FieldTechniqueType.CBCT))
                            Errors[IP].Add("Cannot find CBCT");
                        break;
                    case ImagingProtocolTypes.PostCBCT:
                        if (!IF.Any(x => x.Type == FieldTechniqueType.CBCT))
                            Errors[IP].Add("Cannot find CBCT");
                        break;
                }
            }
            if (CV.ImagingProtocols.Contains(ImagingProtocolTypes.PostCBCT) && CV.ImagingProtocols.Contains(ImagingProtocolTypes.PreCBCT))
            {
                if (IF.Where(x => x.Type == FieldTechniqueType.CBCT).Count() < 2)
                {
                    Errors[ImagingProtocolTypes.PostCBCT].Add("Insufficient CBCT fields for both pre and post imaging");
                    Errors[ImagingProtocolTypes.PreCBCT].Add("Insufficient CBCT fields for both pre and post imaging");
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


        public static Constraint AddConstraint(ConstraintTypes TypeCode, int ComponentID = 0, int StructureId = 1)
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
                while (CurrentProtocol.Structures.Select(x => x.ProtocolStructureName).Any(x => x.Equals(NewStructureName)))
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
        //public static void AddConstraintThreshold(ReferenceThresholdTypes Name, int ConstraintID, double ThresholdValue)
        //{
        //    // ConstraintThreshold CT = new ConstraintThreshold(Name, ConstraintID, ThresholdValue);
        //}

        public static async Task<bool> ImportEclipseProtocol(VMS_XML.Protocol P)
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
                foreach (var ECPStructure in P.StructureTemplate.Structures)
                {
                    StructureLabel SL = await DbController.GetStructureLabel(1);
                    var S = new ProtocolStructure(SL, ECPStructure.ID);
                    S.DefaultEclipseAliases.Add(ECPStructure.ID);
                    EclipseProtocol.Structures.Add(S);
                    EclipseStructureIdToSquintStructureIdMapping.Add(ECPStructure.ID.ToLower(), S.ID);
                }
                int ComponentDisplayOrder = 1;
                foreach (var Ph in P.Phases)
                {

                    int numFractions = Convert.ToInt32(Ph.FractionCount);
                    double totalDose = (double)Ph.PlanTemplate.DosePerFraction * numFractions * 100; // convert to cGy;
                    Component SC = new Component(EclipseProtocol.ID, Ph.ID, ComponentDisplayOrder++, numFractions, totalDose);
                    EclipseProtocol.Components.Add(SC);
                    if (Ph.Prescription.Item != null)
                    {
                        foreach (var Item in Ph.Prescription.Item)
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
                        foreach (var MI in Ph.Prescription.MeasureItem)
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
            SquintProtocol _XMLProtocol;
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
                _XMLProtocol = ser.Deserialize<SquintProtocol>(protocolInput);
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
                _XMLProtocol.Constraints = new List<SquintProtocolConstraint>().ToArray();
            }
            foreach (SquintProtocolComponent cd in _XMLProtocol.Components)
            {
                cd.Id = ComponentPKiterator; // assign primary keys
                bool ValidComponent = false;
                if (cd.ComponentName != null)
                {
                    if (cd.ComponentName != "")
                    {
                        ComponentID2PK.Add(cd.ComponentName, cd.Id);
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
            foreach (SquintProtocolConstraint con in _XMLProtocol.Constraints)
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
                con.ComponentId = ComponentID2PK[con.ComponentName.Trim()];
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
        public static bool SaveXMLProtocolToDatabase(SquintProtocol _XMLProtocol)
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

                if (_XMLProtocol.TreatmentIntents != null)
                {
                    foreach (var TxIntent in _XMLProtocol.TreatmentIntents)
                    {
                        DbTreatmentIntent TI = LocalContext.DbTreatmentIntents.FirstOrDefault(x => x.Intent.Equals(TypeDisplay.Display(TxIntent.Id), StringComparison.OrdinalIgnoreCase));
                        if (P.TreatmentIntents == null)
                            P.TreatmentIntents = new List<DbTreatmentIntent>();
                        P.TreatmentIntents.Add(TI);
                    }
                }
                P.DbApprovalLevel = LocalContext.DbApprovalLevels.Where(x => x.ApprovalLevel == (int)_XMLProtocol.ProtocolMetaData.ApprovalStatus).Single();
                P.DbTreatmentSite = LocalContext.DbTreatmentSites.Where(x => x.TreatmentSite == (int)_XMLProtocol.ProtocolMetaData.DiseaseSite).Single();
                P.DbTreatmentCentre = LocalContext.DbTreatmentCentres.Where(x => x.TreatmentCentre == (int)_XMLProtocol.ProtocolMetaData.TreatmentCentre).Single();
                P.DbProtocolType = LocalContext.DbProtocolTypes.Where(x => x.ProtocolType == (int)_XMLProtocol.ProtocolMetaData.ProtocolType).Single();
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
                foreach (SquintProtocolStructure S in _XMLProtocol.Structures)
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
                        foreach (var EA in S.EclipseAliases)
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
                            DbSC.PointContourThreshold = SC.PointContourCheck.Threshold;
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

                        Checklist.AlgorithmVolumeDose = (int)_XMLProtocol.ProtocolChecklist.Calculation.Algorithm;
                        Checklist.FieldNormalizationMode = (int)_XMLProtocol.ProtocolChecklist.Calculation.FieldNormalizationMode;
                        Checklist.HeterogeneityOn = _XMLProtocol.ProtocolChecklist.Calculation.HeterogeneityOn;
                        Checklist.AlgorithmResolution = _XMLProtocol.ProtocolChecklist.Calculation.AlgorithmResolution;
                        Checklist.AlgorithmVMATOptimization = (int)_XMLProtocol.ProtocolChecklist.Calculation.VMATOptimizationAlgorithm;
                        Checklist.AlgorithmIMRTOptimization = (int)_XMLProtocol.ProtocolChecklist.Calculation.IMRTOptimizationAlgorithm;
                        Checklist.AirCavityCorrectionVMAT = _XMLProtocol.ProtocolChecklist.Calculation.VMATAirCavityCorrection;
                        Checklist.AirCavityCorrectionIMRT = _XMLProtocol.ProtocolChecklist.Calculation.IMRTAirCavityCorrection;
                    }
                    if (_XMLProtocol.ProtocolChecklist.Supports != null)
                    {
                        Checklist.SupportIndication = (int)_XMLProtocol.ProtocolChecklist.Supports.Indication;
                        Checklist.CouchInterior = _XMLProtocol.ProtocolChecklist.Supports.CouchInterior;
                        Checklist.CouchSurface = _XMLProtocol.ProtocolChecklist.Supports.CouchSurface;
                    }
                    if (_XMLProtocol.ProtocolChecklist.Simulation != null)
                    {
                        Checklist.SliceSpacing = _XMLProtocol.ProtocolChecklist.Simulation.SliceSpacing;
                        if (_XMLProtocol.ProtocolChecklist.Simulation.CTDeviceIds != null)
                        {
                            foreach (var CTDeviceId in _XMLProtocol.ProtocolChecklist.Simulation.CTDeviceIds)
                            {
                                DbCTDeviceId DbCTID = LocalContext.DbCTDeviceIds.FirstOrDefault(x => x.CTDeviceId.ToUpper() == CTDeviceId.Id.ToUpper());
                                if (DbCTID == null)
                                {
                                    DbCTID = LocalContext.DbCTDeviceIds.Create();
                                    DbCTID.CTDeviceId = CTDeviceId.Id;
                                    LocalContext.DbCTDeviceIds.Add(DbCTID);
                                }
                                if (Checklist.CTDeviceIds == null)
                                {
                                    Checklist.CTDeviceIds = new List<DbCTDeviceId>();
                                }
                                Checklist.CTDeviceIds.Add(DbCTID);
                            }
                        }
                    }

                    if (_XMLProtocol.ProtocolChecklist.Artifacts != null)
                    {
                        foreach (SquintProtocolProtocolChecklistArtifact A in _XMLProtocol.ProtocolChecklist.Artifacts)
                        {
                            DbArtifact DbA = LocalContext.DbArtifacts.Create();
                            LocalContext.DbArtifacts.Add(DbA);
                            DbA.HU = A.HU;
                            DbA.ToleranceHU = A.ToleranceHU;
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
                foreach (SquintProtocolComponent comp in _XMLProtocol.Components)
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
                        C.NumFractions = comp.Prescription.NumFractions;
                        C.ReferenceDose = comp.Prescription.ReferenceDose;
                        C.PNVMax = comp.Prescription.PNVMax;
                        C.PNVMin = comp.Prescription.PNVMin;
                        C.PrescribedPercentage = comp.Prescription.PrescribedPercentage;
                    }


                    ComponentTypes ComponentType;
                    if (comp.Type == ComponentTypes.Unset)
                    {
                        MessageBox.Show(string.Format("Components must must specify either Plan or Sum in the Type attribute (Protocol: {0} Component: {1})", P.ProtocolName, comp.ComponentName));
                        return false;
                    }
                    else
                        C.ComponentType = (int)comp.Type;
                    LocalContext.DbComponents.Add(C);
                    //LocalContext.SaveChanges(); // component Key available
                    CompName2DB.Add(comp.ComponentName, C);
                    //Add imaging
                    if (comp.ImagingProtocols != null)
                    {
                        ImagingProtocolTypes ProtocolType;
                        DbComponentImaging DbCI = LocalContext.DbComponentImagings.Create();
                        LocalContext.DbComponentImagings.Add(DbCI);
                        DbCI.ID = IDGenerator.GetUniqueId();
                        DbCI.DbComponent = C;
                        foreach (var IP in comp.ImagingProtocols)
                        {
                            DbImaging DbI = LocalContext.DbImagings.Create();
                            DbI.ImagingProtocol = (int)IP.Id;
                            if (string.IsNullOrEmpty(IP.ImagingProtocolDisplayName))
                                DbI.ImagingProtocolName = IP.Id.ToString();
                            else
                                DbI.ImagingProtocolName = IP.ImagingProtocolDisplayName;
                            DbI.ComponentImagingID = DbCI.ID;
                            LocalContext.DbImagings.Add(DbI);
                        }
                    }
                    if (comp.Beams != null)
                    {
                        if (comp.Beams.MinBeams == -1)
                            C.MinBeams = null;
                        else
                            C.MinBeams = comp.Beams.MinBeams;
                        if (comp.Beams.MaxBeams == -1)
                            C.MaxBeams = null;
                        else
                            C.MaxBeams = comp.Beams.MaxBeams;
                        if (comp.Beams.MinColOffset == -1) // -1 is default, which is interpreted as null, meaning no constraint
                            C.MinColOffset = null;
                        else
                            C.MinColOffset = comp.Beams.MinColOffset;
                        C.NumIso = comp.Beams.NumIso; // default is 1   

                        foreach (var b in comp.Beams.Beam)
                        {
                            DbBeam B = LocalContext.DbBeams.Create();
                            LocalContext.DbBeams.Add(B);
                            foreach (var Alias in b.EclipseAliases)
                            {
                                DbBeamAlias DbBA = LocalContext.DbBeamAliases.Create();
                                DbBA.EclipseFieldId = Alias.Id;
                                DbBA.DbBeam = B;
                                LocalContext.DbBeamAliases.Add(DbBA);
                            }
                            B.DbBeamGeometries = new List<DbBeamGeometry>();
                            foreach (var G in b.ValidGeometries)
                            {
                                DbBeamGeometry DbAG = LocalContext.DbBeamGeometries.Create();
                                if (DbAG != null)
                                {
                                    B.DbBeamGeometries.Add(DbAG);
                                    DbAG.DbBeams.Add(B);
                                    DbAG.GeometryName = G.GeometryName;
                                    TrajectoryTypes T;
                                    DbAG.Trajectory = (int)G.Trajectory;
                                    DbAG.StartAngle = G.StartAngle;
                                    DbAG.EndAngle = G.EndAngle;
                                    DbAG.StartAngleTolerance = G.StartAngleTolerance;
                                    DbAG.EndAngleTolerance = G.EndAngleTolerance;
                                    LocalContext.DbBeamGeometries.Add(DbAG);
                                }
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
                            bool UnsetEnergyAdded = false;
                            foreach (var E in b.ValidEnergies)
                            {
                                DbEnergy DbEn = LocalContext.DbEnergies.FirstOrDefault(x => x.EnergyString == TypeDisplay.Display(E.Mode));
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
                            B.Technique = (int)b.Technique;
                            B.JawTracking_Indication = (int)b.JawTracking_Indication;


                            if (b.BolusDefinitions != null)
                            {
                                foreach (var bolus in b.BolusDefinitions)
                                {
                                    if (double.IsNaN(bolus.HU))
                                        continue;
                                    DbBolus DbBolus = LocalContext.DbBoluses.Create();
                                    DbBolus.DbBeam = B;
                                    DbBolus.HU = bolus.HU;
                                    DbBolus.Indication = (int)bolus.Indication;
                                    DbBolus.Thickness = bolus.Thickness;
                                    DbBolus.ToleranceThickness = bolus.ToleranceThickness;
                                    DbBolus.ToleranceHU = bolus.ToleranceHU;
                                    LocalContext.DbBoluses.Add(DbBolus);
                                }
                            }
                        }
                    }
                }
                int ConDisplayOrder = 1;
                foreach (SquintProtocolConstraint con in _XMLProtocol.Constraints)
                {
                    // Input error checking
                    DbConstraint DbCon = LocalContext.DbConstraints.Create();
                    DbCon.ConstraintType = (int)con.ConstraintType;
                    DbCon.ConstraintScale = (int)con.ConstraintUnit;
                    DbCon.ReferenceScale = (int)con.ReferenceUnit;
                    DbCon.ReferenceType = (int)con.ReferenceType;
                    DbCon.ConstraintValue = con.ConstraintValue;
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
                        if (con.ConstraintType == ConstraintTypes.CI)
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
                        DbCon.MajorViolation = con.MajorViolation;
                        DbCon.MinorViolation = con.MinorViolation;
                        DbCon.Stop = con.Stop;
                    }
                    DbConstraintChangelog DbCC = LocalContext.DbConstraintChangelogs.Create();

                    LocalContext.DbConstraintChangelogs.Add(DbCC);
                    DbCC.ChangeDescription = con.Description;
                    DbCC.ConstraintString = "";
                    DbCC.ChangeAuthor = SquintModel.SquintUser;
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
            SquintProtocol XMLProtocol = new SquintProtocol();
            //Assign TEMPORARY primary keys to each constraint, since these are not defined in the XML protocol
            XMLProtocol.ProtocolMetaData = new SquintProtocolProtocolMetaData
            {
                ApprovalStatus = CurrentProtocol.ApprovalLevel,
                DiseaseSite = CurrentProtocol.TreatmentSite.Value,
                Author = CurrentProtocol.Author,
                ProtocolDate = DateTime.Now.ToShortDateString(),
                ProtocolName = CurrentProtocol.ProtocolName,
                ProtocolType = CurrentProtocol.ProtocolType,
                TreatmentCentre = CurrentProtocol.TreatmentCentre.Value
            };
            var intentsToSerialize = new List<SquintProtocolIntent>();
            foreach (var TI in CurrentProtocol.TreatmentIntents)
            {
                var TreatmentIntent = new SquintProtocolIntent(){ Id = TI };
                intentsToSerialize.Add(TreatmentIntent);
            }
            XMLProtocol.TreatmentIntents = intentsToSerialize.ToArray();
            var structuresToSerialize = new List<SquintProtocolStructure>();
            foreach (var S in CurrentProtocol.Structures.OrderBy(x => x.DisplayOrder))
            {
                var XMLStructure = new SquintProtocolStructure
                {
                    Label = S.StructureLabel.LabelName,
                    ProtocolStructureName = S.ProtocolStructureName,
                };
                var aliasesToSerialize = new List<SquintProtocolStructureEclipseId>();
                foreach (string alias in S.DefaultEclipseAliases)
                {
                    aliasesToSerialize.Add(new SquintProtocolStructureEclipseId()
                    {
                        Id = alias
                    });
                }
                XMLStructure.EclipseAliases = aliasesToSerialize.ToArray();
                if (S.CheckList.isPointContourChecked.isDefined)
                {
                    if ((bool)S.CheckList.isPointContourChecked.Value)
                    {
                        XMLStructure.StructureChecklist = new SquintProtocolStructureStructureChecklist()
                        {
                            PointContourCheck = new SquintProtocolStructureStructureChecklistPointContourCheck()
                            {
                                Threshold = (double)S.CheckList.PointContourVolumeThreshold.Value
                            }
                        };
                    }
                }
                structuresToSerialize.Add(XMLStructure);
            }
            XMLProtocol.Structures = structuresToSerialize.ToArray();
            var artifactsToSerialize = new List<SquintProtocolProtocolChecklistArtifact>();
            foreach (var A in CurrentProtocol.Checklist.Artifacts)
            {
                artifactsToSerialize.Add(new SquintProtocolProtocolChecklistArtifact()
                {
                    HU = (double)A.RefHU.Value,
                    ProtocolStructureName = SquintModel.GetProtocolStructure(A.ProtocolStructureId.Value).ProtocolStructureName,
                    ToleranceHU = (double)A.ToleranceHU.Value
                });
            }
            XMLProtocol.ProtocolChecklist.Artifacts = artifactsToSerialize.ToArray();
            XMLProtocol.ProtocolChecklist.Calculation.Algorithm = CurrentProtocol.Checklist.Algorithm.Value;
            XMLProtocol.ProtocolChecklist.Calculation.AlgorithmResolution = CurrentProtocol.Checklist.AlgorithmResolution.Value == null ? double.NaN : (double)CurrentProtocol.Checklist.AlgorithmResolution.Value;
            XMLProtocol.ProtocolChecklist.Calculation.FieldNormalizationMode = CurrentProtocol.Checklist.FieldNormalizationMode.Value;
            XMLProtocol.ProtocolChecklist.Calculation.HeterogeneityOn = CurrentProtocol.Checklist.HeterogeneityOn.Value == null ? true : (bool)CurrentProtocol.Checklist.HeterogeneityOn.Value;
            XMLProtocol.ProtocolChecklist.Calculation.VMATOptimizationAlgorithm = CurrentProtocol.Checklist.AlgorithmVMATOptimization.Value;
            XMLProtocol.ProtocolChecklist.Calculation.IMRTOptimizationAlgorithm = CurrentProtocol.Checklist.AlgorithmIMRTOptimization.Value;
            XMLProtocol.ProtocolChecklist.Calculation.VMATAirCavityCorrection = CurrentProtocol.Checklist.AirCavityCorrectionVMAT.Value;
            XMLProtocol.ProtocolChecklist.Calculation.IMRTAirCavityCorrection = CurrentProtocol.Checklist.AirCavityCorrectionIMRT.Value;
            XMLProtocol.ProtocolChecklist.Simulation.SliceSpacing = CurrentProtocol.Checklist.SliceSpacing.Value == null ? double.NaN : (double)CurrentProtocol.Checklist.SliceSpacing.Value;
            XMLProtocol.ProtocolChecklist.Supports.CouchInterior = CurrentProtocol.Checklist.CouchInterior.Value == null ? double.NaN : (double)CurrentProtocol.Checklist.CouchInterior.Value;
            XMLProtocol.ProtocolChecklist.Supports.CouchSurface = CurrentProtocol.Checklist.CouchSurface.Value == null ? double.NaN : (double)CurrentProtocol.Checklist.CouchSurface.Value;
            XMLProtocol.ProtocolChecklist.Supports.Indication = CurrentProtocol.Checklist.SupportIndication.Value;
            var CTDevicesToSerialize = new List<SquintProtocolProtocolChecklistSimulationCTDeviceId>();
            foreach (var CTDeviceId in CurrentProtocol.Checklist.CTDeviceIds)
            {
                CTDevicesToSerialize.Add(new SquintProtocolProtocolChecklistSimulationCTDeviceId { Id = CTDeviceId });
            }
            XMLProtocol.ProtocolChecklist.Simulation.CTDeviceIds = CTDevicesToSerialize.ToArray();

            var componentsToSerialize = new List<SquintProtocolComponent>();
            foreach (var C in CurrentProtocol.Components)
            {
                SquintProtocolComponent cd = new SquintProtocolComponent()
                {
                    ComponentName = C.ComponentName,
                    Type = C.ComponentType.Value
                };
                componentsToSerialize.Add(cd);
                cd.Prescription.PNVMax = C.PNVMax.Value == null ? double.NaN : (double)C.PNVMax.Value;
                cd.Prescription.PNVMin = C.PNVMin.Value == null ? double.NaN : (double)C.PNVMin.Value;
                cd.Prescription.PrescribedPercentage = C.PrescribedPercentage.Value == null ? double.NaN : (double)C.PrescribedPercentage.Value;
                cd.Prescription.NumFractions = C.NumFractions;
                cd.Prescription.ReferenceDose = C.TotalDose;
                cd.Beams.MaxBeams = C.MaxBeams.Value == null ? -1 : (int)C.MaxBeams.Value;
                cd.Beams.MinBeams = C.MinBeams.Value == null ? -1 : (int)C.MinBeams.Value;
                cd.Beams.NumIso = C.NumIso.Value == null ? 0 : (int)C.NumIso.Value;
                cd.Beams.MinColOffset = C.MinColOffset.Value == null ? double.NaN : (double)C.MinColOffset.Value;
                var beamsToSerialize = new List<SquintProtocolComponentBeamsBeam>();
                foreach (var B in C.Beams)
                {
                    var bd = new SquintProtocolComponentBeamsBeam
                    {
                        CouchRotation = B.CouchRotation.Value == null ? double.NaN : (double)B.CouchRotation.Value,
                        JawTracking_Indication = B.JawTracking_Indication.Value,
                        MaxColRotation = B.MaxColRotation.Value == null ? double.NaN : (double)B.MaxColRotation.Value,
                        MaxX = B.MaxX.Value == null ? double.NaN : (double)B.MaxX.Value,
                        MaxY = B.MaxY.Value == null ? double.NaN : (double)B.MaxY.Value,
                        MinX = B.MinX.Value == null ? double.NaN : (double)B.MinX.Value,
                        MinY = B.MinY.Value == null ? double.NaN : (double)B.MinY.Value,
                        MinMUWarning = B.MinMUWarning.Value == null ? double.NaN : (double)B.MinMUWarning.Value,
                        MaxMUWarning = B.MaxMUWarning.Value == null ? double.NaN : (double)B.MaxMUWarning.Value,
                        ProtocolBeamName = B.ProtocolBeamName,
                        Technique = B.Technique,
                        ToleranceTable = B.ToleranceTable.Value.ToString(),
                        MinColRotation = B.MinColRotation.Value == null ? double.NaN : (double)B.MinColRotation.Value,
                    };
                    beamsToSerialize.Add(bd);
                    var aliasesToSerialize = new List<SquintProtocolComponentBeamsBeamEclipseId>();
                    foreach (string alias in B.EclipseAliases)
                    {
                        aliasesToSerialize.Add(new SquintProtocolComponentBeamsBeamEclipseId { Id = alias });
                    }
                    bd.EclipseAliases = aliasesToSerialize.ToArray();
                    var EnergiesToSerialize = new List<SquintProtocolComponentBeamsBeamEnergy>();
                    foreach (Energies E in B.ValidEnergies)
                    {
                        EnergiesToSerialize.Add(new SquintProtocolComponentBeamsBeamEnergy { Mode = E });
                    }
                    bd.ValidEnergies = EnergiesToSerialize.ToArray();
                    var bolusToSerialize = new List<SquintProtocolComponentBeamsBeamBolus>();
                    foreach (var Bol in B.Boluses)
                    {
                        bolusToSerialize.Add(new SquintProtocolComponentBeamsBeamBolus()
                        {
                            HU = Bol.HU.Value,
                            Indication = Bol.Indication.Value,
                            Thickness = Bol.Thickness.Value,
                            ToleranceHU = Bol.ToleranceHU.Value,
                            ToleranceThickness = Bol.ToleranceThickness.Value
                        });
                    }
                    var geometriesToSerialize = new List<SquintProtocolComponentBeamsBeamGeometry>();
                    foreach (var vg in B.ValidGeometries)
                    {
                        geometriesToSerialize.Add(new SquintProtocolComponentBeamsBeamGeometry()
                        {
                            GeometryName = vg.DisplayName
                        });
                    }
                    bd.ValidGeometries = geometriesToSerialize.ToArray();
                }
                cd.Beams.Beam = beamsToSerialize.ToArray();
                var ImagingProtocolsToSerialize = new List<SquintProtocolComponentImagingProtocol>();
                foreach (var I in C.ImagingProtocols)
                {
                    ImagingProtocolsToSerialize.Add(new SquintProtocolComponentImagingProtocol() { Id = I });
                }
                cd.ImagingProtocols = ImagingProtocolsToSerialize.ToArray();

                // Constraints
                var constraintsToSerialize = new List<SquintProtocolConstraint>();
                foreach (var Con in C.Constraints.OrderBy(x => x.DisplayOrder))
                {
                    switch (Con.ConstraintType)
                    {
                        case ConstraintTypes.Unset:
                            continue;
                        default:
                            SquintProtocolConstraint XMLCon = new SquintProtocolConstraint()
                            {
                                ComponentName = Con.ComponentName,
                                ReferenceType = Con.ReferenceType,
                                ReferenceUnit = Con.ReferenceScale,
                                ConstraintType = Con.ConstraintType,
                                ConstraintUnit = Con.ConstraintScale,
                                ConstraintValue = Con.ConstraintValue,
                                MajorViolation = (double)Con.MajorViolation,
                                MinorViolation = Con.MinorViolation == null ? double.NaN : (double)Con.MinorViolation,
                                ReferenceStructureName = Con.ReferenceStructureName,
                                DataTablePath = Con.ThresholdDataPath,
                                Stop = Con.Stop == null ? double.NaN : (double)Con.Stop,
                                ProtocolStructureName = Con.ProtocolStructureName
                            };
                            constraintsToSerialize.Add(XMLCon);

                            break;
                    }
                }
                XMLProtocol.Constraints = constraintsToSerialize.ToArray();
            }

            Serializer ser = new Serializer();
            try
            {
                ser.Serialize<SquintProtocol>(XMLProtocol, filename);
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
                    SquintModel.ProtocolLoaded = false;
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
            PatientOpen = await Task.Run(() => SquintModel.ESAPIContext.OpenPatient(PID));
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
            foreach (var PA in CurrentSession.PlanAssociations.Where(x => x.ComponentID == Con.ComponentID))
            {
                await Con.EvaluateConstraint(PA);
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
                string SSUID = SquintModel.CurrentStructureSet.UID;
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
                    SquintModel.EndingLongProcess?.Invoke(null, EventArgs.Empty);
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
            return SquintModel.ESAPIContext.Patient.CourseIds;
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
