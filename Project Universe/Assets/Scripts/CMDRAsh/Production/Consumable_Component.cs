using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Data.Libraries;

namespace ProjectUniverse.Production.Resources
{
    public class Consumable_Component : MonoBehaviour
    {
        [SerializeField] private string componentID;
        private IComponentDefinition compDefinition;
        [SerializeField] private int quantity;
        private int Health_Adjusted;
        private int Priority_Inherited;
        private float HealthCurrent;

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

        override
        public string ToString()
        {
            return "ID: " + componentID + "; Quantity: " + quantity + "; Definition: " + compDefinition.GetComponentType();
        }

        public string GetComponentID()
        {
            return componentID;
        }
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
            if (GetComponentID() == comparee.GetComponentID())
            {
                return true;
            }
            Debug.Log("METADATA ERROR");
            return false;
        }
    }
}