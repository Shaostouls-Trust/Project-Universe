using ProjectUniverse.Data.Libraries;
using ProjectUniverse.Data.Libraries.Definitions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Environment.World
{
    public class BlockOreAsteroid : MonoBehaviour
    {

        /// <summary>
        /// Based on the zone and quality of the ore, generate for each block it's probability of being a block of ore.
        /// </summary>
        /// 
        private Transform[] children;
        [SerializeField] private int zone;
        [SerializeField] private Rigidbody rigidbody;
        private int quality;
        System.Random rand = new System.Random();

        void Start()
        {
            quality = AssignOreQualityProbability();
            children = new Transform[transform.childCount];
            //Find all children
            for (int i = 0; i < transform.childCount; i++)
            {
                children[i] = transform.GetChild(i);
                float vol = 0f;
                //mass calcs
                if (children[i].gameObject.TryGetComponent(out BoxCollider bc))
                {
                    vol = bc.size.x * bc.size.y * bc.size.z;
                }
                else if(children[i].gameObject.TryGetComponent(out CapsuleCollider cc)){
                    vol = (cc.radius * (4f / 3f) * Mathf.PI * cc.radius * cc.radius) + (cc.radius*2f*Mathf.PI*cc.height);
                }
                //rigidbody.mass += (vol * children[i].GetComponent<BlockOreSingle>().NDensity);
                //Debug.Log("rgm: "+rigidbody.mass);
                //PLAYER controll/mass is moving the asteroid b/c the pc doesn't rely on rigidbody mass

                bool isore = false;
                double a = rand.NextDouble();
                float target = 0f;
                //based on ore quality, determine percent of ore that is ore and what is stone
                switch (quality)
                {
                    case 0:
                        target = 0.25f;
                        break;
                    case 1:
                        target = 0.35f;
                        break;
                    case 2:
                        target = 0.40f;
                        break;
                    case 3:
                        target = 0.45f;
                        break;
                    case 4:
                        target = 0.5f;
                        break;
                    case 5:
                        target = 0.55f;
                        break;
                    case 6:
                        target = 0.65f;
                        break;
                    case 7:
                        target = 0.75f;
                        break;
                }
                if (a <= target)
                {
                    isore = true;
                }
                else
                {
                    isore = false;
                }
                AssignOreProbability(children[i], isore);
                children[i].GetComponent<BlockOreSingle>().SetBehavior();
            }

        }

        /// <summary>
        /// Choose an ore from the zone's ore probabilities
        /// Give the ore the zone's impurities
        /// </summary>
        /// <param name="childOreBlock"></param>
        public void AssignOreProbability(Transform childOreBlock, bool isore)
        {
            BlockOreSingle bos = childOreBlock.GetComponent<BlockOreSingle>();
            bos.Quality = quality;
            //get oredefinitions
            OreLibrary.OreDictionary.TryGetValue("Ore_Iron", out OreDefinition IRON);
            OreLibrary.OreDictionary.TryGetValue("Ore_Copper", out OreDefinition COPPER);
            OreLibrary.OreDictionary.TryGetValue("Ore_Nickel", out OreDefinition NICKEL);
            OreLibrary.OreDictionary.TryGetValue("Ore_Tin", out OreDefinition TIN);
            OreLibrary.OreDictionary.TryGetValue("Ore_Aluminum", out OreDefinition ALUMINUM);
            OreLibrary.OreDictionary.TryGetValue("Ore_Lithium", out OreDefinition LITHIUM);//inclusion?
            OreLibrary.OreDictionary.TryGetValue("Ore_Titanium", out OreDefinition TITANIUM);
            OreLibrary.OreDictionary.TryGetValue("Ore_Tungsten", out OreDefinition TUNGSTEN);
            OreLibrary.OreDictionary.TryGetValue("Ore_Gold", out OreDefinition GOLD);
            //get probabilities
            int ironprob;
            int copperprob;
            int nickelprob;
            int tinprob;
            int aluminumprob;
            int lithiumprob;
            int titaniumprob;
            int tungstenprob;
            int goldprob;

            ProbabilityLibrary.GetZoneOres(zone, IRON, out ironprob);
            ProbabilityLibrary.GetZoneOres(zone, COPPER, out copperprob);
            ProbabilityLibrary.GetZoneOres(zone, NICKEL, out nickelprob);
            ProbabilityLibrary.GetZoneOres(zone, TIN, out tinprob);
            ProbabilityLibrary.GetZoneOres(zone, ALUMINUM, out aluminumprob);
            ProbabilityLibrary.GetZoneOres(zone, LITHIUM, out lithiumprob);
            ProbabilityLibrary.GetZoneOres(zone, TITANIUM, out titaniumprob);
            ProbabilityLibrary.GetZoneOres(zone, TUNGSTEN, out tungstenprob);
            ProbabilityLibrary.GetZoneOres(zone, GOLD, out goldprob);

            //calculate and assign ore type from probability
            int cumulative = 0;
            OreDefinition[] defs = { IRON, COPPER, NICKEL, TIN, ALUMINUM, LITHIUM, TITANIUM, TUNGSTEN, GOLD };
            float[] probabilities = { ironprob, copperprob, nickelprob, tinprob, aluminumprob, lithiumprob, titaniumprob, tungstenprob, goldprob };
            //Roll a random number between 0 and u

            /// Yields a very low chance to get Iron, Copper, and much higher chance of getting Titanium and Tungsten
            ///
            //Debug.Log("====================");
            for (int i = 0; i < probabilities.Length; i++)
            {
                int prob = rand.Next(0, 100 - cumulative);
                //Debug.Log("Ore: " + prob + " <= " + probabilities[i]);
                if (prob <= probabilities[i] && isore)
                {
                    //select this type
                    //Debug.Log(defs[i].GetOreType());
                    bos.OreDef = defs[i];
                    break;
                }
                else
                {
                    bos.OreDef = null;//regular stone
                }
                cumulative += (int)probabilities[i];
            }
            bos.Zone = zone;
        }

        /// <summary>
        /// According to the below table, calculate ore quality <br/>
        /// Poor II - 15.00<br/>
        /// Poor I - 15.00<br/>
        /// Normal III - 20.00<br/>
        /// Normal II - 20.00<br/>
        /// Normal I - 15.00<br/>
        /// High II - 10.00<br/>
        /// High I - 5.00<br/>
        /// Very High - 1.00<br/>
        /// </summary>
        public int AssignOreQualityProbability()
        {
            int cumulative = 0;
            float[] probabilities = { 15.0f, 15.0f, 20.0f, 20.0f, 15.0f, 10.0f, 5.0f, 1.0f };
            for (int i = 0; i < probabilities.Length; i++)
            {
                //Roll a random number between 0 and u
                int prob = rand.Next(0, 100 - cumulative);
                //Debug.Log("Quality: "+prob+" <= "+probabilities[i]);
                if (prob <= probabilities[i])
                {
                    //select this type
                    //Debug.Log(i);
                    return i;
                }
                cumulative += (int)probabilities[i];
            }
            return 7;//Very High
        }
    }
}