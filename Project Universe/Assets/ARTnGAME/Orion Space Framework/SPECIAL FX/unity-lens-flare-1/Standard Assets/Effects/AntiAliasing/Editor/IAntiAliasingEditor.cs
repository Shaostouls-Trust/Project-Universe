using UnityEditor;

namespace Artngame.Orion.ImageFX
{
    public interface IAntiAliasingEditor
    {
        void OnEnable(SerializedObject serializedObject, string path);
        bool OnInspectorGUI(IAntiAliasing target);
    }
}
