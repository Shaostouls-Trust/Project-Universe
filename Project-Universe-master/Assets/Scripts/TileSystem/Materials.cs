using System.Xml;
using System.Xml.Serialization;


public class Materials
{
    [XmlAttribute("name")]
    public string ID;
    public string ColorMask;
    public string Albedo;
    public string Metal;
    public string Emmisive;
    public string Normal;
    public string Detail;
    public string Dirt;
    public string MainColor;
    public string SecColor;
    public string DetailColor;
    public string TrimColor;
    public string EmissionColor;
    public float EmissiveMulti;

}
