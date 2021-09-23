using Impact.Objects;
using Impact.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Triggers
{
    public enum SlideMode
    {
        Normal = 0,
        None = 1
    }

    public enum RollMode
    {
        Normal = 0,
        None = 1
    }

    public abstract class ImpactSlideAndRollTriggerBase<TCollision, TContact> : ImpactTriggerBase<TCollision, TContact>
        where TCollision : IImpactCollisionWrapper<TContact> where TContact : IImpactContactPoint
    {
        [SerializeField]
        private SlideMode _slideMode;
        [SerializeField]
        private RollMode _rollMode;

        public SlideMode SlideMode
        {
            get { return _slideMode; }
            set { _slideMode = value; }
        }

        public RollMode RollMode
        {
            get { return _rollMode; }
            set { _rollMode = value; }
        }

        protected override void buildInteractionData(IImpactObject target, TCollision collision, TContact contactPoint,
            VelocityData myVelocityData, VelocityData otherVelocityData, ImpactTagMask? tagMask, float compositionValue)
        {
            Vector3 relativeContactPointVelocity = myVelocityData.TotalPointVelocity - otherVelocityData.TotalPointVelocity;

            if (SlideMode != SlideMode.None)
            {
                InteractionData slideParameters = new InteractionData()
                {
                    TagMask = tagMask,
                    Point = contactPoint.Point,
                    Normal = contactPoint.Normal,
                    Velocity = relativeContactPointVelocity,
                    InteractionType = InteractionData.InteractionTypeSlide,
                    ThisObject = contactPoint.ThisObject,
                    OtherObject = contactPoint.OtherObject,
                    CompositionValue = compositionValue
                };

                invokeTriggeredEvent(slideParameters, target);

                ImpactManagerInstance.ProcessContinuousInteraction(slideParameters, target);
            }

            if (RollMode != RollMode.None)
            {
                float roll = 1 - Mathf.Clamp01(relativeContactPointVelocity.magnitude * 0.1f);

                if (roll > 0)
                {
                    Vector3 rollVelocity = myVelocityData.TangentialVelocity * roll;

                    InteractionData rollParameters = new InteractionData()
                    {
                        TagMask = tagMask,
                        Point = contactPoint.Point,
                        Normal = contactPoint.Normal,
                        Velocity = rollVelocity,
                        InteractionType = InteractionData.InteractionTypeRoll,
                        ThisObject = contactPoint.ThisObject,
                        OtherObject = contactPoint.OtherObject,
                        CompositionValue = compositionValue
                    };

                    invokeTriggeredEvent(rollParameters, target);

                    ImpactManagerInstance.ProcessContinuousInteraction(rollParameters, target);
                }
            }
        }
    }
}
