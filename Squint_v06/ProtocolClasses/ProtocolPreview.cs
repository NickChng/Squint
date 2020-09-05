//using System.Windows.Forms;

namespace SquintScript
{

    public class ProtocolPreview
    {
        public ProtocolPreview() { }
        public ProtocolPreview(int _ID, string ProtocolName_in)
        {
            ID = _ID;
            ProtocolName = ProtocolName_in;
        }

        public int ID { get; private set; } = 0;
        public string ProtocolName { get; private set; } = "None selected";
        public ProtocolTypes ProtocolType { get; set; } = ProtocolTypes.Unset;
        public TreatmentCentres TreatmentCentre { get; set; } = TreatmentCentres.Unset;
        public TreatmentSites TreatmentSite { get; set; } = TreatmentSites.Unset;
        public ApprovalLevels Approval { get; set; } = ApprovalLevels.Unset;
        public string LastModifiedBy { get; set; } = "";
    }

}
