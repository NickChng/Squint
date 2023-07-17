using PropertyChanged;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class LoadingViewModel
    {
        public string LoadingMessage { get; set; } = "Loading";
    }




}
