using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PropertyChanged;

namespace SquintScript
{
    [AddINotifyPropertyChangedInterface]
    public class Beam
    {

        //References
        public int ID { get; private set; }
        public int ComponentID { get; private set; }

        public bool ToRetire { get; set; } = false;
        //Properties
        public string ProtocolBeamName { get; set; } = "unset";
        public FieldType Technique { get; set; } = FieldType.Unset;
        public ObservableCollection<Energies> ValidEnergies { get; set; } = new ObservableCollection<Energies>();
        public TrackedValue<string> ToleranceTable { get; set; }
        public TrackedValue<double?> MinMUWarning { get; set; }
        public TrackedValue<double?> MaxMUWarning { get; set; }
        public TrackedValue<double?> MinColRotation { get; set; }
        public TrackedValue<double?> MaxColRotation { get; set; }
        public TrackedValue<double?> CouchRotation { get; set; }
        public TrackedValue<double?> MinX { get; set; }
        public TrackedValue<double?> MaxX { get; set; }
        public TrackedValue<double?> MinY { get; set; }
        public TrackedValue<double?> MaxY { get; set; }
        public TrackedValue<ParameterOptions> JawTracking_Indication { get; set; }
        public ObservableCollection<string> EclipseAliases { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<BeamGeometry> ValidGeometries { get; set; } = new ObservableCollection<BeamGeometry>();
        public List<BolusDefinition> Boluses { get; set; } = new List<BolusDefinition>();
        // Methods
        public void Delete()
        {
            BeamDeleted?.Invoke(this, ID);
        }

        // Events
        public event EventHandler<int> BeamDeleted;


        public Beam(DbBeam DbO)
        {
            ID = DbO.ID;
            ComponentID = DbO.ComponentID;
            ProtocolBeamName = DbO.ProtocolBeamName;
            Technique = (FieldType)DbO.Technique;
            foreach (var E in DbO.DbEnergies)
                ValidEnergies.Add((Energies)E.ID);
            foreach (var A in DbO.DbBeamAliases)
                EclipseAliases.Add(A.EclipseFieldId);
            foreach (var G in DbO.DbBeamGeometries)
            {
                ValidGeometries.Add(new BeamGeometry() { EndAngle = G.EndAngle, StartAngle = G.StartAngle, EndAngleTolerance = G.EndAngleTolerance, StartAngleTolerance = G.StartAngleTolerance, GeometryName = G.GeometryName, Trajectory = (Trajectories)G.Trajectory });
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
        public Beam(int ComponentId)
        {
            ID = IDGenerator.GetUniqueId();
            ComponentID = ComponentId;
            ProtocolBeamName = "New beam";
            Technique = FieldType.Unset;
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
