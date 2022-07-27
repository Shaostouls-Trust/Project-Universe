using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Ship
{
    public class ShipController : MonoBehaviour
    {
        [SerializeField] private string shipName;
        // accel rates
        [SerializeField] private int speedCap;
        [Tooltip("Accel in +Z direction")]
        [SerializeField] private float accel;
        [Tooltip("Accel in -Z direction")]
        [SerializeField] private float decel;
        [Tooltip("Accel in -X direction")]
        [SerializeField] private float leftAcel;
        [Tooltip("Accel in +X direction")]
        [SerializeField] private float rightAcel;
        [Tooltip("Accel in +Y direction")]
        [SerializeField] private float upAcel;
        [Tooltip("Accel in -Y direction")]
        [SerializeField] private float downAcel;
        [SerializeField] private float pitchSpeed;
        [SerializeField] private float rollSpeed;
        [SerializeField] private float yawSpeed;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private List<GameObject> playerPositions;
        [SerializeField] private GameObject tools;
        [SerializeField] private int maxEnergy;
        [SerializeField] private GameObject energyBar;
        private float currentEnergy;
        // input axes
        private float horizMove;
        private float vertMove;
        private float horizLook;
        private float vertLook;
        private float latMove;//up/down
        private float roll;
        //editor options
        [SerializeField] private bool isDrone;
        [Tooltip("Is this drone controlled from the REMO menu in the player inventory.")]
        [SerializeField] private bool remoMenuControlled;
        private float forceAmplifier;
        [SerializeField] private float powerperunitforce = 0.025f;
        [SerializeField] private AudioSource ambientAudSrc;
        [SerializeField] private AudioSource toolSrc;

        public List<GameObject> PlayerPositions
        {
            get { return playerPositions; }
        }
        
        public string ShipName {
            get { return shipName; }
            set { shipName = value; }
        }

        public AudioSource ToolSound
        {
            get { return toolSrc; }
        }

        private void Start()
        {
            currentEnergy = maxEnergy;
            if(ambientAudSrc != null)
            {
                ambientAudSrc.Play();
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (CurrentEnergy > 0f)
            {
                AxisMovement();
                LookRotation();
            }
            //rotation force amp
            forceAmplifier = (rigidbody.mass/100f)*50f;
            
            //move players back into position
            //ReSetPositionRotation();
            
        }
        
        public float CurrentEnergy
        {
            get { return currentEnergy; }
            set { currentEnergy = value; }
        }
        public int MaxEnergy
        {
            get { return maxEnergy; }
        }

        public void DrawEnergy(float amt)
        {
            currentEnergy -= amt * Time.deltaTime;
        }

        private void Update()
        {
            //base energy draw
            DrawEnergy(0.05f);
            //energy ui
            float max = energyBar.transform.parent.GetComponent<RectTransform>().rect.width;
            float scale = max / maxEnergy;
            energyBar.transform.localScale = new Vector3(scale * currentEnergy, 1f, 1f);
        }

        private void AxisMovement()
        {
            float x = 0f;
            float y = 0f;
            float z = 0f;
            
            //L/R
            if (HorizMove > 0f)
            {
                x = HorizMove * accel;
            }
            else if(HorizMove < 0f)
            {
                x = HorizMove * decel;
            }
            else
            {
                //decel to zero
                if (rigidbody.velocity.x > 0f)
                {
                    //x = -decel / 2f;
                }
                else if (rigidbody.velocity.x < 0f)
                {
                    //x = accel / 2f;
                }
                else
                {
                    //x = 0f;
                }
            }
            //F/B
            if (VertMove > 0f)
            {
                z = VertMove * rightAcel;
            }
            else if(VertMove < 0f)
            {
                z = VertMove * leftAcel;
            }
            else
            {
                //decel to zero
                if (rigidbody.velocity.z > 0f)
                {
                    //z = -leftAcel / 2f;
                }
                else if (rigidbody.velocity.z < 0f)
                {
                    //z = rightAcel / 2f;
                }
                else
                {
                    //z = 0f;
                }
            }
            //U/D
            if (LatMove > 0f)
            {
                y = LatMove * upAcel;
            }
            else if(LatMove < 0f)
            {
                y = LatMove * downAcel;
            }
            else
            {
                //decel to zero
                if (rigidbody.velocity.y > 0f)
                {
                    //y = -downAcel / 2f;
                }
                else if (rigidbody.velocity.y < 0f)
                {
                    //y = upAcel / 2f;
                }
                else
                {
                    //y = 0f;
                }
            }
            Vector3 force = new Vector3(x, y, z);
            force = transform.TransformDirection(force);
            rigidbody.AddForce(force, ForceMode.Acceleration);
            if (force.magnitude != 0f)
            {
                DrawEnergy(1f);
            }
        }

        private void LookRotation()
        {
            float addXAngle = 0f;
            float addYAngle = 0f;
            float addZAngle = 0f;
            float decel;
            if (vertLook == 0f)
            {
                if (rigidbody.angularVelocity.x > 0f)
                {
                    decel = -pitchSpeed * Time.deltaTime;
                    if (-decel / 2f > rigidbody.angularVelocity.x)
                    {
                        addXAngle = 0f;
                        // this here causes the pitch-roll issue \/
                        //rigidbody.angularVelocity = new Vector3(0f, rigidbody.angularVelocity.y, rigidbody.angularVelocity.z);
                    }
                    else
                    {
                        addXAngle = decel;
                    }
                }
                else if (rigidbody.angularVelocity.x < 0f)
                {
                    decel = pitchSpeed * Time.deltaTime;
                    if (-decel / 2f < rigidbody.angularVelocity.x)
                    {
                        addXAngle = 0f;
                    }
                    else
                    {
                        addXAngle = decel;
                    }
                }
                else
                {
                    addXAngle = 0f;
                }
                
            }
            else
            {
                addXAngle = pitchSpeed * Time.deltaTime * vertLook;// * -1f;
            }

            
            if (horizLook == 0f)
            {
                if (rigidbody.angularVelocity.y > 0f)
                {
                    decel = -yawSpeed * Time.deltaTime;
                    if (-decel / 2f > rigidbody.angularVelocity.y)
                    {
                        addYAngle = 0f;
                    }
                    else
                    {
                        addYAngle = decel;
                    }
                }
                else if (rigidbody.angularVelocity.y < 0f)
                {
                    decel = yawSpeed * Time.deltaTime;
                    if (-decel / 2f < rigidbody.angularVelocity.y)
                    { 
                        addYAngle = 0f;
                    }
                    else
                    {
                        addYAngle = decel;
                    }
                }
                else
                {
                    addYAngle = 0f;
                }
            }
            else
            {
                addYAngle = yawSpeed * Time.deltaTime * horizLook;
            }

            if (roll == 0f)
            {
                if (rigidbody.angularVelocity.z > 0f)
                {
                    decel = -rollSpeed * Time.deltaTime;
                    if (-decel / 2f > rigidbody.angularVelocity.z)
                    {
                        addZAngle = 0f;
                    }
                    else
                    {
                        addZAngle = decel;
                    }
                }
                else if (rigidbody.angularVelocity.z < 0f)
                {
                    decel = rollSpeed * Time.deltaTime;
                    if (-decel/2f < rigidbody.angularVelocity.z)
                    {
                        addZAngle = 0f;
                    }
                    else
                    {
                        addZAngle = decel;
                    }
                }
                else
                {
                    addZAngle = 0f;
                }
            }
            else
            {
                addZAngle = rollSpeed * Time.deltaTime * roll;
            }
            //Debug.Log(addXAngle + " " + addYAngle + " " + addZAngle);
            Vector3 worldAngles = new Vector3(addXAngle, addYAngle, addZAngle);
            //worldAngles += transform.eulerAngles;
            //rigidbody.MoveRotation(Quaternion.Euler(worldAngles));

            worldAngles = transform.TransformDirection(worldAngles);
            //Debug.Log(worldAngles.x + " " + worldAngles.y + " " + worldAngles.z);

            rigidbody.AddTorque(worldAngles*forceAmplifier, ForceMode.Force);
            if (worldAngles.magnitude != 0f)
            {
                DrawEnergy(0.1f + (forceAmplifier * powerperunitforce));
            }
        }

        private void ReSetPositionRotation()
        {
            for (int i = 0; i < playerPositions.Count; i++)
            {
                RigidbodyJointScript rjs = playerPositions[i].GetComponent<RigidbodyJointScript>();
                //rjs.ResetPosition();
                rjs.SetRotation(transform);
            }
        }

        public float HorizMove
        {
            get { return horizMove; }
            set { horizMove = value; }
        }
        public float VertMove
        {
            get { return vertMove; }
            set { vertMove = value; }
        }
        public float HorizLook
        {
            get { return horizLook; }
            set { horizLook = value; }
        }
        public float VertLook
        {
            get { return vertLook; }
            set { vertLook = value; }
        }
        public float LatMove
        {
            get { return latMove; }
            set { latMove = value; }
        }
        public float Roll
        {
            get { return roll; }
            set { roll = value; }
        }

        public GameObject Tools
        {
            get { return tools; }
        }
    }
}