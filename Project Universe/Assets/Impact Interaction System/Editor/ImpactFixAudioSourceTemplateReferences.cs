using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using Impact.Interactions.Audio;
using Impact.Utility;

namespace Impact.EditorScripts
{
    public static class ImpactFixAudioSourceTemplateReferences
    {
        [MenuItem("Assets/Impact/Fix Audio Source Template References")]
        public static void FixAudioSourceTemplateReferences()
        {
            if (!EditorUtility.DisplayDialog("Fix Audio Source Template References",
                "This tool will attempt to fix broken Audio Source Template references as part of the 1.5.0 update. It will modify your assets, so make sure you have a backup!",
                "Go", "Cancel"))
                return;

            string[] audioAssets = AssetDatabase.FindAssets("t:ImpactAudioInteraction");
            Regex audioSourceGuidRegex = new Regex(@"_audioSourceTemplate:.*{fileID:\s(\d*),\sguid:\s(\w*),", RegexOptions.Multiline | RegexOptions.IgnoreCase);

            for (int i = 0; i < audioAssets.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(audioAssets[i]);
                Debug.Log($"ImpactAudioInteraction path: {path}");

                string audioInteractionAssetText = File.ReadAllText(path);
                Match audioSourceGuidMatch = audioSourceGuidRegex.Match(audioInteractionAssetText);

                if (audioSourceGuidMatch.Success)
                {
                    string originalFileId = audioSourceGuidMatch.Groups[1].Value;
                    string audioSourceTemplateGuid = audioSourceGuidMatch.Groups[2].Value;

                    Debug.Log($"_audioSourceTemplate fileID : {originalFileId}, guid: {audioSourceTemplateGuid}");

                    string audioSourceTemplatePath = AssetDatabase.GUIDToAssetPath(audioSourceTemplateGuid);

                    GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(audioSourceTemplatePath);
                    Debug.Log($"_audioSourceTemplate Game Object: {gameObject.name}");

                    ImpactAudioSource a = gameObject.GetOrAddComponent<ImpactAudioSource>();
                    PrefabUtility.SavePrefabAsset(gameObject);

                    string audioSourcePrefabText = File.ReadAllText(audioSourceTemplatePath);

                    Regex newComponentFileIDRegex = new Regex(@".*component:\s{fileID:\s(.*)}", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    MatchCollection newComponentFileIDMatchCollection = newComponentFileIDRegex.Matches(audioSourcePrefabText);
                    Match newComponentFileIDMatch = newComponentFileIDMatchCollection[newComponentFileIDMatchCollection.Count - 1];

                    string newComponentFileID = newComponentFileIDMatch.Groups[1].Value;
                    Debug.Log($"ImpactAudioSource component fileId: {newComponentFileID}");

                    string newAudioInteractionAssetText = audioInteractionAssetText.Replace(originalFileId, newComponentFileID);

                    File.WriteAllText(path, newAudioInteractionAssetText);

                    Debug.Log($"Success!");
                }
            }

            Debug.Log($"Refreshing asset database...");

            AssetDatabase.Refresh();
        }
    }
}