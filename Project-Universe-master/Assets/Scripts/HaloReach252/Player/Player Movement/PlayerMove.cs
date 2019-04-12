using UnityEngine;
using Mirror;

public class PlayerMove : NetworkBehaviour
{
	#region Variables
	[SyncVar]
	public float defaultSpeed;
	[SyncVar]
	public float speed;
	[SyncVar]
	public float runSpeed;
	[SyncVar]
	public float maxSpeed;
	private Rigidbody rb;
	private GroundCheck groundCheck;

	#endregion

	#region Methods

    void Start()
    {
		if (!isLocalPlayer)
			return;
		groundCheck = GetComponentInChildren<GroundCheck>();
		rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
		if (!isLocalPlayer)
			return;

		CmdMove();
    }

	[Command]
	void CmdMove()
	{
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
