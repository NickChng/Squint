//using System.Windows.Forms;

namespace SquintScript
{

    public class PlanDescriptor
    {
        public string PlanId { get; private set; }
        public string PlanUID { get; private set; }

        public string StructureSetUID { get; private set; }
        public ComponentTypes Type { get; set; }
        public PlanDescriptor(ComponentTypes type, string planId, string planUID, string structureSetUID)
        {
            Type = type;
            PlanId = planId;
            PlanUID = planUID;
            StructureSetUID = structureSetUID;
        }
    }
}
