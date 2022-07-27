using ProjectUniverse.Base;
using ProjectUniverse.PowerSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Production.Machines
{
    public class Mach_StandardLight : IConstructible
    {
        private ISubMachine lightMach;

        // Start is called before the first frame update
        void Start()
        {
            lightMach = GetComponent<ISubMachine>();
            base.Start();
        }

        public void BouncedMessageReceiver(params object[] data)
        {
            MachineMessageReceiver(data);
        }

        protected override void ProcessDamageToComponents()
        {

        }
    }
}
