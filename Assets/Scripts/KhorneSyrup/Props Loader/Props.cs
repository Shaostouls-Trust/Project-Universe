using System.Xml;
using System.Xml.Serialization;


public class Props
{
    [XmlAttribute("name")]
    public string Name;
    public string type;
    public string description;
    public string model_path;
    public string material_path;
}