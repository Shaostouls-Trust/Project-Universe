using System.Xml;
using System.Xml.Serialization;


public class Tiles
{
    [XmlAttribute("name")]
    public string Name;

    public string model_path;
    public string material_path;
}