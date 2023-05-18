using UnityEngine;
namespace Artngame.Orion.MiniGames
{
    public class DestroyByContact : MonoBehaviour
    {
        public GameObject explosionPrefab;
        public GameObject playerExplosionPrefab;
        public int scoreValue = 10;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Boundary") || other.CompareTag("Enemy"))
            {
                return;
            }
            if (other.CompareTag("Player"))
            {
                Instantiate(playerExplosionPrefab, other.transform.position, other.transform.rotation);
                GameController.instance.GameOver();
            }
            if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, transform.rotation);
            }
            Destroy(other.gameObject);
            Destroy(gameObject);
            GameController.instance.AddScore(scoreValue);
        }
    }
}