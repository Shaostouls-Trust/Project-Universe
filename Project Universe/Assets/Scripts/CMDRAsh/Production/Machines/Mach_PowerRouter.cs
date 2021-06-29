using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Base;

namespace ProjectUniverse.Production.Machines
{
    public class Mach_PowerRouter : IConstructible
    {
        public void BouncedMessageReceiver(params object[] data)
        {
            MachineMessageReceiver(data);
        }
    }
}