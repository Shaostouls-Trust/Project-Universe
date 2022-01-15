using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ProjectUniverse.Player;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Data.Libraries;
using ProjectUniverse.Production.Resources;
using ProjectUniverse.Items.Weapons;
using ProjectUniverse.Util;
using System.Reflection;

namespace ProjectUniverse.Base
{
	public class IConstructible : MonoBehaviour
	{
		[SerializeField] private bool AutoBuild = false;
		protected MachineDefinition IConstructible_MachineDefinition;
		private IPlayer_Inventory IConstructible_PlayerInventory;
		protected List<ItemStack> IConstructible_ComponentsReal;
		private (IComponentDefinition, int)[] IConstructible_RequiredComponents;
		private (IComponentDefinition, float)[] IConstructible_UpgradeComponents;
		//The levels, and number of levels for this machine (possibly the comps for them too?)
		private int[] IConstructible_Levels;
		private int IConstructible_CurrentLevel;
		protected bool IConstructible_MachineFullyBuilt;
		private bool Building = false;
		private float IConstructible_BuildTimeRemaining;
		[SerializeField] private string IConstructible_Machine_Type;
		[SerializeField] private float IConstructible_BaseHealth = 150;
		[SerializeField] private int IConstructible_MachineHealthNormal;
		[SerializeField] private float IConstructible_MachineHealthRemaining;

		protected void Start()
		{
			//Debug.Log("IConstructible running on "+name);
			if (MachineLibrary.MachineDictionary.TryGetValue(IConstructible_Machine_Type, out IConstructible_MachineDefinition))
			{
				IConstructible_RequiredComponents = IConstructible_MachineDefinition.GetComponentRecipe().ToArray();
				IConstructible_UpgradeComponents = IConstructible_MachineDefinition.GetComponentUpgradeCost().ToArray();
				IConstructible_Levels = IConstructible_MachineDefinition.GetLevels().ToArray();
				IConstructible_CurrentLevel = 0;//IE Levels[0]
				IConstructible_ComponentsReal = new List<ItemStack>(IConstructible_RequiredComponents.Length);
				//Fill the empty list with empty Itemstacks of the type provided by RequiredComponents
				for(int x = 0;x < IConstructible_RequiredComponents.Length; x++)
				{
					Consumable_Component comp0 = new Consumable_Component(IConstructible_RequiredComponents[x].Item1.GetComponentType(),
							 0, IConstructible_RequiredComponents[x].Item1);
					ItemStack stackx = new ItemStack(comp0.ComponentID, 999, typeof(Consumable_Component));
					IConstructible_ComponentsReal.Add(stackx);
				}
				IConstructible_BuildTimeRemaining = CalculateBuildTime();
				//autobuild
				if (AutoBuild)
				{
					AutoBuildConstructible();
				}
			}
			else
			{
				Debug.LogError("DICTIONARY ERROR");
			}
		}

		/// <summary>
		/// For every required comp, create a real comp and a construction comp
		/// </summary>
		private void AutoBuildConstructible()
		{
			while (!IConstructible_MachineFullyBuilt)
			{
				//determine what component needs grabbed
				for (int i = 0; i < IConstructible_RequiredComponents.Length; i++)
				{
					//if the numbers aren't equal, then we're missing components
					if (IConstructible_ComponentsReal[i].GetRealLength() != IConstructible_RequiredComponents[i].Item2)
					{
						Consumable_Component compToAdd = new Consumable_Component(IConstructible_RequiredComponents[i].Item1.GetComponentType(),
							 1, IConstructible_RequiredComponents[i].Item1);
						ItemStack stack = new ItemStack(compToAdd.ComponentID, 999, typeof(Consumable_Component));
						stack.AddItem(compToAdd);
						bool isAdded = false;
						for (int a = 0; a < IConstructible_ComponentsReal.Count; a++)
						{
							if (IConstructible_ComponentsReal[a].CompareMetaData(stack))
							{
								IConstructible_ComponentsReal[a].AddItemStack(stack);
								isAdded = true;
								break;
							}
						}
						if (!isAdded)
						{
							IConstructible_ComponentsReal.Add(stack);
						}
					}
				}
				IConstructible_MachineFullyBuilt = true;
				for (int i = 0; i < IConstructible_ComponentsReal.Count; i++)
				{
					if (IConstructible_ComponentsReal[i].GetRealLength() != IConstructible_RequiredComponents[i].Item2)
					{
						IConstructible_MachineFullyBuilt = false;
					}
				}
			}
			CalculateMachineHealthReal();
			CalculateHealthRemaining();
			SortComponentsByPriority();
		}

		public void MachineMessageReceiver(params object[] data)
		{
			switch (data.GetValue(0))
			{
				case 0:
					//stop building
					Debug.Log("Stopping building");
					Building = false;
					StopCoroutine("BuildConstructible");
					break;
				case 1:
					//peak at machine component requirements and return them
					object[] prams1 = { IConstructible_RequiredComponents, IConstructible_ComponentsReal.ToArray(), true };
					data.GetValue(1).GetType().GetMethod("IConstructibleCallback").Invoke(data.GetValue(1),
						new[] { prams1 });
					break;
				case 2:
					//Continue welding
					object[] prams2 = { IConstructible_ComponentsReal.ToArray() };
					data.GetValue(1).GetType().GetMethod("IConstructibleCallback").Invoke(data.GetValue(1), new[] { prams2 });
					break;
				case 3:
					//begin building this machine
					PreBuildCheck((IPlayer_Inventory)data.GetValue(2));
					object[] prams3 = { IConstructible_RequiredComponents, IConstructible_ComponentsReal.ToArray() };
					data.GetValue(1).GetType().GetMethod("IConstructibleCallback").Invoke(data.GetValue(1),
						new[] { prams3 });
					break;
			}
		}
		public void PreBuildCheck(IPlayer_Inventory p_Inventory)
		{
			if (!Building)
			{
				if (IConstructible_BuildTimeRemaining > 0)
				{
					Building = true;
					if (p_Inventory.GetPlayerInventory().Count > 0)
					{
						IConstructible_PlayerInventory = p_Inventory;
						StartCoroutine("BuildConstructible");
					}
				}
			}
		}

		private IEnumerator BuildConstructible()
		{
			float compsPerSec = IConstructible_BuildTimeRemaining / CalculateBuildTime();
			float compsPerSecTimer = compsPerSec;
			Debug.Log("comps/s: " + compsPerSec);

			while (!IConstructible_MachineFullyBuilt && Building)
			{
				if (compsPerSecTimer <= 0)
				{
					for (int i = 0; i < IConstructible_RequiredComponents.Length; i++)
					{
						//if the numbers aren't equal, then we're missing components
						if (IConstructible_ComponentsReal[i].GetRealLength() != IConstructible_RequiredComponents[i].Item2)
						{
							//find RequiredComponents[i] in IConstructible_PlayerInventory
							int index = 0;
							IConstructible_PlayerInventory.SearchInventoryForComponent(IConstructible_RequiredComponents[i].Item1, out index);
							if (index >= 0)
							{
								ItemStack stack = IConstructible_PlayerInventory.RemoveFromPlayerInventory(index, 1f);
								Consumable_Component compToAdd = stack.GetItemArray().GetValue(0) as Consumable_Component;
								bool isAdded = false;
								for (int a = 0; a < IConstructible_ComponentsReal.Count; a++)
								{
									//IE if they are the same component type (we can't use i b/c previous elements might have been combined)
									if (IConstructible_ComponentsReal[a].CompareMetaData(stack))
									{
										IConstructible_ComponentsReal[a].AddItemStack(stack);
										isAdded = true;
										break;
									}
								}
								if (!isAdded)
								{
									IConstructible_ComponentsReal.Add(stack);
								}
							}
							//MUST BE A BETTER WAY THAN THIS BREAK!
							break;//build only 1 component per loop
						}
					}
					compsPerSecTimer += compsPerSec;//+= instead of equals to compensate for imprecission
				}
				IConstructible_MachineFullyBuilt = true;
				//determine build status
				for (int i = 0; i < IConstructible_ComponentsReal.Count; i++)
				{
					if (IConstructible_ComponentsReal[i].GetRealLength() != IConstructible_RequiredComponents[i].Item2)
					{
						IConstructible_MachineFullyBuilt = false;
					}
				}
				compsPerSecTimer -= Time.deltaTime;
				IConstructible_BuildTimeRemaining -= Time.deltaTime;
				yield return null;
			}
			Debug.Log("CalcHealthNormal");
			CalculateMachineHealthReal();
			//CalculateMachineHealthNormal();
			CalculateHealthRemaining();
			SortComponentsByPriority();
		}

		/// <summary>
		/// Add to the build recipe the cost of upgrading, multiplied according to the difference between the current and target level.
		/// +1 increases cost by 1 every level
		/// +0.5 increases cost by 1 every other level
		/// </summary>
		/// <param name="currentLevel"> is the current level. Defaults to 1.</param>
		/// <param name="targetLevel"> is the level we are attempting to upgrade to. Cannot exceed max level.</param>
		public void AddUpgradeCostByLevel(int currentLevel,int targetLevel)
		{
			int target = currentLevel - targetLevel;
			Debug.Log("Increase level by "+target);
			for (int a = 0; a < IConstructible_UpgradeComponents.Length; a++)
			{
				for (int b = 0; b < IConstructible_RequiredComponents.Length; b++)
				{
					if (IConstructible_RequiredComponents[b].Item1 == IConstructible_UpgradeComponents[a].Item1)
					{
						int newCost = 0;
						//1 every 4 levels
						if (IConstructible_UpgradeComponents[a].Item2 == 0.25)
						{
							//get the number of components there should now be as a result of upgrading
							//as opposed to the number of comps to add
							target = targetLevel / 4;
							newCost = IConstructible_RequiredComponents[b].Item2 + target;
						}
						//1 every 2
						else if(IConstructible_UpgradeComponents[a].Item2 == 0.5)
						{
							//dividing to an int should shave off the floating point and leave us a whole number
							target = targetLevel / 2;
							newCost = IConstructible_RequiredComponents[b].Item2 + target;
						}
						//1/2/... every 1
						else
						{
							newCost = IConstructible_RequiredComponents[b].Item2 + ((int)IConstructible_UpgradeComponents[a].Item2 * target);
						}
						Debug.Log("New cost of " + IConstructible_RequiredComponents[b].Item1 + " is "+ newCost);
						IConstructible_RequiredComponents[b] = (IConstructible_RequiredComponents[b].Item1,newCost);
					}
				}
			}
			Debug.Log(IConstructible_RequiredComponents.GetValue(0));
		}

		public float CalculateBuildTime()
		{
			float time = 0f;
			for (int a = 0; a < IConstructible_RequiredComponents.Length; a++)
			{
				time += (IConstructible_RequiredComponents[a].Item2 - IConstructible_ComponentsReal[a].GetRealLength());
			}
			return time;
		}

		public void DamageComponent(Consumable_Component component, float amount)
		{
			component.CompTakeDamage(amount);
			CalculateHealthRemaining();
		}

		/// <summary>
		/// Health according to the max health of the components in the machine (which is affected by the mats used in construction)
		/// </summary>
		public void CalculateMachineHealthReal()
		{
			for (int i = 0; i < IConstructible_ComponentsReal.Count; i++)
			{
				for(int j = 0; j < IConstructible_ComponentsReal[i].Length(); j++)
				{
					IConstructible_MachineHealthNormal += (IConstructible_ComponentsReal[i].GetItemArray().GetValue(j) as Consumable_Component).HealthValue;
				}
			}
		}

		/// <summary>
		/// Health according to the recipe, not according to the components
		/// </summary>
		public void CalculateMachineHealthNormal()
		{
			for (int i = 0; i < IConstructible_RequiredComponents.Length; i++)
			{
				IConstructible_MachineHealthNormal = IConstructible_RequiredComponents[i].Item1.GetHealth();
			}
		}

		public void CalculateHealthRemaining()
		{
			for (int i = 0; i < IConstructible_ComponentsReal.Count; i++)
			{
				Consumable_Component comp = IConstructible_ComponentsReal[i].GetItemArray().GetValue(0) as Consumable_Component;
				IConstructible_MachineHealthRemaining += comp.RemainingHealth;
			}
		}

		/// <summary>
		/// Figure all components of each priority level and reorder the CompsReal accordingly
		/// </summary>
		private void SortComponentsByPriority()
		{
			MethodInfo info = IConstructible_ComponentsReal[0].GetItemArray().GetValue(0).GetType().GetMethod("GetPriority");
			Utils.InsertionSort<int>(ref IConstructible_ComponentsReal, info,false);
		}

		public void TakeDamageFromBullet(IBullet bullet)
		{
			//calc pen damage (NYI)(?)
			int compsinlevel = 0;
			int currentPriority = 10;
			float damageToComp = 0f;
			Consumable_Component comp;
			for (int b = 0; b < IConstructible_ComponentsReal.Count; b++)
			{
				comp = IConstructible_ComponentsReal[b].GetItemArray().GetValue(0) as Consumable_Component;
				try { comp.GetQuantity(); }//try to access a pram, will fail if null
				catch {
					Debug.Log("break");
					break; }
				if (comp.RemainingHealth > 0 && comp.GetQuantity() > 0)//null ref if bullets hit empty comp
				{
					int tempPriority = comp.GetPriority();
					currentPriority = Math.Min(tempPriority, currentPriority);
					if (tempPriority == currentPriority)
					{
						compsinlevel += 1;
					}
				}
			}
			//Debug.Log(currentPriority);
			if(compsinlevel > 0)
			{
				damageToComp = bullet.GetDamageAmount() / compsinlevel;
				for (int c = 0; c < IConstructible_ComponentsReal.Count; c++)
				{
					if (IConstructible_ComponentsReal[c] != null)
					{
						comp = IConstructible_ComponentsReal[c].GetItemArray().GetValue(0) as Consumable_Component;//Consumable_Component comp
						if (comp.GetPriority() == currentPriority)
						{
							if (comp.RemainingHealth > 0 && comp.GetQuantity() > 0)
							{
								//apply the above distributed damage to the first comp of this priority level
								//WE NEED TO TRACK HOW MUCH DAMAGE EXCEEDED HEALTH SO THAT THE REMAINING DMG CAN APPLY TO COMP #2/priority+1!
								float remains = comp.CompTakeDamage(damageToComp);
								IConstructible_MachineHealthRemaining -= damageToComp;
								//ASSERT: remains <= 0 means a component was destroyed and one component must be removed
								//ASSERT: each component only contains 1 component (IE component.quantity == 1)
								if (remains <= 0)
								{
									IConstructible_MachineFullyBuilt = false;
									IConstructible_ComponentsReal[c].RemoveTArrayIndex(0);
									//inject a 'dead' dummy component into the last index of the TArray. This is to prevent NullPointers
								    Consumable_Component dummy = new Consumable_Component(comp.ComponentID, 0, comp.GetComponentDefinition());
									dummy.RemainingHealth = 0f;
									//if lastindex is 0 then this is auto null pointer
									if(IConstructible_ComponentsReal[c].LastIndex == 0)
									{
										IConstructible_ComponentsReal[c].GetItemArray().SetValue(dummy, IConstructible_ComponentsReal[c].LastIndex);
									}
									else
									{
										IConstructible_ComponentsReal[c].GetItemArray().SetValue(dummy, IConstructible_ComponentsReal[c].LastIndex - 1);
									}
									
								}
							}
						}
					}
				}
			}
			else
			{
				IConstructible_MachineHealthRemaining -= bullet.GetDamageAmount();
			}
			ProcessDamageToComponents();
		}

		protected virtual void ProcessDamageToComponents() { }
	}
}