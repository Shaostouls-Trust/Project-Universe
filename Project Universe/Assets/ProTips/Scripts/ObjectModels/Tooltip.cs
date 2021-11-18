using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ModelShark
{
    public class Tooltip
    {
        public RectTransform RectTransform { get; set; }
        public TooltipStyle TooltipStyle { get; set; }
        public GameObject GameObject { get; set; }
        public List<TextField> TextFields { get; set; }
        public List<ImageField> ImageFields { get; set; }
        public List<SectionField> SectionFields { get; set; } 
        /// <summary>The tooltip's root parent background image.</summary>
        public Image BackgroundImage { get; set; }
        public CanvasRenderer[] CanvasRenderers { get; set; }
        public Graphic[] Graphics { get; set; }
        public bool StaysOpen { get; set; }
        public bool NeverRotate { get; set; }
        public bool IsBlocking { get; set; }
        public static string Delimiter { get; set; }
        /// <summary>The Tooltip Trigger that was responsible for activating this tooltip.</summary>
        public TooltipTrigger TooltipTrigger { get; set; }

        public void Initialize()
        {
            if (String.IsNullOrEmpty(Delimiter))
                Delimiter = TooltipManager.Instance.TextFieldDelimiter;

            // Get the rect transform.
            RectTransform = GameObject.GetComponent<RectTransform>();

            // Get the game object on the tooltip.
            TooltipStyle = GameObject.GetComponent<TooltipStyle>();

            // Get a reference to the background image.
            BackgroundImage = GameObject.GetComponent<Image>();

            // Get a reference to the canvas renderers.
            CanvasRenderers = GameObject.GetComponentsInChildren<CanvasRenderer>(true);

            // Get a reference to all Graphic elements.
            Graphics = GameObject.GetComponentsInChildren<Graphic>(true);

            // Get Text fields.
            Text[] texts = GameObject.GetComponentsInChildren<Text>(true);
            TextMeshProUGUI[] textsTMP = GameObject.GetComponentsInChildren<TextMeshProUGUI>(true);
            TextFields = new List<TextField>();
            for (int i = 0; i < texts.Length; i++)
            {
                // If the delimiter wasn't found anywhere in the Text field, don't bother adding it.
                if (texts[i].text.Contains(Delimiter))
                    TextFields.Add(new TextField {Text = texts[i], Original = texts[i].text});
            }
            for (int i = 0; i < textsTMP.Length; i++)
            {
                // If the delimiter wasn't found anywhere in the Text field, don't bother adding it.
                if (textsTMP[i].text.Contains(Delimiter))
                    TextFields.Add(new TextField { TextTMP = textsTMP[i], Original = textsTMP[i].text });
            }

            // Get Image fields.
            List<DynamicImage> dynImgs = GameObject.GetComponentsInChildren<DynamicImage>(true).ToList();
            ImageFields = new List<ImageField>();
            for (int i = 0; i < dynImgs.Count; i++)
            {
                Image img = dynImgs[i].GetComponent<Image>();
                ImageFields.Add(new ImageField()
                {
                    Image = img,
                    Name = dynImgs[i].placeholderName.Trim(Delimiter.ToCharArray()),
                    Original = img.sprite
                });
            }

            // Get Dynamic Section fields.
            List<DynamicSection> dynSects = GameObject.GetComponentsInChildren<DynamicSection>(true).ToList();
            SectionFields = new List<SectionField>();
            for (int i = 0; i < dynSects.Count; i++)
            {
                GameObject go = dynSects[i].gameObject;
                SectionFields.Add(new SectionField()
                {
                    GameObject = go,
                    Name = dynSects[i].placeholderName.Trim(Delimiter.ToCharArray()),
                    Original = go.activeSelf
                });
            }
        }

        /// <summary>Activates the tooltip and immediately hides all canvas renderers, so the tooltip doesn't "flash" in before it's positioned and ready.</summary>
        public void WarmUp()
        {
            GameObject.SetActive(true);

            for (int i = 0; i < CanvasRenderers.Length; i++)
                CanvasRenderers[i].SetAlpha(0f);
        }

        public void Deactivate()
        {
            if (TooltipManager.Instance == null) return;
            if (GameObject == null) return;

            ResetParameterizedFields();

            // Remove this as a blocking tooltip if it was one.
            if (TooltipManager.Instance.BlockingTooltip == this)
                TooltipManager.Instance.BlockingTooltip = null;

            // Turn off game object.
            GameObject.SetActive(false);

            // Move the tooltip back under the proper Tooltip Container if it has a different parent.
            GameObject.transform.SetParent(TooltipManager.Instance.TooltipContainer.transform, false);
        }

        public void ResetParameterizedFields()
        {
            if (TooltipManager.Instance == null) return;
            if (GameObject == null) return;

            // Reset parameterized fields back to their original values.
            for (int i = 0; i < TextFields.Count; i++)
            {
                if (TextFields[i].TextTMP != null)
                    TextFields[i].TextTMP.text = TextFields[i].Original;
                else
                TextFields[i].Text.text = TextFields[i].Original;
            }

            for (int i = 0; i < ImageFields.Count; i++)
                ImageFields[i].Image.sprite = ImageFields[i].Original;
        }

        public void Display(float fadeDuration)
        {
            if (fadeDuration > 0f)
            {
                for (int i = 0; i < Graphics.Length; i++)
                    Graphics[i].CrossFadeAlpha(1f, fadeDuration, true);
            }
            else
            {
                for (int i = 0; i < CanvasRenderers.Length; i++)
                    CanvasRenderers[i].SetAlpha(1f);    
            }
        }
    }

    public class ImageField
    {
        /// <summary>A reference to the Image UI component.</summary>
        public Image Image { get; set; }
        /// <summary>The user-friendly name of the dynamic image field (Ex: "BodyImage").</summary>
        public string Name { get; set; }
        /// <summary>The placeholder image to replace, required for resetting the image.</summary>
        public Sprite Original { get; set; }
    }

    public class TextField
    {
        /// <summary>A reference to the Text UI component.</summary>
        public Text Text { get; set; }
        public TextMeshProUGUI TextTMP { get; set; }
        /// <summary>The original text that was in the Text UI component, used for resetting the tooltip.</summary>
        public string Original { get; set; }
    }

    public class SectionField
    {
        /// <summary>A reference to the section's GameObject.</summary>
        public GameObject GameObject { get; set; }
        /// <summary>The user-friendly name of the dynamic section field (Ex: "EquippedItemSection").</summary>
        public string Name { get; set; }
        /// <summary>The original state of the section, as it began in the scene (did it start on or off?). Used for resetting the tooltip.</summary>
        public bool Original { get; set; }
    }
}
