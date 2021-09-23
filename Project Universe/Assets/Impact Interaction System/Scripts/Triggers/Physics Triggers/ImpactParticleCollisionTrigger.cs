using Impact.Objects;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Triggers
{
    [AddComponentMenu("Impact/Impact Particle Collision Trigger")]
    public class ImpactParticleCollisionTrigger : ImpactTriggerBase<ImpactCollisionWrapper, ImpactContactPoint>
    {
        [SerializeField]
        private ParticleSystem _particles;

        private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

        private void OnParticleCollision(GameObject other)
        {
            if (!Enabled || (!HighPriority && ImpactManagerInstance.HasReachedPhysicsInteractionsLimit()))
                return;

            ImpactManagerInstance.IncrementPhysicsInteractionsLimit();

            bool isParticleSystem = _particles != null;
            int numCollisionEvents = 0;

            //If this is a particle system, get the collision events from out particles
            if (isParticleSystem)
                numCollisionEvents = _particles.GetCollisionEvents(other, collisionEvents);
            //This is a non-particle system object, get collision events from the other object
            else
            {
                ParticleSystem p = other.GetComponent<ParticleSystem>();
                numCollisionEvents = p.GetCollisionEvents(this.gameObject, collisionEvents);
            }

            //Process collision for each particle collision event
            for (int i = 0; i < numCollisionEvents; i++)
            {
                processParticleCollision(collisionEvents[i], other, isParticleSystem);
            }
        }

        private void processParticleCollision(ParticleCollisionEvent particleCollisionEvent, GameObject onParticleCollisionObject, bool isParticleSystem)
        {
            IImpactObject myObject;

            //Particle system always uses the main target
            if (isParticleSystem)
                myObject = MainTarget;
            //Non-particle system gets the object from the particle collision event's collider
            else
                myObject = getImpactObject(particleCollisionEvent.colliderComponent.gameObject);

            if (myObject != null)
            {
                IImpactObject otherObject = null;

                //Get other object based on whether or not this object is a particle system
                if (isParticleSystem)
                    otherObject = particleCollisionEvent.colliderComponent.GetComponentInParent<IImpactObject>();
                else
                    otherObject = onParticleCollisionObject.GetComponentInParent<IImpactObject>();

                bool hasOtherObject = otherObject != null;

                if (UseMaterialComposition && hasOtherObject)
                {
                    int count = otherObject.GetMaterialCompositionNonAlloc(particleCollisionEvent.intersection, ImpactManagerInstance.MaterialCompositionBuffer);

                    //Velocity data is dependent on if this is a particle system or a not
                    VelocityData myVelocityData;
                    VelocityData otherVelocityData;
                    if (isParticleSystem)
                    {
                        myVelocityData = new VelocityData(particleCollisionEvent.velocity, Vector3.zero);
                        otherVelocityData = otherObject.GetVelocityDataAtPoint(particleCollisionEvent.intersection);
                    }
                    else
                    {
                        myVelocityData = myObject.GetVelocityDataAtPoint(particleCollisionEvent.intersection);
                        otherVelocityData = new VelocityData(particleCollisionEvent.velocity, Vector3.zero);
                    }

                    Vector3 relativeContactPointVelocity = myVelocityData.TotalPointVelocity - otherVelocityData.TotalPointVelocity;

                    for (int i = 0; i < count; i++)
                    {
                        ImpactMaterialComposition comp = ImpactManagerInstance.MaterialCompositionBuffer[i];

                        if (comp.CompositionValue > 0)
                        {
                            InteractionData interactionData = new InteractionData()
                            {
                                TagMask = comp.Material.MaterialTagsMask,
                                Point = particleCollisionEvent.intersection,
                                Normal = particleCollisionEvent.normal,
                                Velocity = relativeContactPointVelocity,
                                InteractionType = InteractionData.InteractionTypeCollision,
                                ThisObject = this.gameObject,
                                OtherObject = otherObject.GameObject,
                                CompositionValue = 1f
                            };

                            invokeTriggeredEvent(interactionData, myObject);

                            ImpactManagerInstance.ProcessInteraction(interactionData, myObject);
                        }
                    }
                }
                else
                {
                    //Velocity data is dependent on if this is a particle system or a not
                    VelocityData myVelocityData;
                    VelocityData otherVelocityData;
                    if (isParticleSystem)
                    {
                        myVelocityData = new VelocityData(particleCollisionEvent.velocity, Vector3.zero);
                        otherVelocityData = hasOtherObject ? otherObject.GetVelocityDataAtPoint(particleCollisionEvent.intersection) : new VelocityData();
                    }
                    else
                    {
                        myVelocityData = myObject.GetVelocityDataAtPoint(particleCollisionEvent.intersection);
                        otherVelocityData = new VelocityData(particleCollisionEvent.velocity, Vector3.zero);
                    }

                    Vector3 relativeContactPointVelocity = myVelocityData.TotalPointVelocity - otherVelocityData.TotalPointVelocity;

                    //Get physics material ID for material mapping, but only if this object is a particle system.
                    //Particles don't have a collider so no need to try and get the physics material if this is not a particle system
                    int otherPhysicsMaterialID = isParticleSystem ? getPhysicsMaterialID(particleCollisionEvent.colliderComponent) : 0;
                    ImpactTagMask? tagMask = getOtherObjectTagMask(otherObject, particleCollisionEvent.intersection, otherPhysicsMaterialID, hasOtherObject);

                    InteractionData interactionData = new InteractionData()
                    {
                        TagMask = tagMask,
                        Point = particleCollisionEvent.intersection,
                        Normal = particleCollisionEvent.normal,
                        Velocity = relativeContactPointVelocity,
                        InteractionType = InteractionData.InteractionTypeCollision,
                        ThisObject = this.gameObject,
                        OtherObject = particleCollisionEvent.colliderComponent.gameObject,
                        CompositionValue = 1f
                    };

                    invokeTriggeredEvent(interactionData, myObject);

                    ImpactManagerInstance.ProcessInteraction(interactionData, myObject);
                }
            }
            else
            {
                Debug.LogError("Unable to find Impact Object on GameObject " + gameObject.name);
            }
        }

        private int getPhysicsMaterialID(Component colliderComponent)
        {
            Collider collider3D = colliderComponent as Collider;

            if (collider3D != null && collider3D.sharedMaterial != null)
            {
                return collider3D.sharedMaterial.GetInstanceID();
            }

            Collider2D collider2D = colliderComponent as Collider2D;

            if (collider2D != null && collider2D.sharedMaterial != null)
            {
                return collider2D.sharedMaterial.GetInstanceID();
            }

            return 0;
        }
    }
}

