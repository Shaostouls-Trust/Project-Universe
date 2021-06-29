using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization;
using ProjectUniverse.Base;
using System;

namespace ProjectUniverse.Serialization.Surrogates
{
    public class ItemStackSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            ItemStack stack = (ItemStack)obj;
            info.AddValue("itemType", stack.GetStackType());
            info.AddValue("itemCount", stack.Size());
            info.AddValue("maxCount", stack.GetMaxAmount());
            info.AddValue("originalType", stack.GetOriginalType());
            info.AddValue("lastIndex", stack.LastIndex);
            info.AddValue("TArray", stack.GetItemArray());//May need wrapped
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            ItemStack stack = new ItemStack(
                (string)(info.GetValue("itemType",typeof(string))),
                (int)(info.GetValue("maxCount",typeof(int))),
                (Type)info.GetValue("originalType", typeof(Type))
                );
            stack.SetItemCount((float)info.GetValue("itemCount", typeof(float)));
            stack.LastIndex = (int)info.GetValue("lastIndex", typeof(int));
            Array TArray = (Array)info.GetValue("TArray", typeof(Array));
            stack.SetTArray(TArray);
            obj = stack;
            return obj;
        }
    }
}
