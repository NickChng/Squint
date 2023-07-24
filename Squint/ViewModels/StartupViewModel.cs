using PropertyChanged;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class StartupViewModel : ObservableObject
    {
        public string InitializingMessages { get; set; } = "Initializing Squint, please wait...";
    }
}
