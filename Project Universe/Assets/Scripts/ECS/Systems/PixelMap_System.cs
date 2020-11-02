using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class PixelMap_System : ComponentSystem//, IConvertGameObjectToEntity
{
	[SerializeField]
	private GameObject rootGameObject;
	//public bool ECSIntFinished;

    protected override void OnUpdate()
    {
		
	}
	/*
	public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
	{
		using (BlobAssetStore blobAssetStore = new BlobAssetStore())
		{
			//convert whole devship object?
			Entity prefabEntity = GameObjectConversionUtility.ConvertGameObjectHierarchy(this.gameobject,
				GameObjectConversionSettings.FromWorld(dstManager.World, blobAssetStore));
			//store the prefab entity in the list that we will instantiate from
			//shipEntityArray.Add(prefabEntity);
			//instanciate the tile
			Entity tileEntity = dstManager.Instantiate(prefabEntity);
			//dstManager.SetName(tileEntity, prefabID + " " + count++);
			//set the rotation of the object. No idea if the other components need set or not.
			//myEntityManager.SetComponentData(tileEntity,
				//new Translation { Value = Quaternion.Euler(0.0f, rotation, 0.0f).eulerAngles });
		}
	}
	*/
	protected void CustomConvertToEntity()
    {
		//EntityManager myEntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		//create Archetype for tiles (they will eventually be read in just like the maps)
		/*
		EntityArchetype basicTile = myEntityManager.CreateArchetype(
			//typeof(Component_Name), myEntityManager.setName(entity,name)
			//typeof(Component_PrefabEntity),
			typeof(Translation),
			typeof(Rotation),
			typeof(Renderer),
			typeof(LocalToWorld),
			typeof(Collider),
			typeof(Material),
			typeof(Mesh)
			);
		*/
	}
}
