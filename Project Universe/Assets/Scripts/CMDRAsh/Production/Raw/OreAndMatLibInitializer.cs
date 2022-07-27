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
            OreLib.InitializeOreDictionary();
            OreLib.InitializeMaterialDictionary();

            IDL = new InclusionLibrary.InclusionDictionary();
            IDL.InitializeInclusionDictionary();

            IngotLib = new IngotLibrary.IngotDefinitionLibrary();
            IngotLib.InitializeIngotDictionary();

            CDL = new IComponentLibrary.ComponentDefinitionLibrary();
            CDL.InitializeComponentDictionary();

            MDL = new MachineLibrary.MachineDefinitionLibrary();
            MDL.InitializeMachineDictionary();

            PBD = new ProbabilityLibrary.ProbabilityDictionary();
            PBD.InitializeProbabilityDictionary();

            GDL = new GasLibrary.GasDefinitionLibrary();
            GDL.InitializeGasDictionary();
        }
    }
}