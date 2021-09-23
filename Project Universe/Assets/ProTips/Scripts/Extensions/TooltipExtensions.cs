using UnityEngine;
using UnityEngine.UI;

namespace ModelShark
{
    public static class TooltipExtensions
    {
        /// <summary>Sets the position of the tooltip container.</summary>
        public static void SetPosition(this Tooltip tooltip, TooltipTrigger trigger, Canvas canvas, Camera camera)
        {
            Vector3[] triggerCorners = new Vector3[4];
            RectTransform triggerRectTrans = trigger.gameObject.GetComponent<RectTransform>();
            if (trigger.tipPosition == TipPosition.CanvasTopMiddle || trigger.tipPosition == TipPosition.CanvasBottomMiddle)
                triggerRectTrans = canvas.gameObject.GetComponent<RectTransform>();
            if (triggerRectTrans != null)
                triggerRectTrans.GetWorldCorners(triggerCorners);
            else
            {
                // We're not using a trigger from a Canvas, so that means it's a regular world space game object.
                Vector3 center = Vector3.zero;
                Vector3 extents = Vector3.zero;

                // If TooltipManger is set to PositionBounds.Collider, find the collider bounds of the object and use that for the four corners.
                if (TooltipManager.Instance.positionBounds == PositionBounds.Collider)
                {
                    Collider coll = trigger.GetComponent<Collider>();
                    center = coll.bounds.center;
                    extents = coll.bounds.extents;
                }
                else // TooltipManager is set to PositionBounds.Renderer, so find the renderer bounds of the object and use that for the four corners.
                {
                    Renderer rend = trigger.GetComponent<Renderer>();
                    center = rend.bounds.center;
                    extents = rend.bounds.extents;
                }

                Vector3 frontBottomLeftCorner = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
                Vector3 frontTopLeftCorner = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
                Vector3 frontTopRightCorner = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
                Vector3 frontBottomRightCorner = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);

                if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    triggerCorners[0] = frontBottomLeftCorner;
                    triggerCorners[1] = frontTopLeftCorner;
                    triggerCorners[2] = frontTopRightCorner;
                    triggerCorners[3] = frontBottomRightCorner;
                }
                else
                {
                    triggerCorners[0] = camera.WorldToScreenPoint(frontBottomLeftCorner);
                    triggerCorners[1] = camera.WorldToScreenPoint(frontTopLeftCorner);
                    triggerCorners[2] = camera.WorldToScreenPoint(frontTopRightCorner);
                    triggerCorners[3] = camera.WorldToScreenPoint(frontBottomRightCorner);
                }
            }

            // Set the initial tooltip position.
            tooltip.SetPosition(trigger.tipPosition, trigger.tooltipStyle, triggerCorners, tooltip.BackgroundImage, tooltip.RectTransform, canvas, camera);

            // If overflow protection is disabled, exit.
            if (!TooltipManager.Instance.overflowProtection) return;

            // Check for overflow.
            Vector3[] tooltipCorners = new Vector3[4];
            tooltip.RectTransform.GetWorldCorners(tooltipCorners);

            if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
            {
                for (int i = 0; i < tooltipCorners.Length; i++)
                    tooltipCorners[i] = RectTransformUtility.WorldToScreenPoint(camera, tooltipCorners[i]);
            }
            else if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                for (int i = 0; i < tooltipCorners.Length; i++)
                    tooltipCorners[i] = RectTransformUtility.WorldToScreenPoint(null, tooltipCorners[i]);
            }

            Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
            TooltipOverflow overflow = new TooltipOverflow
            {
                BottomLeftCorner = !screenRect.Contains(tooltipCorners[0]),
                // is the tooltip out of bounds on the Bottom Left?
                TopLeftCorner = !screenRect.Contains(tooltipCorners[1]),
                // is the tooltip out of bounds on the Top Left?
                TopRightCorner = !screenRect.Contains(tooltipCorners[2]),
                // is the tooltip out of bounds on the Top Right?
                BottomRightCorner = !screenRect.Contains(tooltipCorners[3])
                // is the tooltip out of bounds on the Bottom Right?
            };

            // If the tooltip overflows its boundary rectange, reposition it.
            if (overflow.IsAny)
                tooltip.SetPosition(overflow.SuggestNewPosition(trigger.tipPosition), trigger.tooltipStyle,
                    triggerCorners, tooltip.BackgroundImage, tooltip.RectTransform, canvas, camera);
        }

        /// <summary>This method does the heavy lifting for setting the correct tooltip position.</summary>
        private static void SetPosition(this Tooltip tooltip, TipPosition tipPosition, TooltipStyle style, Vector3[] triggerCorners, Image bkgImage, RectTransform tooltipRectTrans, Canvas canvas, Camera camera)
        {
            // Tooltip Trigger Corners:
            // 0 = bottom left
            // 1 = top left
            // 2 = top right
            // 3 = bottom right

            Vector3 pos = Vector3.zero;
            Vector2 offsetVector = Vector2.zero;
            bool useMousePos = tipPosition == TipPosition.MouseBottomLeftCorner || tipPosition == TipPosition.MouseTopLeftCorner || tipPosition == TipPosition.MouseBottomRightCorner || tipPosition == TipPosition.MouseTopRightCorner
                || tipPosition == TipPosition.MouseTopMiddle || tipPosition == TipPosition.MouseLeftMiddle || tipPosition == TipPosition.MouseRightMiddle || tipPosition == TipPosition.MouseBottomMiddle;
            Vector3 mousePos = Input.mousePosition;
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                mousePos.z = canvas.planeDistance;
                mousePos = camera.ScreenToWorldPoint(mousePos);
            }

            switch (tipPosition)
            {
                case TipPosition.TopRightCorner:
                case TipPosition.MouseTopRightCorner:
                    offsetVector = new Vector2(-1 * style.tipOffset, -1 * style.tipOffset);
                    pos = useMousePos ? mousePos : triggerCorners[2];
                    tooltipRectTrans.pivot = new Vector2(0, 0);
                    bkgImage.sprite = style.bottomLeftCorner;
                    break;
                case TipPosition.BottomRightCorner:
                case TipPosition.MouseBottomRightCorner:
                    offsetVector = new Vector2(-1 * style.tipOffset, style.tipOffset);
                    pos = useMousePos ? mousePos : triggerCorners[3];
                    tooltipRectTrans.pivot = new Vector2(0, 1);
                    bkgImage.sprite = style.topLeftCorner;
                    break;
                case TipPosition.TopLeftCorner:
                case TipPosition.MouseTopLeftCorner:
                    offsetVector = new Vector2(style.tipOffset, -1 * style.tipOffset);
                    pos = useMousePos ? mousePos : triggerCorners[1];
                    tooltipRectTrans.pivot = new Vector2(1, 0);
                    bkgImage.sprite = style.bottomRightCorner;
                    break;
                case TipPosition.BottomLeftCorner:
                case TipPosition.MouseBottomLeftCorner:
                    offsetVector = new Vector2(style.tipOffset, style.tipOffset);
                    pos = useMousePos ? mousePos : triggerCorners[0];
                    tooltipRectTrans.pivot = new Vector2(1, 1);
                    bkgImage.sprite = style.topRightCorner;
                    break;
                case TipPosition.TopMiddle:
                case TipPosition.MouseTopMiddle:
                    offsetVector = new Vector2(0, -1 * style.tipOffset);
                    pos = useMousePos ? mousePos : triggerCorners[1] + (triggerCorners[2] - triggerCorners[1]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 0);
                    bkgImage.sprite = style.topMiddle;
                    break;
                case TipPosition.BottomMiddle:
                case TipPosition.MouseBottomMiddle:
                    offsetVector = new Vector2(0, style.tipOffset);
                    pos = useMousePos ? mousePos : triggerCorners[0] + (triggerCorners[3] - triggerCorners[0]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 1);
                    bkgImage.sprite = style.bottomMiddle;
                    break;
                case TipPosition.LeftMiddle:
                case TipPosition.MouseLeftMiddle:
                    offsetVector = new Vector2(style.tipOffset, 0);
                    pos = useMousePos ? mousePos : triggerCorners[0] + (triggerCorners[1] - triggerCorners[0]) / 2;
                    tooltipRectTrans.pivot = new Vector2(1, .5f);
                    bkgImage.sprite = style.leftMiddle;
                    break;
                case TipPosition.RightMiddle:
                case TipPosition.MouseRightMiddle:
                    offsetVector = new Vector2(-1 * style.tipOffset, 0);
                    pos = useMousePos ? mousePos : triggerCorners[3] + (triggerCorners[2] - triggerCorners[3]) / 2;
                    tooltipRectTrans.pivot = new Vector2(0, .5f);
                    bkgImage.sprite = style.rightMiddle;
                    break;
                case TipPosition.CanvasTopMiddle:
                    offsetVector = new Vector2(0, -1 * style.tipOffset);
                    pos = triggerCorners[1] + (triggerCorners[2] - triggerCorners[1]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 1);
                    tooltipRectTrans.anchorMin = tooltipRectTrans.anchorMax = new Vector2(.5f, 1);
                    bkgImage.sprite = style.topMiddle;
                    break;
                case TipPosition.CanvasBottomMiddle:
                    offsetVector = new Vector2(0, style.tipOffset);
                    pos = triggerCorners[0] + (triggerCorners[3] - triggerCorners[0]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 0);
                    tooltipRectTrans.anchorMin = tooltipRectTrans.anchorMax = new Vector2(.5f, 0);
                    bkgImage.sprite = style.bottomMiddle;
                    break;
            }
            
            tooltip.GameObject.transform.position = pos;
            tooltipRectTrans.anchoredPosition += offsetVector;
        }
    }
}
