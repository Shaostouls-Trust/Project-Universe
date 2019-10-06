using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("CropsCollection")]
public class CropsContainer
{
    [XmlArray("crop"), XmlArrayItem("Crops")]
    public Crops[] crop;

    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(CropsContainer));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static CropsContainer Load(string path)
    {
        var serializer = new XmlSerializer(typeof(CropsContainer));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as CropsContainer;
        }
    }

    //Loads the xml directly from the given string. Useful in combination with www.text.
    public static CropsContainer LoadFromText(string text)
    {
        var serializer = new XmlSerializer(typeof(CropsContainer));
        return serializer.Deserialize(new StringReader(text)) as CropsContainer;
    }
}





