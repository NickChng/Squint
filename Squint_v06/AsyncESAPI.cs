using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Data;
using System.IO;
using System.ComponentModel;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using EApp = VMS.TPS.Common.Model.API.Application;
using System.Collections.Concurrent;

//[assembly: ESAPIScript(IsWriteable = true)]

namespace SquintScript
{
    public class AsyncESAPI : IDisposable
    {
        class AppTaskScheduler : TaskScheduler, IDisposable
        {
            private BlockingCollection<Task> m_tasks;
            private Thread m_thread;

            public AppTaskScheduler()
            {
                m_tasks = new BlockingCollection<Task>();
                m_thread = new Thread(() =>
                {
                    foreach (var task in m_tasks.GetConsumingEnumerable())
                        TryExecuteTask(task);
                })
                {
                    IsBackground = true
                };
                m_thread.SetApartmentState(ApartmentState.STA);
                m_thread.Start();
            }

            public void Dispose()
            {
                if (m_tasks != null)
                {
                    m_tasks.CompleteAdding();
                    m_thread.Join();
                    m_tasks.Dispose();
                    m_thread = null;
                    m_tasks = null;
                }
            }

            protected override IEnumerable<Task> GetScheduledTasks()
            {
                return m_tasks.ToArray();
            }

            protected override void QueueTask(Task task)
            {
                m_tasks.Add(task);
            }

            protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
            {
                return Thread.CurrentThread.GetApartmentState() == ApartmentState.STA && TryExecuteTask(task);
            }

            public override int MaximumConcurrencyLevel => 1;
        }

        private readonly AppTaskScheduler m_taskScheduler = new AppTaskScheduler();
        private EApp m_application;
        private Patient P = null;
        //private Dictionary<int, PlanSetup> plans = new Dictionary<int, PlanSetup>();
        //private Dictionary<int, PlanSum> plansums = new Dictionary<int, PlanSum>();
        public bool isInit { get; private set; } = false;

        public AsyncESAPI(string username = null, string password = null)
        {
            m_application = Execute(new Func<EApp>(() =>
            {
                return EApp.CreateApplication();
            }));
            isInit = true;
        }

        public AsyncPatient OpenPatient(string PID)
        {
            if (!isInit)
                return null;
            if (P != null)
            {
                Execute(new Action<EApp>((application) =>
                {
                    application.ClosePatient();
                    P = null;
                }));
            }
            return Execute(new Func<EApp, AsyncPatient>((application) =>
            {
                if (P == null)
                {
                    P = application.OpenPatientById(PID);
                    //P.BeginModifications();
                }
                if (P == null)
                    return null;
                else
                    return new AsyncPatient(this, P);
            }));
        }

        public void ClosePatient()
        {
            if (!isInit)
                return;
            if (P != null)
            {
                Execute(new Action<EApp>((application) =>
                {
                    application.ClosePatient();
                    P = null;
                }));
            }
        }

        public string CurrentUserId()
        {
            if (!isInit)
                return "nchng"; // return me by default
            return Execute(new Func<EApp, string>((application) =>
            {
                return application.CurrentUser.Id;
            }));
        }
        public string CurrentUserName()
        {
            if (!isInit)
                return "nchng"; // return me by default
            return Execute(new Func<EApp, string>((application) =>
            {
                return application.CurrentUser.Name;
            }));
        }
        public async Task<T> ExecuteAsync<T>(Func<Application, T> func)
        {
            return await Task.Factory.StartNew(() => func.Invoke(m_application), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<Structure, T> func, Structure s)
        {
            return await Task.Factory.StartNew(() => func.Invoke(s), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<PlanSetup, T> func, PlanSetup p)
        {
            return await Task.Factory.StartNew(() => func.Invoke(p), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<Structure, PlanSetup, T> func, Structure s, PlanSetup p)
        {
            return await Task.Factory.StartNew(() => func.Invoke(s, p), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<PlanSetup, Patient, T> func, PlanSetup p, Patient pt)
        {
            return await Task.Factory.StartNew(() => func.Invoke(p, pt), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<PlanSum, T> func, PlanSum ps)
        {
            return await Task.Factory.StartNew(() => func.Invoke(ps), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }
        public async Task<T> ExecuteAsync<T>(Func<Patient, T> func)
        {
            return await Task.Factory.StartNew(() => func.Invoke(P), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }

        public T Execute<T>(Func<Application, T> func)
        {
            return Task.Run(async () => await ExecuteAsync(func)).Result;
        }
        public T Execute<T>(Func<Patient, T> func)
        {
            return Task.Run(async () => await ExecuteAsync(func)).Result;
        }
        protected T Execute<T>(Func<T> func)
        {
            return Task.Run(async () => await Task.Factory.StartNew(func, CancellationToken.None, TaskCreationOptions.None, m_taskScheduler)).Result;
        }

        public async Task ExecuteAsync(Action<Application> func)
        {
            await Task.Factory.StartNew(() => func.Invoke(m_application), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
        }


        public void Execute(Action<Application> func)
        {
            Task.Run(async () => await ExecuteAsync(func)).Wait();
        }

        public void Dispose()
        {
            if (m_application != null)
            {
                Task.Factory.StartNew(() => m_application.Dispose(), CancellationToken.None, TaskCreationOptions.None, m_taskScheduler);
                m_taskScheduler.Dispose();
            }
        }
    }
    public class AsyncPatient
    {
        private AsyncESAPI A;
        //public event EventHandler AsyncPatientClosing;
        public string Id { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public List<string> CourseIds { get; private set; } = new List<string>();
        public List<string> StructureSetUIDs { get; private set; } = new List<string>();
        //private Dictionary<string, AsyncCourse> Courses = new Dictionary<string, AsyncCourse>();
        public AsyncPatient(AsyncESAPI ACurrent, Patient p)
        {
            A = ACurrent;
            Id = p.Id;
            FirstName = p.FirstName;
            LastName = p.LastName;
            foreach (Course c in p.Courses)
            {
                CourseIds.Add(c.Id);
            }
            foreach (StructureSet SS in p.StructureSets)
            {
                StructureSetUIDs.Add(SS.UID);
            }
        }
        public AsyncStructureSet GetStructureSet(string ssuid)
        {
            if (StructureSetUIDs.Contains(ssuid))
            {
                return A.Execute(new Func<Patient, AsyncStructureSet>((p) =>
                {
                    return new AsyncStructureSet(A, p.StructureSets.FirstOrDefault(x => x.UID == ssuid));
                }));
            }
            else return null;
        }

        public List<Ctr.StructureSetHeader> GetAllStructureSets()
        {
            var L = new List<Ctr.StructureSetHeader>();
                return A.Execute(new Func<Patient, List<Ctr.StructureSetHeader>>((p) =>
                {
                    foreach (StructureSet SS in p.StructureSets)
                        L.Add(new Ctr.StructureSetHeader(SS.Id, SS.UID, ""));
                    return L;
                }));
            
        }
        public async Task<AsyncCourse> GetCourse(string CourseId, IProgress<int> progress)
        {
            if (!CourseIds.Contains(CourseId))
                return null;
            AsyncCourse c = await A.ExecuteAsync(new Func<Patient, AsyncCourse>((p) =>
            {
                if (p.Courses.Select(x => x.Id).Contains(CourseId))
                {
                    Course C = p.Courses.Where(x => x.Id == CourseId).Single();
                    return new AsyncCourse(A, C, p, progress);
                }
                else
                    return null;
            }));
            if (c != null)
                return c;
            else
                return null;
            //Courses.Add(CourseId, c);
            //    }
            //}
            //return Courses[CourseId];
        }
        //public AsyncCourse GetCourse(string CourseId) // non async
        //{
        //    if (!CourseIds.Contains(CourseId))
        //        return null;
        //    else
        //    {
        //        if (!Courses.ContainsKey(CourseId))
        //        {
        //            AsyncCourse c = A.Execute(new Func<Patient, AsyncCourse>((p) =>
        //            {
        //                if (p.Courses.Select(x => x.Id).Contains(CourseId))
        //                {
        //                    Course C = p.Courses.Where(x => x.Id == CourseId).Single();
        //                    return new AsyncCourse(A, C);
        //                }
        //                else
        //                    return null;
        //            }));
        //            if (c != null)
        //                Courses.Add(CourseId, c);
        //        }
        //    }
        //    return Courses[CourseId];
        //}
        public void Close()
        {
            A.Execute(new Action<EApp>((app) =>
            {
                app.ClosePatient();
            }));
        }
    }
    public class AsyncStructureSet
    {
        private AsyncESAPI A;
        public string Id { get; private set; }
        public string UID { get; private set; }
        private Dictionary<string, AsyncStructure> _Structures = new Dictionary<string, AsyncStructure>();
        public AsyncStructureSet(AsyncESAPI _A, StructureSet structureSet)
        {
            A = _A;
            Id = structureSet.Id;
            UID = structureSet.UID;
            foreach (Structure S in structureSet.Structures)
            {
                _Structures.Add(S.Id, new AsyncStructure(A, S, structureSet.Id, structureSet.UID));
            }
        }
        public IEnumerable<string> GetStructureIds()
        {
            return _Structures.Values.Select(x => x.Id);
        }
        public AsyncStructure GetStructure(string Id)
        {
            if (_Structures.ContainsKey(Id))
                return _Structures[Id];
            else
                return null;
        }
        public List<AsyncStructure> GetAllStructures()
        {
            return _Structures.Values.ToList();
        }
    }
    public class AsyncCourse
    {
        private readonly AsyncESAPI A;
        private Patient pt;
        private Course c;
        public string Id { get; private set; }
        //public List<AsyncPlan> Plans { get; private set; } = new List<AsyncPlan>();
        public List<Ctr.PlanDescriptor> PlanDescriptors { get; private set; } = new List<Ctr.PlanDescriptor>();
        private Dictionary<string, bool> isPlanASum = new Dictionary<string, bool>();
        public async Task<AsyncPlan> GetPlan(string Id)
        {
            AsyncPlan AP = null;
            return await A.ExecuteAsync(new Func<EApp, AsyncPlan>((app) =>
            {
                if (PlanDescriptors.Select(x => x.PlanId).Contains(Id))
                {
                    if (isPlanASum[Id])
                        AP = new AsyncPlan(A, c.PlanSums.FirstOrDefault(x => x.Id == Id), pt, this);
                    else
                        AP = new AsyncPlan(A, c.PlanSetups.FirstOrDefault(x => x.Id == Id), pt, this);
                }
                else
                    AP = null;
                return AP;
            }));
        }
        public async Task<AsyncPlan> GetPlanByUID(string UID)
        {
            AsyncPlan AP = null;
            return await A.ExecuteAsync(new Func<EApp, AsyncPlan>((app) =>
            {
                if (PlanDescriptors.Select(x=>x.PlanUID).Contains(UID))
                {
                    return new AsyncPlan(A, c.PlanSetups.FirstOrDefault(x => x.UID == UID), pt, this);
                }
                else
                    AP = null;
            return AP;
            }));
        }
        public AsyncCourse(AsyncESAPI ACurrent, Course cIn, Patient ptIn, IProgress<int> progress = null)
        {
            A = ACurrent;
            pt = ptIn;
            c = cIn;
            Id = c.Id;
            double totalplans = 1;
            if (progress != null)
                totalplans = c.PlanSetups.Count() + c.PlanSums.Count();
            foreach (var PD in c.PlanSetups.Select(x=> new Ctr.PlanDescriptor(x.Id, x.UID, x.StructureSet.UID)))
            {
                PlanDescriptors.Add(PD);
                isPlanASum.Add(PD.PlanId, false);
            }
            foreach (var PD in c.PlanSums.Select(x => new Ctr.PlanDescriptor(x.Id, null, x.StructureSet.UID)))
            {
                PlanDescriptors.Add(PD);
                isPlanASum.Add(PD.PlanId, true);
            }
            
        }
    }
    public class AsyncPlan
    {
        private AsyncESAPI A;
        private Patient pt = null;
        private PlanSetup p = null;
        private PlanSum ps = null;
        public AsyncCourse Course { get; private set; }
        public int HashId { get; private set; }
        public string Id { get; private set; }
        public string StructureSetId { get; private set; } = "";
        public string StructureSetUID { get; private set; } = "";
        public string UID { get; private set; }
        public bool Valid { get; private set; } = true;
        public bool IsDoseValid { get; private set; } = true;
        public int? NumFractions { get; private set; }
        public double? Dose { get; private set; }
        public DateTime HistoryDateTime { get; private set; }
        public ComponentTypes PlanType { get; private set; }
        public List<string> StructureIds { get; private set; } = new List<string>();
        public List<AsyncPlan> ConstituentPlans { get; private set; } = new List<AsyncPlan>();
        private Dictionary<string, Structure> _Structures = new Dictionary<string, Structure>();
        public AsyncPlan(AsyncESAPI ACurrent, PlanSetup pIn, Patient ptIn, AsyncCourse cIn)
        {
            A = ACurrent;
            pt = ptIn;
            p = pIn;
            Course = cIn;
            UID = p.UID;
            HistoryDateTime = p.HistoryDateTime;
            Id = p.Id;
            NumFractions = p.NumberOfFractions;
            HashId = Convert.ToInt32(p.Id.GetHashCode() + p.Course.Id.GetHashCode() + UID.GetHashCode());
            Dose = p.TotalDose.Dose;
            IsDoseValid = p.IsDoseValid;
            if (p.StructureSet == null)
                Valid = false;
            else
            {
                StructureSetId = p.StructureSet.Id;
                StructureSetUID = p.StructureSet.UID;
                foreach (Structure s in p.StructureSet.Structures)
                {

                    StructureIds.Add(s.Id);
                    _Structures.Add(s.Id, s);
                    var AS = new AsyncStructure(ACurrent, s, p.StructureSet.Id, p.StructureSet.UID);
                    Structures.Add(s.Id, AS);
                }
            }
            PlanType = ComponentTypes.Plan;
        }
        public AsyncPlan(AsyncESAPI ACurrent, PlanSum psIn, Patient ptIn, AsyncCourse cIn)
        {
            A = ACurrent;
            pt = ptIn;
            Course = cIn;
            ps = psIn;
            Id = ps.Id;
            var dates = ps.PlanSetups.Select(x => x.HistoryDateTime).OrderByDescending(x => x);
            HistoryDateTime = ps.PlanSetups.Select(x => x.HistoryDateTime).OrderByDescending(x => x).FirstOrDefault();
            PlanType = ComponentTypes.Sum;
            NumFractions = 0;
            Dose = 0;
            if (ps.StructureSet == null)
                Valid = false;
            else
            {
                StructureSetId = ps.StructureSet.Id;
                StructureSetUID = ps.StructureSet.UID;
                foreach (Structure s in ps.StructureSet.Structures)
                {
                    StructureIds.Add(s.Id);
                    _Structures.Add(s.Id, s);
                    Structures.Add(s.Id, new AsyncStructure(ACurrent, s, ps.StructureSet.Id, ps.StructureSet.UID));
                }
            }
            foreach (PlanSetup p in ps.PlanSetups)
            {
                ConstituentPlans.Add(new AsyncPlan(A, p, ptIn, cIn));
            }
            HashId = Convert.ToInt32(ps.Id.GetHashCode() + ps.Course.Id.GetHashCode() + string.Concat(ConstituentPlans.Select(x => x.UID)).GetHashCode());
            UID = String.Join("+", ConstituentPlans.Select(x => x.UID));
        }
        public Dictionary<string, AsyncStructure> Structures { get; private set; } = new Dictionary<string, AsyncStructure>();
        public async Task<double> GetSliceSpacing()
        {
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return Math.Abs(p.StructureSet.Image.ZRes);
                    else return -1;
                }
                else return -1;
            }));
        }
        public async Task<List<AsyncStructure>> GetBolus()
        {
            return await A.ExecuteAsync(new Func<EApp, List<AsyncStructure>>((app) =>
            {
                if (p.StructureSet != null)
                {
                    var Boluses = new List<AsyncStructure>();
                    foreach (Structure s in p.StructureSet.Structures.Where(x => x.DicomType == "BOLUS"))
                    {
                        var Bolus = new AsyncStructure(A, s, p.StructureSet.Id, p.StructureSet.UID);
                        Boluses.Add(Bolus);
                    }
                    return Boluses;
                }
                else return new List<AsyncStructure>();
            }));
        }
        public async Task<string> GetStudyId()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return p.StructureSet.Image.Series.Study.Id;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetAlgorithmModel()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.StructureSet != null)
                {
                    var CalcModel = p.PhotonCalculationModel.ToString();
                    if (CalcModel != null)
                        return CalcModel;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetCourseIntent()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p != null)
                    return p.Course.Intent;
                else if (ps != null)
                    return ps.Course.Intent;
                else
                    return "";

            }));
        }
        public async Task<double> GetCouchSurface()
        {
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                if (p == null)
                    return double.NaN;
                if (p.StructureSet != null)
                {
                    var Couch = p.StructureSet.Structures.FirstOrDefault(x => x.Id == @"CouchSurface");
                    if (Couch != null)
                    {
                        double HU = double.NaN;
                        Couch.GetAssignedHU(out HU);
                        return HU;
                    }
                    else return double.NaN;
                }
                else return double.NaN;
            }));
        }
        public async Task<double> GetCouchInterior()
        {
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                if (p == null)
                    return double.NaN;
                if (p.StructureSet != null)
                {
                    var Couch = p.StructureSet.Structures.FirstOrDefault(x => x.Id == @"CouchInterior");
                    if (Couch != null)
                    {
                        double HU = double.NaN;
                        Couch.GetAssignedHU(out HU);
                        return HU;
                    }
                    else return double.NaN;
                }
                else return double.NaN;
            }));
        }
        public async Task<string> GetHeterogeneityOn()
        {
            if (p == null)
                return "";
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.Dose != null)
                {
                    return (p.PhotonCalculationOptions.Where(x => x.Key == "HeterogeneityCorrection").FirstOrDefault().Value);
                }
                else return "";
            }));
        }
        public async Task<string> GetFieldNormalizationMode()
        {
            if (p == null)
                return "";
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p.Dose != null)
                {
                    return p.PhotonCalculationOptions.Where(x => x.Key == "FieldNormalizationType").FirstOrDefault().Value;
                }
                else return "";
            }));
        }
        public async Task<double> GetPNV()
        {
            if (p == null)
                return double.NaN;
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                if (p.Dose != null)
                {
                    return p.PlanNormalizationValue;
                }
                else return double.NaN;
            }));
        }
        public async Task<double> GetPrescribedPercentage()
        {
            if (p == null)
                return double.NaN;
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                if (p.Dose != null)
                {
                    return p.TreatmentPercentage * 100;
                }
                else return double.NaN;
            }));
        }
        public async Task<double> GetDoseGridResolution()
        {
            return await A.ExecuteAsync(new Func<EApp, double>((app) =>
            {
                double returnVal = double.NaN;
                if (p == null)
                    return double.NaN;
                if (p.StructureSet != null)
                {
                    if (p.Dose != null)
                    {
                        Double.TryParse(p.PhotonCalculationOptions.Where(x => x.Key == "CalculationGridSizeInCM").FirstOrDefault().Value, out returnVal);
                        return returnVal * 10;
                    }
                    else return returnVal;
                }
                else return double.NaN;
            }));
        }
        public async Task<string> GetSeriesId()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p == null)
                    return "";
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return p.StructureSet.Image.Series.Id;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetImageComments()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p == null)
                    return "";
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return p.StructureSet.Image.Comment;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetSeriesComments()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p == null)
                    return "";
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return p.StructureSet.Image.Series.Comment;
                    else return "";
                }
                else return "";
            }));
        }
        public async Task<string> GetLastModifiedDBy()
        {
            return await A.ExecuteAsync(new Func<EApp, string>((app) =>
            {
                if (p == null)
                    return "";
                return p.HistoryUserName;
            }));
        }
        public async Task<int?> GetNumSlices()
        {
            return await A.ExecuteAsync(new Func<EApp, int?>((app) =>
            {
                if (p == null)
                    return null;
                if (p.StructureSet != null)
                {
                    if (p.StructureSet.Image != null)
                        return Math.Abs(p.StructureSet.Image.ZSize);
                    else return -1;
                }
                else return -1;
            }));
        }
        public async Task<int?> GetFractions()
        {
            return await A.ExecuteAsync(new Func<EApp, int?>((app) =>
            {
                if (p == null)
                    return null;
                return p.NumberOfFractions;
            }));
        }
        public Task<Controls.NTODefinition> GetNTOObjective()
        {
            return A.ExecuteAsync(new Func<PlanSetup, Controls.NTODefinition>((p) =>
            {
                if (p == null)
                    return null;
                foreach (OptimizationParameter OP in p.OptimizationSetup.Parameters)
                {
                    var NTO = OP as OptimizationNormalTissueParameter;
                    if (NTO != null)
                    {
                        var SquintNTO = new Controls.NTODefinition();
                        SquintNTO.DistanceFromTargetBorderMM = NTO.DistanceFromTargetBorderInMM;
                        SquintNTO.EndDosePercentage = NTO.EndDosePercentage;
                        SquintNTO.FallOff = NTO.FallOff;
                        SquintNTO.isAutomatic = NTO.IsAutomatic;
                        SquintNTO.StartDosePercentage = NTO.StartDosePercentage;
                        SquintNTO.Priority = NTO.Priority;
                        return SquintNTO;
                    }
                    else
                        return null;
                }
                return null;
            }), p);
        }
        public Task<List<Ctr.ImagingFieldItem>> GetImagingFields()
        {
            return A.ExecuteAsync(new Func<PlanSetup, List<Ctr.ImagingFieldItem>>((p) =>
            {
                var L = new List<Ctr.ImagingFieldItem>();
                foreach (var F in p.Beams.Where(x => x.IsSetupField))
                {
                    L.Add(new Ctr.ImagingFieldItem(F, p.TreatmentOrientation));
                }
                return L;
            }), p);
        }
        public Task<List<Ctr.TxFieldItem>> GetTxFields()
        {
            return A.ExecuteAsync(new Func<PlanSetup, List<Ctr.TxFieldItem>>((p) =>
            {
                var L = new List<Ctr.TxFieldItem>();
                foreach (var F in p.Beams.Where(x => !x.IsSetupField))
                {
                    L.Add(new Ctr.TxFieldItem(p.Course.Id, p.Id, F, p.TreatmentOrientation));
                }
                return L;
            }), p);
        }
        public Task<VVector> GetPlanIsocentre()
        {
            return A.ExecuteAsync(new Func<PlanSetup, VVector>((p) =>
            {
                //
                return p.Beams.First().IsocenterPosition;
            }), p);
        }
        public Task<VVector> GetPatientCentre()
        {
            return A.ExecuteAsync(new Func<PlanSetup, VVector>((p) =>
            {
                var Iso = p.Beams.First().IsocenterPosition;
                var SeriesUID = p.StructureSet.Image.Series.UID;
                var CT = p.StructureSet.Image.Series.Images.Where(x => x.Id == p.StructureSet.Image.Id).FirstOrDefault();
                var zIso = Math.Abs(Math.Round((Iso.z - CT.Origin.z) / CT.ZRes));
                var BODYContour = p.StructureSet.Structures.First(x => x.Id == "BODY").GetContoursOnImagePlane((int)zIso);
                double x0 = BODYContour.SelectMany(x => x.Select(y => y.x)).Average();
                double y0 = BODYContour.SelectMany(x => x.Select(y => y.y)).Average();
                double z0 = BODYContour.SelectMany(x => x.Select(y => y.z)).Average();
                return new VVector(x0, y0, z0);
            }), p);
        }
        public Task<List<Controls.ObjectiveDefinition>> GetObjectiveItems()
        {
            return A.ExecuteAsync(new Func<PlanSetup, List<Controls.ObjectiveDefinition>>((p) =>
            {
                var L = new List<Controls.ObjectiveDefinition>();
                for (int c = 0; c < p.OptimizationSetup.Objectives.Count(); c++)
                {
                    var OD = new Controls.ObjectiveDefinition();
                    var OP = p.OptimizationSetup.Objectives.ToList()[c];
                    switch (OP)
                    {
                        case OptimizationPointObjective OPO:
                            OD.StructureId = OPO.StructureId;
                            OD.Dose = OPO.Dose.Dose;
                            OD.Priority = OPO.Priority;
                            OD.Volume = OPO.Volume;
                            OD.DvhType = Dvh_Types.V;
                            if (p.IsDoseValid)
                            {
                                OD.ResultDose = p.GetDoseAtVolume(OPO.Structure, OD.Volume, VolumePresentation.Relative, DoseValuePresentation.Absolute).Dose;
                                OD.ResultVolume = p.GetVolumeAtDose(OPO.Structure, OPO.Dose, VolumePresentation.Relative);
                            }
                            switch (OPO.Operator)
                            {
                                case OptimizationObjectiveOperator.Lower:
                                    OD.Type = ReferenceTypes.Lower;
                                    OD.DoseDifference = -(OD.ResultDose - OD.Dose);
                                    OD.VolDifference = -(OD.ResultVolume - OD.Volume);
                                    break;
                                case OptimizationObjectiveOperator.Upper:
                                    OD.Type = ReferenceTypes.Upper;
                                    OD.DoseDifference = (OD.ResultDose - OD.Dose);
                                    OD.VolDifference = (OD.ResultVolume - OD.Volume);
                                    break;
                            }
                            L.Add(OD);
                            break;
                        case OptimizationMeanDoseObjective OMO:
                            OD.StructureId = OMO.StructureId;
                            OD.Type = ReferenceTypes.Upper;
                            OD.DvhType = Dvh_Types.M;
                            OD.Dose = OMO.Dose.Dose;
                            OD.Priority = OMO.Priority;
                            OD.Volume = double.NaN;
                            if (p.IsDoseValid)
                            {
                                OD.ResultDose = p.GetDVHCumulativeData(OMO.Structure, DoseValuePresentation.Absolute, VolumePresentation.Relative, 0.1).MeanDose.Dose;
                                OD.DoseDifference = OD.ResultDose - OD.Dose;
                            }
                            L.Add(OD);
                            break;
                        default:
                            L.Add(OD);
                            break;
                    }
                }
                return L;
            }), p);
        }
        public Task<double> GetDoseAtVolume(string StructureId, double ConstraintValue, VolumePresentation VP, DoseValuePresentation DVP)
        {
            switch (PlanType)
            {
                case ComponentTypes.Plan:
                    return A.ExecuteAsync(new Func<PlanSetup, double>((p) =>
                    {

                        return (p.GetDoseAtVolume(_Structures[StructureId], ConstraintValue, VP, DVP)).Dose;
                    }), p);
                case ComponentTypes.Sum:
                    return A.ExecuteAsync(new Func<PlanSum, double>((ps) =>
                    {
                        return (ps.GetDoseAtVolume(_Structures[StructureId], ConstraintValue, VP, DVP)).Dose;
                    }), ps);
                default:
                    return null;
            }
        }
        public Task<double> GetVolumeAtDose(string StructureId, DoseValue Dose, VolumePresentation VP)
        {
            switch (PlanType)
            {
                case ComponentTypes.Plan:
                    return A.ExecuteAsync(new Func<PlanSetup, double>((p) =>
                    {
                        return (p.GetVolumeAtDose(_Structures[StructureId], Dose, VP));
                    }), p);
                case ComponentTypes.Sum:
                    return A.ExecuteAsync(new Func<PlanSum, double>((ps) =>
                    {
                        return (ps.GetVolumeAtDose(_Structures[StructureId], Dose, VP));
                    }), ps);
                default:
                    return null;
            }
        }
        public Task<double> GetMeanDose(string StructureId, VolumePresentation VP, DoseValuePresentation DVP, double binwidth)
        {
            switch (PlanType)
            {
                case ComponentTypes.Plan:
                    return A.ExecuteAsync(new Func<PlanSetup, double>((p) =>
                    {
                        DVHData dvh = p.GetDVHCumulativeData(_Structures[StructureId], DVP, VP, binwidth);
                        if (!ReferenceEquals(null, dvh))
                        {
                            return dvh.MeanDose.Dose;
                        }
                        else return -1;
                    }), p);
                case ComponentTypes.Sum:
                    return A.ExecuteAsync(new Func<PlanSum, double>((ps) =>
                    {
                        DVHData dvh = ps.GetDVHCumulativeData(_Structures[StructureId], DVP, VP, binwidth);
                        if (!ReferenceEquals(null, dvh))
                        {

                            return dvh.MeanDose.Dose;
                        }
                        else return -1;
                    }), ps);
                default:
                    return null;
            }
        }
        public Task<string> GetDoseSOPUID()
        {
            return A.ExecuteAsync(new Func<PlanSetup, string>((p) =>
            {
                return p.Dose.UID;
            }), p);
        }
        public Task<string> GetStructureSetSOPUID()
        {
            return A.ExecuteAsync(new Func<PlanSetup, string>((p) =>
            {
                return p.StructureSet.UID;
            }), p);
        }
        public Task<List<string>> GetCTSOPUIDS()
        {
            return A.ExecuteAsync(new Func<PlanSetup, List<string>>((p) =>
            {
                List<string> returnList = new List<string>();
                return returnList;
            }), p);
        }
        public Task<string> GetImageSeriesUID()
        {
            return A.ExecuteAsync(new Func<PlanSetup, string>((p) =>
            {
                return p.StructureSet.Image.Series.Study.UID;
            }), p);
        }
        public async Task<double> GetBolusThickness(string BolusId)
        {
            return await A.ExecuteAsync(new Func<PlanSetup, double>((pl) =>
            {
                Structure S = pl.StructureSet.Structures.FirstOrDefault(x => x.Id == BolusId && x.DicomType.ToUpper() == @"BOLUS");
                Structure B = pl.StructureSet.Structures.FirstOrDefault(x => x.DicomType.ToUpper() == @"EXTERNAL");
                if (S != null & B != null)
                {
                    int Rate = 1;
                    if (B.Volume > 50000)
                    {
                        Rate = 50;
                    }
                    else if (B.Volume > 10000)
                        Rate = 10;
                    var thickness = SquintStructure.SquintStructureHelper.GetThickness(S.MeshGeometry, B.MeshGeometry, Rate);
                    return thickness;
                }
                else
                    return double.NaN;
            }), p);
        }

        public async Task<double> CheckMargin(string StructureId, double MarginExpansion)
        {
            return await A.ExecuteAsync(new Func<PlanSetup, double>((pl) =>
            {
                Structure S = pl.StructureSet.Structures.FirstOrDefault(x => x.Id == StructureId);
                if (S != null)
                {
                    if (!S.IsEmpty)
                    {

                        var testExpansion = S.Margin(MarginExpansion);
                        var S2 = pl.StructureSet.AddStructure("Control", "Temp");
                        S2.SegmentVolume = testExpansion;
                        return S2.Volume;
                    }
                    else
                        return double.NaN;
                }
                else
                {
                    return double.NaN;
                }

            }), p);
        }
    }
    public class AsyncStructure
    {
        private AsyncESAPI A;
        private Structure S;
        public string Id { get; private set; }
        public string StructureSetID { get; private set; }
        public string StructureSetUID { get; private set; }
        public double Volume { get; private set; }
        public bool isHighResolution { get; private set; }
        public string DicomType { get; private set; }
        public System.Windows.Media.Color Color { get; set; }
        public string Code { get; private set; }
        public double HU { get; private set; }
        public bool isEmpty { get; private set; }

        public string Label { get; private set; }
        public AsyncStructure(AsyncESAPI ACurrent, Structure Sin, string SSID, string SSUID)
        {
            A = ACurrent;
            S = Sin;
            isEmpty = Sin.IsEmpty;
            isHighResolution = S.IsHighResolution;
            Color = S.Color;
            StructureSetUID = SSUID;
            StructureSetID = SSID;
            Code = S.StructureCodeInfos.FirstOrDefault().Code;
            double HU_out = Double.NaN;
            S.GetAssignedHU(out HU_out);
            HU = HU_out;
            DicomType = S.DicomType;
            Volume = S.Volume;
            Id = S.Id;

        }
        // Volume measurements
        private async Task<VVector[][]> GetContours(int planeId)
        {
            return await A.ExecuteAsync(new Func<Structure, VVector[][]>((S) =>
            {
                if (S != null)
                {
                    return S.GetContoursOnImagePlane(planeId);
                }
                else
                {
                    return null;
                }

            }), S);
        }
        private int _NumSeparateParts = -1;
        private List<double> _PartVolumes;
        private async Task<bool> CalcPartVolumes()
        {
            return await A.ExecuteAsync(new Func<Structure, bool>((S) =>
            {
                if (S != null)
                {
                    if (!S.IsEmpty)
                    {
                        _PartVolumes = Helpers.MeshHelper.Volumes(S.MeshGeometry);
                        _PartVolumes.RemoveAll(x => x < 0);
                        _NumSeparateParts = _PartVolumes.Count;
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    return false;
                }

            }), S);
        }
        public async Task<int> GetNumSeperateParts()
        {
            if (_NumSeparateParts > 0)
            {
                return _NumSeparateParts;
            }
            else
            {
                bool Success = await CalcPartVolumes();
                if (Success)
                    return _NumSeparateParts;
                else
                    return -1;
            }
        }
        public async Task<int> GetVMS_NumParts()
        {
            return await A.ExecuteAsync(new Func<Structure, int>((S) =>
            {
                if (S != null)
                {
                    if (!S.IsEmpty)
                        return S.GetNumberOfSeparateParts();
                    else
                        return -1;
                }
                else
                {
                    return -1;
                }

            }), S);
        }
        public async Task<List<double>> GetPartVolumes()
        {
            if (_NumSeparateParts > 0)
            {
                return _PartVolumes;
            }
            else
            {
                bool Success = await CalcPartVolumes();
                if (Success)
                    return _PartVolumes;
                else
                    return null;
            }
        }
    }
}
