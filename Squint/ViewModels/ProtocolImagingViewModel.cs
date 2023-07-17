using System.Collections.Generic;
using PropertyChanged;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ProtocolImagingViewModel
    {
        public int Id { get; set; } // the Db key
        public string ImagingProtocolName { get; set; } = "";
        public List<string> WarningMessages { get; set; } = new List<string>();
        public bool isWarning { get; set; } = false;
    }




}
