using UnityEngine;
namespace Artngame.Orion.MiniGames
{
    public class WeaponController : MonoBehaviour
    {
        public GameObject weaponPrefab;
        public Transform shotSpawn;
        public float startDelay;
        public float fireDelay;

        private AudioSource audioSource;

        public void Fire()
        {
            Instantiate(weaponPrefab, shotSpawn.position, shotSpawn.rotation);
            audioSource.Play();
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            InvokeRepeating("Fire", startDelay, fireDelay);
        }
    }
}