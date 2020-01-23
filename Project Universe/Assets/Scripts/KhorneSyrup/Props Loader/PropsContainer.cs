using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("PropsContainer")]
public class PropsContainer
{
    [XmlArray("props"), XmlArrayItem("Props")]
    public Props[] props;

    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(PropsContainer));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static PropsContainer Load(string path)
    {
        var serializer = new XmlSerializer(typeof(PropsContainer));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as PropsContainer;
        }
    }

    //Loads the xml directly from the given string. Useful in combination with www.text.
    public static PropsContainer LoadFromText(string text)
    {
        var serializer = new XmlSerializer(typeof(PropsContainer));
        return serializer.Deserialize(new StringReader(text)) as PropsContainer;
    }
}