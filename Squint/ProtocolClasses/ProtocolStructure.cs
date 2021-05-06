using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PropertyChanged;
using System.Data;
using SquintScript.Extensions;
using System.Collections.ObjectModel;
using SquintScript.ViewModels;
using VMS.TPS.Common.Model.Types;

namespace SquintScript
{

    [AddINotifyPropertyChangedInterface]
    public class ProtocolStructure : ObservableObject
    {
        //public event PropertyChangedEventHandler PropertyChanged;
        //private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
        public EventHandler NewProtocolStructureCommitting;
        public EventHandler NewProtocolStructureCommitted;
        public EventHandler ProtocolStructureExceptionLoaded;
        public EventHandler ProtocolStructureChanged;
        public EventHandler<int> ProtocolStructureDeleting;

        public ProtocolStructure()
        {
            ID = 1;
            ProtocolStructureName = "Default";
            DisplayOrder = 1;
            AlphaBetaRatioOverride = null;
            CheckList = new StructureCheckList();
            _StructureLabel = new TrackedValue<StructureLabel>(null);
        }
        public ProtocolStructure(StructureLabel label_in, DbProtocolStructure DbO)
        {
            ID = DbO.ID;
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


        public ProtocolStructure(StructureLabel label_in, string NewStructureName)
        {
            ID = IDGenerator.GetUniqueId();
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
        
        public bool? IsHighResolution 
        {
            get
            {
                if (Ctr.CurrentStructureSet != null)
                    return GetStructureResolution(Ctr.CurrentStructureSet.UID);
                else
                    return null;
            }
                
        }
        public string AssignedStructureId { get; set; } = "";

        public double? AssignedHUInCurrentStructureSet // used for density override report
        {
            get 
            {
                if (Ctr.CurrentStructureSet != null)
                    return GetAssignedHU(Ctr.CurrentStructureSet.UID);
                else
                    return null;
            }
        }
                  

        public System.Windows.Media.Color? GetStructureColor(string SSUID)
        {

            if (AssignedStructureId != "")
                return Ctr.GetStructureSet(SSUID).GetStructure(AssignedStructureId).Color;
            else
                return null;
        }
        public string EclipseStructureLabel(string SSUID)
        {
            if (AssignedStructureId == "")
                return "";
            else
                return Ctr.GetStructureSet(SSUID).GetStructure(AssignedStructureId).Label;
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
            var AS = Ctr.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
            if (AS == null)
                return -1;
            return await AS.GetVMS_NumParts();
        }

        public async Task<Tuple<double, VVector>> GetMinArea(string SSUID)
        {
            var AS = Ctr.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
            if (AS == null)
                return null;
            return await AS.GetMinArea();
        }

        public double? Volume(string SSUID)
        {
            var AS = Ctr.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
            if (AS == null)
                return null;
            else
                return AS.Volume;
        }
        public async Task<List<double>> PartVolumes(string SSUID)
        {
            var AS = Ctr.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
            if (AS == null)
                return null;
            return await AS.GetPartVolumes();
        }
        public async Task<int> NumParts(string SSUID)
        {
            var AS = Ctr.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
            if (AS == null)
                return -1;
            return await AS.GetNumSeperateParts();
        }
        public double? GetAssignedHU(string SSUID)
        {
            AsyncStructure AS = Ctr.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
            if (AS == null)
                return null;
            return Math.Round(AS.HU);
        }

        public bool? GetStructureResolution(string SSUID)
        {
            AsyncStructure AS = Ctr.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
            if (AS == null)
                return null;
            return AS.IsHighResolution;
        }

    }

}

