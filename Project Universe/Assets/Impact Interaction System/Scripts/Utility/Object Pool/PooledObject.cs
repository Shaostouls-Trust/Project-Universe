using UnityEngine;

namespace Impact.Utility.ObjectPool
{
    /// <summary>
    /// Base component for objects that are part of an object pool.
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        /// <summary>
        /// The transform of the pool that created this object.
        /// </summary>
        public Transform OriginalParent { get; set; }

        /// <summary>
        /// The frame this object was last retrieved on.
        /// </summary>
        public int LastRetrievedFrame { get; private set; }

        /// <summary>
        /// The priority associated with this pooled object.
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// Retrieve this object from its pool.
        /// </summary>
        public virtual void Retrieve(int priority)
        {
            LastRetrievedFrame = Time.frameCount;
            Priority = priority;

            gameObject.SetActive(true);
        }

        /// <summary>
        /// Make this object available again in its pool.
        /// </summary>
        public virtual void MakeAvailable()
        {
            gameObject.SetActive(false);
            Priority = int.MinValue;

            transform.SetParent(OriginalParent, false);
        }

        /// <summary>
        /// Is the object available to be retrieved?
        /// </summary>
        /// <returns>True if the object is available to be retrieved, false otherwise.</returns>
        public virtual bool IsAvailable()
        {
            return !gameObject.activeSelf;
        }
    }
}
