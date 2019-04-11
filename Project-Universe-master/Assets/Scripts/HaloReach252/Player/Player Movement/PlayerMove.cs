using UnityEngine;

public class PlayerMove : MonoBehaviour
{
	#region Variables
	[SerializeField]
	private float defaultSpeed;
	public float DefaultSpeed { get; private set; }
	[SerializeField]
	private float speed;
	public float Speed { get; private set; }
	[SerializeField]
	private float runSpeed;
	public float RunSpeed { get; private set; }
	[SerializeField]
	private float maxSpeed;
	public float MaxSpeed { get; private set; }

	private Rigidbody rb;
	private GroundCheck groundCheck;

	#endregion

	#region Methods

    void Start()
    {
		groundCheck = GetComponentInChildren<GroundCheck>();
		rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
		Debug.Log("Is grounded: " + groundCheck.IsGrounded());
		if (groundCheck.IsGrounded())
		{
			//Checks if a sprint key is being pressed down
			if (Input.GetAxis("Sprint") != 0)
				speed = runSpeed;
			else
				speed = defaultSpeed;

			//Gets WASD input
			float h = Input.GetAxis("Horizontal");
			float v = Input.GetAxis("Vertical");

			//The vector the player will move in
			Vector3 movePos = new Vector3(h * speed, 0, v * speed);

			//Sets movement relative to rotation, and clamps the velocity to prevent superspeed
			rb.AddRelativeForce(movePos);
			rb.velocity = new Vector3(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y, Mathf.Clamp(rb.velocity.z, -maxSpeed, maxSpeed));
		}
    }
	
	#endregion
}
