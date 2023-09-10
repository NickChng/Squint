using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Data.Common;
using System.Data.SqlClient;

namespace Squint
{
    public class DbController
    {
        public Dictionary<int, int> CourseLookupFromPlan = new Dictionary<int, int>(); // lookup course PK by planPK
        public Dictionary<int, int> CourseLookupFromSum = new Dictionary<int, int>(); // lookup course PK by plansumPK
        private Dictionary<int, StructureLabel> _StructureLabels = new Dictionary<int, StructureLabel>();
        private bool areStructuresLoaded = false;
        private SquintModel _model;

        public DbController(SquintModel model)
        {
            _model = model;
        }

        public void SetDatabaseName(string dbname)
        {
            VersionContextConnection.databaseName = dbname;
        }
        public void RegisterUser(string UserId, string ARIA_ID, string Name)
        {
            using (var Context = new SquintDBModel(_model.DbPaths))
            {
                var User = Context.DbUsers.FirstOrDefault(x => x.ARIA_ID == UserId);
                if (User == null)
                {
                    // Add new user;
                    lock (_model.LockDb)
                    {
                        User = Context.DbUsers.Create();
                        Context.DbUsers.Add(User);
                        User.ID = IDGenerator.GetUniqueId();
                        User.PermissionGroupID = 1;
                        User.FirstName = Name;
                        User.ARIA_ID = ARIA_ID;
                        Context.SaveChanges();
                    }
                }
            }
        }

        public List<ConstraintChangelogViewModel> GetConstraintChangelogs(int ID)
        {
            var L = new List<ConstraintChangelogViewModel>();
            using (var Context = new SquintDBModel(_model.DbPaths))
            {
                foreach (DbConstraintChangelog DbCL in Context.DbConstraints.Find(ID).DbConstraintChangelogs.OrderByDescending(x => x.Date))
                {
                    L.Add(new ConstraintChangelogViewModel(_model, DbCL));
                }
            }
            return L;
        }
        public DatabaseStatus TestDbConnection()
        {
            SqlConnection conn = new SqlConnection(VersionContextConnection.ConnectionString());
            try
            {
                conn.Open();
                conn.Close();
                return DatabaseStatus.Exists;
            }
            catch (SqlException ex)
            {
                return DatabaseStatus.NonExistent;
            }
        }
        public void InitializeDatabase()
        {
            var Context = new SquintDBModel(_model.DbPaths);
            Context.Dispose();
        }

        private async Task LoadStructures()
        {
            _StructureLabels.Clear();
            await Task.Run(() =>
            {
                using (SquintDBModel Context = new SquintDBModel(_model.DbPaths))
                {
                    foreach (DbStructureLabel DbSL in Context.DbStructureLabels)
                        _StructureLabels.Add(DbSL.ID, new StructureLabel(DbSL));
                }
            });
        }

        public async Task<StructureLabel> GetStructureLabel(int? Id)
        {
            if (!areStructuresLoaded)
            {
                await LoadStructures();
                areStructuresLoaded = true;
            }
            if (Id != null)
            {
                if (_StructureLabels.ContainsKey((int)Id))
                    return _StructureLabels[(int)Id];
            }
            // else
            MessageBox.Show(string.Format("Structure Label {0} not found in dictionary, setting default...", Id));
            return _StructureLabels[1];
        }
        public async Task<string> GetStructureCode(int Id)
        {
            if (!areStructuresLoaded)
            {
                await LoadStructures();
                areStructuresLoaded = true;
            }
            return _StructureLabels[Id].Code;
        }
        public IEnumerable<StructureLabel> GetAllStructureLabels()
        {
            return _StructureLabels.Values;
        }


        public List<ProtocolPreview> GetProtocolPreviews()
        {
            List<ProtocolPreview> previewlist = new List<ProtocolPreview>();
            using (var Context = new SquintDBModel(_model.DbPaths))
            {
                if (Context.DbLibraryProtocols != null)
                {
                    List<DbLibraryProtocol> Protocols = Context.DbLibraryProtocols.Where(x => !x.isRetired).ToList();
                    foreach (DbLibraryProtocol DbP in Protocols)
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
        public Protocol GetProtocol(int Id)
        {
            using (var Context = new SquintDBModel(_model.DbPaths))
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
        public async Task<Protocol> LoadProtocol(string ProtocolName)
        {
            using (SquintDBModel Context = new SquintDBModel(_model.DbPaths))
            {
                DbProtocol DbP = Context.DbLibraryProtocols
                    .Include(x => x.ProtocolStructures)
                    .Include(x => x.Components)
                    .Where(x => x.ProtocolName == ProtocolName && !x.isRetired).SingleOrDefault();
                if (DbP == null)
                {
                    return null;
                }
                try
                {
                    Protocol LoadedProtocol = new Protocol(DbP);
                    var DbChecklist = DbP.ProtocolChecklists.FirstOrDefault();
                    if (DbChecklist != null)
                        LoadedProtocol.Checklist = new ProtocolChecklist(DbChecklist);
                    else
                        LoadedProtocol.Checklist = new ProtocolChecklist(DbP.ID);
                    bool AtLeastOneStructure = false;
                    foreach (DbProtocolStructure DbProtocolStructure in DbP.ProtocolStructures)
                    {
                        StructureLabel SL = await GetStructureLabel(DbProtocolStructure.StructureLabelID);
                        LoadedProtocol.Structures.Add(new StructureModel(_model.ESAPIContext, SL, DbProtocolStructure));
                        AtLeastOneStructure = true;
                    }
                    if (!AtLeastOneStructure)
                    {
                        StructureLabel SL = await GetStructureLabel(1);
                        LoadedProtocol.Structures.Add(new StructureModel(_model.ESAPIContext, SL, Context.DbProtocolStructures.Find(1)));  //Initialize non-defined structure
                    }
                    foreach (DbComponent DbC in DbP.Components)
                    {
                        ComponentModel SC = new ComponentModel(DbC);
                        LoadedProtocol.Components.Add(SC);
                        foreach (DbComponentImaging DbCI in DbC.ImagingProtocols)
                        {
                            foreach (DbImaging I in DbCI.Imaging)
                            {
                                SC.ImagingProtocols.Add((ImagingProtocolTypes)I.ImagingProtocol);
                            }
                        }
                        foreach (DbBeam DbB in DbC.DbBeams)
                        {
                            SC.Beams.Add(new BeamViewModel(DbB, _model));
                        }
                        if (DbC.Constraints != null)
                        {
                            foreach (DbConstraint DbCon in DbC.Constraints)
                            {
                                var primaryStructure = LoadedProtocol.Structures.FirstOrDefault(x => x.ID == DbCon.PrimaryStructureID);
                                var referenceStructure = LoadedProtocol.Structures.FirstOrDefault(x => x.ID == DbCon.ReferenceStructureId);
                                SC.ConstraintModels.Add(new ConstraintModel(_model, SC, primaryStructure, referenceStructure, DbCon));
                            }
                        }
                    }
                    return LoadedProtocol;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0}/r/n{1}/r/n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                    return null;
                }
            }
        }
        public List<BeamGeometryDefinition> GetBeamGeometryDefinitions()
        {
            var BGD = new List<BeamGeometryDefinition>();
            using (SquintDBModel Context = new SquintDBModel(_model.DbPaths))
            {
                foreach (DbBeamGeometry DbBG in Context.DbBeamGeometries)
                    BGD.Add(new BeamGeometryDefinition(DbBG));
            }
            return BGD;
        }
        public async Task<Session> Load_Session(int ID)
        {
            using (SquintDBModel Context = new SquintDBModel(_model.DbPaths))
            {
                DbSession DbS = Context.DbSessions.FirstOrDefault(x => x.PID == _model.PatientID && x.ID == ID);
                if (DbS == null)
                    return null;
                DbSessionProtocol DbSP = DbS.DbSessionProtocol;
                if (DbSP == null)
                {
                    return null;
                }
                try
                {
                    Session CurrentSession = new Session();
                    Protocol SessionProtocol = new Protocol(DbSP);
                    CurrentSession.SessionProtocol = SessionProtocol;
                    //foreach (DbPlanAssociation DbP in DbS.SessionPlans)
                    //    // Load associated course
                    //    await Ctr.ESAPIContext.Patient.GetCourse(DbP.CourseName); // this will load all of the Eclipse Courses and populate _AsyncPlans, but we don't create the ECPlan objects until the components are loaded
                    bool AtLeastOneStructure = false;
                    foreach (DbSessionProtocolStructure DbProtocolStructure in DbSP.ProtocolStructures)
                    {
                        StructureLabel SL = await GetStructureLabel(DbProtocolStructure.StructureLabelID);
                        StructureModel E = new StructureModel(_model.ESAPIContext, SL, DbProtocolStructure);
                        SessionProtocol.Structures.Add(E);
                        AtLeastOneStructure = true;
                    }
                    if (!AtLeastOneStructure)
                    {
                        StructureLabel SL = await GetStructureLabel(1);
                        SessionProtocol.Structures.Add(new StructureModel(_model.ESAPIContext, SL, Context.DbProtocolStructures.Find(1)));  //Initialize non-defined structure
                    }
                    foreach (DbSessionComponent DbC in DbSP.Components.OrderBy(x => x.DisplayOrder))
                    {
                        ComponentModel SC = new ComponentModel(DbC);
                        SessionProtocol.Components.Add(SC);
                        foreach (DbBeam DbB in DbC.DbBeams)
                        {
                            SC.Beams.Add(new BeamViewModel(DbB, _model));
                        }
                        foreach (DbComponentImaging DbCI in DbC.ImagingProtocols)
                        {
                            foreach (DbImaging I in DbCI.Imaging)
                            {
                                SC.ImagingProtocols.Add((ImagingProtocolTypes)I.ImagingProtocol);
                            }
                        }
                        if (DbC.Constraints != null)
                        {
                            foreach (DbSessionConstraint DbCon in DbC.Constraints.OrderBy(x => x.DisplayOrder))
                            {
                                var primaryStructure = SessionProtocol.Structures.FirstOrDefault(x => x.ID == DbCon.PrimaryStructureID);
                                var referenceStructure = SessionProtocol.Structures.FirstOrDefault(x => x.ID == DbCon.ReferenceStructureId);
                                SC.ConstraintModels.Add(new ConstraintModel(_model, SC, primaryStructure, referenceStructure, DbCon));
                            }
                        }
                    }
                    foreach (DbAssessment DbA in DbS.SessionAssessments.OrderBy(x => x.DisplayOrder))
                    {
                        AssessmentViewModel A = new AssessmentViewModel(DbA, _model);
                        CurrentSession.Assessments.Add(A);
                    }
                    foreach (DbPlanAssociation DbP in DbS.SessionPlans)
                    {
                        var SC = SessionProtocol.Components.FirstOrDefault(x => x.Id == DbP.SessionComponentID);
                        var SA = CurrentSession.Assessments.FirstOrDefault(x => x.ID == DbP.AssessmentID);
                        PlanAssociationViewModel P = new PlanAssociationViewModel(_model, SC, SA, DbP);
                        CurrentSession.PlanAssociations.Add(P);
                        await P.LoadLinkedPlan(DbP);
                    }
                    if (DbSP.ProtocolChecklists != null)
                    {
                        var DbChecklist = DbSP.ProtocolChecklists.FirstOrDefault();
                        SessionProtocol.Checklist = new ProtocolChecklist(DbChecklist);

                    }
                    _model.SetCurrentStructureSet(DbS.SessionStructureSetUID);
                    return CurrentSession;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("{0}/r/n{1}/r/n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                    return null;
                }
            }
        }
        public List<Session> GetSessions()
        {
            List<Session> S = new List<Session>();
            using (SquintDBModel Context = new SquintDBModel(_model.DbPaths))
            {
                foreach (DbSession DbS in Context.DbSessions.Where(x => x.PID == _model.PatientID).ToList())
                {
                    S.Add(new Session(DbS));
                }
            }
            return S;
        }
        public void Delete_Session(int ID)
        {
            using (SquintDBModel Context = new SquintDBModel(_model.DbPaths))
            {
                DbSession DbS = Context.DbSessions.Find(ID);
                if (DbS != null)
                {
                    Context.DbSessions.Remove(DbS);
                    Context.SaveChanges();
                }
            }
        }
        public async Task<bool> Save_Session(string SessionComment)
        {
            Dictionary<int, int> ComponentLookup = new Dictionary<int, int>();
            Dictionary<int, int> StructureLookup = new Dictionary<int, int>();
            Dictionary<int, int> AssessmentLookup = new Dictionary<int, int>();
            using (SquintDBModel Context = new SquintDBModel(_model.DbPaths))
            {
                // Create session
                DbSession DbS = Context.DbSessions.Create();
                Context.DbSessions.Add(DbS);
                DbS.ID = IDGenerator.GetUniqueId();
                DbS.PID = _model.PatientID;
                DbS.SessionStructureSetUID = _model.CurrentStructureSet.UID;
                DbS.SessionComment = SessionComment;
                DbS.SessionCreator = _model.SquintUser;
                DbS.SessionDateTime = string.Format("{0} {1}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString());
                //Create Session Protocol
                DbSessionProtocol DbP = Context.DbSessionProtocols.Create();
                Context.DbSessionProtocols.Add(DbP);
                DbS.DbSessionProtocol = DbP;
                DbP.ID = IDGenerator.GetUniqueId();
                DbP.ProtocolName = _model.CurrentProtocol.ProtocolName;
                DbP.ApproverID = 1;
                DbP.AuthorID = 1;
                DbP.TreatmentCentreID = Context.DbTreatmentCentres.FirstOrDefault(x => x.TreatmentCentre == (int)_model.CurrentProtocol.TreatmentCentre.Value).ID;
                DbP.TreatmentSiteID = Context.DbTreatmentSites.FirstOrDefault(x => x.TreatmentSite == (int)_model.CurrentProtocol.TreatmentSite.Value).ID;
                DbP.ApprovalLevelID = Context.DbApprovalLevels.FirstOrDefault(x => x.ApprovalLevel == (int)ApprovalLevels.Unapproved).ID;
                DbP.ProtocolTypeID = Context.DbProtocolTypes.FirstOrDefault(x => x.ProtocolType == (int)_model.CurrentProtocol.ProtocolType).ID;
                DbP.LastModifiedBy = _model.SquintUser;
                DbP.LastModified = DateTime.Now.ToBinary();
                DbP.ProtocolParentID = _model.CurrentProtocol.ID;
                // Assessment
                foreach (AssessmentViewModel A in _model.CurrentSession.Assessments)
                {
                    DbAssessment DbA = Context.DbAssessments.Create();
                    Context.DbAssessments.Add(DbA);
                    DbA.ID = IDGenerator.GetUniqueId();
                    AssessmentLookup.Add(A.ID, DbA.ID);
                    DbA.DisplayOrder = A.DisplayOrder;
                    DbA.SessionID = DbS.ID;
                    DbA.PID = _model.PatientID;
                    DbA.PatientName = string.Format("{0}, {1}", _model.PatientLastName, _model.PatientFirstName);
                    DbA.DateOfAssessment = DateTime.Now.ToShortDateString();
                    DbA.SquintUser = _model.SquintUser;
                    DbA.AssessmentName = A.AssessmentName;
                }
                var SessionProtocolStructure_Lookup = new Dictionary<int, int>();
                foreach (StructureModel S in _model.CurrentProtocol.Structures)
                {
                    DbSessionProtocolStructure DbE = Context.DbSessionProtocolStructures.Create();
                    Context.DbSessionProtocolStructures.Add(DbE);
                    DbE.ID = IDGenerator.GetUniqueId();
                    SessionProtocolStructure_Lookup.Add(S.ID, DbE.ID);
                    DbE.SessionId = DbS.ID;
                    DbE.ParentProtocolStructure_Id = S.ID;
                    DbE.AssignedEclipseId = S.AssignedStructureId;
                    DbE.AlphaBetaRatioOverride = S.AlphaBetaRatioOverride;
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
                if (_model.CurrentProtocol.Checklist != null)
                {
                    var C = _model.CurrentProtocol.Checklist;
                    DbPC.DbProtocol = DbP;
                    DbPC.AlgorithmVolumeDose = (int)C.Algorithm.Value;
                    DbPC.FieldNormalizationMode = (int)C.FieldNormalizationMode.Value;
                    DbPC.AlgorithmResolution = C.AlgorithmResolution.Value;
                    DbPC.SliceSpacing = C.SliceSpacing.Value;
                    DbPC.HeterogeneityOn = C.HeterogeneityOn.Value;
                    //Couch
                    DbPC.SupportIndication = (int)C.SupportIndication.Value;
                    DbPC.CouchSurface = C.CouchSurface.Value;
                    DbPC.CouchInterior = C.CouchInterior.Value;
                    //Artifact
                    foreach (Artifact A in C.Artifacts)
                    {
                        DbArtifact DbA = Context.DbArtifacts.Create();
                        Context.DbArtifacts.Add(DbA);
                        DbA.DbProtocolChecklist = DbPC;
                        DbA.ProtocolStructure_ID = SessionProtocolStructure_Lookup[A.ProtocolStructureId.Value]; // will have been duplicated above;
                        DbA.HU = A.RefHU.Value;
                        DbA.ToleranceHU = A.ToleranceHU.Value;
                        DbA.DbProtocolChecklist = DbPC;
                    }
                    foreach (string CTDeviceId in C.CTDeviceIds)
                    {
                        DbCTDeviceId DbCTDI = Context.DbCTDeviceIds.FirstOrDefault(x => x.CTDeviceId.ToUpper() == CTDeviceId.ToUpper());
                        if (DbCTDI == null)
                        {
                            DbCTDI = Context.DbCTDeviceIds.Create();
                            DbCTDI.CTDeviceId = CTDeviceId;
                            DbCTDI.ProtocolChecklist = new List<DbProtocolChecklist>() { DbPC };
                        }
                        else
                            DbCTDI.ProtocolChecklist.Add(DbPC);
                    }
                }
                // structures
                foreach (ComponentModel SC in _model.CurrentProtocol.Components)
                {
                    DbSessionComponent DbC = Context.DbSessionComponents.Create();
                    DbC.ComponentType = (int)SC.ComponentType.Value;
                    Context.DbSessionComponents.Add(DbC);
                    DbC.ID = IDGenerator.GetUniqueId();
                    DbC.SessionID = DbS.ID;
                    DbC.ParentComponentID = SC.Id;
                    ComponentLookup.Add(SC.Id, DbC.ID);
                    DbC.ProtocolID = DbP.ID;
                    //Update
                    DbC.ComponentName = SC.ComponentName;
                    DbC.NumFractions = SC.NumFractions;
                    DbC.ReferenceDose = SC.TotalDose;
                    DbC.DisplayOrder = SC.DisplayOrder;
                    DbC.MinColOffset = SC.MinColOffset.Value;
                    DbC.PNVMax = SC.PNVMax.Value;
                    DbC.PNVMin = SC.PNVMin.Value;
                    DbC.PrescribedPercentage = SC.PrescribedPercentage.Value;
                    DbC.NumIso = SC.NumIso.Value;
                    DbC.MinColOffset = SC.MinColOffset.Value;
                    DbC.MinBeams = SC.MinBeams.Value;
                    DbC.MaxBeams = SC.MaxBeams.Value;
                    foreach (BeamViewModel B in SC.Beams)
                    {
                        var DbB = Context.DbBeams.Create();
                        Context.DbBeams.Add(DbB);
                        DbB.DbBeamGeometries = new List<DbBeamGeometry>();
                        DbB.ComponentID = DbC.ID;
                        DbB.JawTracking_Indication = (int)B.JawTracking_Indication.Value;
                        DbB.MaxColRotation = B.MaxColRotation.Value;
                        DbB.MaxMUWarning = B.MaxMUWarning.Value;
                        DbB.MinMUWarning = B.MinMUWarning.Value;
                        DbB.MaxX = B.MaxX.Value;
                        DbB.MinX = B.MinX.Value;
                        DbB.Technique = (int)B.Technique;
                        DbB.ToleranceTable = B.ToleranceTable.Value;
                        DbB.ProtocolBeamName = B.ProtocolBeamName;
                        DbB.MinColRotation = B.MinColRotation.Value;
                        DbB.MaxY = B.MaxY.Value;
                        DbB.MinY = B.MinY.Value;
                        DbB.CouchRotation = (double)B.CouchRotation.Value;
                        foreach (string Alias in B.EclipseAliases)
                        {
                            var DbBA = Context.DbBeamAliases.Create();
                            Context.DbBeamAliases.Add(DbBA);
                            DbBA.DbBeam = DbB;
                            DbBA.EclipseFieldId = Alias;
                        }
                        foreach (BeamGeometryInstance BG in B.ValidGeometries)
                        {
                            if (BG.Definition == null)
                            {
                                DbBeamGeometry DbBG = Context.DbBeamGeometries.FirstOrDefault(x => x.BeamGeometryID == BG.Definition.Id);
                                if (DbBG != null)
                                    DbB.DbBeamGeometries.Add(DbBG);
                            }
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
                    foreach (ConstraintModel Con in SC.ConstraintModels)
                    {
                        DbSessionConstraint DbO = Context.DbSessionConstraints.Create();
                        Context.DbSessionConstraints.Add(DbO);
                        DbO.ID = IDGenerator.GetUniqueId();
                        DbO.SessionID = DbS.ID;
                        DbO.ParentConstraintID = Con.ID;
                        DbO.ComponentID = ComponentLookup[Con.ComponentID];
                        // Update
                        DbO.PrimaryStructureID = StructureLookup[Con.PrimaryStructureId];
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
                        DbO.OriginalPrimaryStructureID = ConReferenceValues.PrimaryStructureId;
                        DbO.OriginalSecondaryStructureID = ConReferenceValues.ReferenceStructureId;
                        DbO.OriginalReferenceValue = ConReferenceValues.ReferenceValue;
                        DbO.OriginalReferenceType = (int)ConReferenceValues.ReferenceType;
                        DbO.OriginalConstraintType = (int)ConReferenceValues.ConstraintType;
                        DbO.OriginalConstraintScale = (int)ConReferenceValues.ConstraintScale;
                        DbO.OriginalReferenceScale = (int)ConReferenceValues.ReferenceScale;
                        DbO.OriginalConstraintValue = ConReferenceValues.ConstraintValue;
                        //Link Results to Asssessment
                        foreach (AssessmentViewModel A in _model.CurrentSession.Assessments)
                        {
                            ConstraintResultViewModel CRV = Con.GetResult(A.ID);
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
                        DbO.MajorViolation = Con.MajorViolation;
                        DbO.MinorViolation = Con.MinorViolation;
                        DbO.Stop = Con.Stop;
                    }
                }
                foreach (var P in _model.GetPlanAssociations())
                    if (P != null)
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



                try
                {
                    await Context.SaveChangesAsync();
                    return true;
                }
                catch (Exception ex)
                {

                    MessageBox.Show(string.Format("{0} {1} {2}", ex.Message, ex.InnerException.InnerException, ex.StackTrace));
                    return false;
                }
            }
        }
        public async Task Save_UpdateProtocol()
        {
            using (SquintDBModel Context = new SquintDBModel(_model.DbPaths))
            {
                //Update Protocol
                DbLibraryProtocol DbP = Context.DbLibraryProtocols.Find(_model.CurrentProtocol.ID);
                if (DbP == null) // new protocol
                {
                    int renameCounter = 1;
                    string originalName = _model.CurrentProtocol.ProtocolName;
                    while (Context.DbLibraryProtocols.Any(x => x.ProtocolName == _model.CurrentProtocol.ProtocolName))
                    {
                        _model.CurrentProtocol.ProtocolName = string.Format("{0}{1}", originalName, renameCounter++);
                    }
                    DbP = Context.DbLibraryProtocols.Create();
                    Context.DbLibraryProtocols.Add(DbP);
                    DbP.ID = _model.CurrentProtocol.ID;
                }
                DbP.TreatmentCentreID = Context.DbTreatmentCentres.FirstOrDefault(x => x.TreatmentCentre == (int)_model.CurrentProtocol.TreatmentCentre.Value).ID;
                DbP.DbUser_Approver = Context.DbUsers.FirstOrDefault(x => x.ARIA_ID == _model.SquintUser);
                DbP.DbUser_ProtocolAuthor = Context.DbUsers.FirstOrDefault(x => x.ARIA_ID == _model.SquintUser);
                DbP.TreatmentSiteID = Context.DbTreatmentSites.FirstOrDefault(x => x.TreatmentSite == (int)_model.CurrentProtocol.TreatmentSite.Value).ID;
                DbP.ProtocolTypeID = Context.DbProtocolTypes.FirstOrDefault(x => x.ProtocolType == (int)_model.CurrentProtocol.ProtocolType).ID;
                DbP.ApprovalLevelID = Context.DbApprovalLevels.FirstOrDefault(x => x.ApprovalLevel == (int)_model.CurrentProtocol.ApprovalLevel).ID;
                DbP.LastModifiedBy = _model.SquintUser;
                DbP.LastModified = DateTime.Now.ToBinary();

                DbP.ProtocolName = _model.CurrentProtocol.ProtocolName;
                DbP.Comments = _model.CurrentProtocol.Comments;
                //Update Checklist
                Save_UpdateProtocolCheckList(Context, DbP);
               
                //Update Components
                foreach (ComponentModel SC in _model.CurrentProtocol.Components)
                {
                    Save_UpdateComponent(Context, SC, DbP.ID, false);
                    foreach (ConstraintModel con in SC.ConstraintModels)
                    {
                        Save_UpdateConstraint(Context, con, con.ComponentID, con.PrimaryStructureId, con.ReferenceStructureId, false);
                    }
                }
               
                foreach (StructureModel S in _model.CurrentProtocol.Structures)
                {
                    DbProtocolStructure DbS;
                    if (S.ToRetire && !S.isCreated)
                    {
                        DbS = Context.DbProtocolStructures.Find(S.ID);
                        Context.DbProtocolStructures.Remove(DbS);
                        continue;
                    }
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
                    DbS.AlphaBetaRatioOverride = S.AlphaBetaRatioOverride;
                    DbS.DisplayOrder = S.DisplayOrder;

                    Save_UpdateStructureCheckList(Context, DbS, S);
                    Save_UpdateStructureAliases(Context, DbS, S);
                }
                try
                {
                    await Context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Unable to update protocol.", "Error during save");
                }
            }

        }
        public void Save_DuplicateProtocol()
        {
            using (SquintDBModel Context = new SquintDBModel(_model.DbPaths))
            {
                // Name check
                if (Context.DbLibraryProtocols.Select(x => x.ProtocolName).Contains(_model.CurrentProtocol.ProtocolName))
                    _model.CurrentProtocol.ProtocolName = _model.CurrentProtocol.ProtocolName + "_copy";
                //Duplicate Protocol
                DbLibraryProtocol DbP = Context.DbLibraryProtocols.Create();
                Context.DbLibraryProtocols.Add(DbP);
                DbP.ID = IDGenerator.GetUniqueId();
                DbP.ProtocolName = _model.CurrentProtocol.ProtocolName;
                DbP.ApproverID = 1;
                DbP.AuthorID = 1;
                DbP.TreatmentCentreID = Context.DbTreatmentCentres.FirstOrDefault(x => x.TreatmentCentre == (int)_model.CurrentProtocol.TreatmentCentre.Value).ID;
                DbP.TreatmentSiteID = Context.DbTreatmentSites.FirstOrDefault(x => x.TreatmentSite == (int)_model.CurrentProtocol.TreatmentSite.Value).ID;
                DbP.ApprovalLevelID = Context.DbApprovalLevels.FirstOrDefault(x => x.ApprovalLevel == (int)ApprovalLevels.Unapproved).ID;
                DbP.ProtocolTypeID = Context.DbProtocolTypes.FirstOrDefault(x => x.ProtocolType == (int)_model.CurrentProtocol.ProtocolType).ID;
                DbP.LastModifiedBy = _model.SquintUser;
                DbP.LastModified = DateTime.Now.ToBinary();
                DbP.ProtocolParentID = _model.CurrentProtocol.ID;
                DbP.Comments = _model.CurrentProtocol.Comments;
                Save_UpdateProtocolCheckList(Context, DbP);
                //Update Components
                Dictionary<int, int> ComponentLookup = new Dictionary<int, int>();
                Dictionary<int, int> StructureLookup = new Dictionary<int, int>();
                StructureLookup.Add(1, 1); // add dummy structure
                foreach (StructureModel S in _model.CurrentProtocol.Structures)
                {
                    DbProtocolStructure DbS = Context.DbProtocolStructures.Create();
                    Context.DbProtocolStructures.Add(DbS);
                    DbS.ID = IDGenerator.GetUniqueId(); // because we are creating new
                    StructureLookup.Add(S.ID, DbS.ID); // save map from old to new Id
                    DbS.ProtocolID = DbP.ID;
                    DbS.ProtocolStructureName = S.ProtocolStructureName;
                    DbS.StructureLabelID = S.StructureLabelID;
                    DbS.DisplayOrder = S.DisplayOrder;
                    Save_UpdateStructureCheckList(Context, DbS, S);
                    Save_UpdateStructureAliases(Context, DbS, S);
                }
                foreach (ComponentModel SC in _model.CurrentProtocol.Components)
                {
                    var newID = Save_UpdateComponent(Context, SC, DbP.ID, true);
                    if (newID != null)
                        ComponentLookup.Add(SC.Id, (int)newID);
                    foreach (ConstraintModel Con in SC.ConstraintModels)
                    {
                        if (ComponentLookup.ContainsKey(Con.ComponentID))
                            Save_UpdateConstraint(Context, Con, ComponentLookup[Con.ComponentID], StructureLookup[Con.PrimaryStructureId], StructureLookup[Con.ReferenceStructureId], true);
                    }
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
        public bool Delete_Protocol(int Id)
        {
            using (var Context = new SquintDBModel(_model.DbPaths))
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

        private int? Save_UpdateComponent(SquintDBModel Context, ComponentModel comp, int protocolId, bool createCopy)
        {
            DbComponent DbC;
            if (comp.ToRetire)
            {
                DbC = Context.DbComponents.Find(comp.Id);
                if (DbC != null && comp.ToRetire && !createCopy) // don't remove from template if creating copy
                    Context.DbComponents.Remove(DbC);
                return null;
                //update checklist
            }
            else if (comp.Id < 0 || createCopy)
            {
                DbC = Context.DbComponents.Create();
                Context.DbComponents.Add(DbC);
                if (createCopy)
                    DbC.ID = IDGenerator.GetUniqueId();
                else
                    DbC.ID = comp.Id;
            }
            else
            {
                DbC = Context.DbComponents.Find(comp.Id);
            }
            // Update
            DbC.ComponentName = comp.ComponentName;
            DbC.ProtocolID = protocolId;
            DbC.DisplayOrder = comp.DisplayOrder;
            DbC.ComponentType = (int)comp.ComponentType.Value;
            DbC.NumFractions = comp.NumFractions;
            DbC.ReferenceDose = comp.TotalDose;

            //Update beams
            Save_UpdateBeamDefinition(Context, DbC, comp.Id, createCopy);

            //Update imaging protocols
            Save_UpdateImagingDefinitions(Context, DbC, comp.Id, createCopy);

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

        private void Save_UpdateConstraint(SquintDBModel Context, ConstraintModel Con, int ComponentId, int StructureId, int refStructureId, bool createCopy)
        {
            DbConstraint DbO;
            if ((Con.isCreated || createCopy) && !Con.ToRetire)
            {
                DbO = Context.DbConstraints.Create();
                Context.DbConstraints.Add(DbO);
                if (createCopy)
                    DbO.ID = IDGenerator.GetUniqueId();
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
                    DbCC.ChangeDescription = string.Format("Duplicated from protocol ({0})", _model.CurrentProtocol.ProtocolName);
                DbCC.ChangeAuthor = _model.SquintUser;
                DbCC.ConstraintID = DbO.ID;
                DbCC.ConstraintString = Con.GetConstraintString();
                DbCC.ParentLogID = DbCC_ParentID;
                DbCC.Date = DateTime.Now.ToBinary();
                Context.DbConstraintChangelogs.Add(DbCC);
            }
        }

        // Checklist functions
        private void Save_UpdateProtocolCheckList(SquintDBModel Context, DbProtocol DbP)
        {
            DbProtocolChecklist DbPC = Context.DbProtocolChecklists.FirstOrDefault(x => x.ProtocolID == DbP.ID);
            if (DbPC == null)
            {
                DbPC = Context.DbProtocolChecklists.Create();
                Context.DbProtocolChecklists.Add(DbPC);
                DbPC.ID = IDGenerator.GetUniqueId();
                DbPC.DbProtocol = DbP;
            }
            if (DbPC.CTDeviceIds != null)
                DbPC.CTDeviceIds.Clear();
            foreach (string CTDeviceId in _model.CurrentProtocol.Checklist.CTDeviceIds)
            {
                DbCTDeviceId DBCTId = Context.DbCTDeviceIds.FirstOrDefault(x => x.CTDeviceId.ToUpper() == CTDeviceId.ToUpper());
                if (DBCTId == null)
                {
                    DBCTId = Context.DbCTDeviceIds.Create();
                    DBCTId.CTDeviceId = CTDeviceId;
                    Context.DbCTDeviceIds.Add(DBCTId);
                }
                DbPC.CTDeviceIds.Add(DBCTId);
            }


            DbPC.SliceSpacing = _model.CurrentProtocol.Checklist.SliceSpacing.Value;
            _model.CurrentProtocol.Checklist.SliceSpacing.AcceptChanges();
            DbPC.AlgorithmVolumeDose = (int)_model.CurrentProtocol.Checklist.Algorithm.Value;
            _model.CurrentProtocol.Checklist.Algorithm.AcceptChanges();
            DbPC.AlgorithmResolution = _model.CurrentProtocol.Checklist.AlgorithmResolution.Value;
            _model.CurrentProtocol.Checklist.AlgorithmResolution.AcceptChanges();
            DbPC.AirCavityCorrectionIMRT = _model.CurrentProtocol.Checklist.AirCavityCorrectionIMRT.Value;
            DbPC.AirCavityCorrectionVMAT = _model.CurrentProtocol.Checklist.AirCavityCorrectionVMAT.Value;
            DbPC.AlgorithmVMATOptimization = (int)_model.CurrentProtocol.Checklist.AlgorithmVMATOptimization.Value;
            DbPC.AlgorithmIMRTOptimization = (int)_model.CurrentProtocol.Checklist.AlgorithmIMRTOptimization.Value;
            //DbPC.Artifacts
            DbPC.CouchInterior = _model.CurrentProtocol.Checklist.CouchInterior.Value;
            _model.CurrentProtocol.Checklist.CouchInterior.AcceptChanges();
            DbPC.CouchSurface = _model.CurrentProtocol.Checklist.CouchSurface.Value;
            _model.CurrentProtocol.Checklist.CouchSurface.AcceptChanges();
            DbPC.FieldNormalizationMode = (int)_model.CurrentProtocol.Checklist.FieldNormalizationMode.Value;
            _model.CurrentProtocol.Checklist.FieldNormalizationMode.AcceptChanges();
            DbPC.HeterogeneityOn = _model.CurrentProtocol.Checklist.HeterogeneityOn.Value;
            _model.CurrentProtocol.Checklist.HeterogeneityOn.AcceptChanges();
            DbPC.SupportIndication = (int)_model.CurrentProtocol.Checklist.SupportIndication.Value;
            _model.CurrentProtocol.Checklist.SupportIndication.AcceptChanges();
        }

        private void Save_UpdateStructureCheckList(SquintDBModel Context, DbProtocolStructure DbS, StructureModel S)
        {
            if (DbS.DbStructureChecklist == null) // no existing checklist in db
            {
                DbStructureChecklist DbSC = Context.DbStructureChecklists.Create();
                DbSC.StructureChecklistID = DbS.ID;
                Context.DbStructureChecklists.Add(DbSC);
                DbSC.isPointContourChecked = (bool)S.CheckList.isPointContourChecked.Value;
                DbSC.PointContourThreshold = S.CheckList.PointContourVolumeThreshold.Value;
                S.CheckList.isPointContourChecked.AcceptChanges();
                S.CheckList.PointContourVolumeThreshold.AcceptChanges();
            }
            else
            {
                DbS.DbStructureChecklist.isPointContourChecked = (bool)S.CheckList.isPointContourChecked.Value;
                DbS.DbStructureChecklist.PointContourThreshold = S.CheckList.PointContourVolumeThreshold.Value;
            }
        }

        private void Save_UpdateStructureAliases(SquintDBModel Context, DbProtocolStructure DbS, StructureModel S)
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
                    DbSA.ID = IDGenerator.GetUniqueId();
                    Context.DbStructureAliases.Add(DbSA);
                }
                else
                {
                    DbSA.DisplayOrder = S.DefaultEclipseAliases.IndexOf(alias) + 1; // just update order
                }
            }
            foreach (DbStructureAlias DbSA in DbS.DbStructureAliases.Where(x => !S.DefaultEclipseAliases.Contains(x.EclipseStructureId)).ToList())
            {
                Context.DbStructureAliases.Remove(DbSA);
            }

        }

        private void Save_UpdateBeamDefinition(SquintDBModel Context, DbComponent DbC, int sourceComponentID, bool createCopy)
        {
            foreach (BeamViewModel B in _model.CurrentProtocol.Components.FirstOrDefault(x => x.Id == sourceComponentID).Beams.ToList())
            {
                DbBeam DbB = null;
                if (DbC.DbBeams != null)
                    DbB = DbC.DbBeams.FirstOrDefault(x => x.ID == B.ID);
                if (B.ToRetire)
                {
                    if (DbB != null && !createCopy) // don't delete if creating copy, just exclude from duplication
                    {
                        Context.DbBeams.Remove(DbB);
                    }
                    continue;
                }
                if (DbB == null || createCopy)
                {
                    DbB = Context.DbBeams.Create();
                    if (createCopy)
                        DbB.ID = IDGenerator.GetUniqueId();
                    else
                        DbB.ID = B.ID;
                    Context.DbBeams.Add(DbB);
                    DbB.DbComponent = DbC;
                    DbB.DbBoluses = new List<DbBolus>();
                    DbB.DbEnergies = new List<DbEnergy>();
                    DbB.DbBeamAliases = new List<DbBeamAlias>();
                    DbB.DbBeamGeometries = new List<DbBeamGeometry>();
                }

                if (B.Boluses.FirstOrDefault() != null)
                {
                    DbBolus DbBol = DbB.DbBoluses.FirstOrDefault();
                    if (DbBol == null)
                    {
                        DbBol = Context.DbBoluses.Create();
                        Context.DbBoluses.Add(DbBol);
                        DbBol.ID = IDGenerator.GetUniqueId();
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
                DbB.CouchRotation = B.CouchRotation.Value; 
                B.CouchRotation.AcceptChanges();
                DbB.DbEnergies.Clear();
                foreach (var energy in B.ValidEnergies)
                    DbB.DbEnergies.Add(Context.DbEnergies.Find((int)energy));
                DbB.DbBeamGeometries.Clear();
                foreach (var geometry in B.ValidGeometries)
                {
                    if (geometry.Definition != null)
                        DbB.DbBeamGeometries.Add(Context.DbBeamGeometries.Find(geometry.Definition.Id));
                }
                DbB.JawTracking_Indication = (int)B.JawTracking_Indication.Value;
                B.JawTracking_Indication.AcceptChanges();
                DbB.MaxColRotation = B.MaxColRotation.Value;
                B.MaxColRotation.AcceptChanges();
                DbB.MaxMUWarning = B.MaxMUWarning.Value;
                B.MaxMUWarning.AcceptChanges();
                DbB.MinMUWarning = B.MinMUWarning.Value;
                B.MinMUWarning.AcceptChanges();
                DbB.MaxX = B.MaxX.Value;
                B.MaxX.AcceptChanges();
                DbB.MinX = B.MinX.Value;
                B.MinX.AcceptChanges();
                DbB.MaxY = B.MaxY.Value;
                B.MaxY.AcceptChanges();
                DbB.MinY = B.MinY.Value;
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

        private void Save_UpdateImagingDefinitions(SquintDBModel Context, DbComponent DbC, int sourceComponentID, bool createCopy)
        {
            var ImagingProtocols = _model.CurrentProtocol.Components.FirstOrDefault(x => x.Id == sourceComponentID).ImagingProtocols;
            if (DbC.ImagingProtocols != null)
            {
                foreach (DbComponentImaging DbCI in DbC.ImagingProtocols)
                {
                    foreach (DbImaging DbI in DbCI.Imaging)
                    {
                        if (!ImagingProtocols.Contains((ImagingProtocolTypes)DbI.ImagingProtocol))
                            Context.DbImagings.Remove(DbI);
                    }
                }
            }

            foreach (ImagingProtocolTypes IP in _model.CurrentProtocol.Components.FirstOrDefault(x => x.Id == sourceComponentID).ImagingProtocols)
            {
                DbComponentImaging DbCI = DbC.ImagingProtocols.FirstOrDefault();
                if (DbCI == null || createCopy)
                {
                    DbCI = Context.DbComponentImagings.Create();
                    DbCI.ID = IDGenerator.GetUniqueId();
                    DbCI.DbComponent = DbC;
                    DbCI.Imaging = new List<DbImaging>();
                }
                if (!DbC.ImagingProtocols.FirstOrDefault().Imaging.Select(x => x.ImagingProtocol).Contains((int)IP)) // if imaging protocols don't contain IP
                {
                    DbImaging DbI = Context.DbImagings.Create();
                    DbI.ImagingProtocol = (int)IP;
                    DbI.DbComponentImaging = DbCI;
                    DbI.ImagingProtocolName = IP.ToString();
                }

            }

            foreach (BeamViewModel B in _model.CurrentProtocol.Components.FirstOrDefault(x => x.Id == sourceComponentID).Beams.ToList())
            {
                DbBeam DbB = null;
                if (DbC.DbBeams != null)
                    DbB = DbC.DbBeams.FirstOrDefault(x => x.ID == B.ID);
                if (B.ToRetire)
                {
                    if (DbB != null && !createCopy) // don't delete if creating copy, just exclude from duplication
                    {
                        Context.DbBeams.Remove(DbB);
                    }
                    continue;
                }
                if (DbB == null || createCopy)
                {
                    DbB = Context.DbBeams.Create();
                    if (createCopy)
                        DbB.ID = IDGenerator.GetUniqueId();
                    else
                        DbB.ID = B.ID;
                    Context.DbBeams.Add(DbB);
                    DbB.DbComponent = DbC;
                    DbB.DbBoluses = new List<DbBolus>();
                    DbB.DbEnergies = new List<DbEnergy>();
                    DbB.DbBeamAliases = new List<DbBeamAlias>();
                    DbB.DbBeamGeometries = new List<DbBeamGeometry>();
                }

                if (B.Boluses.FirstOrDefault() != null)
                {
                    DbBolus DbBol = DbB.DbBoluses.FirstOrDefault();
                    if (DbBol == null)
                    {
                        DbBol = Context.DbBoluses.Create();
                        Context.DbBoluses.Add(DbBol);
                        DbBol.ID = IDGenerator.GetUniqueId();
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
                DbB.CouchRotation = B.CouchRotation.Value;
                B.CouchRotation.AcceptChanges();
                DbB.DbEnergies.Clear();
                foreach (var energy in B.ValidEnergies)
                    DbB.DbEnergies.Add(Context.DbEnergies.Find((int)energy));
                DbB.DbBeamGeometries.Clear();
                foreach (var geometry in B.ValidGeometries)
                {
                    if (geometry.Definition != null)
                        DbB.DbBeamGeometries.Add(Context.DbBeamGeometries.Find(geometry.Definition.Id));
                }
                DbB.JawTracking_Indication = (int)B.JawTracking_Indication.Value;
                B.JawTracking_Indication.AcceptChanges();
                DbB.MaxColRotation = B.MaxColRotation.Value;
                B.MaxColRotation.AcceptChanges();
                DbB.MaxMUWarning = B.MaxMUWarning.Value;
                B.MaxMUWarning.AcceptChanges();
                DbB.MinMUWarning = B.MinMUWarning.Value;
                B.MinMUWarning.AcceptChanges();
                DbB.MaxX = B.MaxX.Value;
                B.MaxX.AcceptChanges();
                DbB.MinX = B.MinX.Value;
                B.MinX.AcceptChanges();
                DbB.MaxY = B.MaxY.Value;
                B.MaxY.AcceptChanges();
                DbB.MinY = B.MinY.Value;
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
