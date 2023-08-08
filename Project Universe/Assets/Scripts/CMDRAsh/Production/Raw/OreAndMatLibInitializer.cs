using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Data.Libraries
{
    public class OreAndMatLibInitializer : MonoBehaviour
    {
        private OreLibrary.OreMaterialLibrary OreLib;
        private InclusionLibrary.InclusionDictionary IDL;
        private IngotLibrary.IngotDefinitionLibrary IngotLib;
        private IComponentLibrary.ComponentDefinitionLibrary CDL;
        private MachineLibrary.MachineDefinitionLibrary MDL;
        private ProbabilityLibrary.ProbabilityDictionary PBD;
        private GasLibrary.GasDefinitionLibrary GDL;

        void Awake()
        {
            OreLib = new OreLibrary.OreMaterialLibrary();
            try
            {
                OreLib.InitializeOreDictionary();
            }
            catch(Exception e)
            {
                Debug.Log("Ore Library Failed to Load:");
                Debug.LogError(e);
                Debug.Log("===========================");
            }
            try
            {
                OreLib.InitializeMaterialDictionary();
            }
            catch (Exception e)
            {
                Debug.Log("Material Library Failed to Load:");
                Debug.LogError(e);
                Debug.Log("===========================");
            }

            IDL = new InclusionLibrary.InclusionDictionary();
            try
            {
                IDL.InitializeInclusionDictionary();
            }
            catch (Exception e)
            {
                Debug.Log("Inclusion Library Failed to Load:");
                Debug.LogError(e);
                Debug.Log("===========================");
            }

            IngotLib = new IngotLibrary.IngotDefinitionLibrary();
            try
            {
                IngotLib.InitializeIngotDictionary();
            }
            catch (Exception e)
            {
                Debug.Log("Ingot Library Failed to Load:");
                Debug.LogError(e);
                Debug.Log("===========================");
            }

            CDL = new IComponentLibrary.ComponentDefinitionLibrary();
            try
            {
                CDL.InitializeComponentDictionary();
            }
            catch (Exception e)
            {
                Debug.Log("Component Library Failed to Load:");
                Debug.LogError(e);
                Debug.Log("===========================");
            }

            MDL = new MachineLibrary.MachineDefinitionLibrary();
            try
            {
                MDL.InitializeMachineDictionary();
            }
            catch (Exception e)
            {
                Debug.Log("Machine Library Failed to Load:");
                Debug.LogError(e);
                Debug.Log("===========================");
            }

            PBD = new ProbabilityLibrary.ProbabilityDictionary();
            try
            {
                PBD.InitializeProbabilityDictionary();
            }
            catch (Exception e)
            {
                Debug.Log("Probability Library Failed to Load:");
                Debug.LogError(e);
                Debug.Log("===========================");
            }

            GDL = new GasLibrary.GasDefinitionLibrary();
            try
            {
                GDL.InitializeGasDictionary();
            }
            catch (Exception e)
            {
                Debug.Log("Gas Library Failed to Load:");
                Debug.LogError(e);
                Debug.Log("===========================");
            }
        }
    }
}