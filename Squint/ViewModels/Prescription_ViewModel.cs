using PropertyChanged;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class Prescription_ViewModel
    {
        public double RxDose { get; set; } = -1;
        public int Fractions { get; set; } = -1;
        public double PercentRx { get; set; } = -1;
    }




}
