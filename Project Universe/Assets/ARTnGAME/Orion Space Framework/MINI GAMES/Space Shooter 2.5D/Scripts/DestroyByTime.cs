using UnityEngine;
namespace Artngame.Orion.MiniGames
{
    public class DestroyByTime : MonoBehaviour
    {
        public float lifeTime = 2.0f;

        private void Awake()
        {
            Destroy(gameObject, lifeTime);
        }
    }
}