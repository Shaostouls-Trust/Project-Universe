using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using ProjectUniverse.Base;
using ProjectUniverse.Environment.Volumes;
using ProjectUniverse.Serialization.Surrogates;
using ProjectUniverse.Production.Resources;
using UnityEditor;
using UnityEngine;
using ProjectUniverse.Player.PlayerController;
using System;

namespace ProjectUniverse.Serialization.Handler
{

    public static class SerializationHandler
    {
		private static string SavePathDefault = Path.Combine(Application.persistentDataPath,"Data\\Saves");
		private static string SaveExtensionDefault = ".save";
        private static BinaryFormatter formatter;
        private static SurrogateSelector selector;

        public static void SavePlayer(string saveID,PlayerData playerData)
		{
			string path = SavePathDefault+"\\"+(saveID+SaveExtensionDefault);
			FileStream stream;
			if (File.Exists(path))
			{
				stream = new FileStream(path, FileMode.Create);
			}
			else
			{
				Directory.CreateDirectory(SavePathDefault);
				stream = new FileStream(path, FileMode.CreateNew);
			}
			
			GetBinaryFormatter().Serialize(stream, playerData);
			stream.Close();
		}

		public static PlayerData Load(string saveID)
		{
			string path = SavePathDefault + "\\" + (saveID + SaveExtensionDefault);
			if (File.Exists(path))
			{
				formatter = GetBinaryFormatter();
				FileStream file = new FileStream(path, FileMode.Open,FileAccess.Read,FileShare.Read);
				Debug.Log("Loaded... Deserializing");
				PlayerData save = (PlayerData)formatter.Deserialize(file);
				file.Close();
				return save;
			}
			return null;
		}

		private static BinaryFormatter GetBinaryFormatter()
		{
			formatter = new BinaryFormatter();
			selector = new SurrogateSelector();
			Vector3Surrogate v3s = new Vector3Surrogate();
			QuaternionSurrogate qts = new QuaternionSurrogate();
			Matrix4x4Surrogate m44s = new Matrix4x4Surrogate();
			GUIDSurrogate gds = new GUIDSurrogate();
			RigidbodySurrogate rbs = new RigidbodySurrogate();
			BoxColliderSurrogate bcs = new BoxColliderSurrogate();
			MeshColliderSurrogate mcs = new MeshColliderSurrogate();
			ItemStackSurrogate iss = new ItemStackSurrogate();
			PlayerVolumeControllerSurrogate pvcs = new PlayerVolumeControllerSurrogate();
			ConsumableResources_Surrogate.Consumable_Component_Surrogate ccs = new ConsumableResources_Surrogate.Consumable_Component_Surrogate();
			ConsumableResources_Surrogate.Consumable_Ingot_Surrogate cis = new ConsumableResources_Surrogate.Consumable_Ingot_Surrogate();
			ConsumableResources_Surrogate.Consumable_Material_Surrogate cms = new ConsumableResources_Surrogate.Consumable_Material_Surrogate();
			ConsumableResources_Surrogate.Consumable_Ore_Surrogate cos = new ConsumableResources_Surrogate.Consumable_Ore_Surrogate();
			PlayerStats_Surrogate pss = new PlayerStats_Surrogate();
			selector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All), v3s);
			selector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All), qts);
			selector.AddSurrogate(typeof(Matrix4x4), new StreamingContext(StreamingContextStates.All), m44s);
			selector.AddSurrogate(typeof(Guid), new StreamingContext(StreamingContextStates.All), gds);
			selector.AddSurrogate(typeof(Rigidbody), new StreamingContext(StreamingContextStates.All), rbs);
			selector.AddSurrogate(typeof(BoxCollider), new StreamingContext(StreamingContextStates.All), bcs);
			selector.AddSurrogate(typeof(MeshCollider), new StreamingContext(StreamingContextStates.All), mcs);
			selector.AddSurrogate(typeof(ItemStack), new StreamingContext(StreamingContextStates.All), iss);
			selector.AddSurrogate(typeof(PlayerVolumeController), new StreamingContext(StreamingContextStates.All), pvcs);
			selector.AddSurrogate(typeof(Consumable_Component), new StreamingContext(StreamingContextStates.All), ccs);
			selector.AddSurrogate(typeof(Consumable_Ingot), new StreamingContext(StreamingContextStates.All), cis);
			selector.AddSurrogate(typeof(Consumable_Material), new StreamingContext(StreamingContextStates.All), cms);
			selector.AddSurrogate(typeof(Consumable_Ore), new StreamingContext(StreamingContextStates.All), cos);
			selector.AddSurrogate(typeof(SupplementalController), new StreamingContext(StreamingContextStates.All), pss);
			
			formatter.SurrogateSelector = selector;
			return formatter;
		}
	}
}