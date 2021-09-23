using Impact.Objects;
using Impact.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Impact.Triggers
{
    public abstract class ImpactSpeculativeCollisionTriggerBase<TCollision, TContact> : ImpactCollisionTriggerBase<TCollision, TContact>
        where TCollision : IImpactCollisionWrapper<TContact> where TContact : IImpactContactPoint
    {
        [Serializable]
        private struct SpeculativeCollisionContact : IEquatable<SpeculativeCollisionContact>
        {
            public int Lifetime;
            public Vector3 RelativeContactPoint;

            public bool IsAlive
            {
                get { return Lifetime > 0; }
            }

            public bool Equals(SpeculativeCollisionContact other)
            {
                return RelativeContactPoint.Equals(other.RelativeContactPoint);
            }
        }

        [SerializeField]
        private int _maxCollisionsPerFrame = 2;
        [SerializeField]
        private float _contactPointComparisonThreshold = 0.001f;
        [SerializeField]
        private int _contactPointLifetime = 2;

        /// <summary>
        /// The maximum number of collisions that can be generated in a single frame.
        /// </summary>
        public int MaxCollisionsPerFrame
        {
            get { return _maxCollisionsPerFrame; }
            set { _maxCollisionsPerFrame = value; }
        }

        /// <summary>
        /// Value used for comparing contact point relative positions.
        /// Contact points will be considered identical if the sqrMagnitude of their difference is less than this value.
        /// </summary>
        public float ContactPointComparisonThreshold
        {
            get { return _contactPointComparisonThreshold; }
            set { _contactPointComparisonThreshold = value; }
        }

        /// <summary>
        /// How many frames should a contact point be alive for before it is removed from the list of active contacts?
        /// Increasing this value can reduce the likelyhood of interactions happening in quick succession for the same contact point.
        /// </summary>
        public int ContactPointLifetime
        {
            get { return _contactPointLifetime; }
            set { _contactPointLifetime = value; }
        }

        private List<SpeculativeCollisionContact> continousCollisionContacts = new List<SpeculativeCollisionContact>();

        private void FixedUpdate()
        {
            for (int i = 0; i < continousCollisionContacts.Count; i++)
            {
                if (!continousCollisionContacts[i].IsAlive)
                {
                    continousCollisionContacts.RemoveAt(i);
                    i--;
                }
                else
                {
                    SpeculativeCollisionContact c = continousCollisionContacts[i];
                    c.Lifetime -= 1;
                    continousCollisionContacts[i] = c;
                }
            }
        }

        protected void processSpeculativeCollision(TCollision collision)
        {
            int collisions = 0;

            //Loop over all contacts of the collision
            for (int i = 0; i < collision.ContactCount; i++)
            {
                TContact contactPoint = collision.GetContact(i);
                IImpactObject thisObject = getImpactObject(contactPoint.ThisObject);

                if (thisObject != null)
                {
                    SpeculativeCollisionContact c = new SpeculativeCollisionContact()
                    {
                        RelativeContactPoint = thisObject.GetContactPointRelativePosition(contactPoint.Point),
                        Lifetime = _contactPointLifetime
                    };

                    //The relative position of the contact point serves as a unique identifier
                    //So we can keep track of contact points between frames
                    //If there is no existing point with the same relative position (within a threshold), then it is a "new" contact point.
                    //We then process the point, basically mimicking an OnCollisionEnter message.
                    int existingIndex = continousCollisionContacts.FindIndex(e => (e.RelativeContactPoint - c.RelativeContactPoint).sqrMagnitude < _contactPointComparisonThreshold);
                    if (existingIndex == -1)
                    {
                        continousCollisionContacts.Add(c);

                        if (collisions < MaxCollisionsPerFrame)
                        {
                            processCollisionContact(collision, contactPoint);
                            collisions++;
                        }
                    }
                    else
                    {
                        SpeculativeCollisionContact existingContact = continousCollisionContacts[existingIndex];
                        existingContact.Lifetime = _contactPointLifetime;
                        continousCollisionContacts[existingIndex] = existingContact;
                    }
                }
            }
        }
    }
}

