using UnityEngine;

namespace Impact.Utility
{
    /// <summary>
    /// Represents a range between a minimum and a maximum value.
    /// </summary>
    [System.Serializable]
    public struct Range
    {
        /// <summary>
        /// The minimum value of the range.
        /// </summary>
        public float Min;
        /// <summary>
        /// The maximium value of the range.
        /// </summary>
        public float Max;

        public Range(float min, float max)
        {
            this.Min = min;
            this.Max = max;
        }

        /// <summary>
        /// Checks to see if the given value is within the range's min and max values.
        /// </summary>
        public bool IsInRange(float f)
        {
            return (f >= Min && f <= Max);
        }

        /// <summary>
        /// Clamps the given value to be between the range's min and max values.
        /// </summary>
        public float Clamp(float f)
        {
            return Mathf.Clamp(f, Min, Max);
        }

        /// <summary>
        /// Gets a random float value within the range.
        /// </summary>
        public float RandomInRange()
        {
            return UnityEngine.Random.Range(Min, Max);
        }

        /// <summary>
        /// Gets and random int value within the range.
        /// </summary>
        public int RandomInRangeInteger()
        {
            return (int)UnityEngine.Random.Range(Min, Max + 1);
        }

        /// <summary>
        /// Gets a value between min and max, specified by the variable t.
        /// </summary>
        public float Lerp(float t)
        {
            return Min + (Max - Min) * t;
        }

        /// <summary>
        /// Gets a value between 0 and 1 based on the given value, where 0 corresponds to Min and 1 corresponds to Max.
        /// </summary>
        public float Normalize(float val)
        {
            //Special case for when Min and Max are equal
            if (val == Min && val == Max)
                return 1;
            else if (val <= Min)
                return 0;
            else if (val >= Max)
                return 1;

            return (val - Min) / (Max - Min);
        }

        /// <summary>
        /// Normalize the given value, but do not clamp the input to the Min and Max values.
        /// </summary>
        public float NormalizeUnclamped(float val)
        {
            return (val - Min) / (Max - Min);
        }

        /// <summary>
        /// Gets the distance between the max and min values
        /// </summary>
        public float Distance()
        {
            return Max - Min;
        }
    }
}