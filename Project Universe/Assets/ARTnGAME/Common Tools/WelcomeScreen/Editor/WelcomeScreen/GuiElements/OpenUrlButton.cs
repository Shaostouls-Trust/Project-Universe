using UnityEngine;

namespace Artngame.CommonTools.WelcomeScreen.GuiElements
{
    public class OpenUrlButton : ClickableElement
    {
        public string Url { get; set; }

        public OpenUrlButton(string text, string url)
            : base(text, "BuildSettings.Web.Small")
        {
            Url = url;
        }

        public override void OnClick(ProductWelcomeScreenBase welcomeScreen)
        {
            Application.OpenURL(Url);
        }
    }
}