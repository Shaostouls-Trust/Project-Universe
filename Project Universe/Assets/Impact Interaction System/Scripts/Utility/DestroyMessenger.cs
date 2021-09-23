using System;
using UnityEngine;

namespace Impact.Utility
{
    /// <summary>
    /// Component that emits an event before it is destroyed.
    /// </summary>
    public class DestroyMessenger : MonoBehaviour
    {
        /// <summary>
        /// Called when this object is about to be destroyed.
        /// </summary>
        public event Action OnDestroyed;

        private bool suppressDestroyEvent;

        private void OnApplicationQuit()
        {
            suppressDestroyEvent = true;
        }

        private void OnDestroy()
        {
            if (!suppressDestroyEvent && OnDestroyed != null)
                OnDestroyed.Invoke();
        }
    }
}

