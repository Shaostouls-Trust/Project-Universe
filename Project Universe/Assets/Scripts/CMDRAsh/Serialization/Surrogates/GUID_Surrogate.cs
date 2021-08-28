using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using UnityEditor;
using System;

namespace ProjectUniverse.Serialization.Surrogates
{
    public class GUIDSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Guid guid = (Guid)obj;
            info.AddValue("hex", guid.ToString());
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Guid guid;
            guid = new Guid((string)info.GetValue("hex", typeof(string)));
            // Guid.TryParse(info, out GUID) instead?
            obj = guid;
            return obj;
        }
    }
}