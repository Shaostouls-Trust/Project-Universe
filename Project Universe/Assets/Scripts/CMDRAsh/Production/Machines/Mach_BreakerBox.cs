using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Base;

namespace ProjectUniverse.Production.Machines
{
    public class Mach_BreakerBox : IConstructible
    {
        public void BouncedMessageReceiver(params object[] data)
        {
            MachineMessageReceiver(data);
        }
    }
}