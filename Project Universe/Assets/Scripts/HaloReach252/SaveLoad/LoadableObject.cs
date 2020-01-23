using UnityEngine;

public class LoadableObject : MonoBehaviour
{
	#region Variables
	[SerializeField]
	public string objectId;

	public bool savePos=true, saveRot=true, hasRigidbody=false;

	public SaveObjectData m_objectData = new SaveObjectData();
	#endregion

	#region Methods

	void Awake()
	{
		//Assigns the object a GUID for tracking purposes
		objectId = System.Guid.NewGuid().ToString();
	}

	//Explodes the object upwards, used for testing
	public void Explode(float force)
	{
		GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, 4f, 0.7f);
	}

	//Load the given data into this object
	public void LoadObjectData(SaveObjectData data)
	{
		objectId = data.objectId;
		if(savePos)
			transform.position = new Vector3(data.posX, data.posY, data.posZ);
		if(saveRot)
			transform.rotation = Quaternion.Euler(new Vector3(data.rotX, data.rotY, data.rotZ));
		if(hasRigidbody)
			GetComponent<Rigidbody>().velocity = new Vector3(data.velX, data.velY, data.velZ);
	}

	//Creates a new saveobjectdata object for saving purposes, then saves it
	public void SaveObjectData()
	{
		m_objectData = new SaveObjectData();
		m_objectData.GetData(this);
	}

	#endregion
}
