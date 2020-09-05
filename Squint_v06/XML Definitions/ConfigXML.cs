using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SquintScript
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class SquintConfiguration
    {

        private SquintConfigurationSite siteField;

        private SquintConfigurationDatabase[] databasesField;

        private SquintConfigurationPathDef[] beamGeometryDefinitionsField;

        private SquintConfigurationPathDef1[] structureCodesField;

        private SquintConfigurationPathDef2[] clinicalProtocolsField;

        private SquintConfigurationPathDef3[] squintProtocolsField;

        /// <remarks/>
        public SquintConfigurationSite Site
        {
            get
            {
                return this.siteField;
            }
            set
            {
                this.siteField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("Database", IsNullable = false)]
        public SquintConfigurationDatabase[] Databases
        {
            get
            {
                return this.databasesField;
            }
            set
            {
                this.databasesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("PathDef", IsNullable = false)]
        public SquintConfigurationPathDef[] BeamGeometryDefinitions
        {
            get
            {
                return this.beamGeometryDefinitionsField;
            }
            set
            {
                this.beamGeometryDefinitionsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("PathDef", IsNullable = false)]
        public SquintConfigurationPathDef1[] StructureCodes
        {
            get
            {
                return this.structureCodesField;
            }
            set
            {
                this.structureCodesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("PathDef", IsNullable = false)]
        public SquintConfigurationPathDef2[] ClinicalProtocols
        {
            get
            {
                return this.clinicalProtocolsField;
            }
            set
            {
                this.clinicalProtocolsField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayItemAttribute("PathDef", IsNullable = false)]
        public SquintConfigurationPathDef3[] SquintProtocols
        {
            get
            {
                return this.squintProtocolsField;
            }
            set
            {
                this.squintProtocolsField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SquintConfigurationSite
    {

        private string currentSiteField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string CurrentSite
        {
            get
            {
                return this.currentSiteField;
            }
            set
            {
                this.currentSiteField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SquintConfigurationDatabase
    {

        private string siteField;

        private string displayNameField;

        private string databaseNameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Site
        {
            get
            {
                return this.siteField;
            }
            set
            {
                this.siteField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DisplayName
        {
            get
            {
                return this.displayNameField;
            }
            set
            {
                this.displayNameField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string DatabaseName
        {
            get
            {
                return this.databaseNameField;
            }
            set
            {
                this.databaseNameField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SquintConfigurationPathDef
    {

        private string siteField;

        private string pathField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Site
        {
            get
            {
                return this.siteField;
            }
            set
            {
                this.siteField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Path
        {
            get
            {
                return this.pathField;
            }
            set
            {
                this.pathField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SquintConfigurationPathDef1
    {

        private string siteField;

        private string pathField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Site
        {
            get
            {
                return this.siteField;
            }
            set
            {
                this.siteField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Path
        {
            get
            {
                return this.pathField;
            }
            set
            {
                this.pathField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SquintConfigurationPathDef2
    {

        private string siteField;

        private string pathField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Site
        {
            get
            {
                return this.siteField;
            }
            set
            {
                this.siteField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Path
        {
            get
            {
                return this.pathField;
            }
            set
            {
                this.pathField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class SquintConfigurationPathDef3
    {

        private string siteField;

        private string pathField;

        private string exportPathField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Site
        {
            get
            {
                return this.siteField;
            }
            set
            {
                this.siteField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Path
        {
            get
            {
                return this.pathField;
            }
            set
            {
                this.pathField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ExportPath
        {
            get
            {
                return this.exportPathField;
            }
            set
            {
                this.exportPathField = value;
            }
        }
    }






}
