using UnityEngine;
namespace Artngame.Orion.MiniGames
{
    public class RandomRotator : MonoBehaviour
    {
        public float tumble = 5.0f;

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.angularVelocity = Random.insideUnitSphere * tumble;
        }
    }
}