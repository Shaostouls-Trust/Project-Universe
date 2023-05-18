using ProjectUniverse.Base;
using ProjectUniverse.Data.Libraries.Definitions;
using ProjectUniverse.Production.Resources;
using ProjectUniverse.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Environment.World
{
    public class BlockOreSingle : MonoBehaviour
    {
        private float nDensity = 1600f;//kg/m^3
        private OreDefinition oreDef;
        private int quality;
        private int zone;
        private float mass;
        //mine in 10Kg parts
        private float mineHealth = 2.5f;
        private MeshRenderer renderer;

        public OreDefinition OreDef
        {
            get { return oreDef; }
            set { oreDef = value; }
        }

        public int Quality
        {
            get { return quality; }
            set { quality = value; }
        }

        public int Zone
        {
            get { return zone; }
            set { zone = value; }
        }

        public float NDensity
        {
            get { return nDensity; }
            // set { nDensity = value; }
        }

        private void Start()
        {
            renderer = gameObject.GetComponent<MeshRenderer>();
            if(renderer == null)
            {
                renderer = gameObject.GetComponentInChildren<MeshRenderer>();
            }
            //Debug.Log(renderer);
        }

        /// <summary>
        /// Set the block's material to the material of the ore it is now representing.
        /// </summary>
        public void SetBehavior()
        {
            //mass is the volume of the rock times the normal density of the rock
            //We won't always have a box collider!
            Vector3 size = transform.lossyScale;//GetComponent<BoxCollider>().size;//we need the scale not the size
            float vol = size.x * size.y * size.z;
            mass = vol * nDensity;
            GetComponent<Rigidbody>().mass = mass;
            //set material to ore, otherwise set it to the default zone material
            if (OreDef != null)
            {
                string str = "LibMat_" + OreDef.GetOreType() + "_Mat";
                //Debug.Log("Materials/CMDRAsh/RocksNOres/"+str);
                Material oreMat = Resources.Load<Material>("Materials/CMDRAsh/RocksNOres/" + str);
                if (oreMat == null)
                {
                    //Debug.Log("Null");
                    oreMat = Resources.Load<Material>("Materials/CMDRAsh/RocksNOres/LibMat_Rock_Black_Dark");
                }
                renderer.material = oreMat;
            }
            else
            {
                string str = "LibMat_";
                switch (zone)
                {
                    case 0:
                        str += "Rock_35_Mat";
                        break;
                    case 1:
                        str += "Rock_Black_Dark";
                        break;
                    case 2:
                        str += "Rock_31_Mat";
                        break;
                    case 3:
                        str += "Rock_Black";
                        break;
                    case 4:
                        str += "Rock_20_Mat";
                        break;
                    case 5:
                        str += "Rock_Grey_Rough";
                        break;
                    case 6:
                        str += "Rock_Grey";
                        break;
                }
                Material rockMat = Resources.Load<Material>("Materials/CMDRAsh/RocksNOres/" + str);
                renderer.material = rockMat;
            }

        }

        public void DestroyBlock()
        {
            Destroy(this.gameObject);
        }

        /// <summary>
        /// Remove mass according to drill something of the other. Add it to an Itemstack of Ore and return it.
        /// What will we return if it's stone?
        /// </summary>
        public ItemStack MiningCallback(int amount)
        {
            ItemStack stack = null;
            mineHealth -= amount * Time.deltaTime;
            if (mineHealth <= 0f)
            {
                mass -= 10;//10Kg per load
                if (OreDef != null)//only if it's ore. What will we do if it's stone?
                {
                    stack = new ItemStack(OreDef.GetOreType(), 9000, typeof(Consumable_Ore));
                    //Debug.Log(OreDef.GetOreType());
                    Consumable_Ore ore = new Consumable_Ore(OreDef.GetOreType(), Quality, Zone, 10);
                    stack.AddItem(ore);
                    //Debug.Log("Remove Ore");
                }
                mineHealth = 2.5f;
            }
            if (mass <= 0f)
            {
                DestroyBlock();
            }
            return stack;
        }

        /// Not being called. BlockOreAsteroid is called instead.
        /// 
        void LookInfoMsg(LookInfo linf)
        {
            string[] data = new string[1];
            //string s = Math.Round(mineHealth,2) + "/" + "2.5; " + mass;
            if (OreDef != null)
            {
                data[0] =  "" + OreDef.GetOreType() + "; " + mass ;
            }
            else
            {
                data[0] = "Rock; " + mass;
            }
            //return type of soil
            linf.GetType().GetMethod("LookInfoCallback").Invoke(linf, new[] { data });
        }
    }
}