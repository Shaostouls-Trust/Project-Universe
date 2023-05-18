using UnityEngine;
namespace Artngame.Orion.MiniGames
{
    public class BGScroller : MonoBehaviour
    {
        public float scrollSpeed;
        public float tileSizeZ;

        private void Update()
        {
            Vector3 newPos = transform.position;
            newPos.z = -Mathf.Repeat(Time.time * scrollSpeed, tileSizeZ);
            transform.position = newPos;
        }
    }
}