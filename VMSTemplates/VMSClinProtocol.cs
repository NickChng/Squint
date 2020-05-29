
// NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
public partial class Protocol
{

    private ProtocolPreview previewField;

    private ProtocolStructureTemplate structureTemplateField;

    private object fieldAlignmentRulesField;

    private ProtocolPhases phasesField;

    private ProtocolReview reviewField;

    private decimal versionField;

    /// <remarks/>
    public ProtocolPreview Preview
    {
        get
        {
            return this.previewField;
        }
        set
        {
            this.previewField = value;
        }
    }

    /// <remarks/>
    public ProtocolStructureTemplate StructureTemplate
    {
        get
        {
            return this.structureTemplateField;
        }
        set
        {
            this.structureTemplateField = value;
        }
    }

    /// <remarks/>
    public object FieldAlignmentRules
    {
        get
        {
            return this.fieldAlignmentRulesField;
        }
        set
        {
            this.fieldAlignmentRulesField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhases Phases
    {
        get
        {
            return this.phasesField;
        }
        set
        {
            this.phasesField = value;
        }
    }

    /// <remarks/>
    public ProtocolReview Review
    {
        get
        {
            return this.reviewField;
        }
        set
        {
            this.reviewField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal Version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPreview
{

    private decimal versionField;

    private string idField;

    private string typeField;

    private string approvalStatusField;

    private string diagnosisField;

    private string treatmentSiteField;

    private string assignedUsersField;

    private string descriptionField;

    private string lastModifiedField;

    private string approvalHistoryField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal Version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ID
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ApprovalStatus
    {
        get
        {
            return this.approvalStatusField;
        }
        set
        {
            this.approvalStatusField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Diagnosis
    {
        get
        {
            return this.diagnosisField;
        }
        set
        {
            this.diagnosisField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string TreatmentSite
    {
        get
        {
            return this.treatmentSiteField;
        }
        set
        {
            this.treatmentSiteField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string AssignedUsers
    {
        get
        {
            return this.assignedUsersField;
        }
        set
        {
            this.assignedUsersField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Description
    {
        get
        {
            return this.descriptionField;
        }
        set
        {
            this.descriptionField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string LastModified
    {
        get
        {
            return this.lastModifiedField;
        }
        set
        {
            this.lastModifiedField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ApprovalHistory
    {
        get
        {
            return this.approvalHistoryField;
        }
        set
        {
            this.approvalHistoryField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolStructureTemplate
{

    private ProtocolStructureTemplateStructure[] structuresField;

    private decimal versionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Structure", IsNullable = false)]
    public ProtocolStructureTemplateStructure[] Structures
    {
        get
        {
            return this.structuresField;
        }
        set
        {
            this.structuresField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal Version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolStructureTemplateStructure
{

    private ProtocolStructureTemplateStructureIdentification identificationField;

    private byte typeIndexField;

    private string colorAndStyleField;

    private short searchCTLowField;

    private short searchCTHighField;

    private byte dVHLineStyleField;

    private int dVHLineColorField;

    private byte dVHLineWidthField;

    private object eUDAlphaField;

    private object tCPAlphaField;

    private object tCPBetaField;

    private object tCPGammaField;

    private string idField;

    private string nameField;

    /// <remarks/>
    public ProtocolStructureTemplateStructureIdentification Identification
    {
        get
        {
            return this.identificationField;
        }
        set
        {
            this.identificationField = value;
        }
    }

    /// <remarks/>
    public byte TypeIndex
    {
        get
        {
            return this.typeIndexField;
        }
        set
        {
            this.typeIndexField = value;
        }
    }

    /// <remarks/>
    public string ColorAndStyle
    {
        get
        {
            return this.colorAndStyleField;
        }
        set
        {
            this.colorAndStyleField = value;
        }
    }

    /// <remarks/>
    public short SearchCTLow
    {
        get
        {
            return this.searchCTLowField;
        }
        set
        {
            this.searchCTLowField = value;
        }
    }

    /// <remarks/>
    public short SearchCTHigh
    {
        get
        {
            return this.searchCTHighField;
        }
        set
        {
            this.searchCTHighField = value;
        }
    }

    /// <remarks/>
    public byte DVHLineStyle
    {
        get
        {
            return this.dVHLineStyleField;
        }
        set
        {
            this.dVHLineStyleField = value;
        }
    }

    /// <remarks/>
    public int DVHLineColor
    {
        get
        {
            return this.dVHLineColorField;
        }
        set
        {
            this.dVHLineColorField = value;
        }
    }

    /// <remarks/>
    public byte DVHLineWidth
    {
        get
        {
            return this.dVHLineWidthField;
        }
        set
        {
            this.dVHLineWidthField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object EUDAlpha
    {
        get
        {
            return this.eUDAlphaField;
        }
        set
        {
            this.eUDAlphaField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object TCPAlpha
    {
        get
        {
            return this.tCPAlphaField;
        }
        set
        {
            this.tCPAlphaField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object TCPBeta
    {
        get
        {
            return this.tCPBetaField;
        }
        set
        {
            this.tCPBetaField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object TCPGamma
    {
        get
        {
            return this.tCPGammaField;
        }
        set
        {
            this.tCPGammaField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ID
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Name
    {
        get
        {
            return this.nameField;
        }
        set
        {
            this.nameField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolStructureTemplateStructureIdentification
{

    private object volumeIDField;

    private string volumeCodeField;

    private string volumeTypeField;

    private string volumeCodeTableField;

    private ProtocolStructureTemplateStructureIdentificationStructureCode structureCodeField;

    /// <remarks/>
    public object VolumeID
    {
        get
        {
            return this.volumeIDField;
        }
        set
        {
            this.volumeIDField = value;
        }
    }

    /// <remarks/>
    public string VolumeCode
    {
        get
        {
            return this.volumeCodeField;
        }
        set
        {
            this.volumeCodeField = value;
        }
    }

    /// <remarks/>
    public string VolumeType
    {
        get
        {
            return this.volumeTypeField;
        }
        set
        {
            this.volumeTypeField = value;
        }
    }

    /// <remarks/>
    public string VolumeCodeTable
    {
        get
        {
            return this.volumeCodeTableField;
        }
        set
        {
            this.volumeCodeTableField = value;
        }
    }

    /// <remarks/>
    public ProtocolStructureTemplateStructureIdentificationStructureCode StructureCode
    {
        get
        {
            return this.structureCodeField;
        }
        set
        {
            this.structureCodeField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolStructureTemplateStructureIdentificationStructureCode
{

    private string codeField;

    private string codeSchemeField;

    private decimal codeSchemeVersionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Code
    {
        get
        {
            return this.codeField;
        }
        set
        {
            this.codeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string CodeScheme
    {
        get
        {
            return this.codeSchemeField;
        }
        set
        {
            this.codeSchemeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal CodeSchemeVersion
    {
        get
        {
            return this.codeSchemeVersionField;
        }
        set
        {
            this.codeSchemeVersionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhases
{

    private ProtocolPhasesPhase phaseField;

    /// <remarks/>
    public ProtocolPhasesPhase Phase
    {
        get
        {
            return this.phaseField;
        }
        set
        {
            this.phaseField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhase
{

    private string modeField;

    private object defaultEnergyKVField;

    private byte fractionCountField;

    private object fractionsPerWeekField;

    private object fractionsPerDayField;

    private string treatmentUnitField;

    private string treatmentStyleField;

    private object immobilizationDeviceField;

    private object localizationTechniqueField;

    private ProtocolPhasesPhasePrescription prescriptionField;

    private ProtocolPhasesPhasePlanTemplate planTemplateField;

    private ProtocolPhasesPhaseObjectiveTemplate objectiveTemplateField;

    private string idField;

    /// <remarks/>
    public string Mode
    {
        get
        {
            return this.modeField;
        }
        set
        {
            this.modeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object DefaultEnergyKV
    {
        get
        {
            return this.defaultEnergyKVField;
        }
        set
        {
            this.defaultEnergyKVField = value;
        }
    }

    /// <remarks/>
    public byte FractionCount
    {
        get
        {
            return this.fractionCountField;
        }
        set
        {
            this.fractionCountField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object FractionsPerWeek
    {
        get
        {
            return this.fractionsPerWeekField;
        }
        set
        {
            this.fractionsPerWeekField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object FractionsPerDay
    {
        get
        {
            return this.fractionsPerDayField;
        }
        set
        {
            this.fractionsPerDayField = value;
        }
    }

    /// <remarks/>
    public string TreatmentUnit
    {
        get
        {
            return this.treatmentUnitField;
        }
        set
        {
            this.treatmentUnitField = value;
        }
    }

    /// <remarks/>
    public string TreatmentStyle
    {
        get
        {
            return this.treatmentStyleField;
        }
        set
        {
            this.treatmentStyleField = value;
        }
    }

    /// <remarks/>
    public object ImmobilizationDevice
    {
        get
        {
            return this.immobilizationDeviceField;
        }
        set
        {
            this.immobilizationDeviceField = value;
        }
    }

    /// <remarks/>
    public object LocalizationTechnique
    {
        get
        {
            return this.localizationTechniqueField;
        }
        set
        {
            this.localizationTechniqueField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePrescription Prescription
    {
        get
        {
            return this.prescriptionField;
        }
        set
        {
            this.prescriptionField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplate PlanTemplate
    {
        get
        {
            return this.planTemplateField;
        }
        set
        {
            this.planTemplateField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhaseObjectiveTemplate ObjectiveTemplate
    {
        get
        {
            return this.objectiveTemplateField;
        }
        set
        {
            this.objectiveTemplateField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ID
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePrescription
{

    private ProtocolPhasesPhasePrescriptionItem[] itemField;

    private ProtocolPhasesPhasePrescriptionMeasureItem[] measureItemField;

    private decimal versionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Item")]
    public ProtocolPhasesPhasePrescriptionItem[] Item
    {
        get
        {
            return this.itemField;
        }
        set
        {
            this.itemField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("MeasureItem")]
    public ProtocolPhasesPhasePrescriptionMeasureItem[] MeasureItem
    {
        get
        {
            return this.measureItemField;
        }
        set
        {
            this.measureItemField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal Version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePrescriptionItem
{

    private byte typeField;

    private byte modifierField;

    private byte parameterField;

    private decimal doseField;

    private byte totalDoseField;

    private string idField;

    private bool primaryField;

    /// <remarks/>
    public byte Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    public byte Modifier
    {
        get
        {
            return this.modifierField;
        }
        set
        {
            this.modifierField = value;
        }
    }

    /// <remarks/>
    public byte Parameter
    {
        get
        {
            return this.parameterField;
        }
        set
        {
            this.parameterField = value;
        }
    }

    /// <remarks/>
    public decimal Dose
    {
        get
        {
            return this.doseField;
        }
        set
        {
            this.doseField = value;
        }
    }

    /// <remarks/>
    public byte TotalDose
    {
        get
        {
            return this.totalDoseField;
        }
        set
        {
            this.totalDoseField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ID
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool Primary
    {
        get
        {
            return this.primaryField;
        }
        set
        {
            this.primaryField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePrescriptionMeasureItem
{

    private byte typeField;

    private byte modifierField;

    private decimal valueField;

    private decimal typeSpecifierField;

    private bool reportDQPValueInAbsoluteUnitsField;

    private string idField;

    /// <remarks/>
    public byte Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    public byte Modifier
    {
        get
        {
            return this.modifierField;
        }
        set
        {
            this.modifierField = value;
        }
    }

    /// <remarks/>
    public decimal Value
    {
        get
        {
            return this.valueField;
        }
        set
        {
            this.valueField = value;
        }
    }

    /// <remarks/>
    public decimal TypeSpecifier
    {
        get
        {
            return this.typeSpecifierField;
        }
        set
        {
            this.typeSpecifierField = value;
        }
    }

    /// <remarks/>
    public bool ReportDQPValueInAbsoluteUnits
    {
        get
        {
            return this.reportDQPValueInAbsoluteUnitsField;
        }
        set
        {
            this.reportDQPValueInAbsoluteUnitsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ID
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplate
{

    private byte prescribedPercentageField;

    private byte dosePerFractionField;

    private byte fractionCountField;

    private object fieldAlignmentRulesField;

    private ProtocolPhasesPhasePlanTemplatePrescriptionSite prescriptionSiteField;

    private object bolusesField;

    private ProtocolPhasesPhasePlanTemplateFields fieldsField;

    private decimal versionField;

    /// <remarks/>
    public byte PrescribedPercentage
    {
        get
        {
            return this.prescribedPercentageField;
        }
        set
        {
            this.prescribedPercentageField = value;
        }
    }

    /// <remarks/>
    public byte DosePerFraction
    {
        get
        {
            return this.dosePerFractionField;
        }
        set
        {
            this.dosePerFractionField = value;
        }
    }

    /// <remarks/>
    public byte FractionCount
    {
        get
        {
            return this.fractionCountField;
        }
        set
        {
            this.fractionCountField = value;
        }
    }

    /// <remarks/>
    public object FieldAlignmentRules
    {
        get
        {
            return this.fieldAlignmentRulesField;
        }
        set
        {
            this.fieldAlignmentRulesField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplatePrescriptionSite PrescriptionSite
    {
        get
        {
            return this.prescriptionSiteField;
        }
        set
        {
            this.prescriptionSiteField = value;
        }
    }

    /// <remarks/>
    public object Boluses
    {
        get
        {
            return this.bolusesField;
        }
        set
        {
            this.bolusesField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplateFields Fields
    {
        get
        {
            return this.fieldsField;
        }
        set
        {
            this.fieldsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal Version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplatePrescriptionSite
{

    private string volumeIDField;

    private object volumeCodeField;

    private string volumeTypeField;

    private object volumeCodeTableField;

    private ProtocolPhasesPhasePlanTemplatePrescriptionSiteStructureCode structureCodeField;

    /// <remarks/>
    public string VolumeID
    {
        get
        {
            return this.volumeIDField;
        }
        set
        {
            this.volumeIDField = value;
        }
    }

    /// <remarks/>
    public object VolumeCode
    {
        get
        {
            return this.volumeCodeField;
        }
        set
        {
            this.volumeCodeField = value;
        }
    }

    /// <remarks/>
    public string VolumeType
    {
        get
        {
            return this.volumeTypeField;
        }
        set
        {
            this.volumeTypeField = value;
        }
    }

    /// <remarks/>
    public object VolumeCodeTable
    {
        get
        {
            return this.volumeCodeTableField;
        }
        set
        {
            this.volumeCodeTableField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplatePrescriptionSiteStructureCode StructureCode
    {
        get
        {
            return this.structureCodeField;
        }
        set
        {
            this.structureCodeField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplatePrescriptionSiteStructureCode
{

    private string codeField;

    private string codeSchemeField;

    private decimal codeSchemeVersionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Code
    {
        get
        {
            return this.codeField;
        }
        set
        {
            this.codeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string CodeScheme
    {
        get
        {
            return this.codeSchemeField;
        }
        set
        {
            this.codeSchemeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal CodeSchemeVersion
    {
        get
        {
            return this.codeSchemeVersionField;
        }
        set
        {
            this.codeSchemeVersionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplateFields
{

    private ProtocolPhasesPhasePlanTemplateFieldsField fieldField;

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplateFieldsField Field
    {
        get
        {
            return this.fieldField;
        }
        set
        {
            this.fieldField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplateFieldsField
{

    private string typeField;

    private ProtocolPhasesPhasePlanTemplateFieldsFieldTarget targetField;

    private string treatmentUnitField;

    private string techniqueField;

    private ProtocolPhasesPhasePlanTemplateFieldsFieldEnergy energyField;

    private object primaryFluenceModeField;

    private object dRRTemplateField;

    private ushort doseRateField;

    private object sFEDField;

    private ProtocolPhasesPhasePlanTemplateFieldsFieldGantry gantryField;

    private ProtocolPhasesPhasePlanTemplateFieldsFieldCollimator collimatorField;

    private byte tableRtnField;

    private object toleranceTableIDField;

    private byte weightField;

    private ProtocolPhasesPhasePlanTemplateFieldsFieldFieldMargin fieldMarginField;

    private object skinFlashMarginField;

    private object fieldBolusesField;

    private ProtocolPhasesPhasePlanTemplateFieldsFieldIsocenter isocenterField;

    private object blocksField;

    private object mLCPlansField;

    private object wedgesField;

    private object applicatorsField;

    private string idField;

    private bool fixedSSDField;

    private bool usingCompensatorField;

    private bool usingMLCField;

    private bool setupField;

    private bool dRRVisibleField;

    /// <remarks/>
    public string Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplateFieldsFieldTarget Target
    {
        get
        {
            return this.targetField;
        }
        set
        {
            this.targetField = value;
        }
    }

    /// <remarks/>
    public string TreatmentUnit
    {
        get
        {
            return this.treatmentUnitField;
        }
        set
        {
            this.treatmentUnitField = value;
        }
    }

    /// <remarks/>
    public string Technique
    {
        get
        {
            return this.techniqueField;
        }
        set
        {
            this.techniqueField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplateFieldsFieldEnergy Energy
    {
        get
        {
            return this.energyField;
        }
        set
        {
            this.energyField = value;
        }
    }

    /// <remarks/>
    public object PrimaryFluenceMode
    {
        get
        {
            return this.primaryFluenceModeField;
        }
        set
        {
            this.primaryFluenceModeField = value;
        }
    }

    /// <remarks/>
    public object DRRTemplate
    {
        get
        {
            return this.dRRTemplateField;
        }
        set
        {
            this.dRRTemplateField = value;
        }
    }

    /// <remarks/>
    public ushort DoseRate
    {
        get
        {
            return this.doseRateField;
        }
        set
        {
            this.doseRateField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object SFED
    {
        get
        {
            return this.sFEDField;
        }
        set
        {
            this.sFEDField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplateFieldsFieldGantry Gantry
    {
        get
        {
            return this.gantryField;
        }
        set
        {
            this.gantryField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplateFieldsFieldCollimator Collimator
    {
        get
        {
            return this.collimatorField;
        }
        set
        {
            this.collimatorField = value;
        }
    }

    /// <remarks/>
    public byte TableRtn
    {
        get
        {
            return this.tableRtnField;
        }
        set
        {
            this.tableRtnField = value;
        }
    }

    /// <remarks/>
    public object ToleranceTableID
    {
        get
        {
            return this.toleranceTableIDField;
        }
        set
        {
            this.toleranceTableIDField = value;
        }
    }

    /// <remarks/>
    public byte Weight
    {
        get
        {
            return this.weightField;
        }
        set
        {
            this.weightField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplateFieldsFieldFieldMargin FieldMargin
    {
        get
        {
            return this.fieldMarginField;
        }
        set
        {
            this.fieldMarginField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object SkinFlashMargin
    {
        get
        {
            return this.skinFlashMarginField;
        }
        set
        {
            this.skinFlashMarginField = value;
        }
    }

    /// <remarks/>
    public object FieldBoluses
    {
        get
        {
            return this.fieldBolusesField;
        }
        set
        {
            this.fieldBolusesField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplateFieldsFieldIsocenter Isocenter
    {
        get
        {
            return this.isocenterField;
        }
        set
        {
            this.isocenterField = value;
        }
    }

    /// <remarks/>
    public object Blocks
    {
        get
        {
            return this.blocksField;
        }
        set
        {
            this.blocksField = value;
        }
    }

    /// <remarks/>
    public object MLCPlans
    {
        get
        {
            return this.mLCPlansField;
        }
        set
        {
            this.mLCPlansField = value;
        }
    }

    /// <remarks/>
    public object Wedges
    {
        get
        {
            return this.wedgesField;
        }
        set
        {
            this.wedgesField = value;
        }
    }

    /// <remarks/>
    public object Applicators
    {
        get
        {
            return this.applicatorsField;
        }
        set
        {
            this.applicatorsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ID
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool FixedSSD
    {
        get
        {
            return this.fixedSSDField;
        }
        set
        {
            this.fixedSSDField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool UsingCompensator
    {
        get
        {
            return this.usingCompensatorField;
        }
        set
        {
            this.usingCompensatorField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool UsingMLC
    {
        get
        {
            return this.usingMLCField;
        }
        set
        {
            this.usingMLCField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool Setup
    {
        get
        {
            return this.setupField;
        }
        set
        {
            this.setupField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool DRRVisible
    {
        get
        {
            return this.dRRVisibleField;
        }
        set
        {
            this.dRRVisibleField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplateFieldsFieldTarget
{

    private string volumeIDField;

    private object volumeCodeField;

    private string volumeTypeField;

    private object volumeCodeTableField;

    private ProtocolPhasesPhasePlanTemplateFieldsFieldTargetStructureCode structureCodeField;

    /// <remarks/>
    public string VolumeID
    {
        get
        {
            return this.volumeIDField;
        }
        set
        {
            this.volumeIDField = value;
        }
    }

    /// <remarks/>
    public object VolumeCode
    {
        get
        {
            return this.volumeCodeField;
        }
        set
        {
            this.volumeCodeField = value;
        }
    }

    /// <remarks/>
    public string VolumeType
    {
        get
        {
            return this.volumeTypeField;
        }
        set
        {
            this.volumeTypeField = value;
        }
    }

    /// <remarks/>
    public object VolumeCodeTable
    {
        get
        {
            return this.volumeCodeTableField;
        }
        set
        {
            this.volumeCodeTableField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhasePlanTemplateFieldsFieldTargetStructureCode StructureCode
    {
        get
        {
            return this.structureCodeField;
        }
        set
        {
            this.structureCodeField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplateFieldsFieldTargetStructureCode
{

    private string codeField;

    private string codeSchemeField;

    private decimal codeSchemeVersionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Code
    {
        get
        {
            return this.codeField;
        }
        set
        {
            this.codeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string CodeScheme
    {
        get
        {
            return this.codeSchemeField;
        }
        set
        {
            this.codeSchemeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal CodeSchemeVersion
    {
        get
        {
            return this.codeSchemeVersionField;
        }
        set
        {
            this.codeSchemeVersionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplateFieldsFieldEnergy
{

    private string typeField;

    private ushort energyKVField;

    private object maxEnergyKVField;

    /// <remarks/>
    public string Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    public ushort EnergyKV
    {
        get
        {
            return this.energyKVField;
        }
        set
        {
            this.energyKVField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object MaxEnergyKV
    {
        get
        {
            return this.maxEnergyKVField;
        }
        set
        {
            this.maxEnergyKVField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplateFieldsFieldGantry
{

    private byte rtnField;

    private object stopRtnField;

    private string rtnDirectionField;

    /// <remarks/>
    public byte Rtn
    {
        get
        {
            return this.rtnField;
        }
        set
        {
            this.rtnField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object StopRtn
    {
        get
        {
            return this.stopRtnField;
        }
        set
        {
            this.stopRtnField = value;
        }
    }

    /// <remarks/>
    public string RtnDirection
    {
        get
        {
            return this.rtnDirectionField;
        }
        set
        {
            this.rtnDirectionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplateFieldsFieldCollimator
{

    private byte rtnField;

    private string modeField;

    private sbyte x1Field;

    private byte x2Field;

    private sbyte y1Field;

    private byte y2Field;

    /// <remarks/>
    public byte Rtn
    {
        get
        {
            return this.rtnField;
        }
        set
        {
            this.rtnField = value;
        }
    }

    /// <remarks/>
    public string Mode
    {
        get
        {
            return this.modeField;
        }
        set
        {
            this.modeField = value;
        }
    }

    /// <remarks/>
    public sbyte X1
    {
        get
        {
            return this.x1Field;
        }
        set
        {
            this.x1Field = value;
        }
    }

    /// <remarks/>
    public byte X2
    {
        get
        {
            return this.x2Field;
        }
        set
        {
            this.x2Field = value;
        }
    }

    /// <remarks/>
    public sbyte Y1
    {
        get
        {
            return this.y1Field;
        }
        set
        {
            this.y1Field = value;
        }
    }

    /// <remarks/>
    public byte Y2
    {
        get
        {
            return this.y2Field;
        }
        set
        {
            this.y2Field = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplateFieldsFieldFieldMargin
{

    private object leftField;

    private object rightField;

    private object topField;

    private object bottomField;

    private bool bEVMarginFlagField;

    private bool ellipticalMarginFlagField;

    private bool optimizeCollRtnFlagField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object Left
    {
        get
        {
            return this.leftField;
        }
        set
        {
            this.leftField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object Right
    {
        get
        {
            return this.rightField;
        }
        set
        {
            this.rightField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object Top
    {
        get
        {
            return this.topField;
        }
        set
        {
            this.topField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object Bottom
    {
        get
        {
            return this.bottomField;
        }
        set
        {
            this.bottomField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool BEVMarginFlag
    {
        get
        {
            return this.bEVMarginFlagField;
        }
        set
        {
            this.bEVMarginFlagField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool EllipticalMarginFlag
    {
        get
        {
            return this.ellipticalMarginFlagField;
        }
        set
        {
            this.ellipticalMarginFlagField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool OptimizeCollRtnFlag
    {
        get
        {
            return this.optimizeCollRtnFlagField;
        }
        set
        {
            this.optimizeCollRtnFlagField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhasePlanTemplateFieldsFieldIsocenter
{

    private string placementField;

    private decimal xField;

    private decimal yField;

    private decimal zField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Placement
    {
        get
        {
            return this.placementField;
        }
        set
        {
            this.placementField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal X
    {
        get
        {
            return this.xField;
        }
        set
        {
            this.xField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal Y
    {
        get
        {
            return this.yField;
        }
        set
        {
            this.yField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal Z
    {
        get
        {
            return this.zField;
        }
        set
        {
            this.zField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhaseObjectiveTemplate
{

    private string typeField;

    private ProtocolPhasesPhaseObjectiveTemplateHelios heliosField;

    private ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructure[] objectivesAllStructuresField;

    private decimal versionField;

    /// <remarks/>
    public string Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhaseObjectiveTemplateHelios Helios
    {
        get
        {
            return this.heliosField;
        }
        set
        {
            this.heliosField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("ObjectivesOneStructure", IsNullable = false)]
    public ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructure[] ObjectivesAllStructures
    {
        get
        {
            return this.objectivesAllStructuresField;
        }
        set
        {
            this.objectivesAllStructuresField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal Version
    {
        get
        {
            return this.versionField;
        }
        set
        {
            this.versionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhaseObjectiveTemplateHelios
{

    private byte defaultSmoothingXField;

    private byte defaultSmoothingYField;

    private byte defaultMinimizeDoseField;

    private string defaultOptimizationTypeField;

    private byte maxIterationsField;

    private byte maxTimeField;

    private ProtocolPhasesPhaseObjectiveTemplateHeliosNormalTissueObjective normalTissueObjectiveField;

    private ProtocolPhasesPhaseObjectiveTemplateHeliosGeos geosField;

    private ProtocolPhasesPhaseObjectiveTemplateHeliosImat imatField;

    private bool defaultFixedJawsField;

    private bool interpolateField;

    private bool useColorsField;

    private bool targetAutocropField;

    private bool aldoField;

    /// <remarks/>
    public byte DefaultSmoothingX
    {
        get
        {
            return this.defaultSmoothingXField;
        }
        set
        {
            this.defaultSmoothingXField = value;
        }
    }

    /// <remarks/>
    public byte DefaultSmoothingY
    {
        get
        {
            return this.defaultSmoothingYField;
        }
        set
        {
            this.defaultSmoothingYField = value;
        }
    }

    /// <remarks/>
    public byte DefaultMinimizeDose
    {
        get
        {
            return this.defaultMinimizeDoseField;
        }
        set
        {
            this.defaultMinimizeDoseField = value;
        }
    }

    /// <remarks/>
    public string DefaultOptimizationType
    {
        get
        {
            return this.defaultOptimizationTypeField;
        }
        set
        {
            this.defaultOptimizationTypeField = value;
        }
    }

    /// <remarks/>
    public byte MaxIterations
    {
        get
        {
            return this.maxIterationsField;
        }
        set
        {
            this.maxIterationsField = value;
        }
    }

    /// <remarks/>
    public byte MaxTime
    {
        get
        {
            return this.maxTimeField;
        }
        set
        {
            this.maxTimeField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhaseObjectiveTemplateHeliosNormalTissueObjective NormalTissueObjective
    {
        get
        {
            return this.normalTissueObjectiveField;
        }
        set
        {
            this.normalTissueObjectiveField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhaseObjectiveTemplateHeliosGeos Geos
    {
        get
        {
            return this.geosField;
        }
        set
        {
            this.geosField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhaseObjectiveTemplateHeliosImat Imat
    {
        get
        {
            return this.imatField;
        }
        set
        {
            this.imatField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool DefaultFixedJaws
    {
        get
        {
            return this.defaultFixedJawsField;
        }
        set
        {
            this.defaultFixedJawsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool Interpolate
    {
        get
        {
            return this.interpolateField;
        }
        set
        {
            this.interpolateField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool UseColors
    {
        get
        {
            return this.useColorsField;
        }
        set
        {
            this.useColorsField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool TargetAutocrop
    {
        get
        {
            return this.targetAutocropField;
        }
        set
        {
            this.targetAutocropField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool Aldo
    {
        get
        {
            return this.aldoField;
        }
        set
        {
            this.aldoField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhaseObjectiveTemplateHeliosNormalTissueObjective
{

    private bool useField;

    private byte priorityField;

    private decimal distanceFromTargetBorderField;

    private byte startDoseField;

    private byte endDoseField;

    private decimal fallOffField;

    private string modeField;

    private bool autoField;

    /// <remarks/>
    public bool Use
    {
        get
        {
            return this.useField;
        }
        set
        {
            this.useField = value;
        }
    }

    /// <remarks/>
    public byte Priority
    {
        get
        {
            return this.priorityField;
        }
        set
        {
            this.priorityField = value;
        }
    }

    /// <remarks/>
    public decimal DistanceFromTargetBorder
    {
        get
        {
            return this.distanceFromTargetBorderField;
        }
        set
        {
            this.distanceFromTargetBorderField = value;
        }
    }

    /// <remarks/>
    public byte StartDose
    {
        get
        {
            return this.startDoseField;
        }
        set
        {
            this.startDoseField = value;
        }
    }

    /// <remarks/>
    public byte EndDose
    {
        get
        {
            return this.endDoseField;
        }
        set
        {
            this.endDoseField = value;
        }
    }

    /// <remarks/>
    public decimal FallOff
    {
        get
        {
            return this.fallOffField;
        }
        set
        {
            this.fallOffField = value;
        }
    }

    /// <remarks/>
    public string Mode
    {
        get
        {
            return this.modeField;
        }
        set
        {
            this.modeField = value;
        }
    }

    /// <remarks/>
    public bool Auto
    {
        get
        {
            return this.autoField;
        }
        set
        {
            this.autoField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhaseObjectiveTemplateHeliosGeos
{

    private string initialFieldDistributionField;

    private byte minimumNumberOfFieldsField;

    private byte maximumNumberOfFieldsField;

    private byte maximumElevationAngleForNonCoplanarFieldsField;

    private byte maximumCollimatorVariationField;

    private string localGeometricOptimizationModeField;

    /// <remarks/>
    public string InitialFieldDistribution
    {
        get
        {
            return this.initialFieldDistributionField;
        }
        set
        {
            this.initialFieldDistributionField = value;
        }
    }

    /// <remarks/>
    public byte MinimumNumberOfFields
    {
        get
        {
            return this.minimumNumberOfFieldsField;
        }
        set
        {
            this.minimumNumberOfFieldsField = value;
        }
    }

    /// <remarks/>
    public byte MaximumNumberOfFields
    {
        get
        {
            return this.maximumNumberOfFieldsField;
        }
        set
        {
            this.maximumNumberOfFieldsField = value;
        }
    }

    /// <remarks/>
    public byte MaximumElevationAngleForNonCoplanarFields
    {
        get
        {
            return this.maximumElevationAngleForNonCoplanarFieldsField;
        }
        set
        {
            this.maximumElevationAngleForNonCoplanarFieldsField = value;
        }
    }

    /// <remarks/>
    public byte MaximumCollimatorVariation
    {
        get
        {
            return this.maximumCollimatorVariationField;
        }
        set
        {
            this.maximumCollimatorVariationField = value;
        }
    }

    /// <remarks/>
    public string LocalGeometricOptimizationMode
    {
        get
        {
            return this.localGeometricOptimizationModeField;
        }
        set
        {
            this.localGeometricOptimizationModeField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhaseObjectiveTemplateHeliosImat
{

    private byte mUWeightField;

    private byte minMUField;

    private ushort maxMUField;

    private bool useMUField;

    private bool jawTrackingField;

    /// <remarks/>
    public byte MUWeight
    {
        get
        {
            return this.mUWeightField;
        }
        set
        {
            this.mUWeightField = value;
        }
    }

    /// <remarks/>
    public byte MinMU
    {
        get
        {
            return this.minMUField;
        }
        set
        {
            this.minMUField = value;
        }
    }

    /// <remarks/>
    public ushort MaxMU
    {
        get
        {
            return this.maxMUField;
        }
        set
        {
            this.maxMUField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool UseMU
    {
        get
        {
            return this.useMUField;
        }
        set
        {
            this.useMUField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool JawTracking
    {
        get
        {
            return this.jawTrackingField;
        }
        set
        {
            this.jawTrackingField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructure
{

    private ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructureStructureTarget structureTargetField;

    private object distanceField;

    private object samplePointsField;

    private uint colorField;

    private object avoidanceStructureModeField;

    private ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructureObjective[] structureObjectivesField;

    private string idField;

    private string nAMEField;

    private bool surfaceOnlyField;

    /// <remarks/>
    public ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructureStructureTarget StructureTarget
    {
        get
        {
            return this.structureTargetField;
        }
        set
        {
            this.structureTargetField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object Distance
    {
        get
        {
            return this.distanceField;
        }
        set
        {
            this.distanceField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object SamplePoints
    {
        get
        {
            return this.samplePointsField;
        }
        set
        {
            this.samplePointsField = value;
        }
    }

    /// <remarks/>
    public uint Color
    {
        get
        {
            return this.colorField;
        }
        set
        {
            this.colorField = value;
        }
    }

    /// <remarks/>
    public object AvoidanceStructureMode
    {
        get
        {
            return this.avoidanceStructureModeField;
        }
        set
        {
            this.avoidanceStructureModeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Objective", IsNullable = false)]
    public ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructureObjective[] StructureObjectives
    {
        get
        {
            return this.structureObjectivesField;
        }
        set
        {
            this.structureObjectivesField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string ID
    {
        get
        {
            return this.idField;
        }
        set
        {
            this.idField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string NAME
    {
        get
        {
            return this.nAMEField;
        }
        set
        {
            this.nAMEField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool SurfaceOnly
    {
        get
        {
            return this.surfaceOnlyField;
        }
        set
        {
            this.surfaceOnlyField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructureStructureTarget
{

    private object volumeIDField;

    private string volumeCodeField;

    private string volumeTypeField;

    private string volumeCodeTableField;

    private ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructureStructureTargetStructureCode structureCodeField;

    /// <remarks/>
    public object VolumeID
    {
        get
        {
            return this.volumeIDField;
        }
        set
        {
            this.volumeIDField = value;
        }
    }

    /// <remarks/>
    public string VolumeCode
    {
        get
        {
            return this.volumeCodeField;
        }
        set
        {
            this.volumeCodeField = value;
        }
    }

    /// <remarks/>
    public string VolumeType
    {
        get
        {
            return this.volumeTypeField;
        }
        set
        {
            this.volumeTypeField = value;
        }
    }

    /// <remarks/>
    public string VolumeCodeTable
    {
        get
        {
            return this.volumeCodeTableField;
        }
        set
        {
            this.volumeCodeTableField = value;
        }
    }

    /// <remarks/>
    public ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructureStructureTargetStructureCode StructureCode
    {
        get
        {
            return this.structureCodeField;
        }
        set
        {
            this.structureCodeField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructureStructureTargetStructureCode
{

    private string codeField;

    private string codeSchemeField;

    private decimal codeSchemeVersionField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string Code
    {
        get
        {
            return this.codeField;
        }
        set
        {
            this.codeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string CodeScheme
    {
        get
        {
            return this.codeSchemeField;
        }
        set
        {
            this.codeSchemeField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal CodeSchemeVersion
    {
        get
        {
            return this.codeSchemeVersionField;
        }
        set
        {
            this.codeSchemeVersionField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolPhasesPhaseObjectiveTemplateObjectivesOneStructureObjective
{

    private byte typeField;

    private byte operatorField;

    private decimal doseField;

    private string volumeField;

    private byte priorityField;

    private object parameterAField;

    private byte groupField;

    /// <remarks/>
    public byte Type
    {
        get
        {
            return this.typeField;
        }
        set
        {
            this.typeField = value;
        }
    }

    /// <remarks/>
    public byte Operator
    {
        get
        {
            return this.operatorField;
        }
        set
        {
            this.operatorField = value;
        }
    }

    /// <remarks/>
    public decimal Dose
    {
        get
        {
            return this.doseField;
        }
        set
        {
            this.doseField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public string Volume
    {
        get
        {
            return this.volumeField;
        }
        set
        {
            this.volumeField = value;
        }
    }

    /// <remarks/>
    public byte Priority
    {
        get
        {
            return this.priorityField;
        }
        set
        {
            this.priorityField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(IsNullable = true)]
    public object ParameterA
    {
        get
        {
            return this.parameterAField;
        }
        set
        {
            this.parameterAField = value;
        }
    }

    /// <remarks/>
    public byte Group
    {
        get
        {
            return this.groupField;
        }
        set
        {
            this.groupField = value;
        }
    }
}

/// <remarks/>
[System.SerializableAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public partial class ProtocolReview
{

    private object reviewStructuresField;

    private bool showMinField;

    private bool showMaxField;

    private bool showMeanField;

    private bool showModalField;

    private bool showMedianField;

    private bool showStdDevField;

    private bool showEUDField;

    private bool showTCPField;

    private bool showNTCPField;

    private bool showNDRField;

    private bool showEquivalentSphereDiameterField;

    private bool showConformityIndexField;

    private bool showGradientMeasureField;

    /// <remarks/>
    public object ReviewStructures
    {
        get
        {
            return this.reviewStructuresField;
        }
        set
        {
            this.reviewStructuresField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowMin
    {
        get
        {
            return this.showMinField;
        }
        set
        {
            this.showMinField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowMax
    {
        get
        {
            return this.showMaxField;
        }
        set
        {
            this.showMaxField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowMean
    {
        get
        {
            return this.showMeanField;
        }
        set
        {
            this.showMeanField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowModal
    {
        get
        {
            return this.showModalField;
        }
        set
        {
            this.showModalField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowMedian
    {
        get
        {
            return this.showMedianField;
        }
        set
        {
            this.showMedianField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowStdDev
    {
        get
        {
            return this.showStdDevField;
        }
        set
        {
            this.showStdDevField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowEUD
    {
        get
        {
            return this.showEUDField;
        }
        set
        {
            this.showEUDField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowTCP
    {
        get
        {
            return this.showTCPField;
        }
        set
        {
            this.showTCPField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowNTCP
    {
        get
        {
            return this.showNTCPField;
        }
        set
        {
            this.showNTCPField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowNDR
    {
        get
        {
            return this.showNDRField;
        }
        set
        {
            this.showNDRField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowEquivalentSphereDiameter
    {
        get
        {
            return this.showEquivalentSphereDiameterField;
        }
        set
        {
            this.showEquivalentSphereDiameterField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowConformityIndex
    {
        get
        {
            return this.showConformityIndexField;
        }
        set
        {
            this.showConformityIndexField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public bool ShowGradientMeasure
    {
        get
        {
            return this.showGradientMeasureField;
        }
        set
        {
            this.showGradientMeasureField = value;
        }
    }
}


