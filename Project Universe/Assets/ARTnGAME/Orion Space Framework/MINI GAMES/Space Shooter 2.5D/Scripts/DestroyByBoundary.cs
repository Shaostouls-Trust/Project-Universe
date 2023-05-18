using UnityEngine;
namespace Artngame.Orion.MiniGames
{
    public class DestroyByBoundary : MonoBehaviour
    {
        private void OnTriggerExit(Collider other)
        {
            Destroy(other.gameObject);
        }
    }
}