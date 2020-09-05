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

namespace SquintScript
{
    public class BeamGeometry : IDisplayable, IElementOf<BeamGeometry>
    {
        public Trajectories Trajectory { get; set; } = Trajectories.Unset;
        public string GeometryName { get; set; } = "Unset";
        public string DisplayName { get { return GeometryName; } }
        public double StartAngle { get; set; } = double.NaN;
        public double EndAngle { get; set; } = double.NaN;
        public double StartAngleTolerance { get; set; } = 1;
        public double EndAngleTolerance { get; set; } = 1;
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

        public string SuperSetName { get; private set; }
        public bool IsElementOf(BeamGeometry G)
        {

            if (Trajectory == Trajectories.Static)
            {
                if (StartAngle.CloseEnough(G.StartAngle, G.StartAngleTolerance))
                {
                    SuperSetName = G.GeometryName;
                    return true;
                }
                else return false;
            }
            else // some kind of arc
            {
                double InvariantMaxStart = G.GetInvariantAngle(G.StartAngle) + G.StartAngleTolerance;
                double InvariantMinStart = InvariantMaxStart - 2 * G.StartAngleTolerance;
                double InvariantMaxEnd = G.GetInvariantAngle(G.EndAngle) + G.EndAngleTolerance;
                double InvariantMinEnd = InvariantMaxEnd - 2 * G.EndAngleTolerance;

                var FieldStart = G.GetInvariantAngle(StartAngle);
                var FieldEnd = G.GetInvariantAngle(EndAngle);

                if (FieldStart >= InvariantMinStart && FieldStart <= InvariantMaxStart && FieldEnd >= InvariantMinEnd && FieldEnd <= InvariantMaxEnd)
                {
                    SuperSetName = G.GeometryName;
                    return true;
                }
                else return false;
            }
        }
    }

}
