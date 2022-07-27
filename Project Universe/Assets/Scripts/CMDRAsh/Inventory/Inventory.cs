using ProjectUniverse.Base;
using ProjectUniverse.Player;
using ProjectUniverse.Production.Resources;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Items
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private int maxWeight;
        [SerializeField] private int maxVolume;
        [SerializeField] private int volume;
        private List<ItemStack> inventory = new List<ItemStack>();
        //[SerializeField] private CargoUIController cargoui;
        [SerializeField] private Rigidbody cargoRbd;
        private float OrdMass;

        void Start()
        {
            OrdMass = cargoRbd.mass;
            //cargoui.UpdateDisplay(inventory);
            UpdateRBMass();
        }

        public void UpdateRBMass()
        {
            //inventory mass is added to the rigidbody
            foreach (ItemStack item in inventory)//getting null exceptions here for some reason
            {
                //Currently, there exist no items where count is not kg.
                //Ingots added in start are 5Kg added 3x to one stack.
                //components and other things will eventually need mass calc'ed.

                float density = 1.0f;
                //get item def
                if (item.GetOriginalType() == typeof(Consumable_Ingot))
                {
                    Consumable_Ingot ingot = (Consumable_Ingot)item.GetItemArray().GetValue(0);
                    density = ingot.GetIngotMass();
                    //IngotDefinition idef;
                    //IngotLibrary.IngotDictionary.TryGetValue(item.GetStackType(), out idef);
                    //density = idef.GetDensity();
                }
                /*
                else if (item.GetOriginalType() == typeof(Consumable_Component))
                {
                    density = 2.0f;//eventually mass of the component will be the added masses of input materials.
                }
                else
                {
                    density = 1.0f;
                }
                */
                cargoRbd.mass += item.Size() * density;
                OrdMass = cargoRbd.mass;
            }
        }

        public void ExternalInteractFunc()
        {
            //show inventory UI
        }

        public void DisplayInventory()
        {
            //invUI.SetCargoContainer(this);
            //invUI.SetContName(this.gameObject.name);
            //invUI.UpdateDisplay();
        }

        //public InventoryUIController GetInventoryUI()
        //{
        //    return invUI;
        //}

        public void InputFromPlayer(GameObject player)
        {
            IPlayer_Inventory playerInventory = player.GetComponent<IPlayer_Inventory>();
            //inventory.Add(playerInventory.RemoveFromPlayerInventory(0));
            //cargoui.UpdateDisplay(inventory);
        }

        public int MaxWeight
        {
            get { return maxWeight; }
        }

        public int MaxVolume
        {
            get { return maxVolume; }
        }

        public bool IsFull()
        {
            //Debug.Log(volume +">="+ MaxVolume+"||"+OrdMass+">="+MaxWeight);
            return ((volume >= MaxVolume)||(OrdMass >= MaxWeight));
        }

        //public CargoUIController GetCargoUIController()
        //{
        //    return cargoui;
        //}

        public List<ItemStack> GetInventory()
        {
            return inventory;
        }

        public ItemStack RemoveFromInventory<stacktype>(ItemStack removeFromStack, int atIndex)
        {
            int stackIndex = -1;
            for (int i = 0; i < inventory.Count; i++)
            {
                if (inventory[i] == removeFromStack)
                {
                    stackIndex = i;
                }
            }
            if (stackIndex != -1)
            {
                Debug.Log("Removing: " + inventory[stackIndex].GetItemArray().GetValue(atIndex));
                inventory[stackIndex].RemoveTArrayIndex<stacktype>(atIndex, out ItemStack stack);
                if (inventory[stackIndex].GetRealLength() <= 0f)
                {
                    inventory.RemoveAt(stackIndex);
                }
                return stack;
            }
            else
            {
                return null;
            }
        }

        public ItemStack Remove(int index)
        {
            ItemStack stack = inventory[index];
            inventory.RemoveAt(index);
            //UpdateRBMass();
            //sanity check
            for(int i = inventory.Count-1; i >= 0; i--)
            {
                if(inventory[i] != null)
                {
                    if (inventory[i].GetRealLength() == 0)
                    {
                        inventory.RemoveAt(i);
                    }
                }
                else
                {
                    inventory.RemoveAt(i);
                }
            }
            return stack;
        }

        public bool Add(ItemStack stack)
        {
            //if (cargoRbd.mass > maxWeight)
            //{
            for (int i = 0; i < inventory.Count; i++)
            {
                Debug.Log("Checking: " + inventory[i]);
                if (inventory[i] != null)
                {
                    if (inventory[i].CompareMetaData(stack))
                    {
                        Debug.Log("Added to cont inventory");
                        ItemStack slaanesh = inventory[i].AddItemStack(stack);
                        inventory.Add(slaanesh);
                        //UpdateRBMass();
                        return true;
                    }
                }
            }
            Debug.Log("Added to cont inventory");
            //if the return is not hit, then there are no other compatible itemstacks
            inventory.Add(stack);
            //UpdateRBMass();
            return true;
            //}
            //else { return false; }
        }
    }
}