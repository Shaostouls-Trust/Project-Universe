using UnityEngine;
namespace Artngame.Orion.MiniGames
{
    public class Mover : MonoBehaviour
    {
        public float speed = 1.0f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.velocity = speed * transform.forward;
        }
    }
}