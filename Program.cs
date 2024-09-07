// See https://aka.ms/new-console-template for more information
using DynamicXmlSerializer;

var packageList = new PackageList
{
    Type = "SampleType",
    EvidencePackages =
    [
        new EvidencePackage(contra: new Contra
        {
            ContextualImageInfo = new ContextualImageInfo
            {
                TriggerImage = new TriggerImage(
                    imagePath: "J:\\Pictures\\Anya3.png"
                )
            }
        })
    ]
};

DynamicXmlWriter.WriteObjectToXml(packageList, $"J:\\\\Desktop\\{Guid.NewGuid()}-output.xml");

Console.WriteLine("Hello, World!");
