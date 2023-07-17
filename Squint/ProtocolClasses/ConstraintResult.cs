using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Squint.Extensions;

namespace Squint
{
    public class ConstraintResult : INotifyPropertyChanged
        {
            //Events
            public event PropertyChangedEventHandler PropertyChanged;
            public event EventHandler<ConstraintResultRemovedArgs> ConstraintResultRemoved;
            private Constraint con;
            public class ConstraintResultRemovedArgs : EventArgs
            {
                public int ConstraintID; // this will be -1 if constraint already deleted
                public int oldID; // 
                public int AssessmentID;
            }
            //public ConstraintResult(int Id, int AssessmentId, Constraint con_in)
            //{
            //    ID = Id;
            //    con = con_in;
            //    AssessmentID = AssessmentId;
            //    isLoaded = true;
            //}
            public ConstraintResult(int _AssessmentID, Constraint con_in)
            {
                isLoaded = false;
                ThresholdStatus = ReferenceThresholdTypes.Unset;
                ID = (int)IDGenerator.GetUniqueId();
                AssessmentID = _AssessmentID;
                con = con_in;

                SetResultString();
                // Subscribe to constraint
                con.PropertyChanged += OnConstraintPropertyChanged;

            }
            public readonly Object LockSimultaneousEvaluation = new Object();
            public int ID { get; private set; }
            public int AssessmentID { get; private set; }
            public ReferenceTypes ReferenceType { get { return con.ReferenceType; } }
            public int ConstraintID { get; private set; }
            public bool isCalculating = false;
            public string LinkedLabelName = "Not Linked";
            private double _ResultValue;
            public double ResultValue
            {
                get
                {
                    return _ResultValue;
                }
                set
                {
                    _ResultValue = value;
                    SetResultString();
                    isLoaded = false;
                }
            }
            private Dictionary<ConstraintResultStatusCodes, int> _StatusCodes = new Dictionary<ConstraintResultStatusCodes, int>(); // code to entity ID
            public List<ConstraintResultStatusCodes> StatusCodes
            {
                get { return _StatusCodes.Keys.ToList(); }
            }
            public bool isLoaded { get; private set; }
            private string _ResultString;
            private void SetResultString()
            {
                if (!con.isValid())
                    _ResultString = "";
                switch (con.GetReferenceUnit())
                {
                    case ConstraintUnits.cc:
                        {
                            _ResultString = string.Format(@"{0:0.#} {1}", ResultValue, ConstraintUnits.cc.Display());
                            break;
                        }
                    case ConstraintUnits.cGy:
                        {
                            _ResultString = string.Format(@"{0:0} {1}", ResultValue, ConstraintUnits.cGy.Display());
                            break;
                        }
                    case ConstraintUnits.Percent:
                        {
                            _ResultString = string.Format(@"{0:0.#} {1}", ResultValue, ConstraintUnits.Percent.Display());
                            break;
                        }
                    case ConstraintUnits.Unset:
                        {
                            _ResultString = string.Format(@"{0} {1}", ResultValue, ConstraintUnits.Unset.Display());
                            break;
                        }
                    case ConstraintUnits.Multiple:
                        {
                            _ResultString = string.Format(@"{0:0.##} {1}", ResultValue, ConstraintUnits.Multiple.Display());
                            break;
                        }
                    default:
                        _ResultString = "";
                        break;
                }
            }
            public string ResultString
            {
                get
                {
                    if (StatusCodes.Contains(ConstraintResultStatusCodes.StructureEmpty) ||
                        StatusCodes.Contains(ConstraintResultStatusCodes.StructureNotFound) ||
                        StatusCodes.Contains(ConstraintResultStatusCodes.NotLinked) ||
                        StatusCodes.Contains(ConstraintResultStatusCodes.LinkedPlanError) ||
                        StatusCodes.Contains(ConstraintResultStatusCodes.ConstraintUndefined) ||
                        StatusCodes.Contains(ConstraintResultStatusCodes.ErrorUnspecified))
                        return "-";
                    else
                        return _ResultString;
                }
            }
            public void ClearStatusCodes()
            {
                _StatusCodes.Clear();
            }
            public void AddStatusCode(ConstraintResultStatusCodes Code)
            {
                if (_StatusCodes.ContainsKey(Code))
                    return;
                else
                {
                    _StatusCodes.Add(Code, IDGenerator.GetUniqueId());
                }

            }
            public bool HasActiveStatus()
            {
                if (StatusCodes.Count > 0)
                {
                    return true;
                }
                return false;
            }
            public string GetStatuses()
            {
                string returnString = "";
                foreach (ConstraintResultStatusCodes Code in _StatusCodes.Keys)
                {
                    returnString = returnString + Code.Display() + "\r\n";
                }
                return returnString;
            }
            public ReferenceThresholdTypes ThresholdStatus;
            private void OnConstraintPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                SetResultString();
            }
        }
}

