using UnityEngine;

namespace Impact.Triggers
{
    [AddComponentMenu("Impact/2D Collision Triggers/Impact Slide and Roll Trigger 2D", 0)]
    public class ImpactSlideAndRollTrigger2D : ImpactSlideAndRollTriggerBase<ImpactCollisionWrapper, ImpactContactPoint>
    {
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!Enabled || (!HighPriority && ImpactManagerInstance.HasReachedPhysicsInteractionsLimit()))
                return;

            ImpactManagerInstance.IncrementPhysicsInteractionsLimit();

            ImpactCollisionWrapper c = new ImpactCollisionWrapper(collision);
            processCollision(c);
        }
    }
}
