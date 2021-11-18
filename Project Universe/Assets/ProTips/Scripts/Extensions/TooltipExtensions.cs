using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ModelShark
{
    public static class TooltipExtensions
    {
        /// <summary>Sets the position of the tooltip.</summary>
        public static void SetPosition(this Tooltip tooltip, TooltipTrigger trigger, Canvas canvas, Camera camera)
        {
            Vector3[] triggerCorners = new Vector3[4];
            RectTransform triggerRectTrans = trigger.gameObject.GetComponent<RectTransform>();

            // If the Tooltip Position is set to a position on the Canvas, use the Canvas as the trigger rect transform.
            if (trigger.tipPosition == TipPosition.CanvasTopMiddle || trigger.tipPosition == TipPosition.CanvasBottomMiddle)
                triggerRectTrans = canvas.gameObject.GetComponent<RectTransform>();

            // If there's a RectTransform on the tooltip trigger, we are using a Canvas trigger, and need to get the world corners of the RectTransform.
            if (triggerRectTrans != null)
                triggerRectTrans.GetWorldCorners(triggerCorners);
            else // We're not using a trigger from a Canvas, so that means it's a world space game object and we need to get the corners from the collider or mesh bounds.
                triggerCorners = GetWorldObjectTriggerCorners(trigger.transform, canvas, camera);

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
            bool useMousePos;

            // If we're overriding the tooltip position to a different Transform (RectTransform or WorldObject position), use that transform's corners as the tooltip position corners.
            if (tooltip.TooltipTrigger != null && tooltip.TooltipTrigger.shouldOverridePosition && tooltip.TooltipTrigger.overridePositionType == PositionOverride.Transform)
            {
                RectTransform posRectTrans = tooltip.TooltipTrigger.overridePositionTransform.gameObject.GetComponent<RectTransform>();
                if (posRectTrans != null)
                    posRectTrans.GetWorldCorners(triggerCorners);
                else // We're not using a trigger from a Canvas, so that means it's a world space game object and we need to get the corners from the collider or mesh bounds.
                    triggerCorners = GetWorldObjectTriggerCorners(tooltip.TooltipTrigger.overridePositionTransform, canvas, camera);
            }

            if (tooltip.TooltipTrigger != null && tooltip.TooltipTrigger.shouldOverridePosition && tooltip.TooltipTrigger.overridePositionType == PositionOverride.Vector)
                triggerCorners[0] = triggerCorners[1] = triggerCorners[2] = triggerCorners[3] = tooltip.TooltipTrigger.overridePositionVector;

            // Are we using the mouse position?
            useMousePos = tipPosition == TipPosition.MouseBottomLeftCorner || tipPosition == TipPosition.MouseTopLeftCorner || tipPosition == TipPosition.MouseBottomRightCorner || tipPosition == TipPosition.MouseTopRightCorner
                          || tipPosition == TipPosition.MouseTopMiddle || tipPosition == TipPosition.MouseLeftMiddle || tipPosition == TipPosition.MouseRightMiddle || tipPosition == TipPosition.MouseBottomMiddle;
            Vector3 mousePos = GetMousePosition(canvas, camera);

            switch (tipPosition)
            {
                case TipPosition.TopRightCorner:
                case TipPosition.MouseTopRightCorner:
                    offsetVector = new Vector2(-1 * style.tipOffset, -1 * style.tipOffset);
                    pos = triggerCorners[2];
                    tooltipRectTrans.pivot = new Vector2(0, 0);
                    if (style.bottomLeftCorner != null)
                        bkgImage.sprite = style.bottomLeftCorner;
                    break;
                case TipPosition.BottomRightCorner:
                case TipPosition.MouseBottomRightCorner:
                    offsetVector = new Vector2(-1 * style.tipOffset, style.tipOffset);
                    pos = triggerCorners[3];
                    tooltipRectTrans.pivot = new Vector2(0, 1);
                    if (style.topLeftCorner != null)
                        bkgImage.sprite = style.topLeftCorner;
                    break;
                case TipPosition.TopLeftCorner:
                case TipPosition.MouseTopLeftCorner:
                    offsetVector = new Vector2(style.tipOffset, -1 * style.tipOffset);
                    pos = triggerCorners[1];
                    tooltipRectTrans.pivot = new Vector2(1, 0);
                    if (style.bottomRightCorner != null)
                        bkgImage.sprite = style.bottomRightCorner;
                    break;
                case TipPosition.BottomLeftCorner:
                case TipPosition.MouseBottomLeftCorner:
                    offsetVector = new Vector2(style.tipOffset, style.tipOffset);
                    pos = triggerCorners[0];
                    tooltipRectTrans.pivot = new Vector2(1, 1);
                    if (style.topRightCorner != null)
                        bkgImage.sprite = style.topRightCorner;
                    break;
                case TipPosition.TopMiddle:
                case TipPosition.MouseTopMiddle:
                    offsetVector = new Vector2(0, -1 * style.tipOffset);
                    pos = triggerCorners[1] + (triggerCorners[2] - triggerCorners[1]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 0);
                    if (style.topMiddle != null)
                        bkgImage.sprite = style.topMiddle;
                    break;
                case TipPosition.BottomMiddle:
                case TipPosition.MouseBottomMiddle:
                    offsetVector = new Vector2(0, style.tipOffset);
                    pos = triggerCorners[0] + (triggerCorners[3] - triggerCorners[0]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 1);
                    if (style.bottomMiddle != null)
                        bkgImage.sprite = style.bottomMiddle;
                    break;
                case TipPosition.LeftMiddle:
                case TipPosition.MouseLeftMiddle:
                    offsetVector = new Vector2(style.tipOffset, 0);
                    pos = triggerCorners[0] + (triggerCorners[1] - triggerCorners[0]) / 2;
                    tooltipRectTrans.pivot = new Vector2(1, .5f);
                    if (style.leftMiddle != null)
                        bkgImage.sprite = style.leftMiddle;
                    break;
                case TipPosition.RightMiddle:
                case TipPosition.MouseRightMiddle:
                    offsetVector = new Vector2(-1 * style.tipOffset, 0);
                    pos = triggerCorners[3] + (triggerCorners[2] - triggerCorners[3]) / 2;
                    tooltipRectTrans.pivot = new Vector2(0, .5f);
                    if (style.rightMiddle != null)
                        bkgImage.sprite = style.rightMiddle;
                    break;
                case TipPosition.CanvasTopMiddle:
                    offsetVector = new Vector2(0, -1 * style.tipOffset);
                    pos = triggerCorners[1] + (triggerCorners[2] - triggerCorners[1]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 1);
                    tooltipRectTrans.anchorMin = tooltipRectTrans.anchorMax = new Vector2(.5f, 1);
                    if (style.topMiddle != null)
                        bkgImage.sprite = style.topMiddle;
                    break;
                case TipPosition.CanvasBottomMiddle:
                    offsetVector = new Vector2(0, style.tipOffset);
                    pos = triggerCorners[0] + (triggerCorners[3] - triggerCorners[0]) / 2;
                    tooltipRectTrans.pivot = new Vector2(.5f, 0);
                    tooltipRectTrans.anchorMin = tooltipRectTrans.anchorMax = new Vector2(.5f, 0);
                    if (style.bottomMiddle != null)
                        bkgImage.sprite = style.bottomMiddle;
                    break;
            }

            pos = useMousePos ? mousePos : pos;

            if (tooltip.TooltipTrigger != null && tooltip.TooltipTrigger.shouldOverridePosition && tooltip.TooltipTrigger.overridePositionType == PositionOverride.MouseCursor) 
                pos = GetMousePosition(canvas, camera);
            
            tooltip.GameObject.transform.position = pos;
            tooltipRectTrans.anchoredPosition += offsetVector;
        }

        //TODO: Need to test this with the new input system
        private static Vector3 GetMousePosition(Canvas canvas, Camera camera)
        {
            Vector3 mousePos;
#if !ENABLE_INPUT_SYSTEM
            mousePos = Input.mousePosition;
#else
            mousePos = Mouse.current.position.ReadValue();
#endif
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                mousePos.z = canvas.planeDistance;
                mousePos = camera.ScreenToWorldPoint(mousePos);
            }

            return mousePos;
        }

        private static bool GetExtentsFromCollider(this Transform trans, ref Vector3 center, ref Vector3 extents)
        {
            Collider coll = trans.GetComponentInChildren<Collider>();
            Collider2D coll2D = trans.GetComponentInChildren<Collider2D>();

            if (coll != null)
            {
                center = coll.bounds.center;
                extents = coll.bounds.extents;
                return true;
            }
            
            if (coll2D != null)
            {
                center = coll2D.bounds.center;
                extents = coll2D.bounds.extents;
                return true;
            }

            return false;
        }

        private static bool GetExtentsFromRenderer(this Transform trans, ref Vector3 center, ref Vector3 extents)
        {
            Renderer rend = trans.GetComponentInChildren<Renderer>();
            SkinnedMeshRenderer skinRend = trans.GetComponentInChildren<SkinnedMeshRenderer>();
            if (rend != null)
            {
                center = rend.bounds.center;
                extents = rend.bounds.extents;
                return true;
            }
            
            if (skinRend != null)
            {
                Transform rootBone = skinRend.rootBone;
                if (rootBone != null)
                {
                    center = rootBone.position + skinRend.bounds.center;
                    extents = rootBone.position + skinRend.bounds.extents;
                }
                else
                {
                    center = skinRend.bounds.center;
                    extents = skinRend.bounds.extents;
                }

                return true;
            }

            return false;
        }

        /// <summary>Gets the 4 corners of a world space trigger object. (For instance, if you're using a cube 3D object as the tooltip trigger.)</summary>
        private static Vector3[] GetWorldObjectTriggerCorners(this Transform trans, Canvas canvas, Camera camera)
        {
            Vector3[] triggerCorners = new Vector3[4];
            Vector3 center = Vector3.zero;
            Vector3 extents = Vector3.zero;
            bool foundExtents;

            // If TooltipManger is set to PositionBounds.Collider, try first to find the collider bounds of the object and use that for the four corners.
            if (TooltipManager.Instance.positionBounds == PositionBounds.Collider)
            {
                foundExtents = trans.GetExtentsFromCollider(ref center, ref extents);
                if (!foundExtents) // if not found, try to find a Renderer
                    foundExtents = trans.GetExtentsFromRenderer(ref center, ref extents);
            }
            else // TooltipManager is set to PositionBounds.Renderer, so try first to find the renderer bounds of the object and use that for the four corners.
            {
                foundExtents = trans.GetExtentsFromRenderer(ref center, ref extents);
                if (!foundExtents) // if not found, try to find a Collider
                    foundExtents = trans.GetExtentsFromCollider(ref center, ref extents);
            }

            // If we didn't find any collider or renderer extents, just use the transform's position for all of the corners.
            // Otherwise, use the extents for the tooltip corners.
            Vector3 frontBottomLeftCorner = trans.position;
            Vector3 frontTopLeftCorner = trans.position;
            Vector3 frontTopRightCorner = trans.position;
            Vector3 frontBottomRightCorner = trans.position;
            if (foundExtents)
            {
                frontBottomLeftCorner = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
                frontTopLeftCorner = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
                frontTopRightCorner = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
                frontBottomRightCorner = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
            }

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

            return triggerCorners;
        }
    }
}
