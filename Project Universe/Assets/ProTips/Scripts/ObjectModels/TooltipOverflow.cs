namespace ModelShark
{
    /// <summary>This helper class determines which way to flip/position a tooltip that would overflow its bounding rect.</summary>
    public class TooltipOverflow
    {
        public bool IsAny { get { return BottomLeftCorner || TopLeftCorner || TopRightCorner || BottomRightCorner; } }
        public bool TopEdge { get { return TopLeftCorner && TopRightCorner; } }
        public bool RightEdge { get { return TopRightCorner && BottomRightCorner; } }
        public bool LeftEdge { get { return TopLeftCorner && BottomLeftCorner; } }
        public bool BottomEdge { get { return BottomLeftCorner && BottomRightCorner; } }
        public bool TopRightCorner { get; set; }
        public bool TopLeftCorner { get; set; }
        public bool BottomRightCorner { get; set; }
        public bool BottomLeftCorner { get; set; }

        /// <summary>Suggests a better tooltip position, based on where the tooltip is overflowing and the previously-desired position.</summary>
        /// <param name="fromPosition">The previously-desired tip position.</param>
        public TipPosition SuggestNewPosition(TipPosition fromPosition)
        {
            bool useMousePos = fromPosition == TipPosition.MouseBottomLeftCorner || fromPosition == TipPosition.MouseTopLeftCorner || fromPosition == TipPosition.MouseBottomRightCorner || fromPosition == TipPosition.MouseTopRightCorner
                || fromPosition == TipPosition.MouseTopMiddle || fromPosition == TipPosition.MouseLeftMiddle || fromPosition == TipPosition.MouseRightMiddle || fromPosition == TipPosition.MouseBottomMiddle;

            switch (fromPosition) // desired tip position
            {
                case TipPosition.TopRightCorner:
                case TipPosition.MouseTopRightCorner:
                    if (TopEdge && RightEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseBottomLeftCorner : TipPosition.BottomLeftCorner;
                    if (TopEdge) // flip to bottom.
                        return useMousePos ? TipPosition.MouseBottomRightCorner : TipPosition.BottomRightCorner;
                    if (RightEdge) // flip to left.
                        return useMousePos ? TipPosition.MouseTopLeftCorner : TipPosition.TopLeftCorner;
                    break;
                case TipPosition.BottomRightCorner:
                case TipPosition.MouseBottomRightCorner:
                    if (BottomEdge && RightEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseTopLeftCorner : TipPosition.TopLeftCorner;
                    if (BottomEdge) // flip to top.
                        return useMousePos ? TipPosition.MouseTopRightCorner : TipPosition.TopRightCorner;
                    if (RightEdge) // flip to left.
                        return useMousePos ? TipPosition.MouseBottomLeftCorner : TipPosition.BottomLeftCorner;
                    break;
                case TipPosition.TopLeftCorner:
                case TipPosition.MouseTopLeftCorner:
                    if (TopEdge && LeftEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseBottomRightCorner : TipPosition.BottomRightCorner;
                    if (TopEdge) // flip to bottom.
                        return useMousePos ? TipPosition.MouseBottomLeftCorner : TipPosition.BottomLeftCorner;
                    if (LeftEdge) // flip to right.
                        return useMousePos ? TipPosition.MouseTopRightCorner : TipPosition.TopRightCorner;
                    break;
                case TipPosition.BottomLeftCorner:
                case TipPosition.MouseBottomLeftCorner:
                    if (BottomEdge && LeftEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseTopRightCorner : TipPosition.TopRightCorner;
                    if (BottomEdge) // flip to top.
                        return useMousePos ? TipPosition.MouseTopLeftCorner : TipPosition.TopLeftCorner;
                    if (LeftEdge) // flip to right.
                        return useMousePos ? TipPosition.MouseBottomRightCorner : TipPosition.BottomRightCorner;
                    break;
                case TipPosition.TopMiddle:
                case TipPosition.MouseTopMiddle:
                    if (TopEdge && RightEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseBottomLeftCorner : TipPosition.BottomLeftCorner;
                    if (TopEdge && LeftEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseBottomRightCorner : TipPosition.BottomRightCorner;
                    if (TopEdge) // flip to bottom.
                        return useMousePos ? TipPosition.MouseBottomMiddle : TipPosition.BottomMiddle;
                    if (RightEdge) // flip to left.
                        return useMousePos ? TipPosition.MouseLeftMiddle : TipPosition.LeftMiddle;
                    if (LeftEdge) // flip to right.
                        return useMousePos ? TipPosition.MouseRightMiddle : TipPosition.RightMiddle;
                    break;
                case TipPosition.BottomMiddle:
                case TipPosition.MouseBottomMiddle:
                    if (BottomEdge && RightEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseTopLeftCorner : TipPosition.TopLeftCorner;
                    if (BottomEdge && LeftEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseTopRightCorner : TipPosition.TopRightCorner;
                    if (BottomEdge) // flip to top.
                        return useMousePos ? TipPosition.MouseTopMiddle : TipPosition.TopMiddle;
                    if (RightEdge) // flip to left.
                        return useMousePos ? TipPosition.MouseLeftMiddle : TipPosition.LeftMiddle;
                    if (LeftEdge) // flip to right.
                        return useMousePos ? TipPosition.MouseRightMiddle : TipPosition.RightMiddle;
                    break;
                case TipPosition.LeftMiddle:
                case TipPosition.MouseLeftMiddle:
                    if (TopEdge && LeftEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseBottomRightCorner : TipPosition.BottomRightCorner;
                    if (BottomEdge && LeftEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseTopRightCorner : TipPosition.TopRightCorner;
                    if (TopEdge) // flip to bottom.
                        return useMousePos ? TipPosition.MouseBottomMiddle : TipPosition.BottomMiddle;
                    if (BottomEdge) // flip to top.
                        return useMousePos ? TipPosition.MouseTopMiddle : TipPosition.TopMiddle;
                    if (LeftEdge) // flip to right.
                        return useMousePos ? TipPosition.MouseRightMiddle : TipPosition.RightMiddle;
                    break;
                case TipPosition.RightMiddle:
                case TipPosition.MouseRightMiddle:
                    if (TopEdge && RightEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseBottomLeftCorner : TipPosition.BottomLeftCorner;
                    if (BottomEdge && RightEdge) // flip to opposite corner.
                        return useMousePos ? TipPosition.MouseTopLeftCorner : TipPosition.TopLeftCorner;
                    if (TopEdge) // flip to bottom.
                        return useMousePos ? TipPosition.MouseBottomMiddle : TipPosition.BottomMiddle;
                    if (BottomEdge) // flip to top.
                        return useMousePos ? TipPosition.MouseTopMiddle : TipPosition.TopMiddle;
                    if (RightEdge) // flip to left.
                        return useMousePos ? TipPosition.MouseLeftMiddle : TipPosition.LeftMiddle;
                    break;
            }
            return fromPosition;
        }
    }
}
