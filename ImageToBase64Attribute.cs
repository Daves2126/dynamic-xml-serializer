namespace DynamicXmlSerializer;

using System;

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ImageToBase64Attribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
sealed class Base64InElementAttribute : Attribute
{
    public Base64InElementAttribute() { }
}