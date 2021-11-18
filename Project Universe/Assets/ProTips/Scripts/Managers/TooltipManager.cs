using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
// ReSharper disable InconsistentNaming
// ReSharper disable ForCanBeConvertedToForeach
// ReSharper disable LoopCanBeConvertedToQuery

namespace ModelShark
{
    /// <summary>Singleton game object that manages showing, hiding, and resizing tooltips.</summary>
    /// <remarks>Put this script on a game object in your scene (but only once) in order to use ProTips.</remarks>
    public class TooltipManager : MonoBehaviour
    {
        [Tooltip("If you have multiple cameras in your scene, this is the one that will be used by ProTips.")]
        public Camera guiCamera;
        [Tooltip("The RectTransform to match if you have an angled UI and you want tooltips to match the RectTransform's angle.")]
        public RectTransform matchRotationTo;
        [Tooltip("Globally enable or disable tooltips.")]
        public bool tooltipsEnabled = true;
        [Tooltip("When enabled, tooltips will be triggered by pressing-and-holding, not hovering over. They will be dismissed by releasing the hold, instead of hover off.")]
        public bool touchSupport = false;
        [Tooltip("How long to wait before beginning to display a tooltip.")]
        public float tooltipDelay = 0.33f;
        [Tooltip("How long the tooltip fade-in transition will last. Set to 0 for increased performance.")]
        public float fadeDuration = 0.2f;
        [Tooltip("Determines whether tooltips are repositioned when they would flow off the canvas. Disable for increased performance.")]
        public bool overflowProtection = true;
        [Tooltip("For 3D objects, determines whether tooltips are positioned based on the object's collider bounds or mesh renderer bounds.")]
        public PositionBounds positionBounds = 0;

        /// <summary>If you have multiple canvases in your scene, this is the one that will be used by ProTips.</summary>
        public Canvas GuiCanvas;

        /// <summary>For tooltip prefabs, the start/end character that signifies a dynamic field.</summary>
        public string TextFieldDelimiter { get { return "%"; } }

        private static TooltipManager instance;
        private bool isInitialized;

        public static TooltipManager Instance
        {
            get
            {
                if (instance == null)
                    instance = FindObjectOfType<TooltipManager>();
                if (instance == null) return null;
                if (!instance.isInitialized)
                    instance.Initialize();
                return instance;
            }
        }

        /// <summary>The root parent canvas component.</summary>
        private Canvas RootCanvas { get; set; }
        public GameObject TooltipContainer { get; private set; }
        private GameObject TooltipContainerNoAngle { get; set; }

        /// <summary>Holds a reference to the in-scene, runtime gameobject associated with each tooltip style prefab.</summary>
        public Dictionary<TooltipStyle, Tooltip> Tooltips { get; private set; }

        /// <summary>When populated, this is the only tooltip allowed on the screen.</summary>
        public Tooltip BlockingTooltip { get; set; }

        private void Awake()
        {
            instance = this;
            if (!isInitialized)
                Initialize();
        }

        private void Update()
        {
            // If tooltips are disabled globally, hide any tooltips that may already be visible
            if (!tooltipsEnabled && VisibleTooltips().Count > 0)
                HideAll();
        }

        private void Initialize()
        {
            if (isInitialized) return;

            // Set the GUI Canvas.
            RootCanvas = CanvasHelper.GetRootCanvas();
            if (GuiCanvas == null)
                GuiCanvas = RootCanvas;

            // Set the GUI Camera.
            if (guiCamera == null)
                guiCamera = Camera.main;

            Tooltips = new Dictionary<TooltipStyle, Tooltip>();

            // Create a new RectTransform container to hold all the tooltip instances.
            TooltipContainer = CreateTooltipContainer("Tooltip Container");

            // Create a new RectTransform container for tooltips that are never angled/rotated.
            TooltipContainerNoAngle = CreateTooltipContainer("Tooltip Container (No Angle)");

            ResetTooltipRotation();

            isInitialized = true;
        }

        private GameObject CreateTooltipContainer(string containerName)
        {
            GameObject tooltipContainer = GameObject.Find(containerName);
            if (tooltipContainer == null)
            {
                tooltipContainer = new GameObject(containerName);
                tooltipContainer.transform.SetParent(GuiCanvas.transform, false);
                RectTransform rt = tooltipContainer.AddComponent<RectTransform>();
                rt.anchorMin = rt.offsetMin = rt.offsetMax = rt.anchoredPosition = Vector2.zero;
                rt.anchorMax = rt.localScale = Vector3.one;
                tooltipContainer.transform.SetAsLastSibling();
            }
            return tooltipContainer;
        }

        /// <summary>Sets the tooltip rotation. Call this anytime your UI rotation has changed, and you want the tooltips to match it again.</summary>
        public void ResetTooltipRotation()
        {
            // Rotate the TooltipContainer if there is a specified RectTransform to match the angle to.
            TooltipContainer.transform.rotation = matchRotationTo != null ? matchRotationTo.transform.rotation : GuiCanvas.transform.rotation;
        }

        /// <summary>Sets the parameterized text fields on the tooltip.</summary>
        /// <remarks>This is separate from the Show() method because we need to wait a frame or two so the text's RectTransform has time to update its preferredWidth.</remarks>
        public void SetTextAndSize(TooltipTrigger trigger)
        {
            Tooltip tooltip = trigger.Tooltip;
            if (tooltip == null || trigger.parameterizedTextFields == null) return;

            if (tooltip.TextFields == null || tooltip.TextFields.Count == 0) return;

            LayoutElement mainTextContainer = tooltip.TooltipStyle.mainTextContainer;
            if (mainTextContainer == null)
                Debug.LogWarning($"No main text container defined on tooltip style \"{trigger.Tooltip.GameObject.name}\". Note: This LayoutElement is needed in order to resize text appropriately.");
            else
                mainTextContainer.preferredWidth = trigger.minTextWidth;

            for (int i = 0; i < tooltip.TextFields.Count; i++)
            {
                Text txt = tooltip.TextFields[i].Text;

                TextMeshProUGUI txtTMP = tooltip.TextFields[i].TextTMP;
                if (txtTMP != null)
                {
                    SetTextAndSizeTMP(txtTMP, trigger, mainTextContainer);
                    continue;
                }
                SetTextAndSizeUI(txt, trigger, mainTextContainer);
            }
        }

        private void SetTextAndSizeUI(Text txt, TooltipTrigger trigger, LayoutElement mainTextContainer)
        {
            if (txt.text.Length < 3) return; // text is too short to contain a parameter, so skip it.

            for (int j = 0; j < trigger.parameterizedTextFields.Count; j++)
            {
                if (!string.IsNullOrEmpty(trigger.parameterizedTextFields[j].value))
                    txt.text = txt.text.Replace(trigger.parameterizedTextFields[j].placeholder, trigger.parameterizedTextFields[j].value);
            }

            if (mainTextContainer != null)
            {
                // if the text would be wider than allowed, constrain the main text container to that limit.
                if (txt.preferredWidth > trigger.maxTextWidth)
                    mainTextContainer.preferredWidth = trigger.maxTextWidth;
                // otherwise, if it's within the allotted space but bigger than the text container's default width, expand the main text container to accommodate.
                else if (txt.preferredWidth > trigger.minTextWidth && txt.preferredWidth > mainTextContainer.preferredWidth)
                    mainTextContainer.preferredWidth = txt.preferredWidth;
            }
        }

        private void SetTextAndSizeTMP(TextMeshProUGUI txt, TooltipTrigger trigger, LayoutElement mainTextContainer)
        {
            if (txt.text.Length < 3) return; // text is too short to contain a parameter, so skip it.

            for (int j = 0; j < trigger.parameterizedTextFields.Count; j++)
            {
                if (!string.IsNullOrEmpty(trigger.parameterizedTextFields[j].value))
                    txt.text = txt.text.Replace(trigger.parameterizedTextFields[j].placeholder, trigger.parameterizedTextFields[j].value);
            }

            if (mainTextContainer != null)
            {
                // if the text would be wider than allowed, constrain the main text container to that limit.
                if (txt.preferredWidth > trigger.maxTextWidth)
                    mainTextContainer.preferredWidth = trigger.maxTextWidth;
                // otherwise, if it's within the allotted space but bigger than the text container's default width, expand the main text container to accommodate.
                else if (txt.preferredWidth > trigger.minTextWidth && txt.preferredWidth > mainTextContainer.preferredWidth)
                    mainTextContainer.preferredWidth = txt.preferredWidth;
            }
        }

        /// <summary>Displays the tooltip.</summary> 
        /// <remarks>
        /// This method first waits a couple frames before sizing and positioning the tooltip. 
        /// This is necessary in order to get an accurate preferredWidth property of the dynamic text field.
        /// </remarks>
        public IEnumerator Show(TooltipTrigger trigger)
        {
            if (trigger.tooltipStyle == null)
            {
                Debug.LogWarning("TooltipTrigger \"" + trigger.name + "\" has no associated TooltipStyle. Cannot show tooltip.");
                yield break;
            }

            Tooltip tooltip = trigger.Tooltip;
            Image tooltipBkgImg = tooltip.BackgroundImage;

            // If there is another tooltip open besides this one, close it.
            foreach (KeyValuePair<TooltipStyle, Tooltip> tip in Tooltips)
            {
                if (tip.Value.GameObject.activeInHierarchy && tip.Value != tooltip)
                    tip.Value.TooltipTrigger.ForceHideTooltip();
            }

            // Move the tooltip to the No Angle container if it should never be rotated.
            if (tooltip.NeverRotate)
                tooltip.GameObject.transform.SetParent(TooltipContainerNoAngle.transform, false);

            // Replace dynamic image placeholders with the correct images.
            if (trigger.dynamicImageFields != null)
            {
                for (int i = 0; i < trigger.dynamicImageFields.Count; i++)
                {
                    for (int j = 0; j < tooltip.ImageFields.Count; j++)
                    {
                        if (tooltip.ImageFields[j].Name == trigger.dynamicImageFields[i].name)
                        {
                            if (trigger.dynamicImageFields[i].replacementSprite == null)
                                tooltip.ImageFields[j].Image.sprite = tooltip.ImageFields[j].Original;
                            else
                                tooltip.ImageFields[j].Image.sprite = trigger.dynamicImageFields[i].replacementSprite;
                        }
                    }
                }
            }

            // Toggle dynamic sections on or off.
            if (trigger.dynamicSectionFields != null)
            {
                for (int i = 0; i < trigger.dynamicSectionFields.Count; i++)
                {
                    for (int j = 0; j < tooltip.SectionFields.Count; j++)
                    {
                        if (tooltip.SectionFields[j].Name == trigger.dynamicSectionFields[i].name)
                            tooltip.SectionFields[j].GameObject.SetActive(trigger.dynamicSectionFields[i].isOn);
                    }
                }
            }

            // Wait for 2 frames so we get an accurate PreferredWidth on the Text component.
            if (trigger.tipPosition != TipPosition.CanvasBottomMiddle && trigger.tipPosition != TipPosition.CanvasTopMiddle) // unless the tooltip will be positioned in a static location, then there is no need.
                yield return WaitFor.Frames(2);

            // Get the parent canvas for this tooltip trigger.
            GuiCanvas = trigger.GetComponentInParent<Canvas>();

            // If no parent canvas is found for the trigger object, use the main canvas.
            if (GuiCanvas == null)
                GuiCanvas = CanvasHelper.GetRootCanvas();

            // Parent the tooltip container under the correct canvas.
            TooltipContainer.transform.SetParent(GuiCanvas.transform, false);

            tooltip.TooltipTrigger = trigger;

            // Set the position of the tooltip.
            tooltip.SetPosition(trigger, GuiCanvas, guiCamera);

            // Set the tint color of the tooltip panel and tips.
            if (tooltipBkgImg != null)
                tooltipBkgImg.color = trigger.backgroundTint;

            // If this is a blocking tooltip, assign it as such.
            if (tooltip.IsBlocking)
                BlockingTooltip = tooltip;

            // Display the tooltip.
            tooltip.Display(fadeDuration);
        }

        /// <summary>Hides all visible tooltips.</summary>
        /// <remarks>Useful for when the player has a tutorial tooltip up, and hits ESC to go to the main menu.</remarks>
        public void HideAll()
        {
            // NOTE: If you needed to, you could store a reference to the open tooltip in the TooltipManager so you could re-open it later, 
            // when the player picks up where they left off.
            TooltipTrigger[] tooltipTriggers = FindObjectsOfType<TooltipTrigger>();
            foreach (TooltipTrigger trigger in tooltipTriggers)
                trigger.ForceHideTooltip();
        }

        public List<TooltipStyle> VisibleTooltips()
        {
            List<TooltipStyle> visibleTooltips = new List<TooltipStyle>();
            var tooltipStyles = TooltipContainer.GetComponentsInChildren<TooltipStyle>(false);
            var tooltipStylesNoAngle = TooltipContainerNoAngle.GetComponentsInChildren<TooltipStyle>(false);

            for (int i = 0; i < tooltipStyles.Length; i++)
                visibleTooltips.Add(tooltipStyles[i]);
            for (int i=0; i<tooltipStylesNoAngle.Length; i++)
                visibleTooltips.Add(tooltipStylesNoAngle[i]);

            return visibleTooltips;
        }
    }
}