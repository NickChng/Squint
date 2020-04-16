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

namespace SquintScript
{
    public static partial class Ctr
    {
        public static void UpdateChecklistReferenceValue(CheckTypes CheckType, object ReferenceValue)
        {
            switch (CheckType)
            {
                case CheckTypes.Unset:
                    MessageBox.Show("Error: Attempt to save undefined CheckType");
                    break;
                case CheckTypes.SliceSpacing:
                    CurrentProtocol.Checklist.SliceSpacing.Value = (double)ReferenceValue;
                    break;
                    
            }
        }
        public static void UpdateChecklistReferenceRange(CheckTypes CheckType, object Min, object Max)
        {
            switch (CheckType)
            {
                case CheckTypes.Unset:
                    MessageBox.Show("Error: Attempt to save undefined CheckType");
                    break;
                case CheckTypes.PlanNormalization:
                    CurrentProtocol.Checklist.PNVMax.Value = (double)Max;
                    CurrentProtocol.Checklist.PNVMin.Value = (double)Min;
                    break;

            }
        }
        public static void Save_UpdateProtocolChecklist()
        {
            DataCache.Save_UpdateProtocolCheckList();
        }
    }
}
