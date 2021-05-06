using System.Collections.Generic;
using System.Linq;
using System.Data;
using VMS.TPS.Common.Model.API;


namespace SquintScript
{
    public class AsyncStructureSet
    {
        private AsyncESAPI A;
        public string Id { get; private set; }
        public string UID { get; private set; }

        public int NumSlices { get; set; }
        private Dictionary<string, AsyncStructure> _Structures = new Dictionary<string, AsyncStructure>();
        public AsyncStructureSet(AsyncESAPI _A, StructureSet structureSet)
        {
            A = _A;
            Id = structureSet.Id;
            UID = structureSet.UID;
            NumSlices = structureSet.Image.ZSize;
            foreach (Structure S in structureSet.Structures)
            {
                _Structures.Add(S.Id, new AsyncStructure(A, S, structureSet.Id, structureSet.UID, NumSlices, structureSet.Image.UserOrigin));
            }
        }
        public IEnumerable<string> GetStructureIds()
        {
            return _Structures.Values.Select(x => x.Id);
        }
        public AsyncStructure GetStructure(string Id)
        {
            if (_Structures.ContainsKey(Id))
                return _Structures[Id];
            else
                return null;
        }
        public List<AsyncStructure> GetAllStructures()
        {
            return _Structures.Values.ToList();
        }
    }
}
