using ProjectUniverse.Player.PlayerController;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace ProjectUniverse.Serialization.Surrogates
{
    public class PlayerStats_Surrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            SupplementalController sc = (SupplementalController)obj;
            info.AddValue("crouchTog", sc.crouchToggle);
            info.AddValue("crouching", sc.crouching);
            info.AddValue("prone", sc.prone);
            info.AddValue("healthNonStd", sc.PlayerHealth);
            info.AddValue("headHealth", sc.HeadHealth);
            info.AddValue("chestHealth", sc.ChestHealth);
            info.AddValue("LeftArmHealth", sc.LArmHealth);
            info.AddValue("RightArmHealth", sc.RArmHealth);
            info.AddValue("LeftHandHealth", sc.LHandHealth);
            info.AddValue("RightHandHealth", sc.RHandHealth);
            info.AddValue("LeftLegHealth", sc.LLegHealth);
            info.AddValue("RightLegHealth", sc.RLegHealth);
            info.AddValue("LeftFootHealth", sc.LFootHealth);
            info.AddValue("RightFootHealth", sc.RFootHealth);
            info.AddValue("hydration", sc.PlayerHydration);
            info.AddValue("happyStomach", sc.PlayerHappyStomach);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            SupplementalController sc = (SupplementalController)obj;
            sc.crouchToggle = (bool)info.GetValue("crouchTog",typeof(bool));
            sc.crouching = (bool)info.GetValue("crouching", typeof(bool));
            sc.prone = (bool)info.GetValue("prone", typeof(bool));
            sc.PlayerHealth = (float)info.GetValue("healthNonStd", typeof(float));
            sc.HeadHealth = (float)info.GetValue("headHealth", typeof(float));
            sc.ChestHealth = (float)info.GetValue("chestHealth", typeof(float));
            sc.LArmHealth = (float)info.GetValue("LeftArmHealth", typeof(float));
            sc.RArmHealth = (float)info.GetValue("RightArmHealth", typeof(float));
            sc.LHandHealth = (float)info.GetValue("LeftHandHealth", typeof(float));
            sc.RHandHealth = (float)info.GetValue("RightHandHealth", typeof(float));
            sc.LLegHealth = (float)info.GetValue("LeftLegHealth", typeof(float));
            sc.RLegHealth = (float)info.GetValue("RightLegHealth", typeof(float));
            sc.LFootHealth = (float)info.GetValue("LeftFootHealth", typeof(float));
            sc.RFootHealth = (float)info.GetValue("RightFootHealth", typeof(float));
            sc.PlayerHydration = (float)info.GetValue("hydration", typeof(float));
            sc.PlayerHappyStomach = (float)info.GetValue("happyStomach", typeof(float));
            obj = sc;
            return obj;
        }
    }
}