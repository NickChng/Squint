using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;
using System.Runtime.CompilerServices;
using System.Data.Entity;
using PropertyChanged;
using System.Data;

namespace SquintScript
{
    public static partial class Ctr
    {
        public class BeamGeometry
        {
            public Trajectories Trajectory { get; set; } = Trajectories.Unset;
            public string GeometryName { get; set; } = "Unset";
            public double MinStartAngle { get; set; } = -1;
            public double MinEndAngle { get; set; } = -1;
            public double MaxStartAngle { get; set; } = -1;
            public double MaxEndAngle { get; set; } = -1;
            public double GetInvariantAngle(double A)
            {
                switch (Trajectory)
                {
                    case Trajectories.CW:
                        if (A > 180)
                            return -(A - 180);
                        else
                            return A;
                    case Trajectories.CCW:
                        if (A > 180)
                            return -(A - 360);
                        else
                            return -A;
                    default:
                        return A;
                }
            }
        }
        
        [AddINotifyPropertyChangedInterface]
        public class Beam
        {
            //References
            public int ID { get; private set; }
            public int ComponentID { get; private set; }
            //Properties
            public string ProtocolBeamName { get; set; } = "unset";
            public FieldType Technique { get; set; } = FieldType.Unset;
            public List<Energies> ValidEnergies { get; set; } = new List<Energies>();
            public string ToleranceTable { get; set; } = "unset";
            public double MinMUWarning { get; set; } 
            public double MaxMUWarning { get; set; } 
            public double MinColRotation { get; set; }
            public double MaxColRotation { get; set; }
            public double CouchRotation { get; set; } 
            public double MinX { get; set; } = 3;
            public double MaxX { get; set; } = 3;
            public double MinY { get; set; } 
            public double MaxY { get; set; } 
            public ParameterOptions BolusParameter { get; set; }
            public double RefBolusHU { get; set; }
            public double BolusClinicalMinThickness { get; set; }
            public double BolusClinicalMaxThickness { get; set; }
            public ParameterOptions VMAT_JawTracking { get; set; }

            public List<string> EclipseAliases { get; set; } = new List<string>();
            public List<BeamGeometry> ValidGeometries { get; set; } = new List<BeamGeometry>();
            // Methods
            public void Delete()
            {
                BeamDeleted?.Invoke(this, ID);
            }

            // Events
            public event EventHandler<int> BeamDeleted;
            

            public Beam(DbBeam DbO)
            {
                ID = DbO.ID;
                ComponentID = DbO.ComponentID;
                ProtocolBeamName = DbO.ProtocolBeamName;
                Technique = (FieldType)DbO.Technique;
                foreach (var E in DbO.DbEnergies)
                    ValidEnergies.Add((Energies)E.ID);
                foreach (var A in DbO.DbBeamAliases)
                    EclipseAliases.Add(A.EclipseFieldId);
                foreach (var G in DbO.DbBeamGeometries)
                {
                    ValidGeometries.Add(new BeamGeometry() { MinEndAngle = G.MinEndAngle, MinStartAngle = G.MinStartAngle, MaxEndAngle = G.MaxEndAngle, MaxStartAngle = G.MaxStartAngle, GeometryName = G.GeometryName, Trajectory = (Trajectories)G.Trajectory});
                }
                ToleranceTable = DbO.ToleranceTable;
                MinMUWarning = DbO.MinMUWarning;
                MaxMUWarning = DbO.MaxMUWarning;
                MinColRotation = DbO.MinColRotation;
                MaxColRotation = DbO.MaxColRotation;
                CouchRotation = DbO.CouchRotation;
                BolusParameter = (ParameterOptions)DbO.BolusClinicalIndication;
                VMAT_JawTracking = (ParameterOptions)DbO.VMAT_JawTracking;
                RefBolusHU = DbO.BolusClinicalHU;
                BolusClinicalMinThickness = DbO.BolusClinicalMinThickness;
                BolusClinicalMaxThickness = DbO.BolusClinicalMaxThickness;
                MinX = DbO.MinX;
                MaxX = DbO.MaxX;
                MinY = DbO.MinY;
                MaxY = DbO.MaxY;
            }
        }
        
    }
}
