using PropertyChanged;

namespace SquintScript.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class LoadingViewModel
    {
        public string LoadingMessage { get; set; } = "Loading";
    }




}
