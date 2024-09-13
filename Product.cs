namespace DynamicXmlSerializer;

using System.Collections.Generic;
using System.Xml.Serialization;

[XmlRoot("category")]
public class Category
{
    [XmlElement("id")]
    public int Id { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }
    
    [XmlElement("subCategory")]
    public SubCategory SubCategory { get; set; }
}

[XmlRoot("subcategory")]
public class SubCategory
{
    [XmlElement("id")]
    public Guid Id { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }
}

[XmlRoot("product")]
public class Product
{
    [XmlElement("id")]
    public int Id { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("price")]
    public decimal Price { get; set; }

    [XmlElement("category")]
    public Category Category { get; set; }

    [ImageToBase64]
    [Base64InElement]
    public string ImagePath { get; set; }
}

[XmlRoot("order")]
public class Order
{
    [XmlElement("id")]
    public int Id { get; set; }

    [XmlElement("orderDate")]
    public DateTime OrderDate { get; set; }

    [XmlArray] 
    public List<Product> Products { get; set; } = new List<Product>();
    
    [XmlElement("test")]
    public Test Test { get; set; }
}

[XmlRoot("test")]
public class Test
{
    public string? TestString { get; set; }
    public string? AnotherTestString { get; set; }
}