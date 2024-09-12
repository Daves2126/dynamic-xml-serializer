namespace DynamicXmlSerializer;

using System.Collections.Generic;
using System.Xml.Serialization;

public class Category
{
    [XmlElement("Id")]
    public int Id { get; set; }

    [XmlElement("Name")]
    public string Name { get; set; }
}

public class Product
{
    [XmlElement("Id")]
    public int Id { get; set; }

    [XmlElement("Name")]
    public string Name { get; set; }

    [XmlElement("Price")]
    public decimal Price { get; set; }

    [XmlElement("Category")]
    public Category Category { get; set; }

    [ImageToBase64]
    [Base64InElement]
    public string ImagePath { get; set; }
}

[XmlRoot("Order")]
public class Order
{
    [XmlElement("Id")]
    public int Id { get; set; }

    [XmlElement("OrderDate")]
    public DateTime OrderDate { get; set; }

    [XmlArray("Products")]
    [XmlArrayItem("Product")]
    public List<Product> Products { get; set; } = new List<Product>();
}
