using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace ModelShark
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerDownHandler, IPointerUpHandler
    {
        [HideInInspector]
        public TooltipStyle tooltipStyle;
        [HideInInspector]
        public List<ParameterizedTextField> parameterizedTextFields;
        [HideInInspector]
        public List<DynamicImageField> dynamicImageFields;
        [HideInInspector]
        public List<DynamicSectionField> dynamicSectionFields;
        
        [HideInInspector] 
        public bool isRemotelyActivated; // Is this tooltip activated from another game object? (ie, NOT "hover" activated)
        
        /// <summary>The tooltip gameobject instance in the scene that matches this trigger's tooltip style.</summary>
        public Tooltip Tooltip { get; set; }
        [Tooltip("Controls the color and fade amount of the tooltip background.")]
        public Color backgroundTint = Color.white;

        [HideInInspector, Tooltip("Overrides how the tooltip is positioned. You can choose to have it positioned at a specific Vector point on the screen, have it follow another transform, or have it follow the mouse cursor.")]
        public bool shouldOverridePosition;
        [HideInInspector]
        public PositionOverride overridePositionType;
        [HideInInspector]
        public Vector3 overridePositionVector;
        [HideInInspector]
        public Transform overridePositionTransform;

        [HideInInspector]
        public TipPosition tipPosition;
        public int minTextWidth = 100;
        public int maxTextWidth = 200;

        [HideInInspector, Tooltip("Once open, this tooltip will stay open until the user hovers over another tooltip trigger or something (like a script) manually closes it.")]
        public bool staysOpen;

        [HideInInspector, Tooltip("If true, this tooltip will not be angled/rotated along with other tooltips (see MatchRotationTo on TooltipManager).")]
        public bool neverRotate;

        [HideInInspector, Tooltip("While open, this tooltip will prevent any other tooltips from triggering. Something (like a script) will need to manually close it.")]
        public bool isBlocking;

        // Timer variables - these keep track of how much time has elapsed.
        private float hoverTimer;
        private float popupTimer;
        
        private float tooltipDelay = 0.2f; // How long tooltips delay before showing. This is set on the TooltipManager.
        private float popupTime = 2f; // How long the popup tooltip should remain visible before fading out.
        private bool isInitialized;
        private bool isMouseOver;
        private bool isMouseDown;

        public void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (isInitialized) return;

            if (tooltipStyle != null)
            {
                // Check with the TooltipManager to see if there is already a tooltip object instantiated for this trigger's style.
                // If not, instantiate it and add it to the TooltipManager.
                if (!TooltipManager.Instance.Tooltips.ContainsKey(tooltipStyle))
                {
                    TooltipStyle ttStyle = Instantiate(tooltipStyle);
                    ttStyle.name = tooltipStyle.name;
                    if (TooltipManager.Instance.TooltipContainer != null)
                        ttStyle.transform.SetParent(TooltipManager.Instance.TooltipContainer.transform, false);
                    Tooltip newTooltip = new Tooltip() { GameObject = ttStyle.gameObject};
                    newTooltip.Initialize();
                    newTooltip.Deactivate();
                    TooltipManager.Instance.Tooltips.Add(tooltipStyle, newTooltip);
                }
                Tooltip = TooltipManager.Instance.Tooltips[tooltipStyle];
            }
            isInitialized = true;
        }

        private void Update()
        {
            tooltipDelay = TooltipManager.Instance.tooltipDelay;

            // Hover timer update
            if (hoverTimer > 0)
                hoverTimer += Time.unscaledDeltaTime;

            if (hoverTimer > tooltipDelay)
            {
                // Turn off the hover timer and show the tooltip.
                hoverTimer = 0;
                StartHover();
            }

            // Popup timer update
            if (popupTimer > 0)
                popupTimer += Time.unscaledDeltaTime;

            // If the tooltip exists and is currently open and needs to be repositioneed every frame, do so.
            if (Tooltip.TooltipTrigger != null && Tooltip.GameObject.activeInHierarchy && popupTimer <= popupTime && Tooltip != null && shouldOverridePosition)
                Tooltip.SetPosition(this, TooltipManager.Instance.GuiCanvas, TooltipManager.Instance.guiCamera);

            // Turn off the popup timer and hide the tooltip if we've hovered off of it long enough.
            if (popupTimer > popupTime && Tooltip != null && !Tooltip.StaysOpen) 
            {
                // Stop the timer and prevent the tooltip from showing.
                popupTimer = 0; 

                // Reset the text fields and images for the tooltip, and hide it.
                if (Tooltip != null)
                    Tooltip.Deactivate();
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            // Ignore this event if touch support is on.
            if (TooltipManager.Instance.touchSupport) return;

            // Ignore and exit if this tooltip is remotely activated, if there is already a blocking tooltip on the screen, or if the tooltip is already visible.
            if (isRemotelyActivated || TooltipManager.Instance.BlockingTooltip != null) return;
            if (Tooltip.GameObject.activeInHierarchy && Tooltip.TooltipTrigger == this) return;
           
            hoverTimer = 0.001f; // Start the timer.
        }

        public void OnMouseOver()
        {
            // Ignore if we've already captured the MouseOver event.
            if (isMouseOver) return;
            // Ignore this event if touch support is on.
            if (TooltipManager.Instance.touchSupport) return;
            // Ignore and exit if this tooltip is remotely activated, if there is already a blocking tooltip on the screen, or if the tooltip is already visible.
            if (isRemotelyActivated || TooltipManager.Instance.BlockingTooltip != null) return;
            if (Tooltip.GameObject.activeInHierarchy && Tooltip.TooltipTrigger == this) return;

            // Check if the mouse is hovered over a UI element. If so, exit.
            if (EventSystem.current.IsPointerOverGameObject()) return;

            hoverTimer = 0.001f; // Start the timer.
            isMouseOver = true;
        }

        public void OnMouseDown()
        {
            // Ignore if we've already captured the MouseDown event.
            if (isMouseDown) return;
            // Ignore this event if touch support is off.
            if (!TooltipManager.Instance.touchSupport) return;
            // Ignore and exit if this tooltip is remotely activated, if there is already a blocking tooltip on the screen, or if the tooltip is already visible.
            if (isRemotelyActivated || TooltipManager.Instance.BlockingTooltip != null) return;
            if (Tooltip.GameObject.activeInHierarchy && Tooltip.TooltipTrigger == this) return;

            hoverTimer = 0.001f; // Start the timer.
            isMouseDown = true;
        }

        public void OnMouseExit()
        {
            // Ignore this event if touch support is on.
            if (TooltipManager.Instance.touchSupport) return;
            isMouseOver = false;
            StopHover();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Ignore this event if touch support is off.
            if (!TooltipManager.Instance.touchSupport) return;
            // Ignore and exit if this tooltip is remotely activated, if there is already a blocking tooltip on the screen, or if the tooltip is already visible.
            if (isRemotelyActivated || TooltipManager.Instance.BlockingTooltip != null) return;
            if (Tooltip.GameObject.activeInHierarchy && Tooltip.TooltipTrigger == this) return;

            hoverTimer = 0.001f; // Start the timer.
        }
        
        public void OnSelect(BaseEventData eventData)
        {
            // Ignore this event if touch support is on.
            if (TooltipManager.Instance.touchSupport) return;
            // Ignore and exit if this tooltip is remotely activated, if there is already a blocking tooltip on the screen, or if the tooltip is already visible.
            if (isRemotelyActivated || TooltipManager.Instance.BlockingTooltip != null) return;
            // Ignore if Tooltip is null, or Tooltip.GameObject is null, or Tooltip.TooltipTrigger is null.
            if (Tooltip == null || Tooltip.GameObject == null || Tooltip.TooltipTrigger == null) return;

            if (Tooltip.GameObject.activeInHierarchy && Tooltip.TooltipTrigger == this) return;

            hoverTimer = 0.001f; // Start the timer.
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Ignore this event if touch support is on.
            if (TooltipManager.Instance.touchSupport) return;
            StopHover();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            // Ignore this event if touch support is off.
            if (!TooltipManager.Instance.touchSupport) return;
            StopHover();
        }

        public void OnMouseUp()
        {
            // Ignore this event if touch support is off.
            if (!TooltipManager.Instance.touchSupport) return;
            isMouseDown = false;
            StopHover();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            // Ignore this event if touch support is on.
            if (TooltipManager.Instance != null && TooltipManager.Instance.touchSupport) return;
            StopHover();
        }

        public void StartHover()
        {
            if (!TooltipManager.Instance.tooltipsEnabled) return;

            // Fix if minWidth is greater than maxWidth.
            if (minTextWidth > maxTextWidth)
                maxTextWidth = minTextWidth;

            Tooltip.WarmUp();
            Tooltip.ResetParameterizedFields();
            Tooltip.StaysOpen = staysOpen;
            Tooltip.NeverRotate = neverRotate;
            Tooltip.IsBlocking = isBlocking;
            
            TooltipManager.Instance.SetTextAndSize(this);

            // Show and position the tooltip.
            StartCoroutine(TooltipManager.Instance.Show(this));
        }

        /// <summary>Forces the tooltip to be hidden and deactivated, and also resets all timers so the tooltip won't automatically re-enable (unless it's triggered again).</summary>
        public void ForceHideTooltip()
        {
            // Reset all timers to prevent re-showing the tooltip automatically.
            hoverTimer = popupTimer = 0;

            // Reset the text fields and images for the tooltip, and hide it, regardless of how it was triggered (hover over or remote popup, or through code).
            if (Tooltip != null)
                Tooltip.Deactivate();
        } 

        public void StopHover()
        {
            if (Tooltip == null || Tooltip.GameObject == null) return;
            if (isRemotelyActivated || (Tooltip.StaysOpen && Tooltip.IsBlocking)) return;
            if (Tooltip.StaysOpen && Tooltip.TooltipTrigger == this) return;

            hoverTimer = 0; // Stop the timer and prevent the tooltip from showing.

            // Reset the text fields and images for the tooltip, and hide it.
            if (Tooltip != null)
                Tooltip.Deactivate();
        }

        /// <summary>Manually pop up a tooltip without requiring hovering. This is useful for in-game tutorials or NPC barks.</summary>
        /// <param name="duration">Number of seconds to display the tooltip.</param>
        /// <param name="triggeredBy">The game object that triggered this popup. Allows us to prevent multiple triggering.</param>
        public void Popup(float duration, GameObject triggeredBy)
        {
            if (!TooltipManager.Instance.tooltipsEnabled) return;

            if (popupTimer > 0 || TooltipManager.Instance.BlockingTooltip != null) return;
            
            Initialize();

            // Fix if minWidth is greater than maxWidth.
            if (minTextWidth > maxTextWidth)
                maxTextWidth = minTextWidth;

            Tooltip.WarmUp();
            Tooltip.StaysOpen = staysOpen;
            Tooltip.NeverRotate = neverRotate;
            Tooltip.IsBlocking = isBlocking;
            TooltipManager.Instance.SetTextAndSize(this);

            // Show and position the tooltip.
            StartCoroutine(TooltipManager.Instance.Show(this));
            popupTimer = 0.001f; // start the popup timer.
            popupTime = duration; // set the duration of the popup.
        }

        public void SetText(string parameterName, string text)
        {
            // If the list of parameterized text fields doesn't exist, create it.
            if (parameterizedTextFields == null)
                parameterizedTextFields = new List<ParameterizedTextField>();
            
            // Check to see if we find a matching field. If so, set its text to what was passed in.
            bool fieldExists = false;
            foreach (ParameterizedTextField txt in parameterizedTextFields)
            {
                if (txt.name != parameterName) continue;
                txt.value = text;
                fieldExists = true;
            }

            // Finally, if the text field doesn't exist in the parameterized field list, create it and set its text to what was passed in.
            if (fieldExists) return;
            string delimiter = TooltipManager.Instance.TextFieldDelimiter;
            parameterizedTextFields.Add(new ParameterizedTextField { name=parameterName, placeholder = $"{delimiter}{parameterName}{delimiter}", value = text });
        }

        public void SetImage(string parameterName, Sprite sprite)
        {
            // If the list of dynamic image fields doesn't exist, create it.
            if (dynamicImageFields == null)
                dynamicImageFields = new List<DynamicImageField>();

            // Check to see if we find a matching field. If so, set its sprite to what was passed in.
            bool fieldExists = false;
            foreach (DynamicImageField img in dynamicImageFields)
            {
                if (img.name != parameterName) continue;
                img.replacementSprite = sprite;
                fieldExists = true;
            }

            // Finally, if the image field doesn't exist in the list, create it and set its image to what was passed in.
            if (!fieldExists)
                dynamicImageFields.Add(new DynamicImageField() { name = parameterName, placeholderSprite = null, replacementSprite = sprite });
        }

        public void TurnSectionOn(string parameterName)
        {
            ToggleSection(parameterName, true);
        }

        public void TurnSectionOff(string parameterName)
        {
            ToggleSection(parameterName, false);
        }

        private void ToggleSection(string parameterName, bool isOn)
        {
            // If the list of dynamic section fields doesn't exist, create it.
            if (dynamicSectionFields == null)
                dynamicSectionFields = new List<DynamicSectionField>();

            // Check to see if we find a matching field. If so, set its sprite to what was passed in.
            bool fieldExists = false;
            foreach (DynamicSectionField section in dynamicSectionFields)
            {
                if (section.name != parameterName) continue;
                section.isOn = isOn;
                fieldExists = true;
            }

            // Finally, if the image field doesn't exist in the list, create it and set its image to what was passed in.
            if (!fieldExists)
                dynamicSectionFields.Add(new DynamicSectionField() {name = parameterName, isOn = isOn});
        }
    }
}
