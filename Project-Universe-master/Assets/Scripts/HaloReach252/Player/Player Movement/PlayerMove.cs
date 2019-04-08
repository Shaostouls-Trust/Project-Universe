using UnityEngine;

public class PlayerMove : MonoBehaviour
{
	#region Variables
	[SerializeField]
	private float speed;
	public float Speed { get; private set; }
	[SerializeField]
	private float runSpeed;
	public float RunSpeed { get; private set; }

	private Rigidbody rb;
	private bool grounded;
	private GroundCheck groundCheck;

	#endregion

	#region Methods

    void Start()
    {
		groundCheck = GetComponentInChildren<GroundCheck>();
    }

    void Update()
    {
        
    }
	
	#endregion
}
