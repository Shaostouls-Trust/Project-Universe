using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace ProjectUniverse.Serialization.Surrogates {
    public class BoxColliderSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            BoxCollider bc = (BoxCollider)obj;
            info.AddValue("center_x", bc.center.x);
            info.AddValue("center_y", bc.center.y);
            info.AddValue("center_z", bc.center.z);
            info.AddValue("offset", bc.contactOffset);
            info.AddValue("enabled", bc.enabled);
            info.AddValue("isTrigger", bc.isTrigger);
            //info.AddValue("sharedMaterial", bc.sharedMaterial);//Use the string to load from library?
            info.AddValue("size_x", bc.size.x);
            info.AddValue("size_y", bc.size.y);
            info.AddValue("size_z", bc.size.z);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            BoxCollider bc = (BoxCollider)obj;
            bc.center = new Vector3(
                (float)info.GetValue("center_x", typeof(float)),
                (float)info.GetValue("center_x", typeof(float)),
                (float)info.GetValue("center_x", typeof(float))
                );
            bc.contactOffset = (float)info.GetValue("offset", typeof(float));
            bc.enabled = (bool)info.GetValue("enabled", typeof(bool));
            bc.isTrigger = (bool)info.GetValue("isTrigger", typeof(bool));
            bc.size = new Vector3(
                (float)info.GetValue("size_x", typeof(float)),
                (float)info.GetValue("size_x", typeof(float)),
                (float)info.GetValue("size_x", typeof(float))
                );

            obj = bc;
            return obj;
        }
    }

    public class MeshColliderSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            MeshCollider mc = (MeshCollider)obj;
            info.AddValue("convex", mc.convex);
            info.AddValue("offset", mc.contactOffset);
            //info.AddValue("offset", mc.sharedMesh);
            info.AddValue("enabled", mc.enabled);
            info.AddValue("isTrigger", mc.isTrigger);
            //info.AddValue("sharedMaterial", bc.sharedMaterial);//Use the string to load from library?
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            MeshCollider mc = (MeshCollider)obj;
            mc.convex = (bool)(info.GetValue("convex", typeof(bool)));
            mc.contactOffset = (float)info.GetValue("offset", typeof(float));
            mc.enabled = (bool)info.GetValue("enabled", typeof(bool));
            mc.isTrigger = (bool)info.GetValue("isTrigger", typeof(bool));

            obj = mc;
            return obj;
        }
    }
}