using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using ProjectUniverse.Environment.Volumes;

namespace ProjectUniverse.Serialization.Surrogates
{
    public class PlayerVolumeControllerSurrogate : ISerializationSurrogate
    {
        public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            PlayerVolumeController pvc = (PlayerVolumeController)obj;
            info.AddValue("exposureTime", pvc.ExposureTime);
            info.AddValue("absorbedDose", pvc.AbsorbedDose);
            //info.AddValue("playerHealth", pvc.PlayerHealth);
            info.AddValue("playerOxygen", pvc.PlayerOxygen);
            info.AddValue("playerTemp", pvc.PlayerTemp);
            info.AddValue("oxygenUseRate", pvc.OxygenUseRate);
        }

        public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            PlayerVolumeController pvc = (PlayerVolumeController)obj;
            pvc.ExposureTime = (float)info.GetValue("exposureTime", typeof(float));
            pvc.AbsorbedDose = (float)info.GetValue("absorbedDose", typeof(float));
            //pvc.PlayerHealth = (float)info.GetValue("playerHealth", typeof(float));
            pvc.PlayerOxygen = (float)info.GetValue("playerOxygen", typeof(float));
            pvc.PlayerTemp = (float)info.GetValue("playerTemp", typeof(float));
            pvc.OxygenUseRate = (float)info.GetValue("playerTemp", typeof(float));
            obj = pvc;
            return obj;
        }
    }
}