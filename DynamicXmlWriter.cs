namespace DynamicXmlSerializer;

using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public abstract class DynamicXmlWriter
{
    public static void WriteObjectToXml<T>(T? obj, string filePath, int chunkSize = 3072)
    {
        using var xmlWriter = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true });
        xmlWriter.WriteStartDocument();
        WriteObject(xmlWriter, obj, chunkSize);
        xmlWriter.WriteEndDocument();
        xmlWriter.Flush();  // Flush al final para asegurar que todos los datos se escriben
    }

    private static void WriteObject(XmlWriter xmlWriter, object? obj, int chunkSize)
    {
        if (obj == null) return;

        var type = obj.GetType();
        var xmlElementName = type.Name;

        // Use XmlRootAttribute if available
        var rootAttr = type.GetCustomAttribute<XmlRootAttribute>();
        if (rootAttr != null)
        {
            xmlElementName = rootAttr.ElementName ?? xmlElementName;
        }

        xmlWriter.WriteStartElement(xmlElementName);

        foreach (var property in type.GetProperties())
        {
            var value = property.GetValue(obj);
            if (value == null) continue;

            var propertyType = GetPropertyType(property);

            switch (propertyType)
            {
                case PropertyType.Image:
                    WriteImageProperty(xmlWriter, property, value, chunkSize);
                    break;
                case PropertyType.Base64InElement:
                    WriteBase64InElementProperty(xmlWriter, property, value, chunkSize);
                    break;
                case PropertyType.Array:
                    WriteArrayProperty(xmlWriter, property, value, chunkSize);
                    break;
                case PropertyType.XmlElement:
                    WriteXmlElementProperty(xmlWriter, property, value);
                    break;
                case PropertyType.Class:
                    WriteObject(xmlWriter, value, chunkSize); // Llamar recursivamente para propiedades de tipo clase
                    break;
                case PropertyType.Simple:
                    WriteSimpleProperty(xmlWriter, property, value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        xmlWriter.WriteEndElement();
    }

    private static PropertyType GetPropertyType(PropertyInfo property)
    {
        if (property.GetCustomAttribute<Base64InElementAttribute>() != null)
            return PropertyType.Base64InElement;

        if (property.GetCustomAttribute<ImageToBase64Attribute>() != null)
            return PropertyType.Image;

        if (property.GetCustomAttribute<XmlArrayAttribute>() != null)
            return PropertyType.Array;

        if (property.GetCustomAttribute<XmlElementAttribute>() != null)
            return PropertyType.XmlElement;

        if (property.PropertyType.IsClass && property.PropertyType != typeof(string) && !typeof(IEnumerable<object>).IsAssignableFrom(property.PropertyType))
            return PropertyType.Class;

        return PropertyType.Simple;
    }

    private static void WriteImageProperty(XmlWriter xmlWriter, PropertyInfo property, object value, int chunkSize)
    {
        if (value is not string filePath || !File.Exists(filePath)) return;

        var xmlElementName = property.GetCustomAttribute<XmlElementAttribute>()?.ElementName ?? property.Name;
        xmlWriter.WriteStartElement(xmlElementName);
        WriteBase64InChunks(xmlWriter, filePath, chunkSize);
        xmlWriter.WriteEndElement();
        xmlWriter.Flush();  // Flush después de escribir el elemento
    }

    private static void WriteBase64InElementProperty(XmlWriter xmlWriter, PropertyInfo property, object value, int chunkSize)
    {
        if (value is not string filePath || !File.Exists(filePath)) return;

        var xmlElementName = property.GetCustomAttribute<XmlElementAttribute>()?.ElementName ?? property.Name;
        xmlWriter.WriteStartElement(xmlElementName);
        WriteBase64InChunks(xmlWriter, filePath, chunkSize);
        xmlWriter.WriteEndElement();
    }

    private static void WriteArrayProperty(XmlWriter xmlWriter, PropertyInfo property, object value, int chunkSize)
    {
        var list = (IEnumerable<object?>)value;
        var arrayElementName = property.GetCustomAttribute<XmlElementAttribute>()?.ElementName ?? property.Name;
        xmlWriter.WriteStartElement(arrayElementName);
        foreach (var item in list)
        {
            WriteObject(xmlWriter, item, chunkSize);
        }
        xmlWriter.WriteEndElement();
        xmlWriter.Flush();  // Flush después de escribir la lista completa
    }

    private static void WriteXmlElementProperty(XmlWriter xmlWriter, PropertyInfo property, object value)
    {
        var elementName = property.GetCustomAttribute<XmlElementAttribute>()?.ElementName ?? property.Name;
        xmlWriter.WriteElementString(elementName, value.ToString());
        xmlWriter.Flush();  // Flush después de escribir el elemento
    }

    private static void WriteSimpleProperty(XmlWriter xmlWriter, PropertyInfo property, object value)
    {
        xmlWriter.WriteElementString(property.Name, value.ToString());
        xmlWriter.Flush();  // Flush después de escribir el elemento
    }

    private static void WriteBase64InChunks(XmlWriter xmlWriter, string filePath, int chunkSize)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var buffer = new byte[chunkSize];
        int bytesRead;
        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            var base64Chunk = Convert.ToBase64String(buffer, 0, bytesRead);
            xmlWriter.WriteString(base64Chunk);
            xmlWriter.Flush();  // Flush después de escribir cada chunk
        }
    }
}
