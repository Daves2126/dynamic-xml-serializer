namespace DynamicXmlSerializer;

using System;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ImageToBase64Attribute : Attribute
{
}
