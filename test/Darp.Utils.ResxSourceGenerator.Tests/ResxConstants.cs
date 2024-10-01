namespace Darp.Utils.ResxSourceGenerator.Tests;

using System.Globalization;
using System.Text;

public static class ResxConstants
{
    public const string ResxHeader = """
<?xml version="1.0" encoding="utf-8"?>
<root>
  <xsd:schema id="root" xmlns="" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:msdata="urn:schemas-microsoft-com:xml-msdata">
    <xsd:import namespace="http://www.w3.org/XML/1998/namespace" />
    <xsd:element name="root" msdata:IsDataSet="true">
      <xsd:complexType>
        <xsd:choice maxOccurs="unbounded">
          <xsd:element name="metadata">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" />
              </xsd:sequence>
              <xsd:attribute name="name" use="required" type="xsd:string" />
              <xsd:attribute name="type" type="xsd:string" />
              <xsd:attribute name="mimetype" type="xsd:string" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="assembly">
            <xsd:complexType>
              <xsd:attribute name="alias" type="xsd:string" />
              <xsd:attribute name="name" type="xsd:string" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="data">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
                <xsd:element name="comment" type="xsd:string" minOccurs="0" msdata:Ordinal="2" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" msdata:Ordinal="1" />
              <xsd:attribute name="type" type="xsd:string" msdata:Ordinal="3" />
              <xsd:attribute name="mimetype" type="xsd:string" msdata:Ordinal="4" />
              <xsd:attribute ref="xml:space" />
            </xsd:complexType>
          </xsd:element>
          <xsd:element name="resheader">
            <xsd:complexType>
              <xsd:sequence>
                <xsd:element name="value" type="xsd:string" minOccurs="0" msdata:Ordinal="1" />
              </xsd:sequence>
              <xsd:attribute name="name" type="xsd:string" use="required" />
            </xsd:complexType>
          </xsd:element>
        </xsd:choice>
      </xsd:complexType>
    </xsd:element>
  </xsd:schema>
  <resheader name="resmimetype">
    <value>text/microsoft-resx</value>
  </resheader>
  <resheader name="version">
    <value>2.0</value>
  </resheader>
  <resheader name="reader">
    <value>System.Resources.ResXResourceReader, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
  <resheader name="writer">
    <value>System.Resources.ResXResourceWriter, System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</value>
  </resheader>
""";
    public const string ResxFooter = "</root>";

    public const string ResxEmptyDocument = ResxHeader + ResxFooter;
    public const string ResxValueDocument = $"""
{ResxHeader}
  <data name="Name" xml:space="preserve">
    <value>value</value>
    <comment>comment</comment>
  </data>
{ResxFooter}
""";
    public const string ResxMissingValueDocument = $"""
{ResxHeader}
  <data name="Name" xml:space="preserve">
    <comment>comment</comment>
  </data>
{ResxFooter}
""";

    public static string ResxDocument(string key, string value)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(ResxHeader);
        stringBuilder.Append(
            CultureInfo.InvariantCulture,
            $"""
<data name="{key}" xml:space="preserve">
  <value>{value}</value>
</data>
"""
        );
        stringBuilder.Append(ResxFooter);
        return stringBuilder.ToString();
    }

    public static string ResxDocumentWithValues((string Key, string Value)[] keyValues)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.Append(ResxHeader);
        foreach ((var key, var value) in keyValues)
        {
            stringBuilder.Append(
                CultureInfo.InvariantCulture,
                $"""
<data name="{key}" xml:space="preserve">
  <value>{value}</value>
</data>
"""
            );
        }
        stringBuilder.Append(ResxFooter);
        return stringBuilder.ToString();
    }
}
