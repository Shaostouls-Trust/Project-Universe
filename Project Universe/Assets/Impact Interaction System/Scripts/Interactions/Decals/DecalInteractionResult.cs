using Impact.Objects;
using Impact.Utility;
using Impact.Utility.ObjectPool;
using UnityEngine;

namespace Impact.Interactions.Decals
{
    /// <summary>
    /// The result of a decal interaction.
    /// Handles placing of decals for single collisions and sliding and rolling.
    /// </summary>
    public class DecalInteractionResult : IContinuousInteractionResult, IPoolable
    {
        /// <summary>
        /// The original interaction data this result was created from.
        /// </summary>
        public InteractionData OriginalData { get; set; }

        /// <summary>
        /// The decal prefab to place.
        /// </summary>
        public ImpactDecalBase DecalTemplate;
        /// <summary>
        /// Random interval range for placing decals for sliding and rolling.
        /// </summary>
        public Range CreationInterval;
        /// <summary>
        /// Whether the creation interval is based on time or distance.
        /// </summary>
        public InteractionIntervalType CreationIntervalType;

        /// <summary>
        /// The result key for updating sliding and rolling.
        /// </summary>
        public long Key { get; set; }

        /// <summary>
        /// Is a decal template defined?
        /// </summary>
        public bool IsValid
        {
            get { return DecalTemplate != null; }
        }

        /// <summary>
        /// Has this result been updated within the last FixedUpdate call?
        /// </summary>
        public bool IsAlive { get; private set; }

        private float intervalCounter;
        private float currentCreationIntervalTarget;
        private Vector3 previousCreationPosition;

        private IImpactObject parent;

        private bool isAvailable = true;

        /// <summary>
        /// Place decals using our data.
        /// </summary>
        /// <param name="parent">The Impact Object that created this result.</param>
        public void Process(IImpactObject parent)
        {
            this.parent = parent;

            ImpactDecalPool.CreateDecal(this, OriginalData.Point, OriginalData.Normal);
            IsAlive = true;

            currentCreationIntervalTarget = CreationInterval.RandomInRange();
            previousCreationPosition = OriginalData.Point;

            //Dispose immediately for Collision interaction types
            if (OriginalData.InteractionType == InteractionData.InteractionTypeCollision)
                Dispose();
        }

        /// <summary>
        /// Update IsAlive.
        /// </summary>
        public void FixedUpdate()
        {
            IsAlive = false;
        }

        /// <summary>
        /// Updates for sliding and rolling and places new decals if applicable.
        /// </summary>
        /// <param name="newResult">The new interaction result.</param>
        public void KeepAlive(IInteractionResult newResult)
        {
            IsAlive = true;

            DecalInteractionResult decalInteractionResult = newResult as DecalInteractionResult;

            if (CreationIntervalType == InteractionIntervalType.Time)
                intervalCounter += Time.fixedDeltaTime;
            else
                intervalCounter = Vector3.Distance(decalInteractionResult.OriginalData.Point, previousCreationPosition);

            if (intervalCounter >= currentCreationIntervalTarget)
            {
                currentCreationIntervalTarget = CreationInterval.RandomInRange();

                if (newResult.IsValid)
                    ImpactDecalPool.CreateDecal(this, decalInteractionResult.OriginalData.Point, decalInteractionResult.OriginalData.Normal);

                intervalCounter = 0;
                previousCreationPosition = decalInteractionResult.OriginalData.Point;
            }

            OriginalData = decalInteractionResult.OriginalData;
        }

        public void Dispose()
        {
            parent = null;
            DecalTemplate = null;

            MakeAvailable();
        }

        public void Retrieve()
        {
            isAvailable = false;
        }

        public void MakeAvailable()
        {
            isAvailable = true;
        }

        public bool IsAvailable()
        {
            return isAvailable;
        }
    }
}