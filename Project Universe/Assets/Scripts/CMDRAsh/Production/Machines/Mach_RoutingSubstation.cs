using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using ProjectUniverse.Base;
using ProjectUniverse.PowerSystem;
using ProjectUniverse.Production.Resources;

namespace ProjectUniverse.Production.Machines
{
    public class Mach_RoutingSubstation : IConstructible
    {
        private IRoutingSubstation Substation;
        public void Start()
        {
            Substation = GetComponent<IRoutingSubstation>();

            base.Start();
        }

        public void BouncedMessageReceiver(params object[] data)
        {
            MachineMessageReceiver(data);
        }

        /// <summary>
        /// Logic to handle machine behavior in response to component damage.
        /// Damage to specific components will affect machine behavior in specific, unique ways.
        /// 
        /// IRoutingSubstation has:
        /// bool: Built (Handled outside)
        /// int: LegsOut (Critical, Shared)
        /// []: ConnectedMachines (Critical)
        /// float: EnergyBufferMax
        /// float: BufferCurrent
        /// []: RequestedPower
        /// </summary>
        protected override void ProcessDamageToComponents()
        {
            Debug.Log("Overriding base.ProcessDamageToComponents with Mach_Substation");
            
            foreach(ItemStack stack in IConstructible_ComponentsReal)
            {
                //Debug.Log(stack.GetItemArray().Length);//999 loops!!!! MUY MAL!!!!
                foreach (Consumable_Component comp in stack.GetItemArray()) 
                {
                    //try block b/c null checks come back null even on valid components
                    try
                    {
                        int i = comp.HealthValue;
                    }
                    catch(NullReferenceException e) { break; }
                    //Debug.Log("testing "+comp);
                    ///
                    /// Perhaps a behavior library like 'if missing 50% of electrical comps, then do this to pram_A'
                    ///
                    switch (comp.ComponentID)
                    {
                        //for now, we will only have the components in the definition, none will have been added via upgrading
                        //Eventually, damage to components will need to account for something.
                        case "Component_ElectricalComponents":
                            /// 
                            /// Electrical Components affect LegsOut
                            /// 3 comps, 12 legs. 1 comp is 3 legs (3 legs grace).
                            ///
                            if (comp.RemainingHealth <= 0)
                            {
                                //Debug.Log("legs b4 "+Substation.LegsRedux);
                                Substation.LegsRedux += 3;
                                //Debug.Log("legs after "+Substation.LegsRedux);
                            }
                            break;
                        case "Component_ElectronicsComponents":
                            ///
                            /// Electronics Components affects RequestedPower
                            /// 3 comps, 75% power send reduction (25% grace).
                            if (comp.RemainingHealth <= 0)
                            {
                                Substation.RequestRedux += 0.25f;
                            }
                            break;
                        case "Component_CopperComponents":
                            ///
                            /// Copper Components affects EnergyBufferMax
                            /// 1 comp, 15% buffer redux
                            if (comp.RemainingHealth <= 0)
                            {
                                Substation.EnergyBufferRedux -= (Substation.EnergyBufferRedux * 0.15f);
                            }
                            break;
                        case "Component_FiberglassInsulation":
                            ///
                            /// FiberGlass Components affects EnergyBufferMax, BufferCurrent
                            /// 3 comps, 60% redux
                            if (comp.RemainingHealth <= 0)
                            {
                                Substation.EnergyBufferRedux -= (Substation.EnergyBufferRedux * 0.2f);
                                Substation.BufferCurrentRedux -= (Substation.BufferCurrentRedux * 0.2f);
                            }
                            break;
                        case "Component_NickelComponents":
                            ///
                            /// Nickel Components affect EnergyBufferMax
                            /// 1 comp, 15% redux
                            if (comp.RemainingHealth <= 0)
                            {
                                Substation.EnergyBufferRedux -= (Substation.EnergyBufferRedux * 0.15f);
                                Substation.BufferCurrentRedux -= (Substation.BufferCurrentRedux * 0.15f);
                            }
                            break;
                        case "Component_PalladiumComponents":
                            ///
                            /// Palladium Components affect RequestedPower
                            /// 1 comp, 25% requested power
                            if (comp.RemainingHealth <= 0)
                            {
                                Substation.RequestRedux += 0.25f;
                            }
                            break;
                    }
                    
                }
            }
        }
    }
}