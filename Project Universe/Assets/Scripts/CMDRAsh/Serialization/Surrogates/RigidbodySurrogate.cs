using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace ProjectUniverse.Serialization.Surrogates
{
    public class RigidbodySurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Rigidbody rb = (Rigidbody)obj;
            info.AddValue("angularDrag", rb.angularDrag);
            info.AddValue("angularVelocity_x", rb.angularVelocity.x);
            info.AddValue("angularVelocity_y", rb.angularVelocity.y);
            info.AddValue("angularVelocity_z", rb.angularVelocity.z);
            info.AddValue("centerOfMass_x", rb.centerOfMass.x);
            info.AddValue("centerOfMass_y", rb.centerOfMass.y);
            info.AddValue("centerOfMass_z", rb.centerOfMass.z);
            info.AddValue("collisionDetectionMode", rb.collisionDetectionMode);//Enum 0,1,2,3
            info.AddValue("detectCollisions", rb.detectCollisions);
            info.AddValue("drag", rb.drag);
            info.AddValue("interpolation", rb.interpolation);//Enum 0,1,2
            info.AddValue("isKinematic", rb.isKinematic);
            info.AddValue("mass", rb.mass);
            info.AddValue("position_x", rb.position.x);
            info.AddValue("position_y", rb.position.y);
            info.AddValue("position_z", rb.position.z);
            info.AddValue("rotation_x", rb.rotation.x);
            info.AddValue("rotation_y", rb.rotation.y);
            info.AddValue("rotation_z", rb.rotation.z);
            info.AddValue("rotation_w", rb.rotation.w);
            //info.AddValue("tag", rb.tag);
            info.AddValue("useGravity", rb.useGravity);
            info.AddValue("velocity_x", rb.velocity.x);
            info.AddValue("velocity_y", rb.velocity.y);
            info.AddValue("velocity_z", rb.velocity.z);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            //Rigidbody rb = (Rigidbody)obj;
            GameObject go = new GameObject();
            Rigidbody rb = (Rigidbody)go.AddComponent(typeof(Rigidbody));
            //Debug.Log(rb);
            rb.angularDrag = (float)info.GetValue("angularDrag", typeof(float));
            rb.angularVelocity = new Vector3(
                (float)info.GetValue("angularVelocity_x", typeof(float)),
                (float)info.GetValue("angularVelocity_y", typeof(float)),
                (float)info.GetValue("angularVelocity_z", typeof(float))
                );
            rb.centerOfMass = new Vector3(
                (float)info.GetValue("centerOfMass_x", typeof(float)),
                (float)info.GetValue("centerOfMass_y", typeof(float)),
                (float)info.GetValue("centerOfMass_z", typeof(float))
                );
            rb.collisionDetectionMode = ((CollisionDetectionMode)info.GetValue("collisionDetectionMode", typeof(int)));//Iffy
            rb.detectCollisions = (bool)info.GetValue("detectCollisions", typeof(bool));
            rb.drag = (float)info.GetValue("drag", typeof(float));
            rb.interpolation = (RigidbodyInterpolation)info.GetValue("interpolation", typeof(int));//iffy
            rb.isKinematic = (bool)info.GetValue("isKinematic", typeof(bool));
            rb.mass = (float)info.GetValue("mass", typeof(float));
            rb.position = new Vector3(
                (float)info.GetValue("position_x", typeof(float)),
                (float)info.GetValue("position_y", typeof(float)),
                (float)info.GetValue("position_z", typeof(float))
                );
            rb.rotation = new Quaternion(
                (float)info.GetValue("rotation_x", typeof(float)),
                (float)info.GetValue("rotation_y", typeof(float)),
                (float)info.GetValue("rotation_z", typeof(float)),
                (float)info.GetValue("rotation_w", typeof(float))
                );
            //rb.tag = (string)info.GetValue("tag", typeof(string));
            rb.useGravity = (bool)info.GetValue("useGravity", typeof(bool));
            rb.velocity = new Vector3(
                (float)info.GetValue("velocity_x", typeof(float)),
                (float)info.GetValue("velocity_y", typeof(float)),
                (float)info.GetValue("velocity_z", typeof(float))
                );

            obj = rb;
            return obj;
        }
    }
}