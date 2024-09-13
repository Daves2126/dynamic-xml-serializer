namespace DynamicXmlSerializer;

using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Collections;

public abstract class DynamicXmlWriter
{
    /// <summary>
    /// Writes an object to an XML file with specified chunk size for large base64 encoded content.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">The path to the XML file.</param>
    /// <param name="chunkSize">The size of the chunks for base64 encoding, must be a multiple of 3</param>
    /// <exception cref="ArgumentException">Thrown when the file path is invalid or file does not exist.</exception>
    public static void WriteObjectToXml<T>(T? obj, string filePath, int chunkSize = 3072)
    {
        using var xmlWriter = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true });
        xmlWriter.WriteStartDocument();
        WriteObject(xmlWriter, obj, chunkSize);
        xmlWriter.WriteEndDocument();
        xmlWriter.Close();
    }

    private static void WriteObject(XmlWriter xmlWriter, object? obj, int chunkSize)
    {
        if (obj == null || AreAllPropertiesNull(obj)) return;
        var type = obj.GetType();
        var elementName = GetElementName(type);
        xmlWriter.WriteStartElement(elementName);

        foreach (var property in type.GetProperties())
        {
            var value = property.GetValue(obj);
            if (value == null) continue;
            var propertyType = GetPropertyType(property);
            WriteProperty(xmlWriter, propertyType, property, value, chunkSize);
        }

        xmlWriter.WriteEndElement();
    }
    
    private static void WriteProperty(XmlWriter xmlWriter, PropertyType propertyType, PropertyInfo property, object value, int chunkSize)
    {
        switch (propertyType)
        {
            case PropertyType.Image:
                WriteImageProperty(xmlWriter, property, value, chunkSize);
                break;
            case PropertyType.Base64InElement:
                WriteBase64InElementProperty(xmlWriter, value, chunkSize);
                break;
            case PropertyType.Array:
                WriteArrayProperty(xmlWriter, value, chunkSize);
                break;
            case PropertyType.XmlElement:
                WriteXmlElementProperty(xmlWriter, property, value);
                break;
            case PropertyType.Class:
                WriteClassProperty(xmlWriter, value, chunkSize);
                break;
            case PropertyType.Simple:
                WriteSimpleProperty(xmlWriter, property, value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static string GetElementName(Type type)
    {
        var xmlRootAttr = type.GetCustomAttribute<XmlRootAttribute>();
        var xmlElementAttr = type.GetCustomAttribute<XmlElementAttribute>();
        return xmlRootAttr?.ElementName ?? xmlElementAttr?.ElementName ?? type.Name;
    }

    private static PropertyType GetPropertyType(PropertyInfo property)
    {
        if (IsComplexType(property.PropertyType))
            return PropertyType.Class;

        if (property.GetCustomAttribute<Base64InElementAttribute>() != null)
            return PropertyType.Base64InElement;

        if (property.GetCustomAttribute<ImageToBase64Attribute>() != null)
            return PropertyType.Image;

        if (property.GetCustomAttribute<XmlArrayAttribute>() != null)
            return PropertyType.Array;

        if (property.GetCustomAttribute<XmlElementAttribute>() != null)
            return PropertyType.XmlElement;

        return PropertyType.Simple;
    }
    
    private static bool IsComplexType(Type type)
    {
        return type.IsClass && type != typeof(string) && !typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static void WriteClassProperty(XmlWriter xmlWriter, object value, int chunkSize)
    {
        WriteObject(xmlWriter, value, chunkSize);
    }

    private static void WriteImageProperty(XmlWriter xmlWriter, PropertyInfo property, object value, int chunkSize)
    {
        if (value is not string filePath || !File.Exists(filePath)) return;

        var xmlElementName = property.GetCustomAttribute<XmlElementAttribute>()?.ElementName ?? property.Name;
        xmlWriter.WriteStartElement(xmlElementName);
        WriteBase64InChunks(xmlWriter, filePath, chunkSize);
        xmlWriter.WriteEndElement();
    }

    private static void WriteBase64InElementProperty(XmlWriter xmlWriter, object value, int chunkSize)
    {
        if (value is not string filePath || !File.Exists(filePath)) return;
        WriteBase64InChunks(xmlWriter, filePath, chunkSize);
    }

    private static void WriteArrayProperty(XmlWriter xmlWriter, object value, int chunkSize)
    {
        var list = (IEnumerable<object?>)value;
        foreach (var item in list)
        {
            WriteObject(xmlWriter, item, chunkSize);
        }
    }

    private static void WriteXmlElementProperty(XmlWriter xmlWriter, PropertyInfo property, object value)
    {
        var elementName = property.GetCustomAttribute<XmlElementAttribute>()?.ElementName ?? property.Name;
        xmlWriter.WriteElementString(elementName, value.ToString());
        xmlWriter.Flush();
    }

    private static void WriteSimpleProperty(XmlWriter xmlWriter, PropertyInfo property, object value)
    {
        xmlWriter.WriteElementString(property.Name, value.ToString());
        xmlWriter.Flush();
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
            xmlWriter.Flush();
        }
    }
    
    private static bool AreAllPropertiesNull(object obj)
    {
        var properties = obj.GetType().GetProperties();
        return properties.All(property =>
        {
            var value = property.GetValue(obj);
            return value == null || (IsComplexType(property.PropertyType) && AreAllPropertiesNull(value)) ||
                   (value is IEnumerable enumerable && !enumerable.Cast<object>().Any()) ||
                   string.IsNullOrEmpty(value.ToString());
        });
    }

}
