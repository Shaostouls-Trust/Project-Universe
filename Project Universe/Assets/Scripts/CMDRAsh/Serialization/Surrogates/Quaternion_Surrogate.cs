using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProjectUniverse.Serialization.Surrogates
{
    public class QuaternionSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Quaternion qt = (Quaternion)obj;
            info.AddValue("x", qt.x);
            info.AddValue("y", qt.y);
            info.AddValue("z", qt.z);
            info.AddValue("w", qt.w);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Quaternion qt = (Quaternion)obj;
            qt.x = (float)info.GetValue("x", typeof(float));
            qt.y = (float)info.GetValue("y", typeof(float));
            qt.z = (float)info.GetValue("z", typeof(float));
            qt.w = (float)info.GetValue("w", typeof(float));
            obj = qt;
            return obj;
        }
    }
}