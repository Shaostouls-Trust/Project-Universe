using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable_Component : MonoBehaviour
{
    [SerializeField] private string componentID;
    private IComponentDefinition compDefinition;
    [SerializeField] private int quantity;

    public Consumable_Component(string compID,int num,IComponentDefinition definition)
    {
        componentID = compID;
        quantity = num;
        compDefinition = definition;
        //do we want to track what resources make up this component?
    } 

    override
    public string ToString()
    {
        return "ID: "+ componentID + "; Quantity: "+quantity+"; Definition: "+compDefinition.GetComponentType();
    }

    public string GetComponentID()
    {
        return componentID;
    }

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
}
