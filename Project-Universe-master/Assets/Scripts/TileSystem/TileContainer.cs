using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[XmlRoot("TileCollection")]
public class TileCollection
{
    [XmlArray("tiles"), XmlArrayItem("Tiles")]
     public Tiles[] tiles;

    public void Save(string path)
    {
        var serializer = new XmlSerializer(typeof(TileCollection));
        using (var stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, this);
        }
    }

    public static TileCollection Load(string path)
    {
        var serializer = new XmlSerializer(typeof(TileCollection));
        using (var stream = new FileStream(path, FileMode.Open))
        {
            return serializer.Deserialize(stream) as TileCollection;
        }
    }

    //Loads the xml directly from the given string. Useful in combination with www.text.
    public static TileCollection LoadFromText(string text)
    {
        var serializer = new XmlSerializer(typeof(TileCollection));
        return serializer.Deserialize(new StringReader(text)) as TileCollection;
    }
}