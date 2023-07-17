using System.Collections.ObjectModel;
using PropertyChanged;
using Squint.Interfaces;


namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class TestList_ViewModel : ObservableObject
    {
        public void SendUpdate()
        {
            RaisePropertyChangedEvent(nameof(Tests));
        }
        public ObservableCollection<ITestListItem> Tests { get; set; } = new ObservableCollection<ITestListItem>();
    }




}
