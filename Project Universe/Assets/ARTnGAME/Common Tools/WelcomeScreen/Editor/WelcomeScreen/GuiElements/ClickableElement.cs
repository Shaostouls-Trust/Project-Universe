namespace Artngame.CommonTools.WelcomeScreen.GuiElements
{
    public abstract class ClickableElement
    {
        public string Text { get; }
        public string IconName { get; }
        public ClickableElement(string text, string iconName)
        {
            Text = text;
            IconName = iconName;
        }

        public abstract void OnClick(ProductWelcomeScreenBase welcomeScreen);
    }
}