using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;

namespace ProjectUniverse.Serialization.Surrogates
{
    public class Matrix4x4Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Matrix4x4 m = (Matrix4x4)obj;
            info.AddValue("m00", m.m00);
            info.AddValue("m01", m.m01);
            info.AddValue("m02", m.m02);
            info.AddValue("m03", m.m03);
            info.AddValue("m10", m.m10);
            info.AddValue("m11", m.m11);
            info.AddValue("m12", m.m12);
            info.AddValue("m13", m.m13);
            info.AddValue("m20", m.m20);
            info.AddValue("m21", m.m21);
            info.AddValue("m22", m.m22);
            info.AddValue("m23", m.m23);
            info.AddValue("m30", m.m30);
            info.AddValue("m31", m.m31);
            info.AddValue("m32", m.m32);
            info.AddValue("m33", m.m33);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Matrix4x4 m = (Matrix4x4)obj;
            m.m00 = (float)info.GetValue("m00", typeof(float));
            m.m01 = (float)info.GetValue("m01", typeof(float));
            m.m02 = (float)info.GetValue("m02", typeof(float));
            m.m03 = (float)info.GetValue("m03", typeof(float));
            m.m10 = (float)info.GetValue("m10", typeof(float));
            m.m11 = (float)info.GetValue("m11", typeof(float));
            m.m12 = (float)info.GetValue("m12", typeof(float));
            m.m13 = (float)info.GetValue("m13", typeof(float));
            m.m20 = (float)info.GetValue("m20", typeof(float));
            m.m21 = (float)info.GetValue("m21", typeof(float));
            m.m22 = (float)info.GetValue("m22", typeof(float));
            m.m23 = (float)info.GetValue("m23", typeof(float));
            m.m30 = (float)info.GetValue("m30", typeof(float));
            m.m31 = (float)info.GetValue("m31", typeof(float));
            m.m32 = (float)info.GetValue("m32", typeof(float));
            m.m33 = (float)info.GetValue("m33", typeof(float));
            obj = m;
            return obj;
        }
    }
}