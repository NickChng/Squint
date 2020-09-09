using PropertyChanged;

namespace SquintScript
{
    [AddINotifyPropertyChangedInterface]
    public class StructureCheckList
    {
        public StructureCheckList()
        {

        }
        public StructureCheckList(DbStructureChecklist DbO)
        {
            isPointContourChecked = new TrackedValue<bool?>(DbO.isPointContourChecked);
            PointContourVolumeThreshold = new TrackedValue<double?>(DbO.PointContourThreshold);
        }
        public TrackedValue<bool?> isPointContourChecked { get; set; } = new TrackedValue<bool?>(false);
        public TrackedValue<double?> PointContourVolumeThreshold { get; set; } = new TrackedValue<double?>(double.NaN);
    }

}

