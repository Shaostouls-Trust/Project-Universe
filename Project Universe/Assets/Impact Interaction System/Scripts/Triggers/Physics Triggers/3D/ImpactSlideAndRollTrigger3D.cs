using System.Diagnostics;
using UnityEngine;

namespace Impact.Triggers
{
    [AddComponentMenu("Impact/3D Collision Triggers/Impact Slide and Roll Trigger 3D", 0)]
    public class ImpactSlideAndRollTrigger3D : ImpactSlideAndRollTriggerBase<ImpactCollisionWrapper, ImpactContactPoint>
    {
        private void OnCollisionStay(Collision collision)
        {
            if (!Enabled || (!HighPriority && ImpactManagerInstance.HasReachedPhysicsInteractionsLimit()))
                return;

            ImpactManagerInstance.IncrementPhysicsInteractionsLimit();

            ImpactCollisionWrapper c = new ImpactCollisionWrapper(collision);
            processCollision(c);
        }
    }
}
