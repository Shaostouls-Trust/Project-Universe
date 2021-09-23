using UnityEngine;

namespace Impact.Demo
{
    public class DemoSlideAndRollSeesaw : MonoBehaviour
    {
        [SerializeField]
        private Rigidbody seesawRigidbody;
        [SerializeField]
        private Rigidbody rollerRigidbody;
        [SerializeField]
        private float torque;

        public void TiltLeft()
        {
            seesawRigidbody.AddRelativeTorque(Vector3.right * torque * Time.deltaTime);
        }

        public void TiltRight()
        {
            seesawRigidbody.AddRelativeTorque(Vector3.right * -torque * Time.deltaTime);
        }

        public void ToggleRollerFrozen()
        {
            rollerRigidbody.freezeRotation = !rollerRigidbody.freezeRotation;
        }
    }
}

