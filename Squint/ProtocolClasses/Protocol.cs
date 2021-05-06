using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Documents;

namespace SquintScript
{
    public class Protocol
    {

        public List<Component> Components = new List<Component>();
        public List<ProtocolStructure> Structures = new List<ProtocolStructure>();

        public class ProtocolReferenceValues
        {
            public string ProtocolName { get; private set; }
            public ProtocolReferenceValues(string protocolName)
            {
                ProtocolName = protocolName;
            }
        }
        public class ProtocolArgs : EventArgs
        {
            public object Value;
        }
        public Protocol()
        {
            ID = IDGenerator.GetUniqueId();
            Checklist = new ProtocolChecklist(ID);
        }
        public Protocol(DbProtocol DbO)
        {
            ID = DbO.ID;
            ProtocolName = DbO.ProtocolName;
            CreationDate = DbO.CreationDate;
            ApprovalLevel = (ApprovalLevels)DbO.DbApprovalLevel.ApprovalLevel;
            Author = DbO.DbUser_ProtocolAuthor.ARIA_ID;
            LastModifiedBy = DbO.LastModifiedBy;
            ApprovingUser = DbO.DbUser_Approver.ARIA_ID;
            Comments = DbO.Comments;
            ProtocolType = (ProtocolTypes)DbO.DbProtocolType.ProtocolType;
            TreatmentCentre = new TrackedValue<TreatmentCentres>((TreatmentCentres)DbO.DbTreatmentCentre.TreatmentCentre);
            TreatmentSite = new TrackedValue<TreatmentSites>((TreatmentSites)DbO.DbTreatmentSite.TreatmentSite);
            foreach (DbTreatmentIntent DbTI in DbO.TreatmentIntents)
            {
                TreatmentIntents TI;
                if (Enum.TryParse(DbTI.Intent, out TI))
                    TreatmentIntents.Add(TI);
            }
        }
        //Properties
        public int ID { get; private set; }

        private TrackedValue<string> _ProtocolName = new TrackedValue<string>("");
        public string ProtocolName
        {
            get
            {
                return _ProtocolName.Value;
            }
            set
            {
                _ProtocolName.Value = value;
            }
        }

        public ProtocolTypes ProtocolType { get; set; }
        public string CreationDate { get; set; }
        public ApprovalLevels ApprovalLevel { get; set; }
        public string Author { get; set; }
        public string ApprovingUser { get; set; }
        public string LastModifiedBy { get; set; }
        public string Comments { get; set; } = "";

        public ObservableCollection<TreatmentIntents> TreatmentIntents { get; private set; } = new ObservableCollection<TreatmentIntents>();
        public TrackedValue<TreatmentCentres> TreatmentCentre { get; private set; } = new TrackedValue<TreatmentCentres>(TreatmentCentres.Unset);
        public TrackedValue<TreatmentSites> TreatmentSite { get; private set; } = new TrackedValue<TreatmentSites>(TreatmentSites.Unset);
        
        // Protocol Checklist
        public ProtocolChecklist Checklist { get; set; }

        public ProtocolReferenceValues GetReferenceValues()
        {
            return new ProtocolReferenceValues(_ProtocolName.ReferenceValue);
        }

    }

}

