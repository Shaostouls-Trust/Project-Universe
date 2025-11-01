namespace Artngame.CommonTools.WelcomeScreen.GuiElements
{
    public abstract class GuiSectionBase
    {
        public string LabelText { get; }
        public string LabelSubText { get; set; }
        public int WidthPx { get; private set; }

        protected GuiSectionBase(string labelText)
        {
            LabelText = labelText;
        }

        protected GuiSectionBase(string labelText, string labelSubText)
        {
            LabelText = labelText;
            LabelSubText = labelSubText;
        }

        public void InitializeWidthPx(int widthPx)
        {
            WidthPx = widthPx;
        }
    }
}