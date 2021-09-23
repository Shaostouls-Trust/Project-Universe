using UnityEngine;

namespace Impact.Triggers
{
    [AddComponentMenu("Impact/2D Collision Triggers/Impact Speculative Collision Trigger 2D", 0)]
    public class ImpactSpeculativeCollisionTrigger2D : ImpactSpeculativeCollisionTriggerBase<ImpactCollisionWrapper, ImpactContactPoint>
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!Enabled || (!HighPriority && ImpactManagerInstance.HasReachedPhysicsInteractionsLimit()))
                return;

            ImpactManagerInstance.IncrementPhysicsInteractionsLimit();

            ImpactCollisionWrapper c = new ImpactCollisionWrapper(collision);
            processSpeculativeCollision(c);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            if (!Enabled || (!HighPriority && ImpactManagerInstance.HasReachedPhysicsInteractionsLimit()))
                return;

            ImpactManagerInstance.IncrementPhysicsInteractionsLimit();

            ImpactCollisionWrapper c = new ImpactCollisionWrapper(collision);
            processSpeculativeCollision(c);
        }
    }
}
