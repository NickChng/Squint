using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SquintScript
{
    partial class Ctr
    {
        public static class DataCache
        {
            public static class SquintDb
            {
                private static SquintdBModel _Context = new SquintdBModel();
                public static SquintdBModel Context
                {
                    get { return _Context; }
                }
                public static void Reload()
                {
                    _Context = new SquintdBModel();
                }
                public static List<string> GetProtocolNameByProperty(ProtocolTypes ProtocolType = ProtocolTypes.All, string ProtocolAuthor = null, TreatmentCentres TreatmentCentre = TreatmentCentres.All)
                {
                    List<string> ProtocolNames;
                    using (var DbContext = SquintDb.Context)
                    {
                        IQueryable<DbProtocol> ProtocolFilter = DbContext.DbLibraryProtocols;
                        if (ProtocolType != ProtocolTypes.All)
                            ProtocolFilter = ProtocolFilter.Where(x => x.DbProtocolType.ProtocolType == (int)ProtocolType);
                        if (ProtocolAuthor != null)
                            ProtocolFilter = ProtocolFilter.Where(x => x.DbUser_ProtocolAuthor.ARIA_ID == ProtocolAuthor);
                        if (TreatmentCentre != TreatmentCentres.All)
                            ProtocolFilter = ProtocolFilter.Where(x => x.DbTreatmentCentre.TreatmentCentre == (int)TreatmentCentre);
                        return ProtocolNames = ProtocolFilter.Select(x => x.ProtocolName).ToList();
                    }
                }
            }

            //Events
            public static event EventHandler<int> ConstraintDeleted;
            public static event EventHandler<int> ComponentDeleted;
            public static event EventHandler<int> PlanDeleted;
            public static event EventHandler<int> ProtocolStructureDeleted;

            public static Session CurrentSession { get; private set; }
            public static Protocol CurrentProtocol { get; private set; }
            public static AsyncPatient Patient { get; private set; }
            private static Dictionary<int, Beam> _Beams = new Dictionary<int, Beam>(); // lookup Beam by its key
            private static Dictionary<int, Constraint> _Constraints = new Dictionary<int, Constraint>(); // lookup constraint by its key
            private static Dictionary<int, AsyncPlan> _AsyncPlans = new Dictionary<int, AsyncPlan>();
            private static Dictionary<int, PlanAssociation> _Plans = new Dictionary<int, PlanAssociation>();
            private static Dictionary<int, Component> _Components = new Dictionary<int, Component>(); // lookup component by key
            private static Dictionary<int, Assessment> _Assessments = new Dictionary<int, Assessment>();
            //private static Dictionary<int, Constituent> _Constituents = new Dictionary<int, Constituent>();
            private static Dictionary<int, AsyncStructure> _AsyncStructures = new Dictionary<int, AsyncStructure>();
            private static Dictionary<string, AsyncCourse> _Courses = new Dictionary<string, AsyncCourse>();
            //private static Dictionary<int, object> _EclipsePlans = new Dictionary<int, object>(); // may be PlanSetup or PlanSum
            private static Dictionary<string, int> _CourseIDbyName = new Dictionary<string, int>();
            private static Dictionary<int, ProtocolStructure> _ProtocolStructures = new Dictionary<int, ProtocolStructure>();
            private static Dictionary<string, bool> _isCourseLoaded = new Dictionary<string, bool>(); // check to see whether course has been loaded by EclipseID/name
            public static Dictionary<int, int> CourseLookupFromPlan = new Dictionary<int, int>(); // lookup course PK by planPK
            public static Dictionary<int, int> CourseLookupFromSum = new Dictionary<int, int>(); // lookup course PK by plansumPK
            private static Dictionary<int, StructureLabel> _StructureLabels = new Dictionary<int, StructureLabel>();
            private static List<AsyncStructureSet> _asyncStructureSets = new List<AsyncStructureSet>();
            //private static List<AssignedId> AssignedIds = new List<AssignedId>();
            private static bool areStructuresLoaded = false;
            private static readonly object CourseLoadLock = new object();


            public static void RegisterUser(string UserId)
            {
                using (var Context = new SquintdBModel())
                {
                    var User = Context.DbUsers.FirstOrDefault(x => x.ARIA_ID == UserId);
                    if (User == null)
                    {
                        // Add new user;
                        lock (LockDb)
                        {
                            User = Context.DbUsers.Create();
                            Context.DbUsers.Add(User);
                            User.ID = IDGenerator();
                            User.PermissionGroupID = 1;
                            User.FirstName = A.CurrentUserName();
                            User.ARIA_ID = A.CurrentUserId();
                            Context.SaveChanges();
                        }
                    }
                }
            }
            public static void OpenPatient(string PID)
            {
                Patient = A.OpenPatient(PID);
            }
            public static void ClosePatient()
            {
                ClearAssessments(); // method notifies that DataCache.Assessments have been cleared
                ClearPlans();
                ClearEclipseData();
                if (A != null)
                    A.ClosePatient();
            }
            public static void RefreshPatient()
            {
                if (PatientLoaded)
                {
                    ClearEclipseData();

                    if (A != null)
                        A.ClosePatient();
                    Patient = A.OpenPatient(PatientID);
                }
            }
            public static void CreateSession()
            {
                CurrentSession = new Session();
            }
            public static void SetSession(Session S)
            {
                CurrentSession = S;
            }
            public static void SetProtocol(Protocol P)
            {
                CurrentProtocol = P;
            }
            public static void AddPlan(PlanAssociation P)
            {
                _Plans.Add(P.ID, P);
                // P.PlanDeleting += OnPlanDeleting;
            }
            public static void OnPlanDeleting(object sender, int ID)
            {
                //  _Plans[ID].PlanDeleting -= OnPlanDeleting;
                _Plans.Remove(ID);
            }
            public static void DeletePlan(int ID)
            {
                _Plans.Remove(ID);
            }
            public static PlanAssociation GetPlanAssociation(int ID)
            {
                return _Plans[ID];
            }

            public static IEnumerable<PlanAssociation> GetPlanAssociations()
            {
                return _Plans.Values;
            }
            public static void ClearPlans()
            {
                _Plans.Clear();
            }
            public static void ClearPlan(PlanAssociation ECP)
            {
                _Plans.Remove(ECP.ID);
            }
            public static List<Beam> GetBeams(int CompID)
            {
                return new List<Beam>(_Beams.Values.Where(x => x.ComponentID == CompID));
            }
            public static void AddBeam(Beam B)
            {
                _Beams.Add(B.ID, B);
                B.BeamDeleted += OnBeamDeleted;
            }
            public static void OnBeamDeleted(object sender, int ID)
            {
                _Beams[ID].BeamDeleted -= OnBeamDeleted;
                _Beams.Remove(ID);
            }
            public static void AddConstraint(Constraint C)
            {
                _Constraints.Add(C.ID, C);
            }
            public static Constraint GetConstraint(int ID)
            {
                if (_Constraints.ContainsKey(ID))
                    return _Constraints[ID];
                else
                {
                    using (var Context = new SquintdBModel())
                    {
                        var C = new Constraint(Context.DbConstraints.Find(ID));
                        AddConstraint(C);
                        return C;
                    }
                }
            }
            public static List<ConstraintChangelog> GetConstraintChangelogs(int ID)
            {
                var L = new List<ConstraintChangelog>();
                using (var Context = new SquintdBModel())
                {
                    foreach (DbConstraintChangelog DbCL in Context.DbConstraints.Find(ID).DbConstraintChangelogs.OrderByDescending(x => x.Date))
                    {
                        L.Add(new ConstraintChangelog(DbCL));
                    }
                }
                return L;
            }
            public static IEnumerable<Constraint> GetConstraintsInComponent(int CompID)
            {
                return _Constraints.Values.Where(x => x.ComponentID == CompID);
            }
            public static IEnumerable<Constraint> GetAllConstraints()
            {
                return _Constraints.Values;
            }

            public static void AddComponent(Component C)
            {
                _Components.Add(C.ID, C);
            }
            public static Component GetComponent(int ID)
            {
                if (_Components.ContainsKey(ID))
                    return _Components[ID];
                else
                    return null;
            }
            public static IEnumerable<Component> GetAllComponents()
            {
                return _Components.Values.OrderBy(x => x.DisplayOrder);
            }

            //public static void AddConstraintThreshold(ConstraintThreshold C)
            //{
            //    _ConstraintThresholds.Add(C.ID, C);
            //    if (C.ConstraintID == 493)
            //    {
            //        var debugme = "hi";
            //    }
            //}
            //public static void AddConstraintThreshold(ReferenceThresholdTypes Name, ConstraintThresholdTypes Goal, Constraint Con, double value)
            //{
            //    ConstraintThreshold CT = new ConstraintThreshold(Name, Goal, Con, value);
            //    _ConstraintThresholds.Add(CT.ID, CT);
            //    if (Con.ID == 493)
            //    {
            //        var debugme = "hi";
            //    }
            //}

            //public static ConstraintThreshold LoadConstraintThreshold(DbConThreshold DbCT, int ConId)
            //{
            //    ConstraintThreshold CT = new ConstraintThreshold(DbCT, _Constraints[ConId]);
            //    AddConstraintThreshold(CT);
            //    return CT;
            //}
            //public static void DeleteConstraintThreshold(ConstraintThreshold C)
            //{
            //    _ConstraintThresholds.Remove(C.ID);
            //}
            //public static void DeleteConstraintThreshold(int ID)
            //{
            //    _ConstraintThresholds.Remove(ID);
            //}
            //public static ConstraintThreshold GetConstraintThreshold(int ID)
            //{
            //    return _ConstraintThresholds[ID];
            //}
            //public static IEnumerable<ConstraintThreshold> GetConstraintThresholdByConstraintId(int ConID)
            //{
            //    return _ConstraintThresholds.Values.Where(x => x.ConstraintID == ConID);
            //}
            //public static IEnumerable<ConstraintThreshold> GetAllConstraintThresholds()
            //{
            //    return _ConstraintThresholds.Values;
            //}

            //public static void AddConstituent(Constituent C)
            //{
            //    _Constituents.Add(C.ID, C);
            //    C.ConstituentDeleting += OnConstituentDeleting;
            //}
            //public static Constituent DuplicateConstituent(int DupId)
            //{
            //    Constituent SCC = _Constituents[DupId];
            //    Component DupConstituentComponent = DuplicateComponent(SCC.ConstituentCompID); // duplicate the constituent component
            //    AddComponent(DupConstituentComponent);
            //    Constituent DupSCC = new Constituent(SCC.ComponentID, DupConstituentComponent.ID);
            //    AddConstituent(DupSCC);
            //    return DupSCC;
            //}
            //public static void DeleteConstraintThreshold(Constituent C)
            //{
            //    _Constituents.Remove(C.ID);
            //}
            //public static void OnConstituentDeleting(object sender, int ID)
            //{
            //    _Constituents[ID].ConstituentDeleting -= OnConstituentDeleting;
            //    _Constituents.Remove(ID);
            //}
            //public static Constituent GetConstituent(int ID)
            //{
            //    return _Constituents[ID];
            //}
            //public static IEnumerable<Constituent> GetConstituentsByComponentID(int CompID)
            //{
            //    return _Constituents.Values.Where(x => x.ComponentID == CompID);
            //}
            //public static IEnumerable<Constituent> GetAllConstituents()

            public static void AddProtocolStructure(ProtocolStructure E)
            {
                if (!_ProtocolStructures.ContainsKey(E.ID))
                {
                    _ProtocolStructures.Add(E.ID, E);
                    E.ProtocolStructureDeleting += OnProtocolStructureDeleting;
                }
            }
            public static void OnProtocolStructureDeleting(object sender, int ID)
            {
                _ProtocolStructures[ID].ProtocolStructureDeleting -= OnProtocolStructureDeleting;
                _ProtocolStructures.Remove(ID);

            }
            public static ProtocolStructure GetProtocolStructure(int ID)
            {
                if (!_ProtocolStructures.ContainsKey(ID))
                {
                    DbProtocolStructure DbE = SquintDb.Context.DbProtocolStructures.Find(ID);
                    if (DbE == null)
                    {
                        throw new Exception("Can't find key in GetProtocolStructure");
                    }
                    else
                    {
                        AddProtocolStructure(new ProtocolStructure(DbE));
                    }
                }
                return _ProtocolStructures[ID];
            }
            public static IEnumerable<ProtocolStructure> GetAllProtocolStructures()
            {
                return _ProtocolStructures.Values;
            }
            public static List<StructureSetHeader> GetAvailableStructureSets()
            {
                var L = new List<StructureSetHeader>();
                foreach (var p in _Plans.Values)
                {
                    if (p.Linked)
                        L.Add(new StructureSetHeader(p.StructureSetId, p.StructureSetUID, p.PlanId));
                }
                return L;
            }
            public static AsyncStructureSet GetStructureSet(string ssuid)
            {
                var ass = _asyncStructureSets.FirstOrDefault(x => x.UID == ssuid);
                if (ass != null)
                    return ass;
                else
                {
                    ass = Patient.GetStructureSet(ssuid);
                    if (ass != null)
                    {
                        _asyncStructureSets.Add(ass);
                        return ass;
                    }
                    else return null;
                }
            }

            public static bool TestDbConnection()
            {
                using (SquintdBModel Context = new SquintdBModel())
                {
                    if (Context.Database.Exists())
                        return true;
                    else return false;
                }
            }

            private static void LoadStructures()
            {
                _StructureLabels.Clear();
                foreach (DbStructureLabel DbSL in SquintDb.Context.DbStructureLabels)
                    _StructureLabels.Add(DbSL.ID, new StructureLabel(DbSL));
            }

            public static StructureLabel GetStructureLabel(int Id)
            {
                if (!areStructuresLoaded)
                {
                    LoadStructures();
                    areStructuresLoaded = true;
                }
                if (_StructureLabels.ContainsKey(Id))
                    return _StructureLabels[Id];
                else
                {
                    MessageBox.Show(string.Format("Structure Label {0} not found in dictionary, setting default...", Id));
                    return _StructureLabels[1];
                }
            }
            public static string GetStructureCode(int Id)
            {
                if (!areStructuresLoaded)
                {
                    LoadStructures();
                    areStructuresLoaded = true;
                }
                return _StructureLabels[Id].Code;
            }
            public static IEnumerable<StructureLabel> GetAllStructureLabels()
            {
                return _StructureLabels.Values;
            }
            public static string GetLabelByCode(string Code)
            {
                var Label = _StructureLabels.Values.FirstOrDefault(x => x.Code == Code);
                if (Label != null)
                    return Label.LabelName;
                else
                    return "Label not found";
            }
            public static void AddAssessment(Assessment A)
            {
                _Assessments.Add(A.ID, A);
            }

            public static void ClearAssessment(int ID)
            {
                _Assessments.Remove(ID);
            }
            public static void ClearAssessments()
            {
                _Assessments.Clear();
                _AssessmentNameIterator = 1;
            }
            public static Assessment GetAssessment(int ID)
            {
                if (_Assessments.ContainsKey(ID))
                    return _Assessments[ID];
                else
                {
                    throw new Exception("Attempt to get assessment that doesn't exist in cache (GetAssessment)");
                }
            }
            public static IEnumerable<Assessment> GetAllAssessments()
            {
                return _Assessments.Values.OrderBy(x => x.DisplayOrder);
            }


            public static void AddAsyncPlan(AsyncPlan P)
            {
                if (!_AsyncPlans.ContainsKey(P.HashId))
                    _AsyncPlans.Add(P.HashId, P);
            }
            public static bool isPlanLoaded(string CourseName, string PlanId)
            {
                AsyncPlan p = _AsyncPlans.Values.Where(x => x.Course.Id == CourseName && x.Id == PlanId).SingleOrDefault();
                if (p == null)
                    return false;
                else
                    return true;
            }
            //public static AsyncPlan GetAsyncPlan(int ID)
            //{
            //    return _AsyncPlans[ID];
            //}
            public async static Task<AsyncPlan> GetAsyncPlan(string PlanId, string CourseName, ComponentTypes PlanType)
            {
                if (!_Courses.ContainsKey(CourseName))
                    LoadCourse(CourseName);
                AsyncPlan AP = _AsyncPlans.Values.FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseName && x.ComponentType == PlanType);
                if (AP == null)
                {
                    AP = await _Courses[CourseName].GetPlan(PlanId);
                    if (AP != null)
                        AddAsyncPlan(AP);
                    return AP;
                }
                else
                    return AP;
                //return _AsyncPlans.Values.FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseName && x.PlanType == PlanType);
            }
            public async static Task<AsyncPlan> GetAsyncPlan(string PlanUID, string CourseName)
            {
                if (!_Courses.ContainsKey(CourseName))
                    LoadCourse(CourseName);
                AsyncPlan AP = _AsyncPlans.Values.FirstOrDefault(x => x.UID == PlanUID);
                if (AP == null)
                {
                    AP = await _Courses[CourseName].GetPlanByUID(PlanUID);
                    if (AP != null)
                        AddAsyncPlan(AP);
                    return AP;
                }
                else
                    return AP;
                //return _AsyncPlans.Values.FirstOrDefault(x => x.Id == PlanId && x.Course.Id == CourseName && x.PlanType == PlanType);
            }
            private static async void LoadCourse(string CourseName)
            {
                var C = await GetCourse(CourseName);
            }

            //public static IEnumerable<AsyncPlan> GetAsyncPlans()
            //{
            //    return _AsyncPlans.Values;
            //}
            public static async Task<List<AsyncPlan>> GetPlansByCourseName(string CourseName, IProgress<int> progress = null)
            {
                if (!_isCourseLoaded.ContainsKey(CourseName))
                {
                    AsyncCourse C = await GetCourseAsync(CourseName, progress);
                    if (!_isCourseLoaded.ContainsKey(CourseName)) // need to test this in case concurrent threads are awaiting GetCourseAync for the same course.
                        _isCourseLoaded.Add(CourseName, true);
                }
                return _AsyncPlans.Values.Where(x => x.Course.Id == CourseName).ToList();
            }
            public async static Task<List<PlanDescriptor>> GetPlanDescriptors(string CourseName)
            {
                if (!_Courses.ContainsKey(CourseName))
                    await GetCourse(CourseName);
                return _Courses[CourseName].PlanDescriptors;

            }
            public static void ClearAsyncPlans()
            {
                _AsyncPlans.Clear();
            }

            public static void DeleteCourse(string Id)
            {
                _Courses.Remove(Id);
            }
            public static async Task<AsyncCourse> GetCourse(string Id)
            {
                if (_Courses.ContainsKey(Id))
                    return _Courses[Id];
                else
                {
                    var C = await Patient.GetCourse(Id, null);
                    if (C == null)
                    {
                        return null; // no such course
                    }
                    _Courses.Add(C.Id, C);
                    //foreach (AsyncPlan P in C.Plans)
                    //{
                    //    AddAsyncPlan(P);
                    //}
                    return C;
                }
            }
            public async static Task<AsyncCourse> GetCourseAsync(string CourseName, IProgress<int> progress = null, bool refresh = false) // returns new courseID
            {
                if (_Courses.ContainsKey(CourseName))
                    return _Courses[CourseName];
                else
                {
                    var C = await Patient.GetCourse(CourseName, progress);
                    if (C == null)
                    {
                        return null; // no such course
                    }
                    lock (CourseLoadLock)
                    {
                        if (!_Courses.ContainsKey(CourseName))
                        {
                            _Courses.Add(C.Id, C);
                            //foreach (AsyncPlan P in C.Plans)
                            //{
                            //    AddAsyncPlan(P);
                            //}
                        }
                        else
                            return _Courses[CourseName];
                    }
                    return C;
                }
            }
            public static void ClearEclipseData()
            {
                _isCourseLoaded.Clear();
                _Courses.Clear();
                _AsyncPlans.Clear();
                _asyncStructureSets.Clear();

            }

            public static List<ProtocolPreview> GetProtocolPreviews()
            {
                List<ProtocolPreview> previewlist = new List<ProtocolPreview>();
                using (var Context = new SquintdBModel())
                {
                    if (Context.DbLibraryProtocols != null)
                    {
                        List<DbProtocol> Protocols = Context.DbLibraryProtocols.Where(x => !x.isRetired).ToList();
                        foreach (DbProtocol DbP in Protocols)
                        {
                            previewlist.Add(new ProtocolPreview(DbP.ID, DbP.ProtocolName)
                            {
                                TreatmentCentre = (TreatmentCentres)DbP.DbTreatmentCentre.TreatmentCentre,
                                TreatmentSite = (TreatmentSites)DbP.DbTreatmentSite.TreatmentSite,
                                LastModifiedBy = DbP.LastModifiedBy,
                                Approval = (ApprovalLevels)DbP.DbApprovalLevel.ApprovalLevel
                            });
                        }
                    }
                }
                return previewlist;
            }
            public static Protocol GetProtocol(int Id)
            {
                using (var Context = new SquintdBModel())
                {
                    if (Context.DbLibraryProtocols != null)
                    {
                        DbProtocol DbP = Context.DbLibraryProtocols.FirstOrDefault(x => x.ID == Id);
                        if (DbP != null)
                        {
                            return new Protocol(DbP);
                        }
                    }
                }
                return null;
            }

            public static void CreateNewProtocol()
            {
                CurrentProtocol = new Protocol();
            }
            public static void CloseProtocol()
            {
                ClearProtocolData();
                ProtocolLoaded = false;
            }
            public static Protocol LoadProtocol(string ProtocolName)
            {
                using (SquintdBModel Context = new SquintdBModel())
                {
                    if (ProtocolLoaded)
                    {
                        ClearProtocolData();
                        ProtocolLoaded = false;
                    }
                    DbProtocol DbP = Context.DbLibraryProtocols
                        .Include(x => x.ProtocolStructures)
                        .Include(x => x.Components)
                        .Where(x => x.ProtocolName == ProtocolName && !x.isRetired).SingleOrDefault();
                    if (DbP == null)
                    {
                        ProtocolLoaded = false;
                        return null;
                    }
                    try
                    {
                        CurrentProtocol = new Protocol(DbP);
                        CurrentSession.AddProtocol(CurrentProtocol);
                        bool AtLeastOneStructure = false;
                        var test = _ProtocolStructures;
                        foreach (DbProtocolStructure DbProtocolStructure in DbP.ProtocolStructures)
                        {
                            ProtocolStructure E = new ProtocolStructure(DbProtocolStructure);
                            AddProtocolStructure(E);
                            AtLeastOneStructure = true;
                        }
                        if (!AtLeastOneStructure)
                            new ProtocolStructure(Context.DbProtocolStructures.Find(1));  //Initialize non-defined structure
                        foreach (DbComponent DbC in DbP.Components)
                        {
                            Component SC = new Component(DbC);
                            AddComponent(SC);
                            foreach (DbComponentImaging DbCI in DbC.ImagingProtocols)
                            {
                                foreach (DbImaging I in DbCI.Imaging)
                                {
                                    SC.ImagingProtocolsAttached.Add((ImagingProtocols)I.ImagingProtocol);
                                }
                            }
                            foreach (DbBeam DbB in DbC.DbBeams)
                            {
                                AddBeam(new Beam(DbB));
                            }
                            if (DbC.Constraints != null)
                            {
                                foreach (DbConstraint DbCon in DbC.Constraints)
                                {
                                    Constraint Con = new Constraint(DbCon); // starts tracking
                                    AddConstraint(Con);
                                }
                            }
                        }
                        return CurrentProtocol;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("{0}/r/n{1}/r/n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                        return null;
                    }
                }
            }
            private static void ClearProtocolData()
            {
                //foreach (ProtocolStructure E in _ProtocolStructures.Values.ToList())
                //    E.Delete();
                //foreach (Constraint C in _Constraints.Values.ToList())
                //    C.Delete();
                //foreach (Component Comp in _Components.Values.ToList())
                //    Comp.Delete();
                //foreach (Beam B in _Beams.Values.ToList())
                //    B.Delete();
                //foreach (ECPlan P in _Plans.Values.ToList())
                //    P.Delete();
                _ProtocolStructures.Clear();
                _Constraints.Clear();
                _Components.Clear();
                _Beams.Clear();
                _Plans.Clear();
                CurrentProtocol = null;
            }
            public static async Task<bool> Load_Session(int ID)
            {
                using (SquintdBModel Context = new SquintdBModel())
                {
                    DbSession DbS = Context.DbSessions.FirstOrDefault(x => x.PID == PatientID && x.ID == ID);
                    if (DbS == null)
                        return false;
                    DbSessionProtocol DbSP = DbS.DbSessionProtocol;
                    if (DbSP == null)
                    {
                        ProtocolLoaded = false;
                        return false;
                    }
                    if (ProtocolLoaded)
                    {
                        ClearProtocolData();
                        ProtocolLoaded = false;
                    }
                    try
                    {
                        CurrentProtocol = new Protocol(DbSP);
                        CurrentSession.AddProtocol(CurrentProtocol);
                        foreach (DbPlanAssociation DbP in DbS.SessionPlans)
                            // Load associated course
                            await GetCourse(DbP.CourseName); // this will load all of the Eclipse Courses and populate _AsyncPlans, but we don't create the ECPlan objects until the components are loaded
                        bool AtLeastOneStructure = false;
                        foreach (DbSessionProtocolStructure DbProtocolStructure in DbSP.ProtocolStructures)
                        {
                            ProtocolStructure E = new ProtocolStructure(DbProtocolStructure);
                            AddProtocolStructure(E);
                            //var P = _AsyncPlans.Values.FirstOrDefault(x => x.StructureSetUID == DbProtocolStructure.AssignedEclipseStructureSetUID);
                            //if (P != null)
                            //{
                            //if (P.Structures.ContainsKey(DbProtocolStructure.AssignedEclipseId))
                            //{
                            //      E.AssignedStructureId = DbProtocolStructure.AssignedEclipseId;
                            //}
                            //}
                            AtLeastOneStructure = true;
                        }
                        if (!AtLeastOneStructure)
                            new ProtocolStructure(Context.DbProtocolStructures.Find(1));  //Initialize non-defined structure
                        foreach (DbSessionComponent DbC in DbSP.Components)
                        {
                            Component SC = new Component(DbC);
                            AddComponent(SC); foreach (DbComponentImaging DbCI in DbC.ImagingProtocols)
                            {
                                foreach (DbImaging I in DbCI.Imaging)
                                {
                                    SC.ImagingProtocolsAttached.Add((ImagingProtocols)I.ImagingProtocol);
                                }
                            }
                            if (DbC.Constraints != null)
                            {
                                foreach (DbSessionConstraint DbCon in DbC.Constraints)
                                {
                                    Constraint Con = new Constraint(DbCon); // starts tracking
                                    AddConstraint(Con);
                                }
                            }
                        }
                        if (DbSP.ProtocolChecklists != null)
                        {
                            //var DbChecklist = DbSP.ProtocolChecklists.FirstOrDefault(x => x.ProtocolDefault == false);
                            //if (DbChecklist != null)
                            //    CurrentProtocol.Checklist = new ProtocolChecklist(DbChecklist);
                            //else
                            //{
                            var DbChecklist = DbSP.ProtocolChecklists.FirstOrDefault();
                            CurrentProtocol.Checklist = new ProtocolChecklist(DbChecklist);

                        }
                        foreach (DbAssessment DbA in DbS.SessionAssessments)
                        {
                            Assessment A = new Assessment(DbA);
                            AddAssessment(A);
                        }
                        foreach (DbPlanAssociation DbP in DbS.SessionPlans)
                        {
                            PlanAssociation P = new PlanAssociation(DbP);
                            await P.LoadLinkedPlan(DbP);
                            AddPlan(P);
                        }
                        return true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("{0}/r/n{1}/r/n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                        return false;
                    }
                }
            }
            public static List<Session> GetSessions()
            {
                List<Session> S = new List<Session>();
                using (SquintdBModel Context = new SquintdBModel())
                {
                    foreach (DbSession DbS in Context.DbSessions.Where(x => x.PID == PatientID).ToList())
                    {
                        S.Add(new Session(DbS));
                    }
                }
                return S;
            }
            public static void Delete_Session(int ID)
            {
                using (SquintdBModel Context = new SquintdBModel())
                {
                    DbSession DbS = Context.DbSessions.Find(ID);
                    if (DbS != null)
                    {
                        Context.DbSessions.Remove(DbS);
                        Context.SaveChanges();
                    }
                }
            }
            public static async Task<bool> Save_Session(string SessionComment)
            {
                Dictionary<int, int> ComponentLookup = new Dictionary<int, int>();
                Dictionary<int, int> StructureLookup = new Dictionary<int, int>();
                Dictionary<int, int> AssessmentLookup = new Dictionary<int, int>();
                using (SquintdBModel Context = new SquintdBModel())
                {
                    // Create session
                    DbSession DbS = Context.DbSessions.Create();
                    Context.DbSessions.Add(DbS);
                    DbS.ID = IDGenerator();
                    DbS.PID = PatientID;
                    DbS.SessionComment = SessionComment;
                    DbS.SessionCreator = SquintUser;
                    DbS.SessionDateTime = string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
                    //Create Session Protocol
                    DbSessionProtocol DbP = Context.DbSessionProtocols.Create();
                    Context.DbSessionProtocols.Add(DbP);
                    DbS.DbSessionProtocol = DbP;
                    DbP.ID = IDGenerator();
                    DbP.ProtocolName = CurrentProtocol.ProtocolName;
                    DbP.ApproverID = 1;
                    DbP.AuthorID = 1;
                    DbP.TreatmentCentreID = Context.DbTreatmentCentres.FirstOrDefault(x => x.TreatmentCentre == (int)CurrentProtocol.TreatmentCentre).ID;
                    DbP.TreatmentSiteID = Context.DbTreatmentSites.FirstOrDefault(x => x.TreatmentSite == (int)CurrentProtocol.TreatmentSite).ID;
                    DbP.ApprovalLevelID = Context.DbApprovalLevels.FirstOrDefault(x => x.ApprovalLevel == (int)ApprovalLevels.Unapproved).ID;
                    DbP.ProtocolTypeID = Context.DbProtocolTypes.FirstOrDefault(x => x.ProtocolType == (int)CurrentProtocol.ProtocolType).ID;
                    DbP.LastModifiedBy = SquintUser;
                    DbP.LastModified = DateTime.Now.ToBinary();
                    DbP.ProtocolParentID = CurrentProtocol.ID;
                    // Assessment
                    foreach (Assessment A in _Assessments.Values)
                    {
                        DbAssessment DbA = Context.DbAssessments.Create();
                        Context.DbAssessments.Add(DbA);
                        DbA.ID = IDGenerator();
                        AssessmentLookup.Add(A.ID, DbA.ID);
                        DbA.DisplayOrder = A.DisplayOrder;
                        DbA.SessionID = DbS.ID;
                        DbA.PID = Ctr.PatientID;
                        DbA.PatientName = string.Format("{0}, {1}", Ctr.PatientLastName, Ctr.PatientFirstName);
                        DbA.DateOfAssessment = DateTime.Now.ToShortDateString();
                        DbA.SquintUser = Ctr.SquintUser;
                        DbA.AssessmentName = A.AssessmentName;
                    }
                    var SessionProtocolStructure_Lookup = new Dictionary<int, int>();
                    foreach (ProtocolStructure S in _ProtocolStructures.Values)
                    {
                        DbSessionProtocolStructure DbE = Context.DbSessionProtocolStructures.Create();
                        Context.DbSessionProtocolStructures.Add(DbE);
                        DbE.ID = IDGenerator();
                        SessionProtocolStructure_Lookup.Add(S.ID, DbE.ID);
                        DbE.SessionId = DbS.ID;
                        DbE.ParentProtocolStructure_Id = S.ID;
                        DbE.AssignedEclipseId = S.AssignedStructureId;
                        StructureLookup.Add(S.ID, DbE.ID);
                        DbE.ProtocolID = DbP.ID;
                        DbE.ProtocolStructureName = S.ProtocolStructureName;
                        DbE.StructureLabelID = S.StructureLabelID;
                        DbE.DisplayOrder = S.DisplayOrder;

                        Save_UpdateStructureAliases(Context, DbE, S);
                        Save_UpdateStructureCheckList(Context, DbE, S);

                    }
                    // Checklist
                    var DbPC = Context.DbProtocolChecklists.Create();
                    Context.DbProtocolChecklists.Add(DbPC);
                    if (CurrentProtocol.Checklist != null)
                    {
                        var C = CurrentProtocol.Checklist;
                        DbPC.DbProtocol = DbP;
                        DbPC.Algorithm = (int)C.Algorithm.Value;
                        DbPC.FieldNormalizationMode = (int)C.FieldNormalizationMode.Value;
                        DbPC.AlgorithmResolution = C.AlgorithmResolution.Value;
                        DbPC.SliceSpacing = (double)C.SliceSpacing.Value;
                        DbPC.HeterogeneityOn = (bool)C.HeterogeneityOn.Value;
                        //Couch
                        DbPC.SupportIndication = (int)C.SupportIndication.Value;
                        DbPC.CouchSurface = (double)C.CouchSurface.Value;
                        DbPC.CouchInterior = (double)C.CouchInterior.Value;
                        //Artifact
                        foreach (Artifact A in C.Artifacts)
                        {
                            DbArtifact DbA = Context.DbArtifacts.Create();
                            Context.DbArtifacts.Add(DbA);
                            DbA.DbProtocolChecklist = DbPC;
                            DbA.ProtocolStructure_ID = SessionProtocolStructure_Lookup[A.E.ID]; // will have been duplicated above;
                            DbA.HU = (double)A.RefHU.Value;
                            DbA.ToleranceHU = (double)A.ToleranceHU.Value;
                            DbA.DbProtocolChecklist = DbPC;
                        }
                    }
                    // structures
                    foreach (Component SC in _Components.Values)
                    {
                        DbSessionComponent DbC = Context.DbSessionComponents.Create();
                        DbC.ComponentType = (int)SC.ComponentType.Value;
                        Context.DbSessionComponents.Add(DbC);
                        DbC.ID = IDGenerator();
                        DbC.SessionID = DbS.ID;
                        DbC.ParentComponentID = SC.ID;
                        ComponentLookup.Add(SC.ID, DbC.ID);
                        DbC.ProtocolID = DbP.ID;
                        //Update
                        DbC.ComponentName = SC.ComponentName;
                        DbC.NumFractions = SC.NumFractions;
                        DbC.ReferenceDose = SC.ReferenceDose;
                        DbC.DisplayOrder = SC.DisplayOrder;
                        DbC.MinColOffset = SC.MinColOffset.Value;
                        foreach (Beam B in SC.Beams())
                        {
                            var DbB = Context.DbBeams.Create();
                            Context.DbBeams.Add(DbB);
                            DbB.ComponentID = DbC.ID;
                            DbB.CouchRotation = (double)B.CouchRotation.Value;
                            foreach (string Alias in B.EclipseAliases)
                            {
                                var DbBA = Context.DbBeamAliases.Create();
                                Context.DbBeamAliases.Add(DbBA);
                                DbBA.DbBeam = DbB;
                                DbBA.EclipseFieldId = Alias;
                            }
                            foreach (BeamGeometry BG in B.ValidGeometries)
                            {
                                var DbBG = Context.DbBeamGeometries.Create();
                                Context.DbBeamGeometries.Add(DbBG);
                                DbBG.DbBeam = DbB;
                                DbBG.GeometryName = BG.GeometryName;
                                DbBG.EndAngle = BG.EndAngle;
                                DbBG.StartAngle = BG.StartAngle;
                                DbBG.EndAngleTolerance = BG.EndAngleTolerance;
                                DbBG.StartAngleTolerance = BG.StartAngleTolerance;
                            }
                            if (DbB.DbEnergies == null && B.ValidEnergies.Count > 0)
                            {
                                DbB.DbEnergies = new List<DbEnergy>();
                            }
                            foreach (var VE in B.ValidEnergies)
                            {
                                DbB.DbEnergies.Add(Context.DbEnergies.Find((int)VE));
                            }
                        }
                        var Plans = _Plans.Values.Where(x => x.ComponentID == SC.ID);
                    }
                    foreach (PlanAssociation P in _Plans.Values)
                    {
                        if (P.Linked)
                        {
                            DbPlanAssociation DbPl = Context.DbPlans.Create();
                            DbPl.SessionId = DbS.ID;
                            DbPl.AssessmentID = AssessmentLookup[P.AssessmentID];
                            DbPl.SessionComponentID = ComponentLookup[P.ComponentID];
                            DbPl.LastModified = P.LastModified.ToBinary();
                            DbPl.LastModifiedBy = P.LastModifiedBy;
                            DbPl.CourseName = P.CourseId;
                            DbPl.PlanName = P.PlanId;
                            DbPl.UID = P.UID;
                            DbPl.PlanType = (int)P.PlanType;
                            Context.DbPlans.Add(DbPl);
                        }
                    }
                    foreach (Constraint Con in _Constraints.Values)
                    {
                        DbSessionConstraint DbO = Context.DbSessionConstraints.Create();
                        Context.DbSessionConstraints.Add(DbO);
                        DbO.ID = IDGenerator();
                        DbO.SessionID = DbS.ID;
                        DbO.ParentConstraintID = Con.ID;
                        DbO.ComponentID = ComponentLookup[Con.ComponentID];
                        // Update
                        DbO.PrimaryStructureID = StructureLookup[Con.PrimaryStructureID];
                        DbO.ReferenceScale = (int)Con.ReferenceScale;
                        DbO.ReferenceType = (int)Con.ReferenceType;
                        DbO.ReferenceValue = Con.ReferenceValue;
                        if (Con.ReferenceStructureId == 1)
                            DbO.ReferenceStructureId = 1;
                        else
                            DbO.ReferenceStructureId = StructureLookup[Con.ReferenceStructureId];
                        DbO.ConstraintScale = (int)Con.ConstraintScale;
                        DbO.ConstraintType = (int)Con.ConstraintType;
                        DbO.ConstraintValue = Con.ConstraintValue;
                        DbO.DisplayOrder = Con.DisplayOrder.Value;
                        DbO.Fractions = Con.NumFractions;
                        //Save reference values
                        var ConReferenceValues = Con.GetConstraintReferenceValues();
                        DbO.OriginalNumFractions = ConReferenceValues.NumFractions;
                        DbO.OriginalPrimaryStructureID = ConReferenceValues.PrimaryStructureID;
                        DbO.OriginalSecondaryStructureID = ConReferenceValues.ReferenceStructureId;
                        DbO.OriginalReferenceValue = ConReferenceValues.ReferenceValue;
                        DbO.OriginalReferenceType = (int)ConReferenceValues.ReferenceType;
                        DbO.OriginalConstraintType = (int)ConReferenceValues.ConstraintType;
                        DbO.OriginalConstraintScale = (int)ConReferenceValues.ConstraintScale;
                        DbO.OriginalReferenceScale = (int)ConReferenceValues.ReferenceScale;
                        DbO.OriginalConstraintValue = ConReferenceValues.ConstraintValue;
                        //Link Results to Asssessment
                        foreach (Assessment A in _Assessments.Values)
                        {
                            ConstraintResultView CRV = Con.GetResult(A.ID);
                            if (CRV != null)
                            {
                                DbConstraintResult DbCR = Context.DbConstraintResults.Create();
                                Context.DbConstraintResults.Add(DbCR);
                                DbCR.AssessmentID = AssessmentLookup[A.ID];
                                DbCR.SessionConstraintID = DbO.ID;
                                DbCR.ResultString = CRV.Result;
                                DbCR.ResultValue = CRV.ResultValue;
                            }
                        }
                        DbO.ThresholdDataPath = Con.ThresholdDataPath;
                        DbO.MajorViolation = (double)Con.MajorViolation;
                        DbO.MinorViolation = (double)Con.MinorViolation;
                        DbO.Stop = (double)Con.Stop;
                    }

                    try
                    {
                        Context.SaveChanges();
                        return true;
                    }
                    catch (Exception ex)
                    {

                        MessageBox.Show(string.Format("{0} {1} {2}", ex.Message, ex.InnerException.InnerException, ex.StackTrace));
                        return false;
                    }
                }
            }
            public static async Task Save_UpdateProtocol()
            {
                using (SquintdBModel Context = new SquintdBModel())
                {
                    //Update Protocol
                    DbProtocol DbP = Context.DbLibraryProtocols.Find(CurrentProtocol.ID);
                    if (DbP == null) // new protocol
                    {
                        DbP = Context.DbLibraryProtocols.Create();
                        Context.DbLibraryProtocols.Add(DbP);
                        DbP.ID = CurrentProtocol.ID;
                    }
                    DbP.TreatmentCentreID = Context.DbTreatmentCentres.FirstOrDefault(x => x.TreatmentCentre == (int)CurrentProtocol.TreatmentCentre).ID;
                    DbP.DbUser_Approver = Context.DbUsers.FirstOrDefault(x => x.ARIA_ID == SquintUser);
                    DbP.DbUser_ProtocolAuthor = Context.DbUsers.FirstOrDefault(x => x.ARIA_ID == SquintUser);
                    DbP.TreatmentSiteID = Context.DbTreatmentSites.FirstOrDefault(x => x.TreatmentSite == (int)CurrentProtocol.TreatmentSite).ID;
                    DbP.ProtocolTypeID = Context.DbProtocolTypes.FirstOrDefault(x => x.ProtocolType == (int)CurrentProtocol.ProtocolType).ID;
                    DbP.ApprovalLevelID = Context.DbApprovalLevels.FirstOrDefault(x => x.ApprovalLevel == (int)CurrentProtocol.ApprovalLevel).ID;
                    DbP.LastModifiedBy = SquintUser;
                    DbP.LastModified = DateTime.Now.ToBinary();
                    DbP.ProtocolName = CurrentProtocol.ProtocolName;
                    DbP.Comments = CurrentProtocol.Comments;
                    //Update Checklist
                    Save_UpdateProtocolCheckList(Context, DbP);
                    //Update Components
                    foreach (Component SC in _Components.Values)
                    {
                        Save_UpdateComponent(Context, SC, DbP.ID, false);
                    }
                    foreach (ProtocolStructure S in _ProtocolStructures.Values)
                    {
                        DbProtocolStructure DbS;
                        if (S.isCreated)
                        {
                            DbS = Context.DbProtocolStructures.Create();
                            Context.DbProtocolStructures.Add(DbS);
                            DbS.ID = S.ID;
                            DbS.DbLibraryProtocol = DbP;
                            DbS.StructureLabelID = 1;
                        }
                        else
                        {
                            DbS = Context.DbProtocolStructures.Find(S.ID);
                            DbS.StructureLabelID = S.StructureLabelID;
                        }
                        DbS.ProtocolStructureName = S.ProtocolStructureName;
                        DbS.DisplayOrder = S.DisplayOrder;

                        Save_UpdateStructureCheckList(Context, DbS, S);
                        Save_UpdateStructureAliases(Context, DbS, S);
                    }
                    foreach (Constraint con in _Constraints.Values)
                    {
                        Save_UpdateConstraint(Context, con, con.ComponentID, con.PrimaryStructureID, con.ReferenceStructureId, false);
                    }

                    try
                    {
                        await Context.SaveChangesAsync();
                        if (CurrentProtocol.ID < 0) // first save of protocol
                            LoadProtocolFromDb(CurrentProtocol.ProtocolName);
                        CurrentProtocol.LastModifiedBy = SquintUser;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: Unable to update protocol.", "Error during save");
                    }
                }

            }
            public static void Save_DuplicateProtocol()
            {
                using (SquintdBModel Context = new SquintdBModel())
                {
                    // Name check
                    if (Context.DbLibraryProtocols.Select(x => x.ProtocolName).Contains(CurrentProtocol.ProtocolName))
                        CurrentProtocol.ProtocolName = CurrentProtocol.ProtocolName + "_copy";
                    //Duplicate Protocol
                    DbProtocol DbP = Context.DbLibraryProtocols.Create();
                    Context.DbLibraryProtocols.Add(DbP);
                    DbP.ID = IDGenerator();
                    DbP.ProtocolName = CurrentProtocol.ProtocolName;
                    DbP.ApproverID = 1;
                    DbP.AuthorID = 1;
                    DbP.TreatmentCentreID = Context.DbTreatmentCentres.FirstOrDefault(x => x.TreatmentCentre == (int)CurrentProtocol.TreatmentCentre).ID;
                    DbP.TreatmentSiteID = Context.DbTreatmentSites.FirstOrDefault(x => x.TreatmentSite == (int)CurrentProtocol.TreatmentSite).ID;
                    DbP.ApprovalLevelID = Context.DbApprovalLevels.FirstOrDefault(x => x.ApprovalLevel == (int)ApprovalLevels.Unapproved).ID;
                    DbP.ProtocolTypeID = Context.DbProtocolTypes.FirstOrDefault(x => x.ProtocolType == (int)CurrentProtocol.ProtocolType).ID;
                    DbP.LastModifiedBy = SquintUser;
                    DbP.LastModified = DateTime.Now.ToBinary();
                    DbP.ProtocolParentID = CurrentProtocol.ID;
                    DbP.Comments = CurrentProtocol.Comments;
                    Save_UpdateProtocolCheckList(Context, DbP);
                    //Update Components
                    Dictionary<int, int> ComponentLookup = new Dictionary<int, int>();
                    Dictionary<int, int> StructureLookup = new Dictionary<int, int>();
                    StructureLookup.Add(1, 1); // add dummy structure
                    foreach (ProtocolStructure S in _ProtocolStructures.Values)
                    {
                        DbProtocolStructure DbS = Context.DbProtocolStructures.Create();
                        Context.DbProtocolStructures.Add(DbS);
                        DbS.ID = IDGenerator(); // because we are creating new
                        StructureLookup.Add(S.ID, DbS.ID); // save map from old to new Id
                        DbS.ProtocolID = DbP.ID;
                        DbS.ProtocolStructureName = S.ProtocolStructureName;
                        DbS.StructureLabelID = S.StructureLabelID;
                        DbS.DisplayOrder = S.DisplayOrder;
                        Save_UpdateStructureCheckList(Context, DbS, S);
                        Save_UpdateStructureAliases(Context, DbS, S);
                    }
                    foreach (Component SC in _Components.Values)
                    {
                        var newID = Save_UpdateComponent(Context, SC, DbP.ID, true);
                        if (newID != null)
                            ComponentLookup.Add(SC.ID, (int)newID);
                    }
                    foreach (Constraint Con in _Constraints.Values)
                    {
                        if (ComponentLookup.ContainsKey(Con.ComponentID))
                            Save_UpdateConstraint(Context, Con, ComponentLookup[Con.ComponentID], StructureLookup[Con.PrimaryStructureID], StructureLookup[Con.ReferenceStructureId], true);
                    }
                    try
                    {
                        Context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        string debugme = "hi";
                    }
                }
            }
            public static bool Delete_Protocol(int Id)
            {
                using (var Context = new SquintdBModel())
                {
                    var DbP = Context.DbLibraryProtocols.Find(Id);
                    if (DbP != null)
                    {
                        Context.Entry(DbP).State = EntityState.Deleted;
                        Context.SaveChanges();
                        return true;
                    }
                    else
                        return false;
                }

            }

            private static int? Save_UpdateComponent(SquintdBModel Context, Component comp, int protocolId, bool createCopy)
            {
                DbComponent DbC;
                if (comp.ToRetire)
                {
                    DbC = Context.DbComponents.Find(comp.ID);
                    if (DbC != null && comp.ToRetire && !createCopy) // don't remove from template if creating copy
                        Context.DbComponents.Remove(DbC);
                    return null;
                    //update checklist
                }
                else if (comp.ID < 0 || createCopy)
                {
                    DbC = Context.DbComponents.Create();
                    Context.DbComponents.Add(DbC);
                    if (createCopy)
                        DbC.ID = IDGenerator();
                    else
                        DbC.ID = comp.ID;
                }
                else
                {
                    DbC = Context.DbComponents.Find(comp.ID);
                }
                // Update
                DbC.ComponentName = comp.ComponentName;
                DbC.ProtocolID = protocolId;
                DbC.DisplayOrder = comp.DisplayOrder;
                DbC.ComponentType = (int)comp.ComponentType.Value;
                DbC.NumFractions = comp.NumFractions;
                DbC.ReferenceDose = comp.ReferenceDose;
                Save_UpdateBeamDefinition(Context, DbC, comp.ID, createCopy);

                // Update checklist parameters
                DbC.MaxBeams = comp.MaxBeams.Value;
                comp.MaxBeams.AcceptChanges();
                DbC.PNVMin = comp.PNVMin.Value;
                comp.PNVMin.AcceptChanges();
                DbC.PNVMax = comp.PNVMax.Value;
                comp.PNVMax.AcceptChanges();
                DbC.PrescribedPercentage = comp.PrescribedPercentage.Value;
                comp.PrescribedPercentage.AcceptChanges();
                DbC.NumIso = comp.NumIso.Value;
                comp.NumIso.AcceptChanges();
                DbC.MinBeams = comp.MinBeams.Value;
                comp.MinBeams.AcceptChanges();
                DbC.MinColOffset = comp.MinColOffset.Value;
                comp.MinColOffset.AcceptChanges();
                return DbC.ID;
            }

            private static void Save_UpdateConstraint(SquintdBModel Context, Constraint Con, int ComponentId, int StructureId, int refStructureId, bool createCopy)
            {
                DbConstraint DbO;
                if ((Con.isCreated || createCopy) && !Con.ToRetire)
                {
                    DbO = Context.DbConstraints.Create();
                    Context.DbConstraints.Add(DbO);
                    if (createCopy)
                        DbO.ID = IDGenerator();
                    else
                        DbO.ID = Con.ID;
                }
                else
                {
                    DbO = Context.DbConstraints.Find(Con.ID);
                    if (Con.ToRetire)
                    {
                        if (DbO != null)
                            Context.DbConstraints.Remove(DbO);
                        return;
                    }
                }

                // Update
                DbO.PrimaryStructureID = StructureId;
                DbO.ReferenceScale = (int)Con.ReferenceScale;
                DbO.ReferenceType = (int)Con.ReferenceType;
                DbO.ReferenceValue = Con.ReferenceValue;
                DbO.ReferenceStructureId = refStructureId;
                DbO.ComponentID = ComponentId;
                DbO.ConstraintScale = (int)Con.ConstraintScale;
                DbO.ConstraintType = (int)Con.ConstraintType;
                DbO.ConstraintValue = Con.ConstraintValue;
                DbO.DisplayOrder = Con.DisplayOrder.Value;
                DbO.Fractions = Con.NumFractions;
                DbO.ThresholdDataPath = Con.ThresholdDataPath;
                DbO.MajorViolation = Con.MajorViolation;
                DbO.MinorViolation = Con.MinorViolation;
                DbO.Stop = Con.Stop;

                // Update constraint log
                if (Con.isModified() || Con.isCreated)
                {
                    var PreviousLogs = Context.DbConstraintChangelogs.Where(x => x.ConstraintID == Con.ID);
                    int DbCC_ParentID = 1; // root 
                    if (PreviousLogs.Count() > 0)
                        DbCC_ParentID = PreviousLogs.OrderByDescending(x => x.Date).First().ID;
                    DbConstraintChangelog DbCC = Context.DbConstraintChangelogs.Create();
                    if (Con.isCreated)
                        DbCC.ChangeDescription = "New";
                    if (createCopy)
                        DbCC.ChangeDescription = string.Format("Duplicated from protocol ({0})", CurrentProtocol.ProtocolName);
                    DbCC.ChangeAuthor = SquintUser;
                    DbCC.ConstraintID = DbO.ID;
                    DbCC.ConstraintString = Con.GetConstraintString();
                    DbCC.ParentLogID = DbCC_ParentID;
                    DbCC.Date = DateTime.Now.ToBinary();
                    Context.DbConstraintChangelogs.Add(DbCC);
                }
            }

            // Checklist functions
            private static void Save_UpdateProtocolCheckList(SquintdBModel Context, DbProtocol DbP)
            {
                DbProtocolChecklist DbPC = Context.DbProtocolChecklists.FirstOrDefault(x => x.ProtocolID == DbP.ID);
                if (DbPC == null)
                {
                    DbPC = Context.DbProtocolChecklists.Create();
                    Context.DbProtocolChecklists.Add(DbPC);
                    DbPC.ID = IDGenerator();
                    DbPC.DbProtocol = DbP;
                }


                DbPC.SliceSpacing = CurrentProtocol.Checklist.SliceSpacing.Value;
                CurrentProtocol.Checklist.SliceSpacing.AcceptChanges();
                DbPC.Algorithm = (int)CurrentProtocol.Checklist.Algorithm.Value;
                CurrentProtocol.Checklist.Algorithm.AcceptChanges();
                DbPC.AlgorithmResolution = CurrentProtocol.Checklist.AlgorithmResolution.Value;
                CurrentProtocol.Checklist.AlgorithmResolution.AcceptChanges();
                //DbPC.Artifacts
                DbPC.CouchInterior = CurrentProtocol.Checklist.CouchInterior.Value;
                CurrentProtocol.Checklist.CouchInterior.AcceptChanges();
                DbPC.CouchSurface = CurrentProtocol.Checklist.CouchSurface.Value;
                CurrentProtocol.Checklist.CouchSurface.AcceptChanges();
                DbPC.FieldNormalizationMode = (int)CurrentProtocol.Checklist.FieldNormalizationMode.Value;
                CurrentProtocol.Checklist.FieldNormalizationMode.AcceptChanges();
                DbPC.HeterogeneityOn = CurrentProtocol.Checklist.HeterogeneityOn.Value;
                CurrentProtocol.Checklist.HeterogeneityOn.AcceptChanges();
                DbPC.SupportIndication = (int)CurrentProtocol.Checklist.SupportIndication.Value;
                CurrentProtocol.Checklist.SupportIndication.AcceptChanges();
                //DbPC.TreatmentTechniqueType 

            }

            private static void Save_UpdateStructureCheckList(SquintdBModel Context, DbProtocolStructure DbS, ProtocolStructure S)
            {
                if (DbS.DbStructureChecklist == null) // no existing checklist in db
                {
                    DbStructureChecklist DbSC = Context.DbStructureChecklists.Create();
                    DbSC.ProtocolStructureID = DbS.ID;
                    Context.DbStructureChecklists.Add(DbSC);
                    DbSC.isPointContourChecked = (bool)S.CheckList.isPointContourChecked.Value;
                    DbSC.PointContourThreshold = (double)S.CheckList.PointContourVolumeThreshold.Value;
                    S.CheckList.isPointContourChecked.AcceptChanges();
                    S.CheckList.PointContourVolumeThreshold.AcceptChanges();
                }
                else
                {
                    DbS.DbStructureChecklist.isPointContourChecked = (bool)S.CheckList.isPointContourChecked.Value;
                    DbS.DbStructureChecklist.PointContourThreshold = (double)S.CheckList.PointContourVolumeThreshold.Value;
                }
            }

            private static void Save_UpdateStructureAliases(SquintdBModel Context, DbProtocolStructure DbS, ProtocolStructure S)
            {
                if (DbS.DbStructureAliases == null) //no existing aliases
                    DbS.DbStructureAliases = new List<DbStructureAlias>();
                foreach (string alias in S.DefaultEclipseAliases)
                {
                    DbStructureAlias DbSA = DbS.DbStructureAliases.FirstOrDefault(x => x.EclipseStructureId == alias);
                    if (DbSA == null)
                    {
                        DbSA = Context.DbStructureAliases.Create();
                        DbSA.EclipseStructureId = alias;
                        DbSA.DbProtocolStructure = DbS;
                        DbSA.DisplayOrder = S.DefaultEclipseAliases.IndexOf(alias) + 1;
                        DbSA.ID = IDGenerator();
                        Context.DbStructureAliases.Add(DbSA);
                    }
                    else
                    {
                        DbSA.DisplayOrder = S.DefaultEclipseAliases.IndexOf(alias) + 1; // just update order
                    }
                }
                foreach (DbStructureAlias DbSA in DbS.DbStructureAliases.Where(x => !S.DefaultEclipseAliases.Contains(x.EclipseStructureId)))
                {
                    Context.DbStructureAliases.Remove(DbSA);
                }

            }

            private static void Save_UpdateBeamDefinition(SquintdBModel Context, DbComponent DbC, int sourceComponentID, bool createCopy)
            {
                foreach (Beam B in _Beams.Values.Where(x => x.ComponentID == sourceComponentID).ToList())
                {
                    DbBeam DbB = null;
                    if (DbC.DbBeams != null)
                        DbB = DbC.DbBeams.FirstOrDefault(x => x.ID == B.ID);
                    if (B.ToRetire)
                    {
                        if (DbB != null && !createCopy) // don't delete if creating copy, just exclude from duplication
                        {
                            Context.DbBeams.Remove(DbB);
                            _Beams.Remove(B.ID);
                        }
                        continue;
                    }
                    if (DbB == null || createCopy)
                    {
                        DbB = Context.DbBeams.Create();
                        if (createCopy)
                            DbB.ID = IDGenerator();
                        else
                            DbB.ID = B.ID;
                        Context.DbBeams.Add(DbB);
                        DbB.DbComponent = DbC;
                        DbB.DbBoluses = new List<DbBolus>();
                        DbB.DbEnergies = new List<DbEnergy>();
                        DbB.DbBeamAliases = new List<DbBeamAlias>();
                    }

                    if (B.Boluses.FirstOrDefault() != null)
                    {
                        DbBolus DbBol = DbB.DbBoluses.FirstOrDefault();
                        if (DbBol == null)
                        {
                            DbBol = Context.DbBoluses.Create();
                            Context.DbBoluses.Add(DbBol);
                            DbBol.ID = IDGenerator();
                            DbBol.BeamId = DbB.ID;
                        }
                        DbBol.HU = B.Boluses.FirstOrDefault().HU.Value;
                        B.Boluses.FirstOrDefault().HU.AcceptChanges();
                        DbBol.Thickness = B.Boluses.FirstOrDefault().Thickness.Value;
                        B.Boluses.FirstOrDefault().Thickness.AcceptChanges();
                        DbBol.ToleranceHU = B.Boluses.FirstOrDefault().ToleranceHU.Value;
                        B.Boluses.FirstOrDefault().ToleranceHU.AcceptChanges();
                        DbBol.ToleranceThickness = B.Boluses.FirstOrDefault().ToleranceThickness.Value;
                        B.Boluses.FirstOrDefault().ToleranceThickness.AcceptChanges();
                        DbBol.Indication = (int)B.Boluses.FirstOrDefault().Indication.Value;
                        B.Boluses.FirstOrDefault().Indication.AcceptChanges();
                    }
                    DbB.CouchRotation = B.CouchRotation.Value == null ? double.NaN : (double)B.CouchRotation.Value;
                    B.CouchRotation.AcceptChanges();
                    DbB.DbEnergies.Clear();
                    foreach (var energy in B.ValidEnergies)
                        DbB.DbEnergies.Add(Context.DbEnergies.Find((int)energy));
                    DbB.JawTracking_Indication = (int)B.JawTracking_Indication.Value;
                    B.JawTracking_Indication.AcceptChanges();
                    DbB.MaxColRotation = B.MaxColRotation.Value == null ? double.NaN : (double)B.MaxColRotation.Value;
                    B.MaxColRotation.AcceptChanges();
                    DbB.MaxMUWarning = B.MaxMUWarning.Value == null ? double.NaN : (double)B.MaxMUWarning.Value;
                    B.MaxMUWarning.AcceptChanges();
                    DbB.MinMUWarning = B.MinMUWarning.Value == null ? double.NaN : (double)B.MinMUWarning.Value;
                    B.MinMUWarning.AcceptChanges();
                    DbB.MaxX = B.MaxX.Value == null ? double.NaN : (double)B.MaxX.Value;
                    B.MaxX.AcceptChanges();
                    DbB.MinX = B.MinX.Value == null ? double.NaN : (double)B.MinX.Value;
                    B.MinX.AcceptChanges();
                    DbB.MaxY = B.MaxY.Value == null ? double.NaN : (double)B.MaxY.Value;
                    B.MaxY.AcceptChanges();
                    DbB.MinY = B.MinY.Value == null ? double.NaN : (double)B.MinY.Value;
                    B.MinY.AcceptChanges();
                    DbB.ProtocolBeamName = B.ProtocolBeamName;
                    DbB.Technique = (int)B.Technique;
                    DbB.ToleranceTable = B.ToleranceTable.Value;
                    B.ToleranceTable.AcceptChanges();

                    List<string> newAliases = new List<string>(B.EclipseAliases);
                    if (DbB.DbBeamAliases != null)
                    {
                        foreach (DbBeamAlias DbBA in DbB.DbBeamAliases.ToList())
                        {
                            if (newAliases.Contains(DbBA.EclipseFieldId))
                                newAliases.Remove(DbBA.EclipseFieldId);
                            else
                                Context.DbBeamAliases.Remove(DbBA);
                        }
                        foreach (string newAlias in newAliases)
                        {
                            DbBeamAlias DbA = Context.DbBeamAliases.Create();
                            DbA.EclipseFieldId = newAlias;
                            DbA.DbBeam = DbB;
                            Context.DbBeamAliases.Add(DbA);
                        }


                    }
                }

                //foreach (DbBeam DbB in DbC.DbBeams)
                //{
                //    if (_Beams.ContainsKey(DbB.ID))
                //    {
                //        var B = _Beams[DbB.ID];


                //    }
                //    else
                //        MessageBox.Show("Error in Save_UpdateBeamDefinition, component not found");
                //}
            }
        }
    }
}
