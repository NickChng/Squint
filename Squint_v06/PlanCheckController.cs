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
        public static void UpdateChecklistReferenceValue(CheckTypes CheckType, double ReferenceValue)
        {
            switch (CheckType)
            {
                case CheckTypes.SliceSpacing:
                    CurrentProtocol.Checklist.SliceSpacing = ReferenceValue;
                    break;
            }
        }
        public static void Save_UpdateProtocolChecklist()
        {
            DataCache.Save_UpdateProtocolCheckList();
        }
    }
}
