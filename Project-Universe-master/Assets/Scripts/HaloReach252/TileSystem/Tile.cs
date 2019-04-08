using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class Tile
{
	public string t_name;
	public string t_modelPath;
	//public string t_materialPath;
}

public class JsonTile
{
	public List<Tile> tiles = new List<Tile>();

	public void Initialize()
	{
		AssetDatabase.CreateFolder("Assets/", "Modded");
		AssetDatabase.CreateFolder("Assets/Modded/", "Models");
		//AssetDatabase.CreateFolder("Assets/Modded/", "Materials");
		AssetDatabase.CreateFolder("Assets/Modded/", "Tiles");
	}

	public void SaveTile(Tile tile, string path)
	{
		if (!File.Exists(path))
		{
			File.Create(path + tile.t_name + ".json");
			string fileJson = JsonUtility.ToJson(tile);
			File.WriteAllText(path + tile.t_name + ".json",fileJson);
		}
	}

	public Tile LoadTile(string path)
	{
		if (File.Exists(path))
		{
			string fileJson = File.ReadAllText(path);
			return JsonUtility.FromJson<Tile>(fileJson);
		}
		else
		{
			return null;
		}
	}

	public void CreateNewTile(string path)
	{
		Tile tile = LoadTile(path);
		tiles.Add(tile);
	}
}