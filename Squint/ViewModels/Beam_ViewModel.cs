using System.Collections.ObjectModel;
using PropertyChanged;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class Beam_ViewModel
    {
        public string DebugString { get; set; } = "Default_Value";
        public ObservableCollection<BeamListItem> Beams { get; set; } = new ObservableCollection<BeamListItem>();
        public TestList_ViewModel GroupTests { get; set; } = new TestList_ViewModel();
    }




}
