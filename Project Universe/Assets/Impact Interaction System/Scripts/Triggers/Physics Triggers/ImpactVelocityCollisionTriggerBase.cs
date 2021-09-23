using Impact.Objects;
using Impact.Utility;
using UnityEngine;

namespace Impact.Triggers
{
    public abstract class ImpactVelocityCollisionTriggerBase : ImpactTriggerBase<ImpactCollisionWrapper, ImpactContactPoint>
    {
        [SerializeField]
        private float _velocityChangeInfluence = 1;

        private ImpactRigidbodyWrapper rigidbodyWrapper;

        private void Awake()
        {
            rigidbodyWrapper = new ImpactRigidbodyWrapper(this.gameObject);
        }

        protected override void buildInteractionData(IImpactObject target, ImpactCollisionWrapper collision, ImpactContactPoint contactPoint,
            VelocityData myVelocityData, VelocityData otherVelocityData, ImpactTagMask? tagMask, float CompositionValue)
        {
            VelocityData currentVelocityData = rigidbodyWrapper.GetCurrentVelocityData(contactPoint.Point);

            Vector3 velocityChange = myVelocityData.TotalPointVelocity - currentVelocityData.TotalPointVelocity;
            Vector3 relativeContactPointVelocity = myVelocityData.TotalPointVelocity - otherVelocityData.TotalPointVelocity;

            InteractionData interactionData = new InteractionData()
            {
                TagMask = tagMask,
                Point = contactPoint.Point,
                Normal = contactPoint.Normal,
                Velocity = Vector3.Lerp(relativeContactPointVelocity, velocityChange, _velocityChangeInfluence),
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

