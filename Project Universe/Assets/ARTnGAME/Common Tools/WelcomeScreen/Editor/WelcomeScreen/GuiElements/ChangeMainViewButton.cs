using System;

namespace Artngame.CommonTools.WelcomeScreen.GuiElements
{
    public class ChangeMainViewButton : ClickableElement
    {
        protected readonly Action<ProductWelcomeScreenBase> _renderMainScrollViewFn;

        public ChangeMainViewButton(string text, Action<ProductWelcomeScreenBase> renderMainScrollViewFn)
            : base(text, string.Empty)
        {
            _renderMainScrollViewFn = renderMainScrollViewFn;
        }

        public override void OnClick(ProductWelcomeScreenBase welcomeScreen)
        {
            welcomeScreen.ChangeMainScrollViewRenderFn(_renderMainScrollViewFn);
        }
    }
}