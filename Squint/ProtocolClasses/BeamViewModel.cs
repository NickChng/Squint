using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Squint.Interfaces;
using PropertyChanged;

namespace Squint
{
    [AddINotifyPropertyChangedInterface]
    public class BeamViewModel
    {

        //References
        public int ID { get; private set; }
        public int ComponentID { get; private set; }

        private SquintModel _model;
        public bool ToRetire { get; set; } = false;
        //Properties
        public string ProtocolBeamName { get; set; } = "unset";
        public FieldTechniqueType Technique { get; set; } = FieldTechniqueType.Unset;
        public ObservableCollection<Energies> ValidEnergies { get; set; } = new ObservableCollection<Energies>();
        public TrackedValue<string> ToleranceTable { get; private set; }
        public TrackedValue<double?> MinMUWarning { get; private set; }
        public TrackedValue<double?> MaxMUWarning { get; private set; }
        public TrackedValue<double?> MinColRotation { get; private set; }
        public TrackedValue<double?> MaxColRotation { get; private set; }
        public TrackedValue<double?> CouchRotation { get; private set; }
        public TrackedValue<double?> MinX { get; private set; }
        public TrackedValue<double?> MaxX { get; private set; }
        public TrackedValue<double?> MinY { get; private set; }
        public TrackedValue<double?> MaxY { get; private set; }
        public TrackedValue<ParameterOptions> JawTracking_Indication { get; private set; }
        public ObservableCollection<string> EclipseAliases { get; private set; } = new ObservableCollection<string>();
        public ObservableCollection<BeamGeometryInstance> ValidGeometries { get; private set; } = new ObservableCollection<BeamGeometryInstance>();
        public List<BolusDefinition> Boluses { get; set; } = new List<BolusDefinition>();
        // Methods
        public void Delete()
        {
            BeamDeleted?.Invoke(this, ID);
        }

        // Events
        public event EventHandler<int> BeamDeleted;


        public BeamViewModel(DbBeam DbO, SquintModel model)
        {
            _model = model;
            ID = DbO.ID;
            ComponentID = DbO.ComponentID;
            ProtocolBeamName = DbO.ProtocolBeamName;
            Technique = (FieldTechniqueType)DbO.Technique;
            foreach (var E in DbO.DbEnergies)
                ValidEnergies.Add((Energies)E.ID);
            foreach (var A in DbO.DbBeamAliases)
                EclipseAliases.Add(A.EclipseFieldId);
            foreach (var G in DbO.DbBeamGeometries)
            {
                ValidGeometries.Add(new BeamGeometryInstance(G, _model));
            }
            foreach (var B in DbO.DbBoluses)
            {
                Boluses.Add(new BolusDefinition(B));
            }
            ToleranceTable = new TrackedValue<string>(DbO.ToleranceTable);
            MinMUWarning = new TrackedValue<double?>(DbO.MinMUWarning);
            MaxMUWarning = new TrackedValue<double?>(DbO.MaxMUWarning);
            MinColRotation = new TrackedValue<double?>(DbO.MinColRotation);
            MaxColRotation = new TrackedValue<double?>(DbO.MaxColRotation);
            CouchRotation = new TrackedValue<double?>(DbO.CouchRotation);
            JawTracking_Indication = new TrackedValue<ParameterOptions>((ParameterOptions)DbO.JawTracking_Indication);
            MinX = new TrackedValue<double?>(DbO.MinX);
            MaxX = new TrackedValue<double?>(DbO.MaxX);
            MinY = new TrackedValue<double?>(DbO.MinY);
            MaxY = new TrackedValue<double?>(DbO.MaxY);
        }
        public BeamViewModel(int ComponentId, SquintModel model)
        {
            ID = IDGenerator.GetUniqueId();
            _model = model;
            ComponentID = ComponentId;
            ProtocolBeamName = "New beam";
            Technique = FieldTechniqueType.Unset;
            foreach (Energies E in Enum.GetValues(typeof(Energies)))
                ValidEnergies.Add(E);
            EclipseAliases.Add(@"Field1");
            ToleranceTable = new TrackedValue<string>("");
            MinMUWarning = new TrackedValue<double?>(null);
            MaxMUWarning = new TrackedValue<double?>(null);
            MinColRotation = new TrackedValue<double?>(null);
            MaxColRotation = new TrackedValue<double?>(null);
            CouchRotation = new TrackedValue<double?>(null);
            JawTracking_Indication = new TrackedValue<ParameterOptions>(ParameterOptions.Unset);
            MinX = new TrackedValue<double?>(null);
            MaxX = new TrackedValue<double?>(null);
            MinY = new TrackedValue<double?>(null);
            MaxY = new TrackedValue<double?>(null);
        }
    }

}
