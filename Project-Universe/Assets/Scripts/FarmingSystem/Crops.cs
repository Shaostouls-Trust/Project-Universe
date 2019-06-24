using System.Xml;
using System.Xml.Serialization;


public class Crops
{
    [XmlAttribute("name")]
    public string ID;
    public string name;
    public string model;
    public float GrowTime;

}
