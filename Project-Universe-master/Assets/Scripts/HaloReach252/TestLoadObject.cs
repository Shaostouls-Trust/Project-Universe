using UnityEngine;

public class TestLoadObject : MonoBehaviour
{
	#region Variables
	[SerializeField]
	public string objectId;

	public TestLoadObjectData m_objectData = new TestLoadObjectData();
	#endregion

	#region Methods

	void Awake()
	{
		objectId = System.Guid.NewGuid().ToString();
	}

	void Start()
	{
		
	}

	public void Explode(float force)
	{
		GetComponent<Rigidbody>().AddExplosionForce(force, transform.position, 4f, 0.7f);
	}

	public void LoadObjectData(TestLoadObjectData data)
	{
		objectId = data.objectId;
		transform.position = new Vector3(data.posX, data.posY, data.posZ);
		transform.rotation = Quaternion.Euler(new Vector3(data.rotX, data.rotY, data.rotZ));
		GetComponent<Rigidbody>().velocity = new Vector3(data.velX, data.velY, data.velZ);
	}

	public void SaveObjectData()
	{
		m_objectData = new TestLoadObjectData();
		m_objectData.GetData(this);
	}

	#endregion
}