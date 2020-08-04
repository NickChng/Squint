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

namespace SquintScript
{
    //public static partial class Ctr
    //{
    //    //public static void UpdateChecklistReferenceValue(CheckTypes CheckType, object ReferenceValue)
    //    //{
    //    //    switch (CheckType)
    //    //    {
    //    //        case CheckTypes.Unset:
    //    //            MessageBox.Show("Error: Attempt to save undefined CheckType");
    //    //            break;
    //    //        case CheckTypes.SliceSpacing:
    //    //            CurrentProtocol.Checklist.SliceSpacing.Value = (double)ReferenceValue;
    //    //            break;
    //    //        default:
    //    //            MessageBox.Show(string.Format("Error: Attempt to save CheckType {0}", CheckType.Display()));
    //    //            break;

    //    //    }
    //    //}
      

    //    //public static async Task<double> GetVolumeAfterExpansion(PlanSelector ps, string StructureId, double Margin)
    //    //{
    //    //    AsyncPlan p = await DataCache.GetAsyncPlan(ps.PlanUID, ps.CourseId);
    //    //    return await p.CheckMargin(StructureId, Margin);
    //    //}
    //}
}
