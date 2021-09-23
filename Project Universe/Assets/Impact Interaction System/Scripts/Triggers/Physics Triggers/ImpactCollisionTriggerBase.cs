using Impact.Objects;
using UnityEngine;

namespace Impact.Triggers
{
    public abstract class ImpactCollisionTriggerBase<TCollision, TContact> : ImpactTriggerBase<TCollision, TContact>
        where TCollision : IImpactCollisionWrapper<TContact> where TContact : IImpactContactPoint
    {
        protected override void buildInteractionData(IImpactObject target, TCollision collision, TContact contactPoint,
            VelocityData myVelocityData, VelocityData otherVelocityData, ImpactTagMask? tagMask, float CompositionValue)
        {
            Vector3 relativeContactPointVelocity = myVelocityData.TotalPointVelocity - otherVelocityData.TotalPointVelocity;

            InteractionData interactionData = new InteractionData()
            {
                TagMask = tagMask,
                Point = contactPoint.Point,
                Normal = contactPoint.Normal,
                Velocity = relativeContactPointVelocity,
                InteractionType = InteractionData.InteractionTypeCollision,
                ThisObject = contactPoint.ThisObject,
                OtherObject = contactPoint.OtherObject,
                CompositionValue = CompositionValue
            };

            invokeTriggeredEvent(interactionData, target);

            ImpactManagerInstance.ProcessInteraction(interactionData, target);
        }
    }
}