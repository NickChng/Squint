using Squint.ViewModels;

namespace Squint
{
    public class NTODefinition : ObservableObject
    {
        public double Priority { get; set; } = 0;
        public double FallOff { get; set; }
        public bool isAutomatic { get; set; }
        public double DistanceFromTargetBorderMM { get; set; }
        public double StartDosePercentage { get; set; }
        public double EndDosePercentage { get; set; }
    }




}
