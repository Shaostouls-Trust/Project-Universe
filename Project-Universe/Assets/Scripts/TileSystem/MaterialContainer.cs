using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("MaterialCollection")]
public class MaterialContainer
{
    [XmlArray("material"), XmlArrayItem("Materials")]
    public Materials[] material;

    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(MaterialContainer));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static MaterialContainer Load(string path)
    {
        var serializer = new XmlSerializer(typeof(MaterialContainer));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as MaterialContainer;
        }
    }

    //Loads the xml directly from the given string. Useful in combination with www.text.
    public static MaterialContainer LoadFromText(string text)
    {
        var serializer = new XmlSerializer(typeof(MaterialContainer));
        return serializer.Deserialize(new StringReader(text)) as MaterialContainer;
    }
}






