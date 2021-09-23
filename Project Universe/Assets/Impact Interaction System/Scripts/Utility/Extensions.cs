using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Utility
{
    public static class Extensions
    {
        /// <summary>
        /// Sets the bit at the given position to 1.
        /// </summary>
        /// <param name="bitmask">The bitmask to modify.</param>
        /// <param name="pos">The index of the bit to set.</param>
        /// <returns>The bitmask with the bit at the given position set to 1.</returns>
        public static int SetBit(this int bitmask, int pos)
        {
            return bitmask | (1 << pos);
        }

        /// <summary>
        /// Sets the bit at the given position to 0.
        /// </summary>
        /// <param name="bitmask">The bitmask to modify.</param>
        /// <param name="pos">The index of the bit to unset.</param>
        /// <returns>The bitmask with the bit at the given position set to 0.</returns>
        public static int UnsetBit(this int bitmask, int pos)
        {
            return bitmask & ~(1 << pos);
        }

        /// <summary>
        /// Is the bit at the given position set to 1?
        /// </summary>
        /// <param name="bitmask">The bitmask to check against.</param>
        /// <param name="pos">The index of the bit to check.</param>
        /// <returns>True if the bit is set to 1, false otherwise.</returns>
        public static bool IsBitSet(this int bitmask, int pos)
        {
            return (bitmask & (1 << pos)) != 0;
        }

        /// <summary>
        /// Rounds to the given number of places.
        /// </summary>
        /// <param name="num">The number to round.</param>
        /// <param name="places">The number of places to round to.</param>
        /// <returns>The number rounded to the given number of places.</returns>
        public static float Round(this float num, int places)
        {
            return (float)System.Math.Round(num, places);
        }

        /// <summary>
        /// Rounds the X, Y, and Z components of a Vector3 to the given number of places.
        /// </summary>
        /// <param name="a">The Vector3 to round.</param>
        /// <param name="places">The number of places to round to.</param>
        /// <returns>The Vector3 rounded to the given number of places.</returns>
        public static Vector3 Round(this Vector3 a, int places)
        {
            if (places < 0)
                return a;

            a.x = a.x.Round(places);
            a.y = a.y.Round(places);
            a.z = a.z.Round(places);

            return a;
        }

        /// <summary>
        /// Either gets a component on the given game object or adds one.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="gameObject">The game object to get or add the component to.</param>
        /// <returns>A reference to the existing or new component.</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            T existing = gameObject.GetComponent<T>();
            if (existing != null)
                return existing;

            return gameObject.AddComponent<T>();
        }

        public static int IndexOf<T>(this T[] array, Predicate<T> predicate)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (predicate(array[i]))
                    return i;
            }

            return -1;
        }
    }
}

