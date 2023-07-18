using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Data.Libraries;
using System;

namespace ProjectUniverse.Production.Resources
{
    public class Consumable_Component : ScriptableObject//MonoBehaviour
    {
        [SerializeField] private string componentID;
        private IComponentDefinition compDefinition;
        [SerializeField] private int quantity;
        private int Health_Adjusted;
        private int Priority_Inherited;
        private float HealthCurrent;

        public static Consumable_Component ConstructComponent(string compID, int num, IComponentDefinition definition)
        {
            Consumable_Component comp = (Consumable_Component)ScriptableObject.CreateInstance(typeof(Consumable_Component));
            comp.componentID = compID;
            comp.quantity = num;
            comp.compDefinition = definition;
            comp.Health_Adjusted = comp.compDefinition.GetHealth();
            comp.HealthCurrent = comp.Health_Adjusted;
            comp.Priority_Inherited = comp.compDefinition.GetPriority();
            return comp;
        }
        public static Consumable_Component ConstructComponent(string compID, int num)
        {
            Consumable_Component comp = (Consumable_Component)ScriptableObject.CreateInstance(typeof(Consumable_Component));
            comp.componentID = compID;
            comp.quantity = num;
            bool success = IComponentLibrary.ComponentDictionary.TryGetValue(comp.componentID, out comp.compDefinition);
            if (!success)
            {
                Debug.LogError("Failed to grab IComponentDefinition " + comp.componentID + " from IComponentLibrary");
            }
            else
            {
                comp.Health_Adjusted = comp.compDefinition.GetHealth();
                comp.Priority_Inherited = comp.compDefinition.GetPriority();
                comp.HealthCurrent = comp.Health_Adjusted;
            }
            //do we want to track what resources make up this component?
            return comp;
        }

        [Obsolete("use ConstructComponent to create a new consumable component.", false)]
        public Consumable_Component(string compID, int num, IComponentDefinition definition)
        {
            componentID = compID;
            quantity = num;
            compDefinition = definition;
            Health_Adjusted = compDefinition.GetHealth();
            HealthCurrent = Health_Adjusted;
            Priority_Inherited = compDefinition.GetPriority();
            //do we want to track what resources make up this component?
        }

        [Obsolete("use ConstructComponent to create a new consumable component.", false)]
        public Consumable_Component(string compID, int num)
        {
            componentID = compID;
            quantity = num;
            bool success = IComponentLibrary.ComponentDictionary.TryGetValue(componentID, out compDefinition);
            if (!success)
            {
                Debug.LogError("Failed to grab IComponentDefinition " + componentID + " from IComponentLibrary");
            }
            else
            {
                Health_Adjusted = compDefinition.GetHealth();
                Priority_Inherited = compDefinition.GetPriority();
                HealthCurrent = Health_Adjusted;
            }
            //do we want to track what resources make up this component?
        }
      
        public string ComponentID
        {
            get { return componentID; }
            set { componentID = value; }
        }

        override
        public string ToString()
        {
            return "ID: " + componentID + "; Quantity: " + quantity + "; Definition: " + compDefinition.GetComponentType();
        }

        //public string GetComponentID()
        //{
        //    return componentID;
        //}
        //public void SetComponentID(string ONLY_FOR_DESERIALIZATION)
        //{
        //    componentID = ONLY_FOR_DESERIALIZATION;
        //}
        public int GetQuantity()
        {
            return quantity;
        }

        public IComponentDefinition GetComponentDefinition()
        {
            return compDefinition;
        }

        public void RemoveComponentAmount(int amount)
        {
            quantity -= amount;
        }

        public int HealthValue
        {
            get { return Health_Adjusted; }
            set { Health_Adjusted = value; }
        }
        public int GetPriority()
        {
            return Priority_Inherited;
        }
        public float CompTakeDamage(float damage)
        {
            HealthCurrent -= damage;
            //Debug.Log(this + " health at " + HealthCurrent);
            if (HealthCurrent <= 0 && quantity > 1)
            {
                HealthCurrent = 0f;// Health_Adjusted;
                //quantity--;
                //rn the return is to notify the caller that a comp has been destroyed
                //so return zero, instead of the real (full) health
                return 0f;
            }
            else if (HealthCurrent <= 0 && quantity <= 0)
            {
                HealthCurrent = 0f;
                quantity = 0;
                //this comp has been destroyed
                return 0f;
            }
            //this comp has X health remaining
            return HealthCurrent;
        }
        public float RemainingHealth
        {
            get { return HealthCurrent; }
            set { HealthCurrent = value; }
        }

        public bool CompareMetaData(Consumable_Component comparee)
        {
            if (ComponentID == comparee.ComponentID)
            {
                return true;
            }
            Debug.Log("METADATA ERROR");
            return false;
        }
    }
}