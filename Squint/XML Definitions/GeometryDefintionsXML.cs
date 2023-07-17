using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squint.XML_Definitions
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class GeometryDefinitions
    {

        private GeometryDefinitionsGeometry[] geometryField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Geometry")]
        public GeometryDefinitionsGeometry[] Geometry
        {
            get
            {
                return this.geometryField;
            }
            set
            {
                this.geometryField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class GeometryDefinitionsGeometry
    {

        private string geometryNameField;

        private double startAngleField;

        private double startAngleToleranceField;

        private bool startAngleToleranceFieldSpecified;

        private double endAngleField;

        private double endAngleToleranceField;

        private string trajectoryField;

        private double startAngleoleranceField;

        private bool startAngleoleranceFieldSpecified;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string GeometryName
        {
            get
            {
                return this.geometryNameField;
            }
            set
            {
                this.geometryNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double StartAngle
        {
            get
            {
                return this.startAngleField;
            }
            set
            {
                this.startAngleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double StartAngleTolerance
        {
            get
            {
                return this.startAngleToleranceField;
            }
            set
            {
                this.startAngleToleranceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StartAngleToleranceSpecified
        {
            get
            {
                return this.startAngleToleranceFieldSpecified;
            }
            set
            {
                this.startAngleToleranceFieldSpecified = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double EndAngle
        {
            get
            {
                return this.endAngleField;
            }
            set
            {
                this.endAngleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double EndAngleTolerance
        {
            get
            {
                return this.endAngleToleranceField;
            }
            set
            {
                this.endAngleToleranceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Trajectory
        {
            get
            {
                return this.trajectoryField;
            }
            set
            {
                this.trajectoryField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public double StartAngleolerance
        {
            get
            {
                return this.startAngleoleranceField;
            }
            set
            {
                this.startAngleoleranceField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool StartAngleoleranceSpecified
        {
            get
            {
                return this.startAngleoleranceFieldSpecified;
            }
            set
            {
                this.startAngleoleranceFieldSpecified = value;
            }
        }
    }


}
