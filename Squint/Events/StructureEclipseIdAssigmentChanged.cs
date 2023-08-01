using Prism.Events;

namespace Squint.ViewModels
{

    public class StructureEclipseIdAssignmentChangedArgs
    {
        public string ProtocolStructureId;
        public string EclipseStructureId;
    }
    public class StructureEclipseIdAssigmentChanged : PubSubEvent<StructureEclipseIdAssignmentChangedArgs> { }

}