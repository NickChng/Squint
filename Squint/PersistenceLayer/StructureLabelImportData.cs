using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squint.PersistenceLayer
{
    internal class StructureLabelImportData
    {
        public int ID { get; set; }
        public int StructureLabelGroupID { get; set; }
        public string StructureLabel { get; set; }   
        public double AlphaBetaRatio { get; set; }
        public string Description { get; set; }
        public int StructureType { get; set; }
        public string Code { get; set; }
        public string Designator { get; set; }
    }
}
