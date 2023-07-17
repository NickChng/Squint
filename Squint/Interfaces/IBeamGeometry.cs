namespace Squint.Interfaces
{
    
    public interface IBeamGeometry
    {
        Trajectories Trajectory { get; set; } 
        double StartAngle { get; set; } 
        double EndAngle { get; set; }
    }
     
}
