using System.Xml;
using System.Xml.Serialization;


public class Materials
{
    [XmlAttribute("name")]
    public string ID;
    public string ColorMask;
    public string Albedo;
    public string Dirt;
    public string EmmisiveAO;
    public string MetalSmooth;
    public string NormalDM;
    public string MainColor;
    public string SecColor;
    public string TertColor;
    public string QuatColor;
    public string DetailColor;
    public string SecDetailColor;
    public string EmissionColor;
    public float EmissiveMulti;

}
