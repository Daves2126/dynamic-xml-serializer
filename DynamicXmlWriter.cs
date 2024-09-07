namespace DynamicXmlSerializer;

using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

public abstract class DynamicXmlWriter
{

    /// <summary>
    /// Escribe un objeto en un archivo XML de forma progresiva.
    /// </summary>
    /// <typeparam name="T">El tipo del objeto a escribir.</typeparam>
    /// <param name="obj">El objeto a escribir.</param>
    /// <param name="filePath">La ruta del archivo de destino.</param>
    /// <param name="chunkSize">El tamaño de los chunks para la conversión de base64.</param>
    public static void WriteObjectToXml<T>(T? obj, string filePath, int chunkSize = 3072)
    {
        using var xmlWriter = XmlWriter.Create(filePath, new XmlWriterSettings { Indent = true });
        xmlWriter.WriteStartDocument();
        WriteObject(xmlWriter, obj, chunkSize);
        xmlWriter.WriteEndDocument();
    }

    private static void WriteObject(XmlWriter xmlWriter, object? obj, int chunkSize)
    {
        if (obj == null) return;

        var type = obj.GetType();
        xmlWriter.WriteStartElement(type.Name);

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
                case PropertyType.Array:
                    WriteArrayProperty(xmlWriter, property, value, chunkSize);
                    break;
                case PropertyType.XmlElement:
                    WriteXmlElementProperty(xmlWriter, property, value);
                    break;
                case PropertyType.Class:
                    WriteObject(xmlWriter, value, chunkSize);
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
        xmlWriter.Flush();
    }

    private static void WriteArrayProperty(XmlWriter xmlWriter, PropertyInfo property, object value, int chunkSize)
    {
        xmlWriter.WriteStartElement(property.Name);
        var list = (IEnumerable<object?>)value;
        foreach (var item in list)
        {
            WriteObject(xmlWriter, item, chunkSize);
        }
        xmlWriter.WriteEndElement();
        xmlWriter.Flush();
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
}