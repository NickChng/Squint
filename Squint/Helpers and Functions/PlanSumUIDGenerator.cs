using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VMS.TPS.Common.Model.API;
using EApp = VMS.TPS.Common.Model.API.Application;
using System.Collections.Concurrent;

namespace SquintScript
{
    public static class PlanSumUIDGenerator
    {
        public static string GetUID(PlanSum ps)
        {
            string UID = string.Format("{0}_{1}", ps.Id, String.Join("+", ps.PlanSetups.OrderBy(x=>x.UID).Select(x => x.UID)));
            return UID;
        }
    }
}
