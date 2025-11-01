using System;
using UnityEditor.SceneManagement;

namespace Artngame.CommonTools.WelcomeScreen.GuiElements
{
    public class LaunchSceneButton : ClickableElement
    {
        private readonly Func<ProductWelcomeScreenBase, string> _getScenePath;

        public LaunchSceneButton(string text, Func<ProductWelcomeScreenBase, string> getScenePath)
            : base(text, "BuildSettings.Editor.Small")
        {
            _getScenePath = getScenePath;
        }

        public override void OnClick(ProductWelcomeScreenBase welcomeScreen)
        {
            var scenePath = _getScenePath(welcomeScreen);
            EditorSceneManager.OpenScene(scenePath);
        }
    }
}