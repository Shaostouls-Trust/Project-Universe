using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Production
{
    public class IMessagePointer : MonoBehaviour
    {
        [SerializeField] private GameObject messageTarget;
        public void MachineMessageReceiver(params object[] data)
        {
            messageTarget.SendMessage("BouncedMessageReceiver", data, SendMessageOptions.DontRequireReceiver);

        }
    }
}