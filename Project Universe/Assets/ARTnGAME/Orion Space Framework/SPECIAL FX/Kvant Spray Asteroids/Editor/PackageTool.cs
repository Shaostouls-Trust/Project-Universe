using UnityEngine;
using UnityEditor;
namespace Artngame.Orion.Kvant
{
    public class PackageTool
    {
        [MenuItem("Package/Update Package")]
        static void UpdatePackage()
        {
            AssetDatabase.ExportPackage("Assets/Kvant", "KvantSpray.unitypackage", ExportPackageOptions.Recurse);
        }
    }
}