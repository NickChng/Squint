using System.Collections.ObjectModel;
using SquintScript.ViewModels;

namespace SquintScript
{
    public class ObjectiveItem : ObservableObject
    {
        public ObjectiveItem(string StructureId_init, ObjectiveDefinition OPO = null)
        {
            StructureId = StructureId_init;
            if (OPO != null)
                ObjectiveDefinitions.Add(OPO);
        }
        public string StructureId { get; set; } = "Default";
        public ObservableCollection<ObjectiveDefinition> ObjectiveDefinitions { get; set; } = new ObservableCollection<ObjectiveDefinition>();
    }




}
