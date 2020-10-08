using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using VMS.TPS.Common.Model.API;
using EApp = VMS.TPS.Common.Model.API.Application;


namespace SquintScript
{
    public class AsyncCourse
    {
        private readonly AsyncESAPI A;
        private Patient pt;
        private Course c;
        public string Id { get; private set; }
        private List<AsyncPlan> _plans = new List<AsyncPlan>();
        public List<PlanDescriptor> PlanDescriptors { get; private set; } = new List<PlanDescriptor>();
        private Dictionary<string, bool> isPlanASum = new Dictionary<string, bool>();
        private Dictionary<string, bool> isPlanASumByUID = new Dictionary<string, bool>();
        public async Task<AsyncPlan> GetPlan(string Id)
        {
            AsyncPlan AP = _plans.FirstOrDefault(x => x.Id == Id);
            if (AP != null)
                return AP;
            else
            {
                AP = await A.ExecuteAsync(new Func<EApp, AsyncPlan>((app) =>
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
                if (AP != null)
                    _plans.Add(AP);
                return AP;
            }
        }
        public async Task<AsyncPlan> GetPlanByUID(string UID)
        {
            AsyncPlan AP = _plans.FirstOrDefault(x => x.UID == UID);
            if (AP != null)
                return AP;
            else
            {
                return await A.ExecuteAsync(new Func<EApp, AsyncPlan>((app) =>
                {
                    if (PlanDescriptors.Select(x => x.PlanUID).Contains(UID))
                    {
                        string Id = PlanDescriptors.FirstOrDefault(x => x.PlanUID == UID).PlanId;
                        if (isPlanASumByUID[UID])
                            return new AsyncPlan(A, c.PlanSums.FirstOrDefault(x => x.Id == Id), pt, this);
                        else
                            return new AsyncPlan(A, c.PlanSetups.FirstOrDefault(x => x.UID == UID), pt, this);
                    }
                    else
                        AP = null;
                    if (AP != null)
                        _plans.Add(AP);
                    return AP;
                }));
            }
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
            foreach (var PD in c.PlanSetups.Where(x => x.StructureSet != null).Select(x => new PlanDescriptor(ComponentTypes.Phase, x.Id, x.UID, x.StructureSet.UID)))
            {
                PlanDescriptors.Add(PD);
                isPlanASum.Add(PD.PlanId, false);
                isPlanASumByUID.Add(PD.PlanUID, false);
            }
            foreach (var psum in c.PlanSums)
            {
                var PD = new PlanDescriptor(ComponentTypes.Sum, psum.Id, PlanSumUIDGenerator.GetUID(psum), psum.StructureSet.UID);
                PlanDescriptors.Add(PD);
                isPlanASum.Add(PD.PlanId, true);
                isPlanASumByUID.Add(PD.PlanUID, true);
            }

        }
    }
}
