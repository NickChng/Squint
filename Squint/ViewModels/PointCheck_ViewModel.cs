using System.Collections.ObjectModel;
using PropertyChanged;

namespace Squint.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class PointCheck_ViewModel : ObservableObject
    {
        public class PointCheckObject
        {
            public string ProtocolStructureId { get; set; }
            public string MinSubVolume { get; set; }
            public string Centroid_x { get; set; }
            public string Centroid_y { get; set; }
            public string Centroid_z { get; set; }

            public string CentroidString 
            { 
                get
                {
                    if (Warning != null)
                    {
                        if ((bool)Warning)
                            return string.Format("{0},{1},{2}", Centroid_x, Centroid_y, Centroid_z);
                        else
                            return "";
                    }
                    else
                        return "";
                }
            }
            public bool? Warning { get; set; }
            public string WarningString { get; set; }
            public bool Assigned { get; set; }
            public bool isEmpty { get; set; } = false;

            public ParameterOptions ParameterOption { get; set; } = ParameterOptions.Required;

        }
        public ObservableCollection<PointCheckObject> Checks { get; set; } = new ObservableCollection<PointCheckObject>();
    }
}
