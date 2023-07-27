using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squint.Helpers
{
    public static class StructureLabelLookup
    {
        private static bool _initialized = false;
        private static IEnumerable<StructureLabel> StructureLabels;

        public static void SetDictionary(IEnumerable<StructureLabel> structureLabels)
        {
            StructureLabels = structureLabels;
            _initialized = true;
        }

        public static string GetLabelByCode(string Code)
        {
            //GetStructureLabels

            if (!_initialized)
                return "Label dictionary not found.";
            else
            {
                var Label = StructureLabels.FirstOrDefault(x => x.Code == Code);
                if (Label != null)
                    return Label.LabelName;
                else
                    return "Label not found";
            }
        }
    }
}
