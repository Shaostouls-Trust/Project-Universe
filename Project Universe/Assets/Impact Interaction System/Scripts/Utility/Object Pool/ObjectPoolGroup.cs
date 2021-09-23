using System.Collections.Generic;
using UnityEngine;

namespace Impact.Utility.ObjectPool
{
    /// <summary>
    /// Keeps track of a group of object pools with templates of the same type.
    /// </summary>
    /// <typeparam name="TPool">The type of pool that is being grouped.</typeparam>
    /// <typeparam name="TTemplate">The type of the template object.</typeparam>
    public class ObjectPoolGroup<TPool, TTemplate> where TPool : ObjectPool<TTemplate> where TTemplate : PooledObject
    {
        private List<TPool> pools = new List<TPool>();

        /// <summary>
        /// Gets an existing pool with the given template, or creates it if it does not exist.
        /// </summary>
        /// <param name="template">The template object.</param>
        /// <param name="poolSize">The size of the pool if it needs to be created.</param>
        /// <param name="objectPoolFallbackMode">The object pool fallback mode to use for retrieving objects.</param>
        /// <param name="result">The pool corresponding to the template, or null if no pool was found.</param>
        /// <returns>True if a pool was found or was successfully created, false otherwise.</returns>
        public bool GetOrCreatePool(TTemplate template, int poolSize, ObjectPoolFallbackMode objectPoolFallbackMode, out TPool result)
        {
            if (template == null)
            {
                result = null;
                return false;
            }

            TPool pool;
            if (GetPool(template, out pool))
            {
                result = pool;
                return true;
            }

            return CreatePool(template, poolSize, objectPoolFallbackMode, out result);
        }

        /// <summary>
        /// Attempts to get an existing pool with the given template.
        /// </summary>
        /// <param name="template">The template object.</param>
        /// <param name="result">The pool corresponding to the template, or null if no pool was found.</param>
        /// <returns>True if a pool was found, false otherwise.</returns>
        public bool GetPool(TTemplate template, out TPool result)
        {
            for (int i = 0; i < pools.Count; i++)
            {
                if (pools[i].Template == template)
                {
                    result = pools[i];
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Creates a new pool.
        /// </summary>
        /// <param name="template">The template object.</param>
        /// <param name="poolSize">The size of the pool.</param>
        /// <param name="objectPoolFallbackMode">The object pool fallback mode to use for retrieving objects.</param>
        /// <param name="result">The created pool, or null if a pool could not be created.</param>
        /// <returns>True if a pool was successfully created. False otherwise.</returns>
        public bool CreatePool(TTemplate template, int poolSize, ObjectPoolFallbackMode objectPoolFallbackMode, out TPool result)
        {
            if (template == null)
            {
                result = null;
                return false;
            }

            GameObject g = new GameObject("Object Pool (" + template.name + ")");
            Object.DontDestroyOnLoad(g);

            TPool pool = g.AddComponent<TPool>();
            pool.Template = template;
            pool.Initialize(poolSize, objectPoolFallbackMode);

            Add(pool);

            result = pool;
            return true;
        }

        /// <summary>
        /// Adds a pool to the group.
        /// </summary>
        /// <param name="pool">The pool to add.</param>
        public void Add(TPool pool)
        {
            pools.Add(pool);
        }

        /// <summary>
        /// Removes a pool from the group.
        /// </summary>
        /// <param name="pool">The pool to remove.</param>
        public void Remove(TPool pool)
        {
            pools.Remove(pool);
        }
    }
}
