using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Production.Resources;
using System.Runtime.Serialization;

public class ConsumableResources_Surrogate
{
    public class Consumable_Component_Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Consumable_Component comp = (Consumable_Component)obj;
            info.AddValue("componentID", comp.ComponentID);
            info.AddValue("quantity", comp.GetQuantity());
            info.AddValue("healthAdjusted", comp.HealthValue);
            info.AddValue("healthRemaining", comp.RemainingHealth);
    }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Consumable_Component comp = Consumable_Component.ConstructComponent(
                (string)info.GetValue("componentID", typeof(string)),
                (int)info.GetValue("quantity", typeof(int)));
            //{
            comp.HealthValue = (int)info.GetValue("healthAdjusted", typeof(int));
            comp.RemainingHealth = (float)info.GetValue("healthRemaining", typeof(float));
            //};
            obj = comp;
            return obj;
        }
    }
    public class Consumable_Ore_Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Consumable_Ore ore = (Consumable_Ore)obj;
            info.AddValue("oreTypeSingle", ore.GetOreType());
            info.AddValue("oreQuality", ore.GetOreQuality());
            info.AddValue("oreZone", ore.GetOreZone());
            info.AddValue("oreMass", ore.GetOreMass());
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Consumable_Ore ore = new Consumable_Ore(
                (string)info.GetValue("oreTypeSingle", typeof(string)),
                (int)info.GetValue("oreQuality", typeof(int)),
                (int)info.GetValue("oreZone", typeof(int)),
                (int)info.GetValue("oreMass", typeof(int)));
            obj = ore;
            return obj;
        }
    }
    public class Consumable_Material_Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Consumable_Material mat = (Consumable_Material)obj;
            info.AddValue("materialType", mat.GetMaterialID());
            info.AddValue("materialMass", mat.GetMaterialMass());
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Consumable_Material mat = new Consumable_Material(
                (string)info.GetValue("materialType", typeof(string)),
                (float)info.GetValue("materialMass", typeof(float)));
            obj = mat;
            return obj;
        }
    }
    public class Consumable_Ingot_Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Consumable_Ingot ing = (Consumable_Ingot)obj;
            info.AddValue("ingotType", ing.GetIngotType());
            info.AddValue("ingotMass", ing.GetIngotMass());
            info.AddValue("ingotQuality", ing.GetIngotQuality());
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            Consumable_Ingot ing = new Consumable_Ingot((string)info.GetValue("ingotType", typeof(string)),
                (int)info.GetValue("ingotQuality", typeof(int)),
                (float)info.GetValue("ingotMass", typeof(float)));
            obj = ing;
            return obj;
        }
    }
}
