using System.Collections.Generic;
using UnityEngine;

namespace Impact.Utility.ObjectPool
{
    public class ClassPool
    {
        private List<IPoolable> pooledObjects = new List<IPoolable>();
        private int lastAvailable;

        public T GetObject<T>() where T : class, IPoolable, new()
        {
            int checkedIndices = 0;
            int i = lastAvailable;

            while (checkedIndices < pooledObjects.Count)
            {
                IPoolable a = pooledObjects[i];

                if (a.IsAvailable())
                {
                    lastAvailable = i;
                    a.Retrieve();
                    return a as T;
                }

                i++;
                checkedIndices++;

                if (i >= pooledObjects.Count)
                    i = 0;
            }

            T result = new T();
            result.Retrieve();

            pooledObjects.Add(result);

            return result;
        }
    }
}

