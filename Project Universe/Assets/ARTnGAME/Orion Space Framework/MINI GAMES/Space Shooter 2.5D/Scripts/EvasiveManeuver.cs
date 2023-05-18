using System.Collections;
using  Artngame.Orion.MiniGames;
using UnityEngine;
namespace Artngame.Orion.MiniGames
{
    public class EvasiveManeuver : MonoBehaviour
    {
        public Vector2 maneuverSpeed;
        public Vector2 maneuverTime;
        public Vector2 pauseTime;
        public float startDelay;
        public float tilt;

        [Tooltip("This will be a fraction ratio of maneuverTime, so it should be between 0 and 1")]
        public Vector2 accelerateTimeRatio;

        public Boundary boundary;

        private Rigidbody rb;

        public IEnumerator Maneuver()
        {
            yield return new WaitForSeconds(startDelay);
            Vector3 velocity = rb.velocity;
            while (true)
            {
                float moveDir = -Mathf.Sign(rb.position.x);
                float maxSpeed = moveDir * Random.Range(maneuverSpeed.x, maneuverSpeed.y);
                //            rb.velocity = velocity;
                float moveTime = Random.Range(maneuverTime.x, maneuverTime.y);
                float accelerateTime = Random.Range(accelerateTimeRatio.x, accelerateTimeRatio.y) * moveTime;
                var accelerated = 0f;
                while (accelerated < accelerateTime)
                {
                    float currentSpeed = Mathf.Lerp(0, maxSpeed, accelerated / accelerateTime);
                    velocity.x = currentSpeed;
                    rb.velocity = velocity;
                    yield return null;
                    accelerated += Time.deltaTime;
                }
                velocity.x = maxSpeed;
                rb.velocity = velocity;
                yield return new WaitForSeconds(moveTime - accelerated);
                velocity.x = 0.0f;
                rb.velocity = velocity;
                rb.rotation = Quaternion.identity;
                yield return new WaitForSeconds(Random.Range(pauseTime.x, pauseTime.y));
            }
        }

        private void FixedUpdate()
        {
            rb.position = boundary.Clamp(rb.position);
            rb.rotation = Quaternion.Euler(0f, 0f, tilt * rb.velocity.x);
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            StartCoroutine(Maneuver());
        }
    }
}