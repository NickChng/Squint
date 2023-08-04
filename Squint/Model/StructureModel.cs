using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;
using System.Data;
using Squint.Extensions;
using System.Collections.ObjectModel;
using Squint.ViewModels;
using VMS.TPS.Common.Model.Types;
using System.Security.Cryptography;

namespace Squint
{

    [AddINotifyPropertyChangedInterface]
    public class StructureModel : ObservableObject
    {

        //public EventHandler NewProtocolStructureCommitting;
        //public EventHandler NewProtocolStructureCommitted;
        //public EventHandler ProtocolStructureExceptionLoaded;
        //public EventHandler ProtocolStructureChanged;
        //public EventHandler<int> ProtocolStructureDeleting;

        private AsyncESAPI _esapiContext;
        public StructureModel()
        {
            ID = 1;
            ProtocolStructureName = "Default";
            DisplayOrder = 1;
            AlphaBetaRatioOverride = null;
            CheckList = new StructureCheckList();
            _StructureLabel = new TrackedValue<StructureLabel>(null);
        }
        public StructureModel(AsyncESAPI esapiContext, StructureLabel label_in, DbProtocolStructure DbO)
        {
            ID = DbO.ID;
            _esapiContext = esapiContext;
            ProtocolStructureName = DbO.ProtocolStructureName;
            DisplayOrder = DbO.DisplayOrder;
            AlphaBetaRatioOverride = DbO.AlphaBetaRatioOverride;
            if (DbO.DbStructureAliases != null)
            {
                foreach (var DbSA in DbO.DbStructureAliases.OrderBy(x => x.DisplayOrder))
                    DefaultEclipseAliases.Add(DbSA.EclipseStructureId);
            }
            if (DbO.DbStructureChecklist != null)
            {
                CheckList = new StructureCheckList(DbO.DbStructureChecklist);
            }
            else CheckList = new StructureCheckList();
            var DbOS = DbO as DbSessionProtocolStructure;

            if (DbOS != null)
            {
                AssignedStructureId = DbOS.AssignedEclipseId;
            }
            _StructureLabel = new TrackedValue<StructureLabel>(label_in);
        }


        public StructureModel(AsyncESAPI esapiContext, StructureLabel label_in, string NewStructureName)
        {
            ID = IDGenerator.GetUniqueId();
            _esapiContext = esapiContext;
            isCreated = true;
            CheckList = new StructureCheckList();
            ProtocolStructureName = NewStructureName;
            _StructureLabel = new TrackedValue<StructureLabel>(label_in);

        }
        public ObservableCollection<string> DefaultEclipseAliases { get; set; } = new ObservableCollection<string>();

        private TrackedValue<StructureLabel> _StructureLabel;
        public int StructureLabelID
        {
            get
            {
                if (_StructureLabel.Value == null)
                    return 1;
                else
                    return _StructureLabel.Value.ID;
            }
        }
        public StructureLabel StructureLabel
        {
            get
            {
                return _StructureLabel.Value;
            }
            set
            {
                _StructureLabel.Value = value;
            }
        }
        public int ID { get; private set; }
        public int DisplayOrder { get; set; }
        public double? AlphaBetaRatioOverride { get; set; } = null;
        public bool ToRetire { get; private set; } = false;
        public void FlagForDeletion()
        {
            ToRetire = true;
        }
        public bool isCreated { get; private set; } = false;
        public int ProtocolID { get; set; }
        public string ProtocolStructureName { get; set; }
        public StructureCheckList CheckList { get; set; }

        public bool? IsHighResolution(string SSUID)
        {
            var SS = _esapiContext.Patient.GetStructureSet(SSUID);
            if (SS != null)
            {
                var AS = SS.GetStructure(AssignedStructureId);
                if (AS != null)
                    return AS.IsHighResolution;
                else
                    return null;
            }
            else
                return null;
        }
        public string AssignedStructureId { get; set; } = "";

        public double? GetAssignedHU(string SSUID) // used for density override report
        {

            var SS = _esapiContext.Patient.GetStructureSet(SSUID);
            if (SS != null)
            {
                AsyncStructure AS = SS.GetStructure(AssignedStructureId);
                if (AS == null)
                    return null;
                else
                    return Math.Round(AS.HU);
            }
            else
                return null;

        }


        public System.Windows.Media.Color? GetStructureColor(string SSUID)
        {

            if (AssignedStructureId != "")
            {
                var SS = _esapiContext.Patient.GetStructureSet(SSUID);
                if (SS != null)
                    return SS.GetStructure(AssignedStructureId).Color;
                else
                    return null;
            }
            else
                return null;
        }
        public string EclipseStructureLabel(string SSUID)
        {
            if (AssignedStructureId == "")
                return "";
            else
            {
                var SS = _esapiContext.Patient.GetStructureSet(SSUID);
                if (SS != null)
                    return SS.GetStructure(AssignedStructureId).Label;
                else
                    return string.Empty;
            }
        }

        public void ApplyAliasing(AsyncStructureSet SS)
        {
            // this will assign a structure Id if that structure exists in the plan ECP
            List<int> rank = new List<int>();
            List<string> matchedIDs = new List<string>();
            int AliasRank = 0;
            var ssid = SS.Id;
            var ssIds = SS.GetStructureIds();
            foreach (string Alias in DefaultEclipseAliases)
            {
                foreach (string StructureId in SS.GetStructureIds().OrderByDescending(x => x.Count()))
                {
                    if (StructureId.TrimStart(@"B_").ToUpper().Replace(@"_", @"") == Alias.ToUpper().Replace(@"_", @""))
                    {
                        rank.Add(AliasRank);
                        matchedIDs.Add(StructureId);
                        break;
                    }
                }
                AliasRank++;
            }
            if (matchedIDs.Count > 0)
            {
                var sorted_Ids = matchedIDs.Zip(rank, (Ids, Order) => new { Ids, Order }).OrderBy(x => x.Order).Select(x => x.Ids);
                AssignedStructureId = sorted_Ids.First();
            }
            else
            {
                AssignedStructureId = "";
            }
        }

        public async Task<int> VMS_NumParts(string SSUID)
        {
            var SS = _esapiContext.Patient.GetStructureSet(SSUID);
            if (SS != null)
            {
                var AS = SS.GetStructure(AssignedStructureId);
                if (AS == null)
                    return -1;
                return await AS.GetVMS_NumParts();
            }
            else
            {
                return -1;
            }
        }

        public async Task<Tuple<double, VVector>> GetMinArea(string SSUID)
        {
            var SS = _esapiContext.Patient.GetStructureSet(SSUID);
            if (SS != null)
            {
                var AS = SS.GetStructure(AssignedStructureId);
                if (AS == null)
                    return null;
                return await AS.GetMinArea();
            }
            else { return null; }
            
        }

        public double? Volume(string SSUID)
        {
            var SS = _esapiContext.Patient.GetStructureSet(SSUID);
            if (SS != null)
            {
                var AS = SS.GetStructure(AssignedStructureId);
                if (AS == null)
                    return null;
                else
                    return AS.Volume;
            }
            else
                return null;
        }
        public async Task<List<double>> PartVolumes(string SSUID)
        {
            var SS = _esapiContext.Patient.GetStructureSet(SSUID);
            if (SS != null)
            {
                var AS = SS.GetStructure(AssignedStructureId);
                if (AS == null)
                    return null;
                return await AS.GetPartVolumes();
            }
            else return null;
        }
        public async Task<int> NumParts(string SSUID)
        {
            var SS = _esapiContext.Patient.GetStructureSet(SSUID);
            if (SS != null)
            {
                var AS = SS.GetStructure(AssignedStructureId);
                if (AS == null)
                    return -1;
                return await AS.GetNumSeperateParts();
            }
            else
            {
                return -1;
            }
        }
    

    }

}

