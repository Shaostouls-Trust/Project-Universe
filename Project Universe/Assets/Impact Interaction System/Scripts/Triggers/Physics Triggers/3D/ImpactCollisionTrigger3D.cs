using UnityEngine;

namespace Impact.Triggers
{
    [AddComponentMenu("Impact/3D Collision Triggers/Impact Collision Trigger 3D", 0)]
    public class ImpactCollisionTrigger3D : ImpactCollisionTriggerBase<ImpactCollisionWrapper, ImpactContactPoint>
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (!Enabled || (!HighPriority && ImpactManagerInstance.HasReachedPhysicsInteractionsLimit()))
                return;

            ImpactManagerInstance.IncrementPhysicsInteractionsLimit();

            ImpactCollisionWrapper c = new ImpactCollisionWrapper(collision);
            processCollision(c);
        }
    }
}