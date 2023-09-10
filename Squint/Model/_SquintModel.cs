using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.IO;
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
using Squint.Helpers;
using System.Drawing;
using Squint.Structs;

//using System.Windows.Forms;

namespace Squint
{
    public partial class SquintModel
    {
        private Dispatcher _uiDispatcher;
        public AsyncESAPI ESAPIContext { get; private set; }
        // State Data
        public string SquintUser { get; private set; }
        public Protocol CurrentProtocol { get { return CurrentSession.SessionProtocol; } }
        public Session CurrentSession { get; private set; } = new Session();

        private AsyncStructureSet _CurrentStructureSet;
        public AsyncStructureSet CurrentStructureSet
        {
            get { return _CurrentStructureSet; }
            private set { _CurrentStructureSet = value; CurrentStructureSetChanged?.Invoke(null, EventArgs.Empty); }
        }

        private List<BeamGeometryDefinition> _BeamGeometryDefinitions;
        public Dictionary<int, ComponentModel> ComponentViews { get; private set; } = new Dictionary<int, ComponentModel>();
        public Dictionary<int, AssessmentView> AssessmentViews { get; private set; } = new Dictionary<int, AssessmentView>();
        public bool SavingNewProtocol { get; } = false;
        public bool PatientOpen { get; private set; } = false;
        private int _NewStructureCounter = 1;
        private int _NewPlanCounter = 1;
        private int _AssessmentNameIterator = 1;
        public string DefaultNewStructureName = "UserStructure";
        public SquintConfiguration Config;
        //static public bool PatientOpen { get; private set; } = false;
        public bool ProtocolLoaded { get; private set; } = false;
        // Events

        //public static event EventHandler CurrentStructureSetChanged;
        public event EventHandler Initialized;
        public event EventHandler DatabaseInitializing;
        public event EventHandler DatabaseCreating;
        public event EventHandler ESAPIInitializing;
        public event EventHandler AvailableStructureSetsChanged;
        public event EventHandler SynchronizationComplete;
        public event EventHandler ProtocolListUpdated;
        public event EventHandler ProtocolUpdated;
        public event EventHandler CurrentStructureSetChanged;
        public event EventHandler ProtocolConstraintOrderChanged;
        public event EventHandler ProtocolOpened;
        public event EventHandler ProtocolClosed;
        public event EventHandler PatientOpened;
        public event EventHandler PatientClosed;
        public event EventHandler StartingLongProcess;
        public event EventHandler EndingLongProcess;
        public event EventHandler SessionsChanged;
        public event EventHandler<int> ConstraintAdded;

        public DbConfigurationPaths DbPaths { get; private set; }

        // Initialization
        public SquintModel()
        {
            try
            {
                // set dispatcher;
                _uiDispatcher = Dispatcher.CurrentDispatcher;


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

                CurrentSession = new Session();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }
        }

        public void InitializeModel()
        {
            InitializeESAPI();
            InitializeDatabase();
            InitializeDefinitions();
        }

        private void InitializeDefinitions()
        {
            // Automapper.Initialize();  // unable to get this to work globally due to limitations with ConvertUsing in the version of Automapper that supports .net 4.5
            var dbCon = new DbController(this);
            _BeamGeometryDefinitions = dbCon.GetBeamGeometryDefinitions();
            StructureLabelLookup.SetDictionary(dbCon.GetAllStructureLabels());
        }
        private void InitializeESAPI()
        {
            _uiDispatcher.Invoke(ESAPIInitializing, new object[] { null, EventArgs.Empty });
            ESAPIContext = new AsyncESAPI();
            SquintUser = ESAPIContext.CurrentUserId(); // Get user from ESAPI;

        }
        private void InitializeDatabase()
        {
            var dBCon = new DbController(this);
            _uiDispatcher.Invoke(DatabaseInitializing, new object[] { null, EventArgs.Empty });
            DbPaths = new DbConfigurationPaths()
            {
                structureDefinitionPath = Config.StructureCodes.FirstOrDefault(x => x.Site == Config.Site.CurrentSite).Path,
                beamDefinitionPath = Config.BeamGeometryDefinitions.FirstOrDefault(x => x.Site == Config.Site.CurrentSite).Path,
            };
            var database = Config.Databases.FirstOrDefault(x => x.Site == Config.Site.CurrentSite);
            dBCon.SetDatabaseName(database.DatabaseName);
            var DBStatus = dBCon.TestDbConnection();
            if (DBStatus == DatabaseStatus.NonExistent)
            {
                _uiDispatcher.Invoke(DatabaseCreating, new object[] { null, EventArgs.Empty });
                dBCon.InitializeDatabase();
            }
            dBCon.RegisterUser(SquintUser, ESAPIContext.CurrentUserId(), ESAPIContext.CurrentUserName());
        }

        public string PatientFirstName
        {
            get
            {
                if (ReferenceEquals(null, ESAPIContext.Patient))
                    return string.Empty;
                else
                    return ESAPIContext.Patient.FirstName;
            }
        }
        public string PatientLastName
        {
            get
            {
                if (ReferenceEquals(null, ESAPIContext.Patient))
                    return string.Empty;
                else
                    return ESAPIContext.Patient.LastName;
            }
        }
        public string PatientID
        {
            get
            {
                if (ReferenceEquals(null, ESAPIContext.Patient))
                    return string.Empty;
                else
                    return ESAPIContext.Patient.Id;
            }
        }
        public async Task<AsyncPlan> GetAsyncPlan(string CourseId, string PlanId)
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
        public async Task<AsyncPlan> GetAsyncPlanByUID(string CourseId, string PlanUID)
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
        public List<StructureSetHeader> GetAvailableStructureSets()
        {
            var L = new List<StructureSetHeader>();
            foreach (var p in CurrentSession.PlanAssociations)
            {
                if (p.Linked)
                    L.Add(new StructureSetHeader(p.StructureSetId, p.StructureSetUID, p.PlanId));
            }
            return L;
        }
        public AsyncStructureSet GetStructureSet(string ssuid)
        {
            var ass = ESAPIContext.Patient.GetStructureSet(ssuid);
            if (ass != null)
            {
                return ass;
            }
            else return null;
        }

        public async Task<List<ObjectiveDefinition>> GetOptimizationObjectiveList(string CourseId, string PlanId)
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
        public async Task<double> GetSliceSpacing(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetSliceSpacing();
            else
                return double.NaN;
        }
        public async Task<double?> GetDoseGridResolution(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetDoseGridResolution();
            else
                return null;
        }
        public async Task<FieldNormalizationTypes> GetFieldNormalizationMode(string CourseId, string PlanId)
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
        public async Task<double> GetPNV(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetPNV();
            else return double.NaN;
        }
        public async Task<double?> GetRxDose(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P != null)
                return P.Dose;
            else
            {
                return double.NaN;
            }
        }
        public async Task<int?> GetNumFractions(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return P.NumFractions;
            else
                return null;
        }
        public async Task<double> GetPrescribedPercentage(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetPrescribedPercentage();
            else
                return double.NaN;
        }
        public async Task<string> GetCourseIntent(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            return await P.GetCourseIntent();
        }
        public async Task<string> GetAlgorithmModel(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetAlgorithmModel();
            else
                return "";
        }
        public async Task<string> GetVMATAlgorithmModel(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetVMATAlgorithmModel();
            else
                return "";
        }
        public async Task<string> GetAirCavityCorrectionVMAT(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetVMATAirCavityOption();
            else
                return null;
        }
        public async Task<string> GetIMRTAlgorithmModel(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetIMRTAlgorithmModel();
            else
                return "";
        }
        public async Task<string> GetAirCavityCorrectionIMRT(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetIMRTAirCavityOption();
            else
                return null;
        }
        public async Task<double> GetCouchSurface(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetCouchSurface();
            else
                return double.NaN;
        }

        public double GetAssignedHU(string ESId, string structureSetUID)
        {
            var AS = GetStructureSet(structureSetUID).GetStructure(ESId);
            if (AS != null)
                return AS.HU;
            else
                return double.NaN;
        }
        public async Task<double> GetCouchInterior(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetCouchInterior();
            else return double.NaN;
        }
        public async Task<bool?> GetHeterogeneityOn(string CourseId, string PlanId)
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
        public async Task<string> GetStudyId(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetStudyId();
            else return "";
        }

        public async Task<string> GetCTDeviceId(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetCTDeviceId();
            else return "";
        }

        public async Task<string> GetSeriesId(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetSeriesId();
            else return "";
        }
        public async Task<string> GetSeriesComments(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetSeriesComments();
            else
                return "";
        }
        public async Task<double> GetBolusThickness(string CourseId, string PlanId, string BolusId)
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

        public async Task<string> GetImageComments(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetImageComments();
            else return "";
        }
        public async Task<int?> GetNumSlices(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            if (P.ComponentType == ComponentTypes.Phase)
                return await P.GetNumSlices();
            else return null;
        }
        public async Task<List<TxFieldItem>> GetTxFieldItems(string CourseId, string PlanId)
        {
            AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
            return await P.GetTxFields();
        }
        public async Task<List<ImagingFieldItem>> GetImagingFieldList(string CourseId, string PlanId)
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
        public Dictionary<ImagingProtocolTypes, List<string>> CheckImagingProtocols(ComponentModel CV, List<ImagingFieldItem> IF)
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
        public async Task<NTODefinition> GetNTOObjective(string CourseId, string PlanId)
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
        public List<ProtocolPreview> GetProtocolPreviewList()
        {
            var dbCon = new DbController(this);
            return dbCon.GetProtocolPreviews().Where(x => x.ID > 1).OrderBy(x => x.ProtocolName).ToList();
        }
        public Protocol GetProtocol(int Id)
        {
            var dbCon = new DbController(this);
            return dbCon.GetProtocol(Id);
        }

        public PlanAssociationViewModel GetPlanAssociation(int ComponentID, int AssessmentID)
        {
            PlanAssociationViewModel ECP = CurrentSession.PlanAssociations.FirstOrDefault(x => x.ComponentID == ComponentID && x.AssessmentID == AssessmentID);
            if (ECP != null)
                return ECP;
            else
                return null;
        }

        public List<PlanAssociationViewModel> GetPlanAssociations()
        {
            return CurrentSession.PlanAssociations;
        }
        public void ClearPlanAssociation(int componentId, int assessmentId)
        {
            foreach (var PA in CurrentSession.PlanAssociations)
            {
                PA.UpdateLinkedPlan(null, true);
            }
        }
        public List<AssessmentViewModel> GetAssessmentList()
        {
            return CurrentSession.Assessments.OrderBy(x => x.DisplayOrder).ToList();
        }
        public List<AssessmentPreviewViewModel> GetAssessmentPreviews(string PID)
        {
            //return DataCache.GetAssessmentPreviews(PID);
            return null;

        }
        public readonly object LockConstraint = new object();
        public readonly object LockSimultaneousEvaluation = new object();
        public readonly object LockIDGen = new object();
        public readonly object LockDb = new object();
        private Progress<int> _ProgressBar;
        public Progress<int> ProgressBar
        {
            get { return _ProgressBar; }
            set
            {
                _ProgressBar = value;
            }
        }

        public int NumAssessments
        {
            get { return CurrentSession.Assessments.Count(); }
        }

        public void SetCurrentStructureSet(string ssuid)
        {
            if (ssuid == null)
                CurrentStructureSet = null;
            else
                CurrentStructureSet = GetStructureSet(ssuid);
            CurrentStructureSetChanged?.Invoke(null, EventArgs.Empty);
        }
        public List<string> GetCurrentStructures()
        {
            if (CurrentStructureSet != null)
            {
                var structureIds = CurrentStructureSet.GetStructureIds().ToList();
                structureIds.Add("");
                return structureIds;
            }
            else return new List<string>();
        }
        public List<string> GetAvailableStructureSetIds()
        {
            var SSs = GetAvailableStructureSets();
            return SSs.Select(x => x.StructureSetId).ToList();
        }


        public ConstraintModel AddConstraint(ConstraintTypes TypeCode, int ComponentID = 0, int StructureId = 1)
        {
            if (!ProtocolLoaded)
                return null;
            ComponentModel parentComponent = CurrentProtocol.Components.FirstOrDefault(x => x.Id == ComponentID);
            StructureModel primaryStructure = CurrentProtocol.Structures.FirstOrDefault(x => x.ID == StructureId);
            ConstraintModel Con = new ConstraintModel(this, parentComponent, primaryStructure, null, TypeCode);
            CurrentProtocol.Components.FirstOrDefault(x => x.Id == ComponentID).ConstraintModels.Add(Con);
            return Con;
        }


        public void DeleteConstraint(int Id)
        {
            var AllConstraints = GetAllConstraints();
            var Con = AllConstraints.FirstOrDefault(x => x.ID == Id);
            Con.FlagForDeletion();
            int NewDisplayOrder = 1;
            foreach (ConstraintModel C in CurrentProtocol.Components.FirstOrDefault(x => x.Id == Con.ComponentID).ConstraintModels.Where(x => !x.ToRetire))
            {
                Con.DisplayOrder.Value = NewDisplayOrder++;
            }
        }
        public List<ConstraintModel> GetAllConstraints()
        {
            return CurrentProtocol.Components.SelectMany(x => x.ConstraintModels).ToList();
        }
        public ConstraintModel GetConstraint(int Id)
        {
            return GetAllConstraints().FirstOrDefault(x => x.ID == Id);
        }
        public void DeleteComponent(int Id)
        {
            if (CurrentProtocol.Components.Count() > 1)
            {
                GetComponent(Id).FlagForDeletion();
                //Re-index displayorder
                int NewDisplayOrder = 1;
                foreach (ComponentModel Comp in CurrentProtocol.Components.OrderBy(x => x.DisplayOrder))
                {
                    Comp.DisplayOrder = NewDisplayOrder++;
                }
            }
        }
        public void DeleteStructure(int Id)
        {
            if (CurrentProtocol.Structures.Count() > 1)
            {
                GetProtocolStructure(Id).FlagForDeletion();
                int NewDisplayOrder = 1;
                foreach (StructureModel PS in CurrentProtocol.Structures.OrderBy(x => x.DisplayOrder))
                {
                    PS.DisplayOrder = NewDisplayOrder++;
                }
            }

        }
        public void ShiftConstraintUp(int Id)
        {
            ConstraintModel Con = GetConstraint(Id);
            if (Con != null)
            {
                ConstraintModel ConSwitch = GetAllConstraints().FirstOrDefault(x => x.DisplayOrder.Value == Con.DisplayOrder.Value - 1);
                if (ConSwitch != null)
                {
                    ConSwitch.DisplayOrder = Con.DisplayOrder;
                    Con.DisplayOrder.Value = Con.DisplayOrder.Value - 1;
                }
                ProtocolConstraintOrderChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public void ShiftConstraintDown(int Id)
        {
            ConstraintModel Con = GetConstraint(Id);
            if (Con != null)
            {
                ConstraintModel ConSwitch = GetAllConstraints().FirstOrDefault(x => x.DisplayOrder.Value == Con.DisplayOrder.Value + 1);
                if (ConSwitch != null)
                {
                    ConSwitch.DisplayOrder = Con.DisplayOrder;
                    Con.DisplayOrder.Value = Con.DisplayOrder.Value + 1;
                }
                ProtocolConstraintOrderChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public void DuplicateConstraint(int ConstraintID)
        {
            if (CurrentProtocol == null)
                return;
            ConstraintModel Con2Dup = GetConstraint(ConstraintID);
            foreach (ConstraintModel Con in GetAllConstraints().Where(x => x.DisplayOrder.Value > Con2Dup.DisplayOrder.Value))
            {
                Con.DisplayOrder.Value = Con.DisplayOrder.Value + 1;
            }
            ConstraintModel DupCon = new ConstraintModel(Con2Dup);
            CurrentProtocol.Components.FirstOrDefault(x => x.Id == Con2Dup.ComponentID).ConstraintModels.Add(DupCon);
            ConstraintAdded?.Invoke(null, DupCon.ID);
        }

        public ComponentModel GetComponent(int ID)
        {
            return CurrentProtocol.Components.FirstOrDefault(x => x.Id == ID);
        }
        public ComponentModel AddComponent()
        {
            if (!ProtocolLoaded)
                return null;
            string ComponentName = string.Format("NewPhase{0}", _NewPlanCounter++);
            int DisplayOrder = CurrentProtocol.Components.Count() + 1;
            var newComponent = new ComponentModel(CurrentProtocol.ID, ComponentName, DisplayOrder);
            CurrentProtocol.Components.Add(newComponent);
            foreach (AssessmentViewModel SA in CurrentSession.Assessments)
                CurrentSession.PlanAssociations.Add(new PlanAssociationViewModel(this, newComponent, SA));
            return newComponent;
        }
        public async Task<StructureModel> AddNewStructure()
        {
            if (CurrentProtocol == null)
                return null;
            else
            {
                var dbCon = new DbController(this);
                string NewStructureName = string.Format("{0}{1}", DefaultNewStructureName, _NewStructureCounter++);
                while (CurrentProtocol.Structures.Select(x => x.ProtocolStructureName).Any(x => x.Equals(NewStructureName)))
                {
                    NewStructureName = string.Format("{0}{1}", DefaultNewStructureName, _NewStructureCounter++);
                }
                StructureLabel SL = await dbCon.GetStructureLabel(1);
                StructureModel newProtocolStructure = new StructureModel(ESAPIContext, SL, NewStructureName);
                newProtocolStructure.DisplayOrder = CurrentProtocol.Structures.Count() + 1;
                newProtocolStructure.ProtocolID = CurrentProtocol.ID;
                CurrentProtocol.Structures.Add(newProtocolStructure);
                return newProtocolStructure;
            }
        }

        public async Task<bool> ImportEclipseProtocol(VMS_XML.Protocol P)
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
                var dbCon = new DbController(this);
                foreach (var ECPStructure in P.StructureTemplate.Structures)
                {
                    StructureLabel SL = await dbCon.GetStructureLabel(1);
                    var S = new StructureModel(ESAPIContext, SL, ECPStructure.ID);
                    S.DefaultEclipseAliases.Add(ECPStructure.ID);
                    EclipseProtocol.Structures.Add(S);
                    EclipseStructureIdToSquintStructureIdMapping.Add(ECPStructure.ID.ToLower(), S.ID);
                }
                int ComponentDisplayOrder = 1;
                foreach (var Ph in P.Phases)
                {

                    int numFractions = Convert.ToInt32(Ph.FractionCount);
                    double totalDose = (double)Ph.PlanTemplate.DosePerFraction * numFractions * 100; // convert to cGy;
                    ComponentModel SC = new ComponentModel(EclipseProtocol.ID, Ph.ID, ComponentDisplayOrder++, numFractions, totalDose);
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
                            ConstraintModel Con = new ConstraintModel(SC, primaryStructure, Item);
                            SC.ConstraintModels.Add(Con);
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
                            ConstraintModel Con = new ConstraintModel(SC, primaryStructure, MI);
                            SC.ConstraintModels.Add(Con);
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



        public bool ImportProtocolFromXML(string filename, bool refresh)
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
        public void Dispose()
        {
            if (ESAPIContext != null)
                ESAPIContext.Dispose();
        }
        public bool SaveXMLProtocolToDatabase(SquintProtocol _XMLProtocol)
        {
            List<string> ExistingProtocolNames;
            using (SquintDBModel LocalContext = new SquintDBModel(DbPaths))
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
                        string TxIntentString = TypeDisplay.Display(TxIntent.Id); // necessary as EF can't interpret this inline qith the query
                        DbTreatmentIntent TI = LocalContext.DbTreatmentIntents.FirstOrDefault(x => x.Intent.Equals(TxIntentString, StringComparison.OrdinalIgnoreCase));
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
                    LocalContext.SaveChanges(); // protocol key is now available
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
                        Checklist.AlgorithmResolution = double.IsNaN(_XMLProtocol.ProtocolChecklist.Calculation.AlgorithmResolution) ? Checklist.AlgorithmResolution : _XMLProtocol.ProtocolChecklist.Calculation.AlgorithmResolution;
                        Checklist.AlgorithmVMATOptimization = (int)_XMLProtocol.ProtocolChecklist.Calculation.VMATOptimizationAlgorithm;
                        Checklist.AlgorithmIMRTOptimization = (int)_XMLProtocol.ProtocolChecklist.Calculation.IMRTOptimizationAlgorithm;
                        Checklist.AirCavityCorrectionVMAT = _XMLProtocol.ProtocolChecklist.Calculation.VMATAirCavityCorrection;
                        Checklist.AirCavityCorrectionIMRT = _XMLProtocol.ProtocolChecklist.Calculation.IMRTAirCavityCorrection;
                    }
                    if (_XMLProtocol.ProtocolChecklist.Supports != null)
                    {
                        Checklist.SupportIndication = (int)_XMLProtocol.ProtocolChecklist.Supports.Indication;
                        Checklist.CouchInterior = double.IsNaN(_XMLProtocol.ProtocolChecklist.Supports.CouchInterior) ? Checklist.CouchInterior : _XMLProtocol.ProtocolChecklist.Supports.CouchInterior;
                        Checklist.CouchSurface = double.IsNaN(_XMLProtocol.ProtocolChecklist.Supports.CouchSurface) ? Checklist.CouchSurface : _XMLProtocol.ProtocolChecklist.Supports.CouchSurface;
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
                            DbA.HU = double.IsNaN(A.HU) ? DbA.HU : A.HU;
                            DbA.ToleranceHU = double.IsNaN(A.ToleranceHU) ? DbA.ToleranceHU : A.ToleranceHU;
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
                try
                {
                    LocalContext.SaveChanges(); // protocol key is now available
                }
                catch (Exception ex)
                {
                    MessageBox.Show(P.ProtocolName);
                }
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
                        C.PNVMax = double.IsNaN(comp.Prescription.PNVMax) ? C.PNVMax : comp.Prescription.PNVMax;
                        C.PNVMin = double.IsNaN(comp.Prescription.PNVMin) ? C.PNVMin : comp.Prescription.PNVMin;
                        C.PrescribedPercentage = double.IsNaN(comp.Prescription.PrescribedPercentage) ? C.PrescribedPercentage : comp.Prescription.PrescribedPercentage;
                    }
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
                        DbComponentImaging DbCI = LocalContext.DbComponentImagings.Create();
                        LocalContext.DbComponentImagings.Add(DbCI);
                        DbCI.ID = IDGenerator.GetUniqueId();
                        DbCI.DbComponent = C;
                        foreach (var IP in comp.ImagingProtocols)
                        {
                            DbImaging DbI = LocalContext.DbImagings.Create();
                            DbI.ImagingProtocol = (int)IP.Id;
                            if (string.IsNullOrEmpty(IP.ImagingProtocolDisplayName))
                                DbI.ImagingProtocolName = TypeDisplay.Display(IP.Id);
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
                            C.MinColOffset = double.IsNaN(comp.Beams.MinColOffset) ? C.MinColOffset : comp.Beams.MinColOffset;
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
                                    DbAG.DbBeam = B;
                                    DbAG.GeometryName = G.GeometryName;
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
                            B.CouchRotation = double.IsNaN(b.CouchRotation) ? B.CouchRotation : b.CouchRotation;
                            B.MaxColRotation = double.IsNaN(b.MaxColRotation) ? B.MaxColRotation : b.MaxColRotation; 
                            B.MinColRotation = double.IsNaN(b.MinColRotation) ? B.MinColRotation : b.MinColRotation;
                            B.MaxMUWarning = double.IsNaN(b.MaxMUWarning) ? B.MaxMUWarning : b.MaxMUWarning;
                            B.MinMUWarning = double.IsNaN(b.MinMUWarning) ? B.MinMUWarning : b.MinMUWarning;
                            B.MinX = double.IsNaN(b.MinX) ? B.MinX : b.MinX;
                            B.MaxX = double.IsNaN(b.MaxX) ? B.MaxX : b.MaxX;
                            B.MinY = double.IsNaN(b.MinY) ? B.MinY : b.MinY;
                            B.MaxY = double.IsNaN(b.MaxY) ? B.MaxY : b.MaxY;
                            B.ToleranceTable = b.ToleranceTable;
                            bool UnsetEnergyAdded = false;
                            foreach (var E in b.ValidEnergies)
                            {
                                string EnergyString = TypeDisplay.Display(E.Mode);
                                DbEnergy DbEn = LocalContext.DbEnergies.FirstOrDefault(x => x.EnergyString == EnergyString);
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
                try
                {
                    LocalContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0} {1} {2}", ex.Message, ex.InnerException, ex.StackTrace));
                }

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
                        DbCon.MajorViolation = double.IsNaN(con.MajorViolation) ? DbCon.MajorViolation : con.MajorViolation;
                        DbCon.MinorViolation = double.IsNaN(con.MinorViolation) ? DbCon.MinorViolation : con.MinorViolation;
                        DbCon.Stop = double.IsNaN(con.Stop) ? DbCon.Stop : con.Stop;
                    }
                    DbConstraintChangelog DbCC = LocalContext.DbConstraintChangelogs.Create();

                    LocalContext.DbConstraintChangelogs.Add(DbCC);
                    DbCC.ChangeDescription = con.Description;
                    DbCC.ConstraintString = "";
                    DbCC.ChangeAuthor = SquintUser;
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

        public bool ExportProtocolAsXML(string filename)
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
                var TreatmentIntent = new SquintProtocolIntent() { Id = TI };
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
                    ProtocolStructureName = GetProtocolStructure(A.ProtocolStructureId.Value).ProtocolStructureName,
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
                foreach (var Con in C.ConstraintModels.OrderBy(x => x.DisplayOrder))
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
        public async Task<bool> LoadProtocolFromDb(string ProtocolName, IProgress<int> progress = null) // this method reloads the context.
        {
            try
            {
                StartNewSession();
                var dbCon = new DbController(this);
                CurrentSession.SessionProtocol = await dbCon.LoadProtocol(ProtocolName);
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

        public async Task Save_UpdateProtocol()
        {
            var dbCon = new DbController(this);
            CurrentProtocol.LastModifiedBy = SquintUser;
            await dbCon.Save_UpdateProtocol();
            string ProtocolNameToReload = CurrentProtocol.ProtocolName;
            await LoadProtocolFromDb(ProtocolNameToReload);
            _uiDispatcher.Invoke(ProtocolUpdated, new[] { null, EventArgs.Empty });
            ProtocolListUpdated?.Invoke(null, EventArgs.Empty);
        }
        public void Save_DuplicateProtocol()
        {
            var dbCon = new DbController(this);
            dbCon.Save_DuplicateProtocol();
            ProtocolListUpdated?.Invoke(null, EventArgs.Empty);
        }
        public void DeleteProtocol(int Id)
        {
            var dbCon = new DbController(this);
            bool Deleted = dbCon.Delete_Protocol(Id);
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
        public async Task<bool> Save_Session(string SessionComment)
        {
            if (PatientOpen & ProtocolLoaded)
            {
                try
                {
                    var dbCon = new DbController(this);
                    bool Success = await dbCon.Save_Session(SessionComment);
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
        public void Delete_Session(int ID)
        {
            var dbCon = new DbController(this);
            dbCon.Delete_Session(ID);
            SessionsChanged?.Invoke(null, EventArgs.Empty);
        }
        public async Task<bool> Load_Session(int ID)
        {
            if (PatientOpen)
            {
                var dbCon = new DbController(this);
                if (ProtocolLoaded)
                {
                    CloseProtocol();
                    StartNewSession();
                }
                Session LoadedSession = await dbCon.Load_Session(ID);
                if (LoadedSession != null)  // true if successful load
                {
                    CurrentSession = LoadedSession;
                    ProtocolLoaded = true;
                    AvailableStructureSetsChanged?.Invoke(null, EventArgs.Empty);
                    CurrentStructureSetChanged?.Invoke(null, EventArgs.Empty);
                    _uiDispatcher.Invoke(ProtocolOpened, new object[] { null, EventArgs.Empty });
                    foreach (StructureModel PS in CurrentSession.SessionProtocol.Structures)
                    {
                        UpdateConstraintThresholds(PS);
                    }
                }
                else
                    ProtocolLoaded = false;
                return true;
            }
            else return false;
        }
        public async void SynchronizePlans()
        {
            await Task.Run(() => SynchronizePlansInternal());
            SynchronizationComplete?.Invoke(null, EventArgs.Empty);
        }
        public void RefreshPatient()
        {
            if (PatientOpen)
            {
                string PID = PatientID;
                if (ESAPIContext != null)
                    ESAPIContext.ClosePatient();
                ESAPIContext.OpenPatient(PID);
            }
        }
        private async Task SynchronizePlansInternal()
        {
            if (ESAPIContext.Patient != null)
            {
                string PID = ESAPIContext.Patient.Id;
                RefreshPatient();
                // Disable autorefresh (see below for reasons)
                try
                {
                    foreach (PlanAssociationViewModel ECP in CurrentSession.PlanAssociations)
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

        public void CloseProtocol()
        {
            CurrentSession.SessionProtocol = null;
            ProtocolLoaded = false;
            ProtocolClosed?.Invoke(null, EventArgs.Empty);
        }
        public void StartNewSession()
        {
            CurrentSession = new Session();
            SetCurrentStructureSet(null);
            _AssessmentNameIterator = 1;
        }

        public void ClosePatient()
        {
            CurrentSession.PlanAssociations.Clear();
            CurrentStructureSet = null;
            ESAPIContext.ClosePatient();
            PatientOpen = false;
            PatientClosed?.Invoke(null, EventArgs.Empty);
        }
        public async Task<bool> OpenPatient(string PID)
        {
            PatientOpen = await Task.Run(() => ESAPIContext.OpenPatient(PID));
            if (PatientOpen)
                PatientOpened?.Invoke(null, EventArgs.Empty);
            SessionsChanged?.Invoke(null, EventArgs.Empty);
            return PatientOpen;
            //_uiDispatcher.Invoke(PatientOpened, new[] { null, EventArgs.Empty });
        }
        public void ChangeConstraintComponent(int ConId, int CompId)
        {
            ConstraintModel Con = GetAllConstraints().FirstOrDefault(x => x.ID == ConId);
            CurrentProtocol.Components.FirstOrDefault(x => x.Id == Con.ComponentID).ConstraintModels.Remove(Con);
            ComponentModel newComponent = CurrentProtocol.Components.FirstOrDefault(x => x.Id == CompId);
            newComponent.ConstraintModels.Add(Con);
            Con.parentComponent = newComponent;
        }

        public async void UpdateConstraint(int ConId)
        {
            ConstraintModel Con = GetAllConstraints().FirstOrDefault(x => x.ID == ConId);
            foreach (var PA in CurrentSession.PlanAssociations.Where(x => x.ComponentID == Con.ComponentID))
            {
                await Con.EvaluateConstraint(PA);
            }
        }
        public async void UpdateConstraints()
        {
            foreach (var PA in CurrentSession.PlanAssociations)
            {
                if (PA.Linked)
                {
                    foreach (ConstraintModel Con in CurrentProtocol.Components.First(x => x.Id == PA.ComponentID).ConstraintModels)
                        await Con.EvaluateConstraint(PA);
                }
            }
        }

        public async Task<bool> UpdateConstraints(int CompId, int? AssessmentId)
        {

            var PAs = CurrentSession.PlanAssociations.Where(x => x.ComponentID == CompId);
            if (AssessmentId != null)
                PAs = PAs.Where(x => x.AssessmentID == AssessmentId);
            foreach (var PA in PAs.ToList())
            {
                if (PA.Linked)
                {
                    foreach (ConstraintModel Con in CurrentProtocol.Components.First(x => x.Id == PA.ComponentID).ConstraintModels)
                        await Con.EvaluateConstraint(PA);
                }
            }
            return true;
        }

        public async void UpdateConstraints(StructureModel PS)
        {

            foreach (var PA in CurrentSession.PlanAssociations)
            {
                if (PA.Linked)
                {
                    foreach (ConstraintModel Con in CurrentProtocol.Components.First(x => x.Id == PA.ComponentID).ConstraintModels.Where(x => x.PrimaryStructureId == PS.ID || x.ReferenceStructureId == PS.ID))
                        await Con.EvaluateConstraint(PA);
                }
            }
        }

        public void UpdateConstraintThresholds(StructureModel E) // update interpolated thresholds based on change to ProtocolStructureId
        {
            foreach (ConstraintModel Con in GetAllConstraints().Where(x => x.ReferenceStructureId == E.ID))
            {
                string SSUID = CurrentStructureSet.UID;
                if (!string.IsNullOrEmpty(SSUID))
                    Con.UpdateThresholds(E.Volume(SSUID));
            }
        }
        public bool MatchStructuresByAlias()
        {
            foreach (StructureModel E in CurrentProtocol.Structures) // notify structures
            {
                E.ApplyAliasing(CurrentStructureSet);
                UpdateConstraintThresholds(E);
            }
            return true;
        }

        public StructureModel GetProtocolStructure(int Id)
        {
            return CurrentProtocol.Structures.FirstOrDefault(x => x.ID == Id);
        }

        public async Task<StructureLabel> GetStructureLabel(int? Id = null)
        {
            var dbCon = new DbController(this);
            if (Id == null)
                return await dbCon.GetStructureLabel(1);
            else
                return await dbCon.GetStructureLabel((int)Id);
        }

        public async Task<string> GetStructureCode(int? Id = null)
        {
            var dbCon = new DbController(this);
            if (Id == null)
                return await dbCon.GetStructureCode(1);
            else
                return await dbCon.GetStructureCode((int)Id);
        }
        public IEnumerable<StructureLabel> GetStructureLabels()
        {
            var dbCon = new DbController(this);
            return dbCon.GetAllStructureLabels();
        }

        public async Task<List<ComponentStatusCodes>> AssociatePlanToComponent(int AssessmentID, int ComponentID, string CourseId, string PlanId, ComponentTypes Type, bool ClearWarnings) // ClearWarnings
        {
            try
            {
                StartingLongProcess?.Invoke(null, EventArgs.Empty);
                AsyncPlan P = await GetAsyncPlan(CourseId, PlanId);
                return await Task.Run(() =>
                {
                    PlanAssociationViewModel PA = CurrentSession.PlanAssociations.FirstOrDefault(x => x.AssessmentID == AssessmentID && x.ComponentID == ComponentID);
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
                    EndingLongProcess?.Invoke(null, EventArgs.Empty);
                    return PA.GetErrorCodes();
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2})", ex.Message, ex.InnerException, ex.StackTrace));
                return null;
            }
        }
        public List<SessionView> GetSessionViews()
        {
            List<SessionView> SVs = new List<SessionView>();
            if (ESAPIContext != null)
            {
                var dbCon = new DbController(this);
                foreach (Session S in dbCon.GetSessions())
                {
                    SVs.Add(new SessionView(S));
                }
            }
            return SVs;
        }
        public AssessmentViewModel NewAssessment()
        {
            AssessmentViewModel NewAssessment = new AssessmentViewModel(CurrentSession.ID, string.Format("Assessment#{0}", _AssessmentNameIterator), _AssessmentNameIterator, this);
            CurrentSession.Assessments.Add(NewAssessment);
            foreach (ComponentModel SC in CurrentProtocol.Components)
            {
                CurrentSession.PlanAssociations.Add(new PlanAssociationViewModel(this, SC, NewAssessment));
            }
            _AssessmentNameIterator++;
            return NewAssessment;
        }
        public void RemoveAssessment(int AssessmentID)
        {
            foreach (PlanAssociationViewModel PL in CurrentSession.PlanAssociations.Where(x => x.AssessmentID == AssessmentID).ToList())
            {
                CurrentSession.PlanAssociations.Remove(PL);
            }
            CurrentSession.Assessments.Remove(CurrentSession.Assessments.FirstOrDefault(x => x.ID == AssessmentID));
        }

        public async Task<List<PlanDescriptor>> GetPlanDescriptors(string CourseId)
        {
            AsyncCourse C = await ESAPIContext.Patient.GetCourse(CourseId);
            if (C != null)
            {
                return C.PlanDescriptors;
            }
            else return null;
        }
        public List<string> GetCourseNames()
        {
            return ESAPIContext.Patient.CourseIds;
        }

        public void AddNewContourCheck(StructureModel S)
        {
            S.CheckList.isPointContourChecked.Value = true;
            S.CheckList.PointContourVolumeThreshold.Value = 0.05;
        }
        public void RemoveNewContourCheck(StructureModel S)
        {
            S.CheckList.isPointContourChecked.Value = false;
            S.CheckList.PointContourVolumeThreshold.Value = 0;
        }
        public void AddNewBeamCheck(int ComponentId)
        {
            CurrentProtocol.Components.FirstOrDefault(x => x.Id == ComponentId).Beams.Add(new BeamViewModel(ComponentId, this));
        }

        public void AddStructureAlias(int StructureId, string NewAlias)
        {
            GetProtocolStructure(StructureId).DefaultEclipseAliases.Add(NewAlias);
        }
        public void RemoveStructureAlias(int StructureId, string AliasToRemove)
        {
            GetProtocolStructure(StructureId).DefaultEclipseAliases.Remove(AliasToRemove);
        }

        public List<BeamGeometryDefinition> GetBeamGeometryDefinitions()
        {
            return new List<BeamGeometryDefinition>(_BeamGeometryDefinitions);
        }


    }
}
