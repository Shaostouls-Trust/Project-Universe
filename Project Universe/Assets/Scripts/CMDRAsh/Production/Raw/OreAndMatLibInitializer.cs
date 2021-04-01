using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OreAndMatLibInitializer : MonoBehaviour
{
    private OreLibrary.OreMaterialLibrary OreLib;
    private InclusionLibrary.InclusionDictionary IDL;
    private IngotLibrary.IngotDefinitionLibrary IngotLib;
    private IComponentLibrary.ComponentDefinitionLibrary CDL;
    private MachineLibrary.MachineDefinitionLibrary MDL;

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
}
