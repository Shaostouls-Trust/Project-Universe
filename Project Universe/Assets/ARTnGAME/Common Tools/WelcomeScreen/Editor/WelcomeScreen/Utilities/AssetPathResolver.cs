using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Artngame.CommonTools.WelcomeScreen.Utilities
{
    public class AssetPathResolver
    {
        public static string GetAssetPathRelativeToScript(ScriptableObject scriptableObject)
        {
            var ms = MonoScript.FromScriptableObject(scriptableObject);
            return AssetDatabase.GetAssetPath(ms);
        }

        public static string GetAssetFolderPathRelativeToScript(ScriptableObject scriptableObject, int folderDistanceFromFileRoot = 0)
        {
            var scriptAssetPath = GetAssetPathRelativeToScript(scriptableObject);
            var slashIndexes = AllIndexesOf(scriptAssetPath, "/").ToList();
            return scriptAssetPath.Substring(0, slashIndexes[slashIndexes.Count - 1 - (folderDistanceFromFileRoot)]);
        }

        private static IEnumerable<int> AllIndexesOf(string str, string searchStr)
        {
            var minIndex = str.IndexOf(searchStr);
            while (minIndex != -1)
            {
                yield return minIndex;
                minIndex = str.IndexOf(searchStr, minIndex + searchStr.Length);
            }
        }
    }
}
