using System;
using System.Linq;

namespace SquintScript
{
    public static partial class Ctr
    {
        public class Protocol
        {
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
                ID = Ctr.IDGenerator();
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
                _TreatmentCentre = new TrackedValue<TreatmentCentres>((TreatmentCentres)DbO.DbTreatmentCentre.TreatmentCentre);
                _TreatmentSite = new TrackedValue<TreatmentSites>((TreatmentSites)DbO.DbTreatmentSite.TreatmentSite);
                _TreatmentIntent = new TrackedValue<TreatmentIntents>((TreatmentIntents)DbO.TreatmentIntent);
                var DbChecklist = DbO.ProtocolChecklists.FirstOrDefault();
                if (DbChecklist != null)
                    Checklist = new ProtocolChecklist(DbChecklist);
                else
                    Checklist = new ProtocolChecklist(ID);
            }
            //Properties
            public int ID { get; private set; }

            private TrackedValue<string> _ProtocolName = new TrackedValue<string>("");
            public string ProtocolName
            {
                get { return _ProtocolName.Value; }
                set { _ProtocolName.Value = value; }
            }
            public ProtocolTypes ProtocolType { get; set; }
            public string CreationDate { get; set; }
            public ApprovalLevels ApprovalLevel { get; set; }
            public string Author { get; set; }
            public string ApprovingUser { get; set; }
            public string LastModifiedBy { get; set; }
            public string Comments { get; set; } = "";
            public TrackedValue<TreatmentCentres> _TreatmentCentre { get; private set; } = new TrackedValue<TreatmentCentres>(TreatmentCentres.Unset);
            public TreatmentCentres TreatmentCentre
            {
                get { return _TreatmentCentre.Value; }
                set { _TreatmentCentre.Value = value; }
            }
            public TrackedValue<TreatmentSites> _TreatmentSite { get; private set; } = new TrackedValue<TreatmentSites>(TreatmentSites.Unset);
            public TreatmentSites TreatmentSite
            {
                get { return _TreatmentSite.Value; }
                set { _TreatmentSite.Value = value; }
            }
            public TrackedValue<TreatmentIntents> _TreatmentIntent { get; private set; } = new TrackedValue<TreatmentIntents>(TreatmentIntents.Unset);
            public TreatmentIntents TreatmentIntent
            {
                get { return _TreatmentIntent.Value; }
                set { _TreatmentIntent.Value = value; }
            }
            // Protocol Checklist
            public ProtocolChecklist Checklist { get; set; }

            public void AcceptChanges()
            {
                _ProtocolName.AcceptChanges();
            }
            public ProtocolReferenceValues GetReferenceValues()
            {
                return new ProtocolReferenceValues(_ProtocolName.ReferenceValue);
            }

        }

    }
}

