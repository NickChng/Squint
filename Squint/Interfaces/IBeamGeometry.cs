namespace Squint.Interfaces
{
    
    public interface IBeamGeometry
    {
        TrajectoryTypes Trajectory { get; set; } 
        double StartAngle { get; set; } 
        double EndAngle { get; set; }
    }
     
}
