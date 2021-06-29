using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using UnityEditor;

namespace ProjectUniverse.Serialization.Surrogates
{
    public class GUIDSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            GUID guid = (GUID)obj;
            info.AddValue("hex", guid.ToString());
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            GUID guid;
            guid = new GUID((string)info.GetValue("hex", typeof(string)));
            //GUID.TryParse(info, out GUID) instead?
            obj = guid;
            return obj;
        }
    }
}