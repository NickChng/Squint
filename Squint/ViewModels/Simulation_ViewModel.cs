using PropertyChanged;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class Simulation_ViewModel
    {
        public double SliceSpacingProtocol { get; set; } = -1;
        public double SliceSpacingDetected { get; set; } = -1;
        public string SeriesComment { get; set; } = "(No comments found)";
        public string ImageComment { get; set; } = "(No comments found)";
        public string StudyId { get; set; } = "(No Id found)";
        public string SeriesId { get; set; } = "(No Id found)";
        public int NumSlices { get; set; } = 0;
        public bool Warning { get; set; }
        public string WarningMessage { get; set; }
    }




}
