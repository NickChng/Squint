using System;
using System.Collections.Generic;
using System.Linq;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using SquintScript.Extensions;


namespace SquintScript
{
    public class TxFieldItem
    {
        public class BolusInfo
        {
            public string Id;
            public double HU;
        }
        public TxFieldItem(string RefCourseId, string RefPlanId, VMS.TPS.Common.Model.API.Beam Field = null, VMS.TPS.Common.Model.Types.PatientOrientation O = VMS.TPS.Common.Model.Types.PatientOrientation.NoOrientation)
        {
            CourseId = RefCourseId;
            PlanId = RefPlanId;
            if (Field != null)
            {
                Id = Field.Id;
                switch (Field.Technique.Id)
                {
                    case "ARC":
                        Type = FieldType.ARC;
                        break;
                    case "SRS_ARC":
                        Type = FieldType.ARC;
                        break;
                    case "STATIC":
                        Type = FieldType.STATIC;
                        break;
                }
                switch (Field.EnergyModeDisplayName)
                {
                    case "6X":
                        Energy = Energies.Photons6;
                        break;
                    case "10X-FFF":
                        Energy = Energies.Photons10FFF;
                        break;
                    case "15X":
                        Energy = Energies.Photons15;
                        break;
                    case "10X":
                        Energy = Energies.Photons10;
                        break;
                }
                GantryDirection = Field.GantryDirection;
                Isocentre = Field.IsocenterPosition;
                CouchRotation = Field.ControlPoints.FirstOrDefault().PatientSupportAngle;
                GantryStart = Field.ControlPoints.FirstOrDefault().GantryAngle;
                GantryEnd = Field.ControlPoints.LastOrDefault().GantryAngle;
                CollimatorAngle = Field.ControlPoints.FirstOrDefault().CollimatorAngle;
                ToleranceTable = Field.ToleranceTableLabel;
                foreach (Bolus Bolus in Field.Boluses)
                {
                    BolusInfo BI = new BolusInfo();
                    BI.Id = Bolus.Id;
                    BI.HU = Math.Round(Bolus.MaterialCTValue);
                    BolusInfos.Add(BI);
                }
                var CP = Field.ControlPoints;
                if (CP.Count > 0)
                {
                    X1max = CP.Max(x => Math.Abs(x.JawPositions.X1)) / 10; // cm
                    X2max = CP.Max(x => x.JawPositions.X2) / 10;
                    Y1max = CP.Max(x => Math.Abs(x.JawPositions.Y1)) / 10;
                    Y2max = CP.Max(x => x.JawPositions.Y2) / 10;
                    X1min = CP.Min(x => Math.Abs(x.JawPositions.X1)) / 10;
                    X2min = CP.Min(x => x.JawPositions.X2) / 10;
                    Y1min = CP.Min(x => Math.Abs(x.JawPositions.Y1)) / 10;
                    Y2min = CP.Min(x => x.JawPositions.Y2) / 10;
                    Xmax = CP.Max(x => Math.Abs(x.JawPositions.X1) + x.JawPositions.X2) / 10;
                    Ymax = CP.Max(x => Math.Abs(x.JawPositions.Y1) + x.JawPositions.Y2) / 10;
                    Xmin = CP.Min(x => Math.Abs(x.JawPositions.X1) + x.JawPositions.X2) / 10;
                    Ymin = CP.Min(x => Math.Abs(x.JawPositions.Y1) + x.JawPositions.Y2) / 10;
                    if ((Math.Abs(X1max - X1min) > 1E-5) || (Math.Abs(X2max - X2min) > 1E-5) || (Math.Abs(Y1max - Y1min) > 1E-5) || (Math.Abs(Y2max - Y2min) > 1E-5))
                        isJawTracking = true;

                }
                if (Field.Meterset.Unit == DosimeterUnit.MU)
                    MU = Field.Meterset.Value;
                else
                {
                    throw new Exception(@"Meterset unit is not MU");
                }

            }
            if (Type == FieldType.Unset)
            {
                Warning = true;
                WarningMessage = "Name / angle mismatch";
            }

        }
        public FieldType Type { get; private set; } = FieldType.Unset;
        public string TypeString
        {
            get { return Type.Display(); }
        }
        public string CourseId;
        public string PlanId;
        public GantryDirection GantryDirection { get; set; }

        public Trajectories Trajectory
        {
            get
            {
                if (GantryDirection == GantryDirection.Clockwise)
                    return Trajectories.CW;
                if (GantryDirection == GantryDirection.CounterClockwise)
                    return Trajectories.CCW;
                return Trajectories.Unset;
            }
        }
        public Energies Energy { get; set; }
        public string Id { get; set; } = "Default Field";
        public double MU { get; set; } = 0;
        public double GantryEnd { get; set; } = 0;
        public double GantryStart { get; set; } = 0;
        public double CollimatorAngle { get; set; } = 0;
        public double CouchRotation { get; set; } = 0;

        public VVector Isocentre { get; set; }
        public double Xmax { get; set; } = 0;
        public double Ymax { get; set; } = 0;
        public double Ymin { get; set; } = 0;
        public double Xmin { get; set; } = 0;
        public double X1max { get; set; } = 0;
        public double Y1max { get; set; } = 0;
        public double X2max { get; set; }
        public double Y2max { get; set; }
        public double X1min { get; set; }
        public double Y1min { get; set; }
        public double X2min { get; set; }
        public double Y2min { get; set; }
        public string ToleranceTable { get; set; }
        public List<BolusInfo> BolusInfos { get; set; } = new List<BolusInfo>();
        public bool Warning { get; set; } = false;
        public string WarningMessage { get; set; } = "";
        public bool isJawTracking { get; set; } = false;
    }

}
