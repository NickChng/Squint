using System.Collections.ObjectModel;
using PropertyChanged;

namespace SquintScript.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class Imaging_ViewModel
    {
        public ObservableCollection<ImagingFieldItem> ImagingFields { get; set; } = new ObservableCollection<ImagingFieldItem>() { new ImagingFieldItem(), new ImagingFieldItem() };
        public ObservableCollection<string> GeneralErrors { get; set; } = new ObservableCollection<string>();
        public bool isGeneralErrors { get { if (GeneralErrors.Count > 0) return true; else return false; } }
        public bool isProtocolAttached { get { if (ImagingProtocols.Count > 0) return true; else return false; } }
        public ObservableCollection<ProtocolImagingViewModel> ImagingProtocols { get; set; } = new ObservableCollection<ProtocolImagingViewModel>();
    }




}
