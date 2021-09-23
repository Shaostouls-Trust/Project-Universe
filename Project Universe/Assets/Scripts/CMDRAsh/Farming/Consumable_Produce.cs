using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Items.Consumable
{
    /// <summary>
    /// Container/Type for fruits, veggies, stuff that is grown that can be eaten
    /// </summary>
    public class Consumable_Produce : MonoBehaviour
    {
        [SerializeField] private string typeSingle;
        //private ProduceDefinition IngotDef;
        [SerializeField] private int produceCount;

        public Consumable_Produce(string type, int count)
        {
            typeSingle = type;
            produceCount = count;
        }

        override
        public string ToString()
        {
            return typeSingle + "; Quantity: " + produceCount;
        }

        public float GetProduceCount()
        {
            return produceCount;
        }

        public string ProduceType
        {
            get { return typeSingle; }
        }

        public bool CompareMetaData(Consumable_Produce comparee)
        {
            Debug.Log(this + " v " + comparee);
            if (ProduceType == comparee.ProduceType)//null ref
            {
                return true;
            }
            Debug.Log("METADATA ERROR");
            return false;
        }
    }
}