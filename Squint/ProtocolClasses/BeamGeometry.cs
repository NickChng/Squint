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
using System.Data;
using SquintScript.Interfaces;
using SquintScript.Extensions;
using System.Windows.Media;
using AutoMapper;

namespace SquintScript
{

    public class BeamGeometryInstance : IDisplayable
    {

        public Trajectories Trajectory { get; private set; } = Trajectories.Unset;
        public double StartAngle { get; private set; } = double.NaN;
        public double EndAngle { get; private set; } = double.NaN;
        public string DisplayName { get; private set; } = "Undefined geometry";
        public BeamGeometryDefinition Definition { get; private set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as BeamGeometryInstance);
        }

        public bool Equals(BeamGeometryInstance that)
        {
            if (that == null)
                return false;
            if (Definition != null && that.Definition != null)
                if (Definition.Id == that.Definition.Id)
                    return true;
            return false;
        }

        public BeamGeometryInstance(DbBeamGeometry DbBG)
        {
            Definition = new BeamGeometryDefinition(DbBG);
            DisplayName = Definition.DisplayName;
            StartAngle = Definition.StartAngle;
            EndAngle = Definition.EndAngle;
            Trajectory = Definition.Trajectory;
        }

        public BeamGeometryInstance()
        {

        }

        public BeamGeometryInstance(double startAngle, double endAngle, Trajectories trajectory)
        {
            StartAngle = startAngle;
            EndAngle = endAngle;
            Trajectory = trajectory;
            foreach (BeamGeometryDefinition bgd in Ctr.GetBeamGeometryDefinitions())
                if (bgd.Contains(this))
                {
                    DisplayName = bgd.GeometryName;
                    Definition = bgd;
                }
        }
    }

    public class BeamGeometryDefinition : IDisplayable, IContains<BeamGeometryInstance>
    {
        public int Id { get; set; }
        public Trajectories Trajectory { get; set; } = Trajectories.Unset;
        public double StartAngle { get; set; } = double.NaN;
        public double EndAngle { get; set; } = double.NaN;
        public double StartAngleTolerance { get; set; } = 1;
        public double EndAngleTolerance { get; set; } = 1;

        public string GeometryName { get; set; }

        public string DisplayName { get { return GeometryName; } }

        public BeamGeometryDefinition(DbBeamGeometry DbBG)
        {
            //Automapper.SquintMapper.Map(DbBG, this);  // unable to get this to work globally due to limitations with ConvertUsing in the version of Automapper that supports .net 4.5
            Id = DbBG.ID;
            Trajectory = (Trajectories)DbBG.Trajectory;
            StartAngle = DbBG.StartAngle;
            EndAngle = DbBG.EndAngle;
            StartAngleTolerance = DbBG.StartAngleTolerance;
            EndAngleTolerance = DbBG.EndAngleTolerance;
            GeometryName = DbBG.GeometryName;
        }

        public double GetInvariantAngle(double A)
        {
            switch (Trajectory)
            {
                case Trajectories.CW:
                    if (A > 180)
                        return A - 180;
                    else
                        return A + 180;
                case Trajectories.CCW:
                    if (A > 180)
                        return 540 -A;
                    else
                        return 180 - A;
                default:
                    return A;
            }
        }
        public bool Contains(BeamGeometryInstance beamGeometryInstance)
        {
            var startAngle = beamGeometryInstance.StartAngle;
            var endAngle = beamGeometryInstance.EndAngle;
            var trajectory = beamGeometryInstance.Trajectory;

            if (Trajectory == Trajectories.Static && trajectory == Trajectory)
            {
                if (StartAngle.CloseEnough(startAngle, StartAngleTolerance))
                {
                    //SuperSetName = G.GeometryName;
                    return true;
                }
                else return false;
            }
            else // some kind of arc
            {
                double InvariantMaxStart = GetInvariantAngle(StartAngle) + StartAngleTolerance;
                double InvariantMinStart = InvariantMaxStart - 2 * StartAngleTolerance;
                double InvariantMaxEnd = GetInvariantAngle(EndAngle) + EndAngleTolerance;
                double InvariantMinEnd = InvariantMaxEnd - 2 * EndAngleTolerance;

                var FieldStart = GetInvariantAngle(startAngle);
                var FieldEnd = GetInvariantAngle(endAngle);

                if (FieldStart >= InvariantMinStart && FieldStart <= InvariantMaxStart && FieldEnd >= InvariantMinEnd && FieldEnd <= InvariantMaxEnd && trajectory == Trajectory)
                {
                    //SuperSetName = G.GeometryName;
                    return true;
                }
                else return false;
            }
        }
    }

}
