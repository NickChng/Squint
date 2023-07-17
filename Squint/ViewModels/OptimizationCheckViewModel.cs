using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using VMS.TPS.Common.Model;
using VMS.TPS.Common.Model.API;
using System.Windows.Controls.Primitives;
using ESAPI = VMS.TPS.Common.Model.API.Application;
using VMSAPI = VMS.TPS.Common.Model.API;
using PropertyChanged;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class OptimizationCheckViewModel
    {
        public NTODefinition NTO { get; set; }
        public bool NoNTO
        {
            get
            {
                if (NTO == null)
                    return true;
                else
                    return false;
            }
        }
        public bool NoObjectives
        {
            get
            {
                if (Objectives.Count == 0)
                    return true;
                else return false;
            }
        }
        public ObservableCollection<ObjectiveItem> Objectives { get; set; } = new ObservableCollection<ObjectiveItem>() { new ObjectiveItem("Default"), new ObjectiveItem("Default1") };
    }
}
