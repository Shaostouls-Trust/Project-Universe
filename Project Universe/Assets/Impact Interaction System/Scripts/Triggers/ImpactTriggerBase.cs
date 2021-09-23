using Impact.Materials;
using Impact.Objects;
using System;
using UnityEngine;

namespace Impact.Triggers
{
    /// <summary>
    /// A base class for implementing Impact Triggers.
    /// It is not necessary to inherit from this class when making your own trigger, but it contains methods and properties you might find useful.
    /// <typeparam name="TCollision">The IImpactCollisionWrapper implementation being used.</typeparam>
    /// <typeparam name="TContact">The IImpactContactPoint implementation being used.</typeparam>
    /// </summary>
    public abstract class ImpactTriggerBase<TCollision, TContact> : MonoBehaviour where TCollision : IImpactCollisionWrapper<TContact> where TContact : IImpactContactPoint
    {
        /// <summary>
        /// Invoked when triggered. Sends the Interaction Data and the Impact Object that was triggered.
        /// </summary>
        public event Action<InteractionData, IImpactObject> OnTriggered;

        [SerializeField]
        private bool _enabled = true;
        [SerializeField]
        private ImpactObjectBase _target;
        [SerializeField]
        private bool _useMaterialComposition;
        [SerializeField]
        private ImpactTriggerContactMode _contactMode;
        [SerializeField]
        private bool _highPriority;

        /// <summary>
        /// Should this trigger process any collisions? 
        /// You should use this instead of the normal enabled property because collision messages are still sent to disabled components.
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        /// <summary>
        /// The ImpactObjectBase this trigger will use for interaction calculations.
        /// </summary>
        public ImpactObjectBase MainTarget
        {
            get { return _target; }
            set
            {
                _target = value;
                hasMainTarget = _target != null;
            }
        }

        /// <summary>
        /// Should this trigger use the material composition of the objects it hits?
        /// If true, interaction data will be sent for each material at the interaction point.
        /// If false, interaction data will only be sent for the primary material at the interaction point. 
        /// </summary>
        public bool UseMaterialComposition
        {
            get { return _useMaterialComposition; }
            set { _useMaterialComposition = value; }
        }

        /// <summary>
        /// How collision contacts should be handled.
        /// </summary>
        public ImpactTriggerContactMode ContactMode
        {
            get { return _contactMode; }
            set { _contactMode = value; }
        }

        /// <summary>
        /// Should this trigger ignore the Physics Interactions Limit set in the Impact Manager?
        /// </summary>
        public bool HighPriority
        {
            get { return _highPriority; }
            set { _highPriority = value; }
        }

        private bool hasMainTarget;

        private void Awake()
        {
            hasMainTarget = MainTarget != null;
        }

        private void Reset()
        {
            //Automatically find Impact Object on current game object.
            MainTarget = GetComponent<ImpactObjectBase>();
        }

        /// <summary>
        /// Attempts to retrieve an IImpactObject to send interaction data to.
        /// If MainTarget is set, MainTarget will be returned.
        /// Otherwise it will attempt to get the IImpactObject component from the given game object or one of its parents.
        /// </summary>
        /// <param name="collider">The game object to use if MainTarget is null.</param>
        /// <returns>The IImpactObject to send interaction data to, if one can be found.</returns>
        protected IImpactObject getImpactObject(GameObject collider)
        {
            if (hasMainTarget)
                return MainTarget;
            else
                return collider.GetComponentInParent<IImpactObject>();
        }

        /// <summary>
        /// Takes the given IImpactCollisionWrapper and processes it based on the trigger settings.
        /// Override buildAndSendCollisionParameters to receive the built data and convert it to IInteractionData to send to an IImpactObject.
        /// </summary>
        /// <param name="collision">The collision to process.</param>
        protected void processCollision(TCollision collision)
        {
            if (ContactMode == ImpactTriggerContactMode.Single)
                processCollisionSingleContact(collision);
            else if (ContactMode == ImpactTriggerContactMode.SingleAverage)
                processCollisionSingleAveragedContact(collision);
            else if (ContactMode == ImpactTriggerContactMode.Multiple)
                processCollisionMultipleContacts(collision);
        }

        /// <summary>
        /// Takes the given IImpactCollisionWrapper and processes it for a single contact only (the first contact).
        /// Override buildAndSendCollisionParameters to receive the data and convert it to IInteractionData to send to an IImpactObject.
        /// </summary>
        /// <param name="collision">The collision to process.</param>
        protected void processCollisionSingleContact(TCollision collision)
        {
            processCollisionContact(collision, collision.GetContact(0));
        }

        /// <summary>
        /// Takes the given IImpactCollisionWrapper and processes it for a single averaged contact.
        /// Override buildAndSendCollisionParameters to receive the data and convert it to IInteractionData to send to an IImpactObject.
        /// </summary>
        /// <param name="collision">The collision to process.</param>
        protected void processCollisionSingleAveragedContact(TCollision collision)
        {
            //Clone first contact point 
            TContact averagedContactPoint = collision.GetContact(0);

            //Iterate over all contact points (excluding the first)
            for (int i = 1; i < collision.ContactCount; i++)
            {
                TContact contactPoint = collision.GetContact(i);
                averagedContactPoint.Point += contactPoint.Point;
                averagedContactPoint.Normal += contactPoint.Normal;
            }

            averagedContactPoint.Point /= collision.ContactCount;
            averagedContactPoint.Normal = averagedContactPoint.Normal.normalized;

            processCollisionContact(collision, averagedContactPoint);
        }

        /// <summary>
        /// Takes the given IImpactCollisionWrapper and processes all of it's contact points.
        /// Override buildAndSendCollisionParameters to receive the data and convert it to IInteractionData to send to an IImpactObject.
        /// </summary>
        /// <param name="collision">The collision to process.</param>
        protected void processCollisionMultipleContacts(TCollision collision)
        {
            //Iterate over all contact points
            for (int i = 0; i < collision.ContactCount; i++)
            {
                TContact contactPoint = collision.GetContact(i);
                processCollisionContact(collision, contactPoint);
            }
        }

        /// <summary>
        /// Process a collision contact to get all material and velocity data.
        /// The material and velocity data will be sent to buildInteractionData, which you can override.
        /// </summary>
        /// <param name="collision">The original collision data.</param>
        /// <param name="contactPoint">The contact point of the collision to use in calculations.</param>
        protected void processCollisionContact(TCollision collision, TContact contactPoint)
        {
            IImpactObject myObject = getImpactObject(contactPoint.ThisObject);

            if (myObject != null)
            {
                IImpactObject otherObject = contactPoint.OtherObject.GetComponentInParent<IImpactObject>();
                bool hasOtherObject = otherObject != null;

                //Material composition is enabled
                if (UseMaterialComposition && hasOtherObject)
                {
                    //Get material composition
                    int count = otherObject.GetMaterialCompositionNonAlloc(contactPoint.Point, ImpactManagerInstance.MaterialCompositionBuffer);

                    //Get velocity data
                    VelocityData myVelocityData = myObject.GetVelocityDataAtPoint(contactPoint.Point);
                    VelocityData otherVelocityData = otherObject.GetVelocityDataAtPoint(contactPoint.Point);

                    //Create interaction for each material
                    for (int i = 0; i < count; i++)
                    {
                        ImpactMaterialComposition comp = ImpactManagerInstance.MaterialCompositionBuffer[i];
                        if (comp.CompositionValue > 0)
                            buildInteractionData(myObject, collision, contactPoint, myVelocityData, otherVelocityData, comp.Material.MaterialTagsMask, comp.CompositionValue);
                    }
                }
                //Material composition is not enabled
                else
                {
                    //Get velocity data
                    VelocityData otherVelocityData = hasOtherObject ? otherObject.GetVelocityDataAtPoint(contactPoint.Point) : new VelocityData();
                    VelocityData myVelocityData = myObject.GetVelocityDataAtPoint(contactPoint.Point);

                    //Get tag mask
                    ImpactTagMask? tagMask = getOtherObjectTagMask(otherObject, contactPoint.Point, contactPoint.OtherPhysicsMaterialID, hasOtherObject);

                    //Create interaction
                    buildInteractionData(myObject, collision, contactPoint, myVelocityData, otherVelocityData, tagMask, 1);
                }
            }
            else
            {
                //Impact object could not be found. MainTarget is not assigned and no Impact Object could be determined from the contact point.
                Debug.LogError("Unable to find Impact Object on GameObject " + gameObject.name);
            }
        }

        protected ImpactTagMask? getOtherObjectTagMask(IImpactObject impactObject, Vector3 point, int otherPhysicsMaterialID, bool hasOtherObject)
        {
            if (hasOtherObject)
            {
                IImpactMaterial m = impactObject.GetPrimaryMaterial(point);

                if (m != null)
                    return m.MaterialTagsMask;
                else if (ImpactManagerInstance.UseMaterialMapping && ImpactManagerInstance.TryGetImpactMaterialFromMapping(otherPhysicsMaterialID, out m))
                    return m.MaterialTagsMask;
            }
            else
            {
                IImpactMaterial m;
                if (ImpactManagerInstance.UseMaterialMapping && ImpactManagerInstance.TryGetImpactMaterialFromMapping(otherPhysicsMaterialID, out m))
                    return m.MaterialTagsMask;
            }

            return null;
        }

        /// <summary>
        /// Called by the process methods to build IInteractionData.
        /// Override this if your custom triggers are using any of the process methods.
        /// </summary>
        /// <param name="target">The target IImpactObject to send data to. You do not necessarily have to send data to this object.</param>
        /// <param name="collision">The collision being processed.</param>
        /// <param name="contactPoint">The collision contact point.</param>
        /// <param name="myVelocityData">The velocity data of this object.</param>
        /// <param name="otherVelocityData">The velocity data of the object being collided with.</param>
        /// <param name="tagMask">The tag mask obtained from the other object. Can be null.</param>
        /// <param name="compositionValue">The material composition value.</param>
        protected virtual void buildInteractionData(IImpactObject target, TCollision collision, TContact contactPoint, VelocityData myVelocityData, VelocityData otherVelocityData, ImpactTagMask? tagMask, float compositionValue)
        { }

        protected void invokeTriggeredEvent(InteractionData interactionData, IImpactObject impactObject)
        {
            OnTriggered?.Invoke(interactionData, impactObject);
        }
    }
}
