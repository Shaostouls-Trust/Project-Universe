using UnityEngine;
using UnityEngine.UI;

namespace ModelShark
{
    [RequireComponent(typeof(Button))]
    public class EnableTouch : MonoBehaviour
    {
        public Button enableMouseButton;
        public GameObject popupOnObject;

        public void Start()
        {
            Button button = gameObject.GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        public void OnClick()
        {
            TooltipManager.Instance.touchSupport = true;
            enableMouseButton.gameObject.SetActive(true);
            gameObject.SetActive(false);

            PopupTooltipRemotely popupFunction = gameObject.GetComponent<PopupTooltipRemotely>();
            if (popupFunction == null) return;

            popupFunction.PopupTooltip(popupOnObject, "You've enabled touch controls. To activate tooltips in this mode, press and hold over items.", "Okay");
        }
    }
}