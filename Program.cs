// See https://aka.ms/new-console-template for more information
using DynamicXmlSerializer;

var category1 = new Category
{
    Id = 1,
    Name = "Smartphones",
    SubCategory = new SubCategory
    {
        Id = Guid.NewGuid(),
        Name = "Electronics"
    }
};
var category2 = new Category
{
    Id = 2,
    Name = "Books",
    SubCategory = new SubCategory
    {
        Id = Guid.NewGuid(),
        Name = "Literature"
    }
};

var product1 = new Product
{
    Id = 1,
    Name = "Smartphone",
    Price = 299.99m,
    Category = category1,
    ImagePath = "J:\\\\Documents\\\\GitHub\\\\dynamic-xml-serializer\\\\apache.png"
};

var product2 = new Product
{
    Id = 2,
    Name = "Novel",
    Price = 19.99m,
    Category = category2,
    ImagePath = "J:\\\\Documents\\\\GitHub\\\\dynamic-xml-serializer\\\\apache.png"
};

// Crear orden
var order = new Order
{
    Id = 1,
    OrderDate = DateTime.Now,
    Products = new List<Product> { product1, product2 }
};


DynamicXmlWriter.WriteObjectToXml(order, $"J:\\Documents\\GitHub\\dynamic-xml-serializer\\{Guid.NewGuid()}-output.xml");

Console.WriteLine("Hello, World!");
