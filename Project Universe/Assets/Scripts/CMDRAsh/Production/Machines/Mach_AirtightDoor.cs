using ProjectUniverse.Animation.Controllers;
using ProjectUniverse.Base;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Production.Machines
{
    public class Mach_AirtightDoor : IConstructible
    {
        private DoorAnimator door;

        // Start is called before the first frame update
        void Start()
        {
            door = GetComponent<DoorAnimator>();
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