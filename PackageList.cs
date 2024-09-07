using System.Xml.Serialization;

namespace DynamicXmlSerializer;

using System.Collections.Generic;

public class PackageList
{
    public string Type { get; set; }
    
    [XmlArray("EvidencePackages")]
    [XmlArrayItem("EvidencePackage")]
    public List<EvidencePackage> EvidencePackages { get; set; }
}

public class EvidencePackage
{
    public EvidencePackage()
    {
    }

    public EvidencePackage(Contra contra)
    {
        Contra = contra;
    }

    public Contra Contra { get; set; }
}

public class Contra
{
    public ContextualImageInfo ContextualImageInfo { get; set; }
}

public class ContextualImageInfo
{
    public TriggerImage TriggerImage { get; set; }
}

public class TriggerImage
{
    public TriggerImage()
    {
    }

    public TriggerImage(string imagePath)
    {
        ImagePath = imagePath;
    }

    [XmlElement("Text")]
    [ImageToBase64] // Set this property to be converted as base64
    public string ImagePath { get; set; } // Route where the file is located
}
