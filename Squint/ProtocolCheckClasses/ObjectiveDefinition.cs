﻿using SquintScript.ViewModels;
using SquintScript.Extensions;

namespace SquintScript
{
    public class ObjectiveDefinition : ObservableObject
    {
        public string StructureId { get; set; } = "DefaultStructure";
        public string Definition
        {
            get
            {
                switch (DvhType)
                {
                    case Dvh_Types.M:
                        return string.Format("Mean Dose < {0:0.0} cGy", Dose);
                    case Dvh_Types.V:
                        return string.Format("V{0:0.0} cGy {1} {2:0.0}%", Dose, Type.Display(), Volume);
                    default:
                        return "";
                }
            }
        }
        public double ResultDose { get; set; } = double.NaN;
        public double ResultVolume { get; set; } = double.NaN;
        public string ResultString
        {
            get
            {
                switch (DvhType)
                {
                    case Dvh_Types.M:
                        return string.Format("{0:0.#} cGy", ResultDose);
                    case Dvh_Types.V:
                        return string.Format("{0:0.#} cGy, {1:0.#} %", ResultDose, ResultVolume);
                    default:
                        return "";
                }
            }
        }
        public Dvh_Types DvhType { get; set; } = Dvh_Types.Unset;
        public ReferenceTypes Type { get; set; } = ReferenceTypes.Unset;
        public double DoseDifference { get; set; } = 0;
        public double VolDifference { get; set; } = 0;
        public string DoseDifferenceString
        {
            get
            {
                return string.Format("{0:0.0} cGy", DoseDifference);
            }
        }
        public string VolDifferenceString
        {
            get
            {
                switch (DvhType)
                {
                    case Dvh_Types.V:
                        return string.Format(", {0:0.0} %", VolDifference);
                    default:
                        return "";
                }

            }
        }
        public string WarningText
        {
            get
            {
                if (isInactive)
                    return "No influence";
                else
                    return "";
            }
        }
        public bool isInactive
        {
            get
            {
                if (VolDifference < -0.1 || DoseDifference < -0.1)
                    return true;
                else return false;
            }
        }
        public double Priority { get; set; } = 80;
        public double PriorityWidth { get { return Priority / 2; } }
        public double Volume { get; set; } = 50;
        public double Dose { get; set; } = 1000;
    }




}
