using UnityEngine;

namespace Impact.Triggers
{
    [AddComponentMenu("Impact/3D Collision Triggers/Impact Collision Trigger With Cooldown 3D", 0)]
    public class ImpactCollisionTriggerWithCooldown3D : ImpactCollisionTriggerBase<ImpactCollisionWrapper, ImpactContactPoint>
    {
        [SerializeField]
        private float cooldown;

        private float lastCollisionTime;

        private void OnCollisionEnter(Collision collision)
        {
            float currentTime = Time.time;

            if (!Enabled || (currentTime - lastCollisionTime < cooldown) || (!HighPriority && ImpactManagerInstance.HasReachedPhysicsInteractionsLimit()))
                return;

            lastCollisionTime = currentTime;
            ImpactManagerInstance.IncrementPhysicsInteractionsLimit();

            ImpactCollisionWrapper c = new ImpactCollisionWrapper(collision);
            processCollision(c);
        }
    }
}