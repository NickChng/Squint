using PropertyChanged;

namespace SquintScript
{
        [AddINotifyPropertyChangedInterface]
        public class Artifact
        {
            public Artifact(DbArtifact DbA)
            {
                RefHU = new TrackedValue<double?>(DbA.HU);
                ToleranceHU = new TrackedValue<double?>(DbA.ToleranceHU);
                ProtocolStructureId = new TrackedValue<int>(DbA.ProtocolStructure_ID);
            }
            public TrackedValue<double?> RefHU { get; set; }
            public TrackedValue<double?> ToleranceHU { get; set; }
            public TrackedValue<int> ProtocolStructureId { get; set; } 
        }
}

