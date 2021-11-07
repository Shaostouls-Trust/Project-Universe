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

        }

        public OreLibrary.OreMaterialLibrary GetOreDictionary()
        {
            return OreLib;
        }

        public InclusionLibrary.InclusionDictionary GetInclusionDictionary()
        {
            return IDL;
        }

        public IngotLibrary.IngotDefinitionLibrary GetIngotDictionary()
        {
            return IngotLib;
        }

        public IComponentLibrary.ComponentDefinitionLibrary GetComponentDictionary()
        {
            return CDL;
        }

        public MachineLibrary.MachineDefinitionLibrary GetMachineDictionary()
        {
            return MDL;
        }

        public ProbabilityLibrary.ProbabilityDictionary GetProbabilityDictionary()
        {
            return PBD;
        }
    }
}