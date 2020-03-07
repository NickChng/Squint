using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SquintScript
{
    [XmlRoot("StructureLabels")]
    public class StructureLabelXML
    {
        [XmlElement("Structures")]
        public StructuresXML Structures = new StructuresXML();
    }
    public class StructuresXML
    {
        [XmlElement("Structure")]
        public List<StructureXML> Structure = new List<StructureXML>();
    }
    public class StructureXML
    {
        [XmlAttribute("Label")]
        public string Label;
        [XmlAttribute("StructureType")]
        public string StructureType;
        [XmlAttribute("AlphaBetaRatio")]
        public double AlphaBetaRatio;
        [XmlAttribute("Description")]
        public string Description;
    }
}
