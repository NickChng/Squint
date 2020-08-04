using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;
using PropertyChanged;
using SquintScript.Extensions;

namespace SquintScript
{
    public static partial class Ctr
    {
        [AddINotifyPropertyChangedInterface]
        public class PlanAssociation
        {
            //private Dictionary<int, bool> ComponentValidity = new Dictionary<int, bool>(); // component ID to validity
            public event EventHandler<int> PlanDeleting;
            public event EventHandler<int> PlanMappingChanged;
            public PlanAssociation(DbPlanAssociation DbO)
            {
                ID = DbO.ID;
                AssessmentID = DbO.AssessmentID;
                ComponentID = DbO.SessionComponentID;
                DataCache.GetComponent(ComponentID).PropertyChanged += OnComponentPropertyChanged;
            }
            public PlanAssociation(AsyncPlan p, int AssessmentID_in, int ComponentID_in)
            {
                ID = Ctr.IDGenerator();
                UpdateLinkedPlan(p, false);
                AssessmentID = AssessmentID_in;
                ComponentID = ComponentID_in;
                DataCache.GetComponent(ComponentID).PropertyChanged += OnComponentPropertyChanged;
            }
            public AsyncPlan LinkedPlan { get; private set; }
            public string PlanId { get; private set; } // deliberately don't reference these as the reference gets lost on a resync

            public string PlanUID { get; private set; }
            public string CourseId { get; private set; }
            public string StructureSetId { get; private set; }
            public string StructureSetUID { get; private set; }
            public DateTime LastModified { get; private set; }
            public string LastModifiedBy { get; private set; }
            public async void UpdateLinkedPlan(AsyncPlan p, bool ClearWarnings)
            {
                bool Changed = true;
                if (ClearWarnings) // only want to do this if Reassociation is done by user
                {
                    LoadWarning = false;
                    LoadWarningString = "";
                }
                LinkedPlan = p;
                if (p != null)
                {
                    PlanId = p.Id;
                    CourseId = p.Course.Id;
                    PlanUID = p.UID;
                    StructureSetId = p.StructureSetId;
                    StructureSetUID = p.StructureSetUID;
                    var prevLastMod = LastModified;
                    LastModified = p.HistoryDateTime;
                    LastModifiedBy = await p.GetLastModifiedDBy();
                    Changed = prevLastMod != LastModified;
                }
                else
                {
                    PlanId = "Not Linked";
                    PlanUID = "Not Linked";
                    CourseId = "Not Linked";
                    StructureSetId = "Not Linked";
                    StructureSetUID = "Not Linked";
                    LastModifiedBy = "Not Linked";
                    LastModified = DateTime.MinValue;

                }
                if (Changed)
                    //PlanMappingChanged?.Invoke(this, ComponentID);

                ValidateComponentAssociations();
            }

            public async Task LoadLinkedPlan(DbPlanAssociation DbO)
            {
                AsyncPlan p = await DataCache.GetAsyncPlan(DbO.PlanName, DbO.CourseName, (ComponentTypes)DbO.PlanType);
                UpdateLinkedPlan(p, false);
                if (LinkedPlan == null)
                {
                    LoadWarning = true;
                    LoadWarningString = string.Format(@"{0}\{1} Not Found!", DbO.CourseName, DbO.PlanName);
                }
                else
               if (LinkedPlan.HistoryDateTime != DateTime.FromBinary(DbO.LastModified))
                {
                    LoadWarning = true;
                    LoadWarningString = ComponentStatusCodes.ChangedSinceLastSession.Display();
                }
            }
            public bool LoadWarning { get; private set; } = false;
            public string LoadWarningString { get; private set; } = "";
            public string PID { get; private set; }
            public int AssessmentID { get; private set; }
            public int ComponentID { get; private set; }
            public ComponentTypes PlanType
            {
                get
                {
                    if (Linked)
                        return LinkedPlan.ComponentType;
                    else
                        return ComponentTypes.Unset;
                }
            }
            public int ID { get; private set; }
            public string UID
            {
                get
                {
                    if (LinkedPlan != null)
                        return LinkedPlan.UID;
                    else
                        return "";
                }
            }
            public IEnumerable<string> GetStructureNames
            {
                get
                {
                    if (LinkedPlan != null)
                        return LinkedPlan.StructureIds;
                    else
                        return Enumerable.Empty<string>();
                }
            }
            public bool? isStructureEmpty(string StructureID)
            {
                if (Linked)
                {
                    if (!LinkedPlan.StructureIds.Contains(StructureID))
                        return null;
                    else
                    {
                        if (LinkedPlan.Structures[StructureID].Volume == 0)
                            return true;
                        else
                            return false;
                    }
                }
                else return null;
            }
            public bool Linked
            {
                get { if (LinkedPlan != null) return true; else return false; }
            }
            //methods
            public List<ComponentStatusCodes> GetErrorCodes()
            {
                return ValidateComponentAssociations();
            }

            private List<ComponentStatusCodes> ValidateComponentAssociations()
            {
                // This performs error checking on the plans linked to this component and returns a status code indicating an error. 0 is no error
                List<ComponentStatusCodes> ErrorCodes = new List<ComponentStatusCodes>();
                if (!Linked)
                {
                    ErrorCodes.Add(ComponentStatusCodes.NotLinked);
                    return ErrorCodes;
                }
                if (!LinkedPlan.IsDoseValid)
                {
                    ErrorCodes.Add(ComponentStatusCodes.NoDoseDistribution);
                    return ErrorCodes;
                }
                ErrorCodes.Add(ComponentStatusCodes.Evaluable);
                Component SC = DataCache.GetComponent(ComponentID);
                if (SC.ComponentType.Value == ComponentTypes.Phase)
                {
                    if (!LinkedPlan.IsDoseValid)
                        ErrorCodes.Add(ComponentStatusCodes.NoDoseDistribution);
                    if (Math.Abs((double)LinkedPlan.Dose - SC.ReferenceDose) > 1)
                    {
                        // Reference dose mismatch
                        ErrorCodes.Add(ComponentStatusCodes.ReferenceDoseMismatch);
                    }
                    if (Math.Abs((int)LinkedPlan.NumFractions - SC.NumFractions) > 0)
                    {
                        // Num fraction mismatch
                        ErrorCodes.Add(ComponentStatusCodes.NumFractionsMismatch);
                    }
                }
                if (CurrentStructureSet != null)
                    if (LinkedPlan.StructureSetUID != CurrentStructureSet.UID)
                        ErrorCodes.Add(ComponentStatusCodes.StructureSetDiscrepancy);

                return ErrorCodes;
            }
            //public void Delete()
            //{
                //PlanDeleting?.Invoke(this, ID);
                //PlanMappingChanged?.Invoke(this, ComponentID);
            //}
            //Events
            private void OnComponentPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                ValidateComponentAssociations();
            }
        }

    }
}

