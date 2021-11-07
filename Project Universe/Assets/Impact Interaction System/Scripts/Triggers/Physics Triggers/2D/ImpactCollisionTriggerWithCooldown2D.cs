using UnityEngine;

namespace Impact.Triggers
{
    [AddComponentMenu("Impact/2D Collision Triggers/Impact Collision Trigger With Cooldown 2D", 0)]
    public class ImpactCollisionTriggerWithCooldown2D : ImpactCollisionTriggerBase<ImpactCollisionWrapper, ImpactContactPoint>
    {
        [SerializeField]
        private float cooldown;

        private float lastCollisionTime;

        private void OnCollisionEnter2D(Collision2D collision)
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