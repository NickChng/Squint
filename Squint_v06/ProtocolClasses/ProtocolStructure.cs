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

namespace SquintScript
{
    public static partial class Ctr
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
            public ProtocolStructure(DbProtocolStructure DbO)
            {
                ID = DbO.ID;
                ProtocolStructureName = DbO.ProtocolStructureName;
                DisplayOrder = DbO.DisplayOrder;
                if (DbO.DbStructureAliases != null)
                {
                    foreach (var DbSA in DbO.DbStructureAliases.OrderBy(x=>x.DisplayOrder))
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
                _StructureLabel = new TrackedValue<StructureLabel>(DataCache.GetStructureLabel(DbO.StructureLabelID));
            }


            public ProtocolStructure(string NewStructureName, int StructureLabelID_in = 1)
            {
                ID = Ctr.IDGenerator();
                isCreated = true;
                CheckList = new StructureCheckList();
                ProtocolStructureName = NewStructureName;
                DisplayOrder = DataCache.GetAllProtocolStructures().Count() + 1;
                ProtocolID = DataCache.CurrentProtocol.ID;
                _StructureLabel = new TrackedValue<StructureLabel>(GetStructureLabel());

            }
            public ObservableCollection<string> DefaultEclipseAliases { get; set; } = new ObservableCollection<string>();
            public string GetStructureDescription(bool GetParentValues = false)
            {
                return string.Format("{0} (Label: {1}, \u03B1/\u03B2={2})", ProtocolStructureName,
                    DataCache.SquintDb.Context.DbStructureLabels.Find(StructureLabelID).StructureLabel,
                    DataCache.SquintDb.Context.DbStructureLabels.Find(StructureLabelID).AlphaBetaRatio);
            }

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
            public ExceptionTypes ExceptionType
            {
                get;
                private set;
            }
            public int ID { get; private set; }
            public int DisplayOrder { get; set; }
            public bool isCreated { get; private set; } = false;
            public int ProtocolID { get; private set; }
            public string ProtocolStructureName { get; set; }
            public StructureCheckList CheckList { get; set; }
            public string AssignedStructureId { get; set; } = "";

            public System.Windows.Media.Color? GetStructureColor(string SSUID)
            {

                if (AssignedStructureId != "")
                    return DataCache.GetStructureSet(SSUID).GetStructure(AssignedStructureId).Color;
                else
                    return null;
            }
            public string EclipseStructureLabel(string SSUID)
            {
                if (AssignedStructureId == "")
                    return "";
                else
                    return DataCache.GetLabelByCode(DataCache.GetStructureSet(SSUID).GetStructure(AssignedStructureId).Code);
            }
            public void Delete()
            {
                ProtocolStructureDeleting?.Invoke(this, ID);
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
                var AS = DataCache.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
                if (AS == null)
                    return -1;
                return await AS.GetVMS_NumParts();
            }

            public double? Volume(string SSUID)
            {
                var AS = DataCache.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
                if (AS == null)
                    return -1;
                else
                    return AS.Volume;
            }
            public async Task<List<double>> PartVolumes(string SSUID)
            {
                var AS = DataCache.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
                if (AS == null)
                    return null;
                return await AS.GetPartVolumes();
            }
            public async Task<int> NumParts(string SSUID)
            {
                var AS = DataCache.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
                if (AS == null)
                    return -1;
                return await AS.GetNumSeperateParts();
            }
            public double AssignedHU(string SSUID)
            {
                var AS = DataCache.GetStructureSet(SSUID).GetStructure(AssignedStructureId);
                if (AS == null)
                    return double.NaN;
                return Math.Round(AS.HU);
            }
            private void OnESPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ES")); // let subscribers know that the Eclipse structure internal properties have changed
                RaisePropertyChangedEvent(e.PropertyName);
            }
        }

    }
}

