using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor.ProjectWindowCallback;
using System.IO;

namespace UnityEditor.VFXToolbox.ImageSequencer
{
    public class ImageSequenceAssetFactory
    {
        [MenuItem("Assets/Create/VFX Toolbox/Image Sequence", priority = 301)]
        private static void MenuCreatePostProcessingProfile()
        {
            var icon = EditorGUIUtility.FindTexture("ImageSequence Icon");
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, ScriptableObject.CreateInstance<DoCreateImageSequenceAsset>(), "New Image Sequence.asset", icon, null);
        }

        internal static ImageSequence CreateVFXToolboxAssetAtPath(string path)
        {
            ImageSequence asset = ScriptableObject.CreateInstance<ImageSequence>();
            asset.name = Path.GetFileName(path);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }
    }

    internal class DoCreateImageSequenceAsset : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            ImageSequence asset = ImageSequenceAssetFactory.CreateVFXToolboxAssetAtPath(pathName);
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }
    }

    public class ImageSequenceAssetCallbackHandler
    {
        [OnOpenAsset(1)]
        public static bool OpenImageSequenceAsset(int instanceID, int line)
        {
            ImageSequence asset = EditorUtility.InstanceIDToObject(instanceID) as ImageSequence;
            if(asset != null) // We opened an image sequence asset, open the editor.
            {
                ImageSequencer.OpenEditor();
                ImageSequencer window = EditorWindow.GetWindow<ImageSequencer>();
                window.Focus();
                return true;
            }
            else
                return false;
        }
    }
}
