using UnityEngine;
using UnityEngine.UI;

namespace ModelShark
{
    [RequireComponent(typeof(Button))]
    public class EnableMouse : MonoBehaviour
    {
        public Button enableTouchButton;
        public GameObject popupOnObject;

        public void Start()
        {
            Button button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        public void OnClick()
        {
            TooltipManager.Instance.touchSupport = false;
            enableTouchButton.gameObject.SetActive(true);
            gameObject.SetActive(false);

            PopupTooltipRemotely popupFunction = gameObject.GetComponent<PopupTooltipRemotely>();
            if (popupFunction == null) return;

            popupFunction.PopupTooltip(popupOnObject, "You've enabled mouse control. To activate tooltips in this mode, hover over items.\n", "Okay");
        }
    }
}