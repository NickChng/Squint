using PropertyChanged;
//using System.Windows.Forms;

namespace Squint
{
    [AddINotifyPropertyChangedInterface]
    public class StructureSetHeader
    {
        public string LinkedPlanId { get; private set; }
        public string StructureSetId { get; private set; }
        public string StructureSetUID { get; private set; }
        public StructureSetHeader(string structureSetId, string structureSetUID, string linkedPlanId)
        {
            StructureSetId = structureSetId;
            StructureSetUID = structureSetUID;
            LinkedPlanId = linkedPlanId;
        }

    }
}
